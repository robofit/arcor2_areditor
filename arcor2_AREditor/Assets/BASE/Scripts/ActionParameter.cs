using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Text.RegularExpressions;

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
            } else {
                switch (Type) {
                    case "relative_pose":
                        SetValue(Regex.Replace(new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(), position: new IO.Swagger.Model.Position()).ToJson(), @"\t|\n|\r", ""));
                        break;
                    case "integer_enum":
                        SetValue(actionParameterMetadata.AllowedValues[0]);
                        break;
                    case "string_enum":
                        SetValue(actionParameter.Value = (string) actionParameterMetadata.AllowedValues[0]);
                        break;
                    case "pose":
                        List<string> poses = new List<string>(action.ActionPoint.GetPoses().Keys);
                        if (poses.Count == 0) {
                            SetValue("");
                            //TODO: where to get valid ID?
                        } else {
                            SetValue(action.ActionPoint.ActionObject.Data.Id + "." + action.ActionPoint.Data.Id + "." + poses[0]);
                        }
                        break;
                    case "joints":
                        List<string> joints = new List<string>(action.ActionPoint.GetJoints().Keys);
                        if (joints.Count == 0) {
                            SetValue("");
                            //TODO: where to get valid ID?
                        } else {
                            SetValue(action.ActionPoint.ActionObject.Data.Id + "." + action.ActionPoint.Data.Id + "." + joints[0]);
                        }
                        break;
                    case "int":
                        SetValue(0);
                        break;
                    case "double":
                        SetValue(0d);
                        break;
                    case "string":
                        SetValue("");
                        break;
                    default:
                        //actionParameter.Value = actionParameterMetadata.DefaultValue;
                        break;

                }
            }
        }

        public void UpdateActionParameter(IO.Swagger.Model.ActionParameter actionParameter) {
            Value = actionParameter.Value;
        }

        /*public ActionParameter(object value, IO.Swagger.Model.ActionParameterMeta actionParameterMetadata) {
            //Value = value;
            ActionParameterMetadata = actionParameterMetadata;
        }*/

        public async Task<List<string>> LoadDynamicValues(List<IO.Swagger.Model.IdValue> parentParams) {
            if (!ActionParameterMetadata.DynamicValue) {
                return new List<string>();
            }
            return await GameManager.Instance.GetActionParamValues(Action.ActionProvider.GetProviderName(), ActionParameterMetadata.Name, parentParams);
        }

        public T GetValue<T>() {            
            return JsonConvert.DeserializeObject<T>(Value);
        }

        public void SetValue(object newValue) {
            Value = JsonConvert.SerializeObject(newValue);
        }
    }

}
