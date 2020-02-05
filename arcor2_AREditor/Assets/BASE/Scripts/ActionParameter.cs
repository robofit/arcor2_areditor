using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base {
    public class ActionParameter : IO.Swagger.Model.ActionParameter {
        public IO.Swagger.Model.ActionParameterMeta ActionParameterMetadata;
        // Reference to parent Action
        public Action Action;

        /// <summary>
        /// Creates action parameter based on it's metadata, parent action and action paramater swagger model.
        /// </summary>
        /// <param name="actionParameterMetadata"></param>
        /// <param name="action"></param>
        /// <param name="actionParameter"></param>
        public ActionParameter(IO.Swagger.Model.ActionParameterMeta actionParameterMetadata, Action action, IO.Swagger.Model.ActionParameter actionParameter = null) {
            ActionParameterMetadata = actionParameterMetadata;
            Id = ActionParameterMetadata.Name;
            Type = ActionParameterMetadata.Type;
            Action = action;
            if (actionParameter != null) {
                Value = actionParameter.Value;
            }
        }

        public void UpdateActionParameter(IO.Swagger.Model.ActionParameter actionParameter) {
            Value = actionParameter.Value;
        }

        public ActionParameter(object value, IO.Swagger.Model.ActionParameterMeta actionParameterMetadata) {
            Value = value;
            ActionParameterMetadata = actionParameterMetadata;
        }

        public async Task<List<string>> LoadDynamicValues(List<IO.Swagger.Model.IdValue> parentParams) {
            if (!ActionParameterMetadata.DynamicValue) {
                return new List<string>();
            }
            return await GameManager.Instance.GetActionParamValues(Action.ActionProvider.GetProviderName(), ActionParameterMetadata.Name, parentParams);
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
