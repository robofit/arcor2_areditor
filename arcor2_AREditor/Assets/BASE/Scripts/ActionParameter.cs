using System;

namespace Base {
    public class ActionParameter {
        private JSONObject value;
        private ActionParameterMetadata actionParameterMetadata;


        public ActionParameter(ActionParameterMetadata actionParameterMetadata) {
            Value = actionParameterMetadata.DefaultValue;
            ActionParameterMetadata = actionParameterMetadata;
        }

        public ActionParameter(JSONObject value, ActionParameterMetadata actionParameterMetadata) {
            Value = value;
            this.actionParameterMetadata = actionParameterMetadata;
        }


        public void GetValue(out string value, string def = "") {
            try {

                value = Value["value"].str;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out long value, long def = 0) {
            try {
                value = Value["value"].i;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out bool value, bool def = false) {
            try {

                value = Value["value"].b;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void SetValue(string value) {
            Value = new JSONObject(JSONObject.Type.OBJECT);
            Value.AddField("value", value);
        }

        public void SetValue(long value) {
            Value = new JSONObject(JSONObject.Type.OBJECT);
            Value.AddField("value", value);
        }

        public void SetValue(bool value) {
            Value = new JSONObject(JSONObject.Type.OBJECT);
            Value.AddField("value", value);
        }

        public ActionParameterMetadata ActionParameterMetadata {
            get => actionParameterMetadata; set => actionParameterMetadata = value;
        }
        public JSONObject Value {
            get => value; set => this.value = value;
        }
    }

}
