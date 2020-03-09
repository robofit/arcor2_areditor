using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base {
    public class ActionParameterMetadata : IO.Swagger.Model.ActionParameterMeta {

        public ARServer.Models.BaseParameterExtra ParameterExtra;

        public ActionParameterMetadata(IO.Swagger.Model.ActionParameterMeta actionParameterMeta): base(actionParameterMeta.DefaultValue, actionParameterMeta.Description, actionParameterMeta.DynamicValue,
            actionParameterMeta.DynamicValueParents, actionParameterMeta.Extra, actionParameterMeta.Name, actionParameterMeta.Type) {
            switch (Type) {
                case "string_enum":
                    ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.StringEnumParameterExtra>(Extra);
                    break;
                case "integer_enum":
                    ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntegerEnumParameterExtra>(Extra);
                    break;
                case "int":
                    ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.IntParameterExtra>(Extra);
                    break;
                case "double":
                    ParameterExtra = JsonConvert.DeserializeObject<ARServer.Models.DoubleParameterExtra>(Extra);
                    break;
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
