using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using MiniJSON;
using RosSharp.Urdf;

namespace Base {
    public class Parameter : IO.Swagger.Model.ActionParameter {
        public ParameterMetadata ParameterMetadata;

        public delegate void OnChangeParameterHandlerDelegate(string parameterId, object newValue, string type, bool isValueValid = true);
        public delegate DropdownParameter GetDropdownParameterDelegate(string parameterId, GameObject parentParam);

        /// <summary>
        /// Creates action parameter based on it's metadata, parent action and action parameter swagger model.
        /// </summary>
        /// <param name="parameterMetadata"></param>
        /// <param name="action"></param>
        /// <param name="value"></param>
        public Parameter(IO.Swagger.Model.ParameterMeta parameterMetadata, string type, string value) {
            Debug.Assert(value != null);
            ParameterMetadata = new ParameterMetadata(parameterMetadata);
            Name = ParameterMetadata.Name;
            Type = type;
            Value = value;
            
        }

        public Parameter(IO.Swagger.Model.ParameterMeta parameterMetadata, string value) : this(parameterMetadata, parameterMetadata.Type, value) { }

        public Parameter(IO.Swagger.Model.ParameterMeta parameterMetadata, object value) : this(parameterMetadata, parameterMetadata.Type, value) { }


        public Parameter(IO.Swagger.Model.ParameterMeta actionParameterMetadata, string type, object value) {
            ParameterMetadata = new ParameterMetadata(actionParameterMetadata);
            Name = ParameterMetadata.Name;
            Type = type;

            switch (Type) {
                case ParameterMetadata.INT_ENUM:
                case ParameterMetadata.INT:
                    SetValue((int) value);
                    break;
                case ParameterMetadata.STR_ENUM:
                case ParameterMetadata.POSE:
                case ParameterMetadata.POSITION:
                case ParameterMetadata.JOINTS:
                case ParameterMetadata.STR:
                    SetValue((string) value);
                    break;
                case ParameterMetadata.DOUBLE:
                    SetValue((double) value);
                    break;
                case ParameterMetadata.BOOL:
                    SetValue((bool) value);
                    break;
            }
        }



        public void UpdateActionParameter(IO.Swagger.Model.Parameter parameter) {
            Value = parameter.Value;
            Type = parameter.Type;
        }

        public T GetValue<T>() {
            return JsonConvert.DeserializeObject<T>(Value);
        }

        public static T GetValue<T>(string value, string type = null) {
            if (value == null) {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static string Encode(string value, string type) {
            switch (type) {
                case ParameterMetadata.INT_ENUM:
                case ParameterMetadata.INT:
                    return JsonConvert.SerializeObject(int.Parse(value));
                case ParameterMetadata.STR_ENUM:
                case ParameterMetadata.POSE:
                case ParameterMetadata.POSITION:
                case ParameterMetadata.JOINTS:
                case ParameterMetadata.STR:
                    return JsonConvert.SerializeObject(value);                    
                case ParameterMetadata.DOUBLE:
                    return JsonConvert.SerializeObject(double.Parse(value, CultureInfo.InvariantCulture));
                case ParameterMetadata.BOOL:
                    return JsonConvert.SerializeObject(bool.Parse(value));
            }
            throw new RequestFailedException("Unknown parameter type (" + type + ")");
        }

        public string GetStringValue() {
            return GetStringValue(Value, Type);
        }


        public static string GetStringValue(string value, string type) {
            switch (type) {
                case ParameterMetadata.INT_ENUM:
                case ParameterMetadata.INT:
                    return GetValue<int>(value).ToString();
                case ParameterMetadata.STR_ENUM:
                case ParameterMetadata.POSE:
                case ParameterMetadata.POSITION:
                case ParameterMetadata.JOINTS:
                case ParameterMetadata.STR:
                case LinkableParameter.LINK:
                    return GetValue<string>(value).ToString();
                case ParameterMetadata.DOUBLE:
                    return GetValue<double>(value).ToString();
                case ParameterMetadata.BOOL:
                    return GetValue<bool>(value).ToString();
            }
            throw new RequestFailedException("Unknown parameter type");
        } 

        public void SetValue(object newValue) {
            if (newValue == null)
                Value = null;
            else
                Value = JsonConvert.SerializeObject(newValue);
        }
        public static IParameter InitializeStringParameter(ParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, string value, string type, bool linkable) {
            string selectedValue = "";
            if (value != null) {
                selectedValue = Parameter.GetValue<string>(value);
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = JsonConvert.DeserializeObject<string>(actionParameterMetadata.DefaultValue);
            }
            if (actionParameterMetadata.DynamicValue) {

                DropdownParameter input = InitializeDropdownParameter(actionParameterMetadata, new List<string>(), selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownPrefab).GetComponent<DropdownParameter>();
                input.GetComponent<DropdownParameter>().SetLoading(true);
                return input;
            } else {
                LinkableInput input = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterInputPrefab).GetComponent<LinkableInput>();
                
                input.GetComponent<LinkableInput>().Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
                return input;
            }
        }

        public static IParameter InitializeRelativePoseParameter(Base.ParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, IO.Swagger.Model.Pose value, bool linkable) {
            RelPoseParam input;
            IO.Swagger.Model.Pose selectedValue = new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(),
                position: new IO.Swagger.Model.Position());
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(actionParameterMetadata.DefaultValue);
            }

