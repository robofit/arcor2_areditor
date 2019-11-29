
namespace Base {
    public class ActionParameterMetadatas : IO.Swagger.Model.ObjectActionArgs {
        private string name;
        private IO.Swagger.Model.ActionParameter.TypeEnum type;
        private object defaultValue;

        

        public ActionParameterMetadatas(string name, IO.Swagger.Model.ActionParameter.TypeEnum type, object defaultValue) {
            this.name = name;
            this.type = type;
            this.defaultValue = defaultValue;
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
