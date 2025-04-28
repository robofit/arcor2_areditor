using System.Collections.Generic;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using ARServer.Models;
using Newtonsoft.Json;
using RestSharp.Extensions;
using UnityEngine;

namespace Base {
    public class ParameterMetadata : ParameterMeta {

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

        public BaseParameterExtra ParameterExtra = null;

        public ParameterMetadata(ParameterMeta actionParameterMeta): base(defaultValue: actionParameterMeta.DefaultValue, description: actionParameterMeta.Description, dynamicValue: actionParameterMeta.DynamicValue,
            dynamicValueParents: actionParameterMeta.DynamicValueParents, extra: actionParameterMeta.Extra, name: actionParameterMeta.Name, type: actionParameterMeta.Type) {
            if (Extra != null && Extra != "{}") {// TODO solve better than with test of brackets

                switch (Type) {
                    case STR_ENUM:
                        ParameterExtra = JsonConvert.DeserializeObject<StringEnumParameterExtra>(Extra);
                        break;
                    case INT_ENUM:
                        ParameterExtra = JsonConvert.DeserializeObject<IntegerEnumParameterExtra>(Extra);
                        break;
                    case INT:
                        ParameterExtra = JsonConvert.DeserializeObject<IntParameterExtra>(Extra);
                        break;
                    case DOUBLE:
                        ParameterExtra = JsonConvert.DeserializeObject<DoubleParameterExtra>(Extra);
                        break;
                }
            }
            
        }

        public async Task<List<string>> LoadDynamicValues(string actionProviderId, List<IdValue> parentParams) {
            if (!DynamicValue) {
                return new List<string>();
            }

            try {
                return (await CommunicationManager.Instance.Client.GetActionParameterValuesAsync(
                    new ActionParamValuesRequestArgs(actionProviderId, Name, parentParams))).Data;
            } catch {
                return new List<string>();
            }
        }

        public T GetDefaultValue<T>() {
            if (DefaultValue == null)
                if (ParameterExtra != null) {
                    switch (Type) {
                        case INT:
                            return (T) ((IntParameterExtra) ParameterExtra).Minimum.ChangeType(typeof(T));
                        case DOUBLE:
                            return (T) ((DoubleParameterExtra) ParameterExtra).Minimum.ChangeType(typeof(T));
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
                    throw new Arcor2ConnectionException("Unknown type");

            }
        }

    }

}