            input = GameObject.Instantiate(ActionsManager.Instance.ParameterRelPosePrefab).GetComponent<RelPoseParam>();
            input.SetValue(selectedValue);
            input.OnValueChangedEvent.AddListener((IO.Swagger.Model.Pose newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, newValue, actionParameterMetadata.Type));
            return input;
        }

        public static void OnChangeRelativePose(string parameterName, string newValue, OnChangeParameterHandlerDelegate onChangeParameterHandler) {
            try {
                IO.Swagger.Model.Pose pose = JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(newValue);
                onChangeParameterHandler(parameterName, pose, "relative_pose");
            } catch (JsonReaderException) {
                onChangeParameterHandler(parameterName, null, "relative_pose", false);
            }
        }

        public static GameObject InitializeDropdownParameter(ParameterMetadata actionParameterMetadata, List<string> data, string selectedValue, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, GameObject dropdownPrefab) {
            DropdownParameter dropdownParameter = GameObject.Instantiate(dropdownPrefab).GetComponent<DropdownParameter>();
            dropdownParameter.Init(layoutGroupToBeDisabled, canvasRoot, actionParameterMetadata.Type);
            DropdownParameterPutData(dropdownParameter, data, selectedValue, actionParameterMetadata.Name, onChangeParameterHandler, actionParameterMetadata.Type);
            return dropdownParameter.gameObject;
        }

