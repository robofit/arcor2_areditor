
namespace Base {
    public class ActionParameterMetadata {
        string _name;
        Types _type;
        JSONObject _defaultValue;

        public enum Types {
            Integer, Double, String, ActionPoint, Bool, Unknown
        }

        public ActionParameterMetadata(string name, string type, JSONObject defaultValue) {
            _name = name;
            _type = StringToType(type);
            _defaultValue = defaultValue;
        }

        static public Types StringToType(string type) {
            switch (type) {
                case "str":
                    return Types.String;
                case "int":
                    return Types.Integer;
                case "ActionPoint":
                    return Types.ActionPoint;
                case "double":
                    return Types.Double;
                case "bool":
                    return Types.Bool;
                default:
                    return Types.Unknown;
            }
        }

        static public string TypeToString(Types type) {
            switch (type) {
                case Types.String:
                    return "str";
                case Types.Integer:
                    return "int";
                case Types.ActionPoint:
                    return "ActionPoint";
                case Types.Double:
                    return "double";
                case Types.Bool:
                    return "bool";
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
            get => _name; set => _name = value;
        }
        public Types Type {
            get => _type; set => _type = value;
        }
        public JSONObject DefaultValue {
            get => _defaultValue; set => _defaultValue = value;
        }
    }

}
