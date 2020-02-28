using Newtonsoft.Json;

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

    }

}