        public static IParameter InitializeStringEnumParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable) {
            string selectedValue = null;
            if (value != null) {
                selectedValue = Parameter.GetValue<string>(value);
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<string>();
            }
            List<string> data = new List<string>();
            foreach (string item in ((ARServer.Models.StringEnumParameterExtra) actionParameterMetadata.ParameterExtra).AllowedValues)
                data.Add(item);
            LinkableDropdown dropdownParameter = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterDropdownPrefab).GetComponent<LinkableDropdown>();
            dropdownParameter.Init(actionParameterMetadata, type, null, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
            DropdownParameterPutData(dropdownParameter.DropdownParameter, data, selectedValue, actionParameterMetadata.Name, onChangeParameterHandler, actionParameterMetadata.Type);
            return dropdownParameter;

        }

        public static IParameter InitializeIntegerEnumParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable) {
            List<string> options = new List<string>();
            int? selectedValue = null;
            if (value != null) {
                selectedValue = Parameter.GetValue<int?>(value);
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<int>();
            }
            foreach (int item in ((ARServer.Models.IntegerEnumParameterExtra) actionParameterMetadata.ParameterExtra).AllowedValues) {
                options.Add(item.ToString());
            }
            string selectedValueString = null;
            if (selectedValue != null) {
                selectedValueString = selectedValue.ToString();
            }
            LinkableDropdown dropdownParameter = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterDropdownPrefab).GetComponent<LinkableDropdown>();
            dropdownParameter.Init(actionParameterMetadata, type, null, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
            DropdownParameterPutData(dropdownParameter.DropdownParameter, options, selectedValueString, actionParameterMetadata.Name, onChangeParameterHandler, actionParameterMetadata.Type);
            return dropdownParameter;


        }

        public static IParameter InitializePoseParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint) {

            LinkableDropdownPoses dropdownParameter = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterDropdownPosesPrefab).GetComponent<LinkableDropdownPoses>();
            string selectedValue = null;
            if (value != null) {
                selectedValue = Parameter.GetValue<string>(value);
            } 
            dropdownParameter.Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, windowToHideWhenRequestingObj, actionPoint, linkable);
            return dropdownParameter;

           
        }



        public static IParameter InitializePositionParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint) {

            LinkableDropdownPositions dropdownParameter = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterDropdownPositionsPrefab).GetComponent<LinkableDropdownPositions>();
            string selectedValue = null;
            if (value != null) {
                selectedValue = Parameter.GetValue<string>(value);
            }
            dropdownParameter.Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, windowToHideWhenRequestingObj, actionPoint, linkable);
            return dropdownParameter;


        }

        public static IParameter InitializeJointsParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string actionProviderId = "") {
            Dictionary<string, bool> options = new Dictionary<string, bool>();
            foreach (Base.ActionPoint ap in Base.ProjectManager.Instance.GetAllActionPoints()) {
                foreach (IO.Swagger.Model.ProjectRobotJoints joints in ap.GetAllJoints(false, null, false).Values) {
                    string prefix = "";
                    if (joints.RobotId != actionProviderId)
                        prefix = "(another robot) ";
                    else if (!joints.IsValid) {
                        prefix = "(invalid) ";
                    }
                    options.Add(prefix + ap.Data.Name + "." + joints.Name, joints.IsValid);
                }
            }
            string selectedValue = null;
            if (value != null) {
                try {
                    ActionPoint actionPoint = ProjectManager.Instance.GetActionPointWithJoints(value);
                    IO.Swagger.Model.ProjectRobotJoints joints = actionPoint.GetJoints(value);
                    selectedValue = actionPoint.Data.Name + "." + joints.Name;
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }

            }
            DropdownParameterJoints dropdownParameter = GameObject.Instantiate(ActionsManager.Instance.ParameterDropdownJointsPrefab).GetComponent<DropdownParameterJoints>();
            dropdownParameter.Init(layoutGroupToBeDisabled, canvasRoot, actionParameterMetadata.Type);
            dropdownParameter.PutData(options, selectedValue,
                (_) => onChangeParameterHandler(actionParameterMetadata.Name, dropdownParameter.GetValue(), actionParameterMetadata.Type));
            if (selectedValue == "" || selectedValue == null) {
                string v;
                if (dropdownParameter.Dropdown.dropdownItems.Count == 0)
                    v = "";
                else
                    v = dropdownParameter.Dropdown.selectedText.text;

                onChangeParameterHandler(actionParameterMetadata.Name, dropdownParameter.GetValue(), actionParameterMetadata.Type);
            }
            return dropdownParameter;
        }

        public static IParameter InitializeIntegerParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable) {
            LinkableInput input = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterInputPrefab).GetComponent<LinkableInput>();
            object selectedValue = null;
            if (value != null) {
                if (type == LinkableParameter.PROJECT_PARAMETER || type == LinkableParameter.LINK)
                    selectedValue = value; //id of project parameter or link to action
                else
                    selectedValue = Parameter.GetValue<int?>(value.ToString());
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<int>();
            }
            input.Input.Input.onValueChanged.AddListener((string newValue)
                => ValidateIntegerParameter(input.Input, actionParameterMetadata, int.Parse(newValue)));
            input.Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);

            return input;
        }

        public static IParameter InitializeBooleanParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable) {
            LinkableBoolParameter parameter = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterBooleanPrefab).GetComponent<LinkableBoolParameter>();
            object selectedValue = null;
            if (value != null) {
                if (type == LinkableParameter.PROJECT_PARAMETER || type == LinkableParameter.LINK)
                    selectedValue = value; //id of project parameter or link to action
                else
                    selectedValue = Parameter.GetValue<bool?>(value.ToString());
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<bool>();
            }
            parameter.Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
            
            return parameter;
        }

        public static IParameter InitializeDoubleParameter(ParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value, string type, bool linkable) {
            LinkableInput input = GameObject.Instantiate(ActionsManager.Instance.LinkableParameterInputPrefab).GetComponent<LinkableInput>();

            object selectedValue = null;
            if (value != null) {
                if (type == LinkableParameter.PROJECT_PARAMETER || type == LinkableParameter.LINK)
                    selectedValue = value; //id of project parameter or link to action
                else
                    selectedValue = Parameter.GetValue<double?>(value.ToString());
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<double>();
            }

            input.Input.Input.onValueChanged.AddListener((string newValue)
                => ValidateDoubleParameter(input.Input, actionParameterMetadata, ParseDouble(newValue)));
            input.Init(actionParameterMetadata, type, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
            return input;
        }


        public static double ParseDouble(string value) {
            if (string.IsNullOrEmpty(value))
                return 0;
            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {

            }
            return result;
        }

        private static void DropdownParameterPutData(DropdownParameter dropdownParameter, List<string> data, string selectedValue, string parameterId, OnChangeParameterHandlerDelegate onChangeParameterHandler, string type) {
            dropdownParameter.PutData(data, selectedValue,
                (value) => onChangeParameterHandler(parameterId, dropdownParameter.GetValue(), type));
            if (selectedValue == "" || selectedValue == null) {
                string value;
                if (dropdownParameter.Dropdown.dropdownItems.Count == 0)
                    value = "";
                else
                    value = dropdownParameter.Dropdown.selectedText.text;

                onChangeParameterHandler(parameterId, dropdownParameter.GetValue(), type);
            }
        }

        public static async Task LoadDropdownValues(string actionProviderId, string selectedValue, DropdownParameter dropdownParameter, ParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, GameObject parentObject, UnityAction callback = null) {
            List<string> values = new List<string>();
            List<IO.Swagger.Model.IdValue> args = new List<IO.Swagger.Model.IdValue>();
            if (actionParameterMetadata.DynamicValueParents != null) {
                foreach (string parent_param_id in actionParameterMetadata.DynamicValueParents) {
                    DropdownParameter parent_param = GetDropdownParameter(parent_param_id, parentObject);

                    string value = (string) parent_param.GetValue();
                    if (value == null) {
                        Debug.LogError("Parent param has no value, this should never happen!");
                        continue;
                    }

                    IO.Swagger.Model.IdValue idValue = new IO.Swagger.Model.IdValue(id: parent_param_id, value: value);
                    args.Add(idValue);


                    if (callback != null)
                        AddOnChangeToDropdownParameter(parent_param, callback);
                }
            }
            values = await actionParameterMetadata.LoadDynamicValues(actionProviderId, args);
            DropdownParameterPutData(dropdownParameter, values, selectedValue, actionParameterMetadata.Name, onChangeParameterHandler, actionParameterMetadata.Type);
        }

        private static void AddOnChangeToDropdownParameter(DropdownParameter dropdownParameter, UnityAction callback) {
            foreach (CustomDropdown.Item item in dropdownParameter.Dropdown.dropdownItems) {
                item.OnItemSelection.AddListener(callback);
            }
        }

        public static List<IParameter> InitParameters(List<ParameterMetadata> parameter_metadatas, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot, bool darkMode, bool linkable, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint) {
            List<IParameter> parameters = new List<IParameter>();
            foreach (ParameterMetadata parameterMetadata in parameter_metadatas) {
                IParameter param = InitializeParameter(parameterMetadata, handler, dynamicContentLayout, canvasRoot, null, parameterMetadata.Type, windowToHideWhenRequestingObj, actionPoint, darkMode, "", linkable);
                if (param == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameterMetadata.Type);
                    continue;
                }
                parameters.Add(param);
                param.GetTransform().SetParent(parentObject.transform);
                param.GetTransform().localScale = new Vector3(1, 1, 1);
            }
            return parameters;
        }

        public static List<IParameter> InitParameters(List<Parameter> _parameters, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot, bool darkMode, bool linkable, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint) {
            List<IParameter> parameters = new List<IParameter>();
            foreach (Parameter parameter in _parameters) {
                IParameter param = InitializeParameter(parameter.ParameterMetadata, handler, dynamicContentLayout, canvasRoot, parameter.Value, parameter.Type, windowToHideWhenRequestingObj, actionPoint, darkMode, "", linkable);
                if (param == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameter.ParameterMetadata.Type);
                    continue;
                }
                parameters.Add(param);
                param.GetTransform().SetParent(parentObject.transform);
                param.GetTransform().localScale = new Vector3(1, 1, 1);
            }
            return parameters;
        }

        public static async Task<List<IParameter>> InitActionParameters(string actionProviderId, List<ParameterMetadata> parameter_metadatas, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot, ActionPoint actionPoint, bool darkMode, CanvasGroup windowToHideWhenRequestingObj) {
            List<Tuple<DropdownParameter, ParameterMetadata>> dynamicDropdowns = new List<Tuple<DropdownParameter, ParameterMetadata>>();
            List<IParameter> actionParameters = new List<IParameter>();
            foreach (ParameterMetadata parameterMetadata in parameter_metadatas) {
                string value = null;
                switch (parameterMetadata.Type) {
                    case ParameterMetadata.POSE:
                        try {
                            value = actionPoint.GetFirstOrientation().Id;
                        } catch (ItemNotFoundException) {
                            // there is no orientation on this action point
                            try {
                                value = actionPoint.GetFirstOrientationFromDescendants().Id;
                            } catch (ItemNotFoundException) {
                                try {
                                    value = ProjectManager.Instance.GetAnyNamedOrientation().Id;
                                } catch (ItemNotFoundException) {                                
                                       
                                }
                            }                         
                        }
                        break;
                    case ParameterMetadata.POSITION:
                        value = actionPoint.GetId();                        
                        break;
                    case ParameterMetadata.JOINTS:
                        try {
                            value = actionPoint.GetFirstJoints().Id;
                        } catch (ItemNotFoundException) {
                            // there are no valid joints on this action point
                            try {
                                value = actionPoint.GetFirstJointsFromDescendants().Id;
                            } catch (ItemNotFoundException) {
                                try {
                                    value = ProjectManager.Instance.GetAnyJoints().Id;
                                } catch (ItemNotFoundException) {
                                    // there are no valid joints in the scene
                                }
                            }
                            
                        }
                        break;
                }
                if (value != null) {
                    value = JsonConvert.SerializeObject(value);
                }
                IParameter param = InitializeParameter(parameterMetadata, handler, dynamicContentLayout, canvasRoot, value, parameterMetadata.Type, windowToHideWhenRequestingObj, actionPoint, darkMode, actionProviderId);
                if (param == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameterMetadata.Type);
                    continue;
                }
                actionParameters.Add(param);
                if (param == null)
                    continue;
                if (parameterMetadata.DynamicValue) {
                    dynamicDropdowns.Add(new Tuple<DropdownParameter, ParameterMetadata>(param.GetTransform().GetComponent<DropdownParameter>(), parameterMetadata));
                }
                param.GetTransform().SetParent(parentObject.transform);
                param.GetTransform().localScale = new Vector3(1, 1, 1);
            }
            GetDropdownParameterDelegate handlerGetDropdownParameter = GetDropdownParameter;

            int parentCount = 0;

            while (dynamicDropdowns.Count > 0) {
                for (int i = dynamicDropdowns.Count - 1; i >= 0; i--) {
                    Tuple<DropdownParameter, ParameterMetadata> tuple = dynamicDropdowns[i];
                    if ((tuple.Item2.DynamicValueParents == null && parentCount == 0) || tuple.Item2.DynamicValueParents.Count == parentCount) {
                        try {
                            await LoadDropdownValues(actionProviderId, null, tuple.Item1, tuple.Item2, handler, parentObject,
                                async () => await LoadDropdownValues(actionProviderId, null, tuple.Item1, tuple.Item2, handler, parentObject));
                        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) {
                            Debug.LogError(ex);
                        } finally {
                            dynamicDropdowns.RemoveAt(i);
                        }
                    }
                }
                parentCount += 1;
            }
            return actionParameters;
        }

        public static async Task<List<IParameter>> InitActionParameters(string actionProviderId, List<Parameter> parameters, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot, bool darkMode, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint) {
            List<Tuple<DropdownParameter, Parameter>> dynamicDropdowns = new List<Tuple<DropdownParameter, Parameter>>();
            List<IParameter> actionParameters = new List<IParameter>();
            foreach (Parameter parameter in parameters) {
                IParameter param = InitializeParameter(parameter.ParameterMetadata, handler, dynamicContentLayout, canvasRoot, parameter.Value, parameter.Type, windowToHideWhenRequestingObj, actionPoint, darkMode, actionProviderId);

                if (param == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameter.ParameterMetadata.Name);
                    continue;
                }
                actionParameters.Add(param);
                if (param == null)
                    continue;
                if (parameter.ParameterMetadata.DynamicValue) {
                    dynamicDropdowns.Add(new Tuple<DropdownParameter, Parameter>(param.GetTransform().GetComponent<DropdownParameter>(), parameter));
                }
                param.GetTransform().SetParent(parentObject.transform);
                param.GetTransform().localScale = new Vector3(1, 1, 1);
            }
            GetDropdownParameterDelegate handlerGetDropdownParameter = GetDropdownParameter;

            int parentCount = 0;

            while (dynamicDropdowns.Count > 0) {
                for (int i = dynamicDropdowns.Count - 1; i >= 0; i--) {
                    Tuple<DropdownParameter, Parameter> tuple = dynamicDropdowns[i];
                    if ((tuple.Item2.ParameterMetadata.DynamicValueParents == null && parentCount == 0) || tuple.Item2.ParameterMetadata.DynamicValueParents.Count == parentCount) {
                        try {

                            await LoadDropdownValues(actionProviderId, tuple.Item2.GetValue<string>(), tuple.Item1, tuple.Item2.ParameterMetadata, handler, parentObject,
                                async () => await LoadDropdownValues(actionProviderId, tuple.Item2.GetValue<string>(), tuple.Item1, tuple.Item2.ParameterMetadata, handler, parentObject));
                        } catch (Exception ex) when (ex is Base.ItemNotFoundException || ex is Base.RequestFailedException) {
                            Debug.LogError(ex);
                        } finally {
                            dynamicDropdowns.RemoveAt(i);
                        }
                    }
                }
                parentCount += 1;
            }
            return actionParameters;
        }

        public static DropdownParameter GetDropdownParameter(string param_id, GameObject parentObject) {
            foreach (DropdownParameter dropdownParameter in parentObject.GetComponentsInChildren<DropdownParameter>()) {
                if (dropdownParameter.Label.text == param_id)
                    return dropdownParameter;
            }
            throw new Base.ItemNotFoundException("Parameter not found: " + param_id);
        }

        public static IParameter InitializeParameter(ParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup layoutGroupToBeDisabled,
            GameObject canvasRoot, string value, string type, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint = null, bool darkMode = false, string actionProviderId = "", bool linkable = true) {
            IParameter parameter = null;


            switch (actionParameterMetadata.Type) {
                case ParameterMetadata.STR:
                    parameter = InitializeStringParameter(actionParameterMetadata, handler, layoutGroupToBeDisabled, canvasRoot, value, type, linkable);
                    break;
                case ParameterMetadata.POSE:
                    parameter = InitializePoseParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable, windowToHideWhenRequestingObj, actionPoint);
                    break;
                case ParameterMetadata.POSITION:
                    parameter = InitializePositionParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable, windowToHideWhenRequestingObj, actionPoint);
                    break;
                case ParameterMetadata.JOINTS:
                    parameter = InitializeJointsParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, Parameter.GetValue<string>(value), actionProviderId);
                    break;
                case ParameterMetadata.STR_ENUM:
                    parameter = InitializeStringEnumParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable);
                    break;
                case ParameterMetadata.INT_ENUM:
                    parameter = InitializeIntegerEnumParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable);
                    break;
                case ParameterMetadata.INT:
                    parameter = InitializeIntegerParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable);
                    break;
                case ParameterMetadata.DOUBLE:
                    parameter = InitializeDoubleParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable);
                    break;
                case ParameterMetadata.BOOL:
                    parameter = InitializeBooleanParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, value, type, linkable);
                    break;

            }
            if (parameter == null) {
                return null;
            } else {
                parameter.SetLabel(actionParameterMetadata.Name, actionParameterMetadata.Description);
                parameter.SetDarkMode(darkMode);
                return parameter;
            }

        }

        public static bool CheckIfAllValuesValid(List<IParameter> actionParameters) {
            foreach (IParameter actionParameter in actionParameters) {
                if (actionParameter.GetValue() == null) {
                    Base.Notifications.Instance.ShowNotification("Invalid parameter value", actionParameter.GetName() + " parameter value is not valid");
                    return false;
                }
            }
            return true;
        }



        private static void ValidateIntegerParameter(LabeledInput input, ParameterMetadata actionMetadata, int newValue) {
            if (actionMetadata.ParameterExtra == null)
                return;
            ARServer.Models.IntParameterExtra intParameterExtra = (ARServer.Models.IntParameterExtra) actionMetadata.ParameterExtra;
            bool valid = true;
            if (newValue < intParameterExtra.Minimum) {
                input.Input.text = intParameterExtra.Minimum.ToString();
                valid = false;
            } else if (newValue > intParameterExtra.Maximum) {
                input.Input.text = intParameterExtra.Maximum.ToString();
                valid = false;
            }
            if (!valid) {
                Notifications.Instance.ShowNotification("Not valid value", "Parameter " + actionMetadata.Name +
                    " has to be between " + intParameterExtra.Minimum.ToString() + " and " + intParameterExtra.Maximum);
            }
        }

        private static void ValidateDoubleParameter(LabeledInput input, ParameterMetadata actionMetadata, double newValue) {
            if (actionMetadata.ParameterExtra == null)
                return;
            ARServer.Models.DoubleParameterExtra doubleParameterExtra = (ARServer.Models.DoubleParameterExtra) actionMetadata.ParameterExtra;
            bool valid = true;
            if (newValue < doubleParameterExtra.Minimum) {
                input.Input.text = doubleParameterExtra.Minimum.ToString();
                valid = false;
            } else if (newValue > doubleParameterExtra.Maximum) {
                input.Input.text = doubleParameterExtra.Maximum.ToString();
                valid = false;
            }
            if (!valid) {
                Notifications.Instance.ShowNotification("Not valid value", "Parameter " + actionMetadata.Name +
                    " has to be between " + doubleParameterExtra.Minimum.ToString() + " and " + doubleParameterExtra.Maximum);
            }
        }

    }

    

}
