using System;

namespace Base {
    public class ActionParameter : IO.Swagger.Model.ActionParameter {
        public IO.Swagger.Model.ObjectActionArg ActionParameterMetadata;
        //public IO.Swagger.Model.ActionParameter Data = new IO.Swagger.Model.ActionParameter(id: "", value: "", type: new IO.Swagger.Model.ActionParameter.TypeEnum());

        public ActionParameter(IO.Swagger.Model.ActionParameter actionParameter) : base (id: actionParameter.Id, value: actionParameter.Value) {

        }
        public ActionParameter(IO.Swagger.Model.ObjectActionArg actionParameterMetadata) {
            ActionParameterMetadata = actionParameterMetadata;
            Id = ActionParameterMetadata.Name;
            Type = (IO.Swagger.Model.ActionParameter.TypeEnum) ActionParameterMetadata.Type;
            //Value = ActionParameterMetadata.DefaultValue;
        }

        public ActionParameter(object value, IO.Swagger.Model.ObjectActionArg actionParameterMetadata) {
            Value = value;
            ActionParameterMetadata = actionParameterMetadata;
        }

        public void GetValue(out string value, string def = "") {
            try {

                value = (string) Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out long value, long def = 0) {
            try {
                value = (long) Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out bool value, bool def = false) {
            try {

                value = (bool) Value;
            } catch (NullReferenceException e) {
                value = def;
            }
        }

        public void GetValue(out double value, double def = 0) {
            try {

                value = (double) Value;
            } catch (Exception ex) when (ex is NullReferenceException || ex is InvalidCastException) {
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
