
namespace Base {
    public class ActionParameterMetadata {
        private string name;
        private IO.Swagger.Model.ActionParameter.TypeEnum type;
        private object defaultValue;

        

        public ActionParameterMetadata(string name, IO.Swagger.Model.ActionParameter.TypeEnum type, object defaultValue) {
            this.name = name;
            this.type = type;
            this.defaultValue = defaultValue;
        }

        static public IO.Swagger.Model.ActionParameter.TypeEnum StringToType(string type) {
            switch (type) {
                case "str":
                    return IO.Swagger.Model.ActionParameter.TypeEnum.String;
                case "double":
                    return IO.Swagger.Model.ActionParameter.TypeEnum.Double;
                case "int":
                    return IO.Swagger.Model.ActionParameter.TypeEnum.Integer;
                case "ActionPoint":
                    return IO.Swagger.Model.ActionParameter.TypeEnum.ActionPoint;
            }
            return new IO.Swagger.Model.ActionParameter.TypeEnum();
        }

        static public string TypeToString(IO.Swagger.Model.ActionParameter.TypeEnum type) {
            switch (type) {
                case IO.Swagger.Model.ActionParameter.TypeEnum.String:
                    return "str";
                case IO.Swagger.Model.ActionParameter.TypeEnum.ActionPoint:
                    return "ActionPoint";
                case IO.Swagger.Model.ActionParameter.TypeEnum.Double:
                    return "double";
                case IO.Swagger.Model.ActionParameter.TypeEnum.Integer:
                    return "int";
                default:
                    return "unknown";
            }
        }

        public void SetType(string type) {
            Type = StringToType(type);
        }

        public string GetStringType() {
            return TypeToString(Type);
        }


        public string Name {
            get => name; set => name = value;
        }
        public IO.Swagger.Model.ActionParameter.TypeEnum Type {
            get => type; set => type = value;
        }
        public object DefaultValue {
            get => defaultValue; set => defaultValue = value;
        }
    }

}
