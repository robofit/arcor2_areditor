
namespace Base {
    public class ActionParameterMetadatas : IO.Swagger.Model.ActionParameterMeta {
        private string name;
        private string type;
        private object defaultValue;

        

        public ActionParameterMetadatas(string name, string type, object defaultValue) {
            this.name = name;
            this.type = type;
            this.defaultValue = defaultValue;
        }

      


        public string Name {
            get => name; set => name = value;
        }
        public string Type {
            get => type; set => type = value;
        }
        public object DefaultValue {
            get => defaultValue; set => defaultValue = value;
        }
    }

}
