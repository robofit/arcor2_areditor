using System;

namespace Base {
    public class ActionParameter {
        public ActionParameterMetadata ActionParameterMetadata;
        public IO.Swagger.Model.ActionParameter Data = new IO.Swagger.Model.ActionParameter("", new IO.Swagger.Model.ActionParameter.TypeEnum(), "");

        public ActionParameter() {

        }
        public ActionParameter(ActionParameterMetadata actionParameterMetadata) {
            ActionParameterMetadata = actionParameterMetadata;
            Data.Id = ActionParameterMetadata.Name;
            Data.Type = ActionParameterMetadata.Type;
            Data.Value = ActionParameterMetadata.DefaultValue;
        }

        public ActionParameter(object value, ActionParameterMetadata actionParameterMetadata) {
            Data.Value = value;
            ActionParameterMetadata = actionParameterMetadata;
        }

        public void GetValue(out string value, string def = "") {
            try {

                value = (string) Data.Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out long value, long def = 0) {
            try {
                value = (long) Data.Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out bool value, bool def = false) {
            try {

                value = (bool) Data.Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }


        /*
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
        }*/
    }

}
