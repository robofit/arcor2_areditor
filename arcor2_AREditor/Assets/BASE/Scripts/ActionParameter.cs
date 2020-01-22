using System;

namespace Base {
    public class ActionParameter : IO.Swagger.Model.ActionParameter {
        public IO.Swagger.Model.ActionParameterMeta ActionParameterMetadata;
       
        public ActionParameter(IO.Swagger.Model.ActionParameterMeta actionParameterMetadata, IO.Swagger.Model.ActionParameter actionParameter = null) {
            ActionParameterMetadata = actionParameterMetadata;
            Id = ActionParameterMetadata.Name;
            Type = ActionParameterMetadata.Type;
            if (actionParameter != null) {
                Value = actionParameter.Value;
            }
        }       

        public ActionParameter(object value, IO.Swagger.Model.ActionParameterMeta actionParameterMetadata) {
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
    }

}
