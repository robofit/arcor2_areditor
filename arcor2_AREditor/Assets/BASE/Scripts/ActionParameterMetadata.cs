using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Base {
    public class ActionParameterMetadata : IO.Swagger.Model.ActionParameterMeta {

        public ARServer.Models.BaseParameterExtra ParameterExtra = null;

        public ActionParameterMetadata(IO.Swagger.Model.ActionParameterMeta actionParameterMeta): base(defaultValue: actionParameterMeta.DefaultValue, description: actionParameterMeta.Description, dynamicValue: actionParameterMeta.DynamicValue,
            dynamicValueParents: actionParameterMeta.DynamicValueParents, extra: actionParameterMeta.Extra, name: actionParameterMeta.Name, type: actionParameterMeta.Type) {
            if (Extra != null && Extra != "{}") {// TODO solve better than with test of brackets

                switch (Type) {
                    case "string_enum":
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.StringEnumParameterExtra>(Extra);
                        break;
                    case "integer_enum":
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntegerEnumParameterExtra>(Extra);
                        break;
                    case "integer":
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntParameterExtra>(Extra);
                        break;
                    case "double":
                        ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.DoubleParameterExtra>(Extra);
                        break;
                }
            }
            
        }

        public async Task<List<string>> LoadDynamicValues(string actionProviderId, List<IO.Swagger.Model.IdValue> parentParams) {
            if (!DynamicValue) {
                return new List<string>();
            }
            return await GameManager.Instance.GetActionParamValues(actionProviderId, Name, parentParams);
        }

        public T GetDefaultValue<T>() {
            return JsonConvert.DeserializeObject<T>(DefaultValue);
        } 

    }

}
