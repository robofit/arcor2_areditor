using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Base {
    public class ActionParameter : IO.Swagger.Model.ActionParameter {
        public ActionParameterMetadata ActionParameterMetadata;
        // Reference to parent Action
        public Action Action;

        /// <summary>
        /// Creates action parameter based on it's metadata, parent action and action paramater swagger model.
        /// </summary>
        /// <param name="actionParameterMetadata"></param>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public ActionParameter(IO.Swagger.Model.ActionParameterMeta actionParameterMetadata, Action action, string value) {
            Debug.Assert(value != null);
            ActionParameterMetadata = new ActionParameterMetadata(actionParameterMetadata);
            Id = ActionParameterMetadata.Name;
            Type = ActionParameterMetadata.Type;
            Action = action;
            Value = value;
            /* else {
                switch (Type) {
                    case "relative_pose":
                        //SetValue(Regex.Replace(new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(), position: new IO.Swagger.Model.Position()).ToJson(), @"\t|\n|\r", ""));
                        SetValue(new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(), position: new IO.Swagger.Model.Position()));
                        break;
                    case "integer_enum":
                        if (ActionParameterMetadata.DefaultValue != null)
                            Value = ActionParameterMetadata.DefaultValue;
                        else
                            SetValue(((ARServer.Models.IntegerEnumParameterExtra) ActionParameterMetadata.ParameterExtra).AllowedValues[0].ToString());
                        break;
                    case "string_enum":
                        if (ActionParameterMetadata.DefaultValue != null)
                            Value = ActionParameterMetadata.DefaultValue;
                        else
                            SetValue(((ARServer.Models.StringEnumParameterExtra) ActionParameterMetadata.ParameterExtra).AllowedValues[0]);
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
                    default:
                        Value = ActionParameterMetadata.DefaultValue;
                        break;

                }
            }*/
        }

        public ActionParameter(IO.Swagger.Model.ActionParameterMeta actionParameterMetadata, Action action, object value) {
            ActionParameterMetadata = new ActionParameterMetadata(actionParameterMetadata);
            Id = ActionParameterMetadata.Name;
            Type = ActionParameterMetadata.Type;
            Action = action;

            switch (Type) {
                case "relative_pose":
                    SetValue((IO.Swagger.Model.Pose) value);
                    break;
                case "integer_enum":
                case "int":
                    SetValue((int) value);
                    break;
                case "string_enum":
                case "pose":
                case "joints":
                case "string":
                    SetValue((string) value);
                    break;
                case "double":
                    SetValue((double) value);
                    break;
            }
        }



        public void UpdateActionParameter(IO.Swagger.Model.ActionParameter actionParameter) {
            Value = actionParameter.Value;
        }

        /*public ActionParameter(object value, IO.Swagger.Model.ActionParameterMeta actionParameterMetadata) {
            //Value = value;
            ActionParameterMetadata = actionParameterMetadata;
        }*/

        public T GetValue<T>() {
            return JsonConvert.DeserializeObject<T>(Value);
        }

        public static T GetValue<T>(string value) {
            if (value == null) {
                return default; 
            }                
            return JsonConvert.DeserializeObject<T>(value);
        }

        public void SetValue(object newValue) {
            if (newValue == null)
                Value = null;
            else
                Value = JsonConvert.SerializeObject(newValue);
        }


    }

}
