using Newtonsoft.Json;
using RestSharp.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Base {
    public class ParameterMetadata : IO.Swagger.Model.ParameterMeta {

        public const string INT = "integer";
        public const string DOUBLE = "double";
        public const string STR = "string";
        public const string STR_ENUM = "string_enum";
        public const string INT_ENUM = "integer_enum";
        public const string REL_POSE = "relative_pose";
        public const string JOINTS = "joints";
        public const string BOOL = "boolean";
        public const string POSE = "pose";
        public const string POSITION = "position";

        public ARServer.Models.BaseParameterExtra ParameterExtra = null;

        public ParameterMetadata(IO.Swagger.Model.ParameterMeta actionParameterMeta): base(defaultValue: actionParameterMeta.DefaultValue, description: actionParameterMeta.Description, dynamicValue: actionParameterMeta.DynamicValue,
            dynamicValueParents: actionParameterMeta.DynamicValueParents, extra: actionParameterMeta.Extra, name: actionParameterMeta.Name, type: actionParameterMeta.Type) {
            if (Extra != null && Extra != "{}") {// TODO solve better than with test of brackets

                switch (Type) {
                    case STR_ENUM:
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.StringEnumParameterExtra>(Extra);
                        break;
                    case INT_ENUM:
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntegerEnumParameterExtra>(Extra);
                        break;
                    case INT:
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntParameterExtra>(Extra);
                        break;
                    case DOUBLE:
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.DoubleParameterExtra>(Extra);
                        break;
                }
            }
            
        }

        public async Task<List<string>> LoadDynamicValues(string actionProviderId, List<IO.Swagger.Model.IdValue> parentParams) {
            if (!DynamicValue) {
                return new List<string>();
            }
            return await WebsocketManager.Instance.GetActionParamValues(actionProviderId, Name, parentParams);
        }

        public T GetDefaultValue<T>() {
            if (DefaultValue == null)
                if (ParameterExtra != null) {
                    switch (Type) {
                        case INT:
                            return (T) ((ARServer.Models.IntParameterExtra) ParameterExtra).Minimum.ChangeType(typeof(T));
                        case DOUBLE:
                            return (T) ((ARServer.Models.DoubleParameterExtra) ParameterExtra).Minimum.ChangeType(typeof(T));
                    }
                } else {
                    return default;
                }
            return JsonConvert.DeserializeObject<T>(DefaultValue);
        }

        public object GetDefaultValue() {
            switch (Type) {
                case INT:
                    return GetDefaultValue<int>();
                case DOUBLE:
                    return GetDefaultValue<double>();
                case STR:
                    return GetDefaultValue<string>();
                case STR_ENUM:
                    return GetDefaultValue<string>();
                case INT_ENUM:
                    return GetDefaultValue<int>();
                case REL_POSE:
                    return GetDefaultValue<string>();
                case JOINTS:
                    return GetDefaultValue<string>();
                case BOOL:
                    return GetDefaultValue<bool>();
                case POSE:
                    try {
                        return ProjectManager.Instance.GetAnyNamedOrientation().Id;
                    } catch (ItemNotFoundException) {
                        return null;
                    }
                case POSITION:
                    try {
                        return ProjectManager.Instance.GetAnyActionPoint().GetId();
                    } catch (ItemNotFoundException) {
                        return null;
                    }
                default:
                    Debug.LogError($"Trying to use unsupported parameter type: {Type}");
                    throw new RequestFailedException("Unknown type");

            }
        }

    }

}
