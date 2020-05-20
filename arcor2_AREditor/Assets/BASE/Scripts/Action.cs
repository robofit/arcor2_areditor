using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;

namespace Base {
    public abstract class Action : Clickable {
        // Metadata of this Action
        private ActionMetadata metadata;
        // Dictionary of all action parameters for this Action
        private Dictionary<string, ActionParameter> parameters = new Dictionary<string, ActionParameter>();
        
        public PuckInput Input;
        public PuckOutput Output;

        public GameObject InputArrow, OutputArrow;
        public IActionProvider ActionProvider;

        public ActionPoint ActionPoint;

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action("", new List<IO.Swagger.Model.ActionIO>(), "", new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionParameter>(), "");

        public delegate void OnChangeParameterHandlerDelegate(string parameterId, object newValue);
        public delegate DropdownParameter GetDropdownParameterDelegate(string parameterId, GameObject parentParam);

        public void Init(string id, string name, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider) {

            ActionPoint = ap;
            this.metadata = metadata;
            this.ActionProvider = actionProvider;

            Data.Id = id;

            if (!Base.ProjectManager.Instance.Project.HasLogic) {
                InputArrow.gameObject.SetActive(false);
                OutputArrow.gameObject.SetActive(false);
            }


            UpdateName(name);
            //UpdateUuid(Guid.NewGuid().ToString());
            UpdateType();
            foreach (InputOutput io in GetComponentsInChildren<InputOutput>()) {
                io.InitData();
            }



        }

        public virtual void ActionUpdateBaseData(IO.Swagger.Model.Action action) {
            Data.Name = action.Name;
        }

        public virtual void ActionUpdate(IO.Swagger.Model.Action action, bool updateConnections = false) {

            // Updates (or creates new) parameters of current action
            foreach (IO.Swagger.Model.ActionParameter projectActionParameter in action.Parameters) {
                try {
                    // If action parameter exist in action dictionary, then just update that parameter value (it's metadata will always be unchanged)
                    if (Parameters.TryGetValue(projectActionParameter.Id, out ActionParameter actionParameter)) {
                        actionParameter.UpdateActionParameter(projectActionParameter);
                    }
                    // Otherwise create a new action parameter, load metadata for it and add it to the dictionary of action
                    else {
                        // Loads metadata of specified action parameter - projectActionParameter. Action.Metadata is created when creating Action.
                        IO.Swagger.Model.ActionParameterMeta actionParameterMetadata = Metadata.GetParamMetadata(projectActionParameter.Id);

                        actionParameter = new ActionParameter(actionParameterMetadata, this, projectActionParameter.Value);
                        Parameters.Add(actionParameter.Id, actionParameter);
                    }
                } catch (ItemNotFoundException ex) {
                    Debug.LogError(ex);
                }
            }
            if (updateConnections) {
                string actionOutput = "end";
                if (action.Outputs.Count > 0) {
                    actionOutput = action.Outputs[0].Default;
                }

                if(actionOutput != Output.Data.Default) {
                    //at the moment, each action has exactly one input and one output
                    Action refAction = null;
                    if (actionOutput != "start" && actionOutput != "end") {
                        refAction = ProjectManager.Instance.GetAction(actionOutput);
                    }

                    if (Output.Connection != null) {
                        ConnectionManagerArcoro.Instance.Connections.Remove(Output.Connection);
                        Destroy(Output.Connection.gameObject);
                    }

                    // Create new connection only if connected action exists (it is not start nor end)
                    if (refAction != null) {
                        // Create new one
                        PuckInput input = refAction.Input;

                        GameObject c = Instantiate(ProjectManager.Instance.ConnectionPrefab);
                        c.transform.SetParent(ConnectionManager.instance.transform);
                        Connection newConnection = c.GetComponent<Connection>();
                        // We are always connecting output to input.
                        newConnection.target[0] = Output.gameObject.GetComponent<RectTransform>();
                        newConnection.target[1] = input.gameObject.GetComponent<RectTransform>();

                        input.Connection = newConnection;
                        Output.Connection = newConnection;
                        input.Data.Default = Data.Id;
                        Output.Data.Default = refAction.Data.Id;
                        ConnectionManagerArcoro.Instance.Connections.Add(newConnection);
                    } else {
                        refAction = ProjectManager.Instance.GetAction(Output.Data.Default);
                        refAction.Input.InitData();
                        Output.InitData();
                    }


                }
            }
            
        }

        public void UpdateType() {
            Data.Type = GetActionType();
        }        

        public virtual void UpdateName(string newName) {
            Data.Name = newName;

        }

        public string GetActionType() {
            return ActionProvider.GetProviderType() + "/" + metadata.Name; //TODO: AO|Service/Id
        }

        public void DeleteAction() {
            // Delete connection on input and set the gameobject that was connected through its output to the "end" value.
            if (Input.Connection != null) {
                InputOutput connectedActionIO = Input.Connection.target[0].GetComponent<InputOutput>();
                connectedActionIO.Data.Default = "end";
                // Remove the reference in connections manager.
                ConnectionManagerArcoro.Instance.Connections.Remove(Input.Connection);
                Destroy(Input.Connection.gameObject);
            }
            // Delete connection on output and set the gameobject that was connected through its input to the "start" value.
            if (Output.Connection != null) {
                InputOutput connectedActionIO = Output.Connection.target[1].GetComponent<InputOutput>();
                connectedActionIO.Data.Default = "start";
                // Remove the reference in connections manager.
                ConnectionManagerArcoro.Instance.Connections.Remove(Output.Connection);
                Destroy(Output.Connection.gameObject);
            }

            Destroy(gameObject);

            ActionPoint.Actions.Remove(Data.Id);

        }

        public Dictionary<string, ActionParameter> Parameters {
            get => parameters; set => parameters = value;
        }

        public ActionMetadata Metadata {
            get => metadata; set => metadata = value;
        }

        public virtual void RunAction() {

        }

        public virtual void StopAction() {

        }


        public static GameObject InitializeStringParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, string value) {
            GameObject input;
            string selectedValue = "";
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = JsonConvert.DeserializeObject<string>(actionParameterMetadata.DefaultValue);
            }
            if (actionParameterMetadata.DynamicValue) {
                input = InitializeDropdownParameter(actionParameterMetadata, new List<string>(), selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownPrefab);
                input.GetComponent<DropdownParameter>().SetLoading(true);
            } else {
                input = Instantiate(ActionsManager.Instance.ParameterInputPrefab);
                input.GetComponent<LabeledInput>().SetType(actionParameterMetadata.Type);
                input.GetComponent<LabeledInput>().SetValue(selectedValue);
                input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
                    => onChangeParameterHandler(actionParameterMetadata.Name, newValue));
            }
            return input;
        }

        public static GameObject InitializeRelativePoseParameter(Base.ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, IO.Swagger.Model.Pose value) {
            GameObject input;
            string selectedValue = JsonConvert.SerializeObject(new IO.Swagger.Model.Pose(new IO.Swagger.Model.Orientation(), new IO.Swagger.Model.Position()));
            if (value != null) {
                selectedValue = JsonConvert.SerializeObject(value);
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.DefaultValue;
            }
            input = Instantiate(ActionsManager.Instance.ParameterInputPrefab);
            input.GetComponent<LabeledInput>().SetType(actionParameterMetadata.Type);
            input.GetComponent<LabeledInput>().SetValue(selectedValue);
            input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(newValue)));

            return input;
        }
       
        public static GameObject InitializeDropdownParameter(ActionParameterMetadata actionParameterMetadata, List<string> data, string selectedValue, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, GameObject dropdownPrefab) {
            DropdownParameter dropdownParameter = Instantiate(dropdownPrefab).GetComponent<DropdownParameter>();
            dropdownParameter.Init(layoutGroupToBeDisabled, canvasRoot);
            DropdownParameterPutData(dropdownParameter, data, selectedValue, actionParameterMetadata.Name, onChangeParameterHandler);
            return dropdownParameter.gameObject;
        }

        public static GameObject InitializeStringEnumParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value) {
            string selectedValue = null;
            if (value != null) {
                selectedValue = (string) value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<string>();
            }
            List<string> data = new List<string>();
            foreach (string item in ((ARServer.Models.StringEnumParameterExtra) actionParameterMetadata.ParameterExtra).AllowedValues)
                data.Add(item);
            return InitializeDropdownParameter(actionParameterMetadata, data, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownPrefab);
        }

        public static GameObject InitializeIntegerEnumParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, int? value) {
            List<string> options = new List<string>();
            int? selectedValue = null;
            if (value != null) {
                selectedValue = value;
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
            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValueString, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownPrefab);
        }

        public static GameObject InitializePoseParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value) {
            List<string> options = new List<string>();

            foreach (Base.ActionPoint ap in Base.ProjectManager.Instance.GetAllActionPoints()) {
                foreach (IO.Swagger.Model.NamedOrientation orientation in ap.GetNamedOrientations()) {                    
                    options.Add(ap.Data.Name + "." + orientation.Name);
                }
            }
            string selectedValue = null;
            if (value != null) {
                try {
                    ActionPoint actionPoint = ProjectManager.Instance.GetActionPointWithOrientation(value);
                    IO.Swagger.Model.NamedOrientation namedOrientation = actionPoint.GetNamedOrientation(value);
                    selectedValue = actionPoint.Data.Name + "." + namedOrientation.Name;
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }
                
            } 

            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownPosesPrefab);
        }

        public static GameObject InitializeJointsParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value) {
            List<string> options = new List<string>();
            foreach (Base.ActionPoint ap in Base.ProjectManager.Instance.GetAllActionPoints()) {
                foreach (IO.Swagger.Model.ProjectRobotJoints joints in ap.GetAllJoints(false, null, true).Values) {
                    options.Add(ap.Data.Name + "." + joints.Name);
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

            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, ActionsManager.Instance.ParameterDropdownJointsPrefab);
        }

        public static GameObject InitializeIntegerParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, int? value) {
            LabeledInput input = Instantiate(ActionsManager.Instance.ParameterInputPrefab).GetComponent<LabeledInput>();
            int? selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<int>();
            }
            input.SetType(actionParameterMetadata.Type);
            input.Input.text = selectedValue != null ? selectedValue.ToString() : "0";
            input.Input.onEndEdit.AddListener((string newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, int.Parse(newValue)));
            input.Input.onEndEdit.AddListener((string newValue)
                => ValidateIntegerParameter(input, actionParameterMetadata, int.Parse(newValue)));
            return input.gameObject;
        }

        public static GameObject InitializeDoubleParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, double? value) {
            LabeledInput input = Instantiate(ActionsManager.Instance.ParameterInputPrefab).GetComponent<LabeledInput>();
            input.SetType(actionParameterMetadata.Type);
            double? selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<double>();
            }
            input.Input.text = selectedValue != null ? selectedValue.ToString() : "0";
            input.Input.onEndEdit.AddListener((string newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, ParseDouble(newValue)));
            input.Input.onEndEdit.AddListener((string newValue)
                => ValidateDoubleParameter(input, actionParameterMetadata, ParseDouble(newValue)));
            return input.gameObject;
        }

        public static double ParseDouble(string value) {
            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {

            }
            return result;
        }

        private static void DropdownParameterPutData(DropdownParameter dropdownParameter, List<string> data, string selectedValue, string parameterId, OnChangeParameterHandlerDelegate onChangeParameterHandler) {
            dropdownParameter.PutData(data, selectedValue,
                () => onChangeParameterHandler(parameterId, dropdownParameter.GetValue()));
            if (selectedValue == "" || selectedValue == null) {
                string value;
                if (dropdownParameter.Dropdown.dropdownItems.Count == 0)
                    value = "";
                else
                    value = dropdownParameter.Dropdown.selectedText.text;

                onChangeParameterHandler(parameterId, dropdownParameter.GetValue());
            }
        }

        public static async Task LoadDropdownValues(string actionProviderId, string selectedValue, DropdownParameter dropdownParameter, ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, GameObject parentObject, UnityAction callback = null) {
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
            DropdownParameterPutData(dropdownParameter, values, selectedValue, actionParameterMetadata.Name, onChangeParameterHandler);
        }

        private static void AddOnChangeToDropdownParameter(DropdownParameter dropdownParameter, UnityAction callback) {
            foreach (CustomDropdown.Item item in dropdownParameter.Dropdown.dropdownItems) {
                item.OnItemSelection.AddListener(callback);
            }
        }

        public static async Task<List<IActionParameter>> InitParameters(string actionProviderId, List<ActionParameterMetadata> parameter_metadatas, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot, ActionPoint actionPoint) {
            List<Tuple<DropdownParameter, ActionParameterMetadata>> dynamicDropdowns = new List<Tuple<DropdownParameter, ActionParameterMetadata>>();
            List<IActionParameter> actionParameters = new List<IActionParameter>();
            foreach (ActionParameterMetadata parameterMetadata in parameter_metadatas) {
                string value = null;
                switch (parameterMetadata.Type) {
                    case "pose":
                        try {
                            value = actionPoint.GetFirstOrientation().Id;
                        } catch (ItemNotFoundException ex) {
                            // there is no orientation on this action point
                        }
                        break;
                    case "joints":
                        try {
                            value = actionPoint.GetFirstJoints().Id;
                        } catch (ItemNotFoundException ex) {
                            // there are no valid joints on this action point
                        }
                        break;
                }
                if (value != null) {
                    value = JsonConvert.SerializeObject(value);
                }
                GameObject paramGO = InitializeParameter(parameterMetadata, handler, dynamicContentLayout, canvasRoot, value);
                if (paramGO == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameterMetadata.Type);
                    continue;
                }
                actionParameters.Add(paramGO.GetComponent<IActionParameter>());
                if (paramGO == null)
                    continue;
                if (parameterMetadata.DynamicValue) {
                    dynamicDropdowns.Add(new Tuple<DropdownParameter, ActionParameterMetadata>(paramGO.GetComponent<DropdownParameter>(), parameterMetadata));
                }
                paramGO.transform.SetParent(parentObject.transform);
                paramGO.transform.localScale = new Vector3(1, 1, 1);
            }
            GetDropdownParameterDelegate handlerGetDropdownParameter = GetDropdownParameter;

            int parentCount = 0;
            
            while (dynamicDropdowns.Count > 0) {
                for (int i = dynamicDropdowns.Count - 1; i >= 0; i--) {
                    Tuple<DropdownParameter, ActionParameterMetadata> tuple = dynamicDropdowns[i];
                    if ((tuple.Item2.DynamicValueParents == null && parentCount == 0) || tuple.Item2.DynamicValueParents.Count == parentCount) {
                        try {
                            await Base.Action.LoadDropdownValues(actionProviderId, null, tuple.Item1, tuple.Item2, handler, parentObject,
                                async () => await Base.Action.LoadDropdownValues(actionProviderId, null, tuple.Item1, tuple.Item2, handler, parentObject));
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

        public static async Task<List<IActionParameter>> InitParameters(string actionProviderId, List<ActionParameter> parameters, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup dynamicContentLayout, GameObject canvasRoot) {
            List<Tuple<DropdownParameter, ActionParameter>> dynamicDropdowns = new List<Tuple<DropdownParameter, ActionParameter>>();
            List<IActionParameter> actionParameters = new List<IActionParameter>();
            foreach (ActionParameter parameter in parameters) {
                GameObject paramGO = InitializeParameter(parameter.ActionParameterMetadata, handler, dynamicContentLayout, canvasRoot, parameter.Value);

                if (paramGO == null) {
                    Notifications.Instance.ShowNotification("Plugin missing", "Ignoring parameter of type: " + parameter.ActionParameterMetadata.Name);
                    continue;
                }
                actionParameters.Add(paramGO.GetComponent<IActionParameter>());
                if (paramGO == null)
                    continue;
                if (parameter.ActionParameterMetadata.DynamicValue) {
                    dynamicDropdowns.Add(new Tuple<DropdownParameter, ActionParameter>(paramGO.GetComponent<DropdownParameter>(), parameter));
                }
                paramGO.transform.SetParent(parentObject.transform);
                paramGO.transform.localScale = new Vector3(1, 1, 1);
            }
            GetDropdownParameterDelegate handlerGetDropdownParameter = GetDropdownParameter;

            int parentCount = 0;

            while (dynamicDropdowns.Count > 0) {
                for (int i = dynamicDropdowns.Count - 1; i >= 0; i--) {
                    Tuple<DropdownParameter, ActionParameter> tuple = dynamicDropdowns[i];
                    if ((tuple.Item2.ActionParameterMetadata.DynamicValueParents == null && parentCount == 0) || tuple.Item2.ActionParameterMetadata.DynamicValueParents.Count == parentCount) {
                        try {
                            
                            await Base.Action.LoadDropdownValues(actionProviderId, tuple.Item2.GetValue<string>(), tuple.Item1, tuple.Item2.ActionParameterMetadata, handler, parentObject,
                                async () => await Base.Action.LoadDropdownValues(actionProviderId, tuple.Item2.GetValue<string>(), tuple.Item1, tuple.Item2.ActionParameterMetadata, handler, parentObject));
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

        private static GameObject InitializeParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, string value) {
            GameObject parameter = null;
            
            switch (actionParameterMetadata.Type) {
                case "string":
                    parameter = Action.InitializeStringParameter(actionParameterMetadata, handler, layoutGroupToBeDisabled, canvasRoot, ActionParameter.GetValue<string>(value));
                    break;
                case "relative_pose":
                    parameter = Action.InitializeRelativePoseParameter(actionParameterMetadata, handler, ActionParameter.GetValue<IO.Swagger.Model.Pose>(value));
                    break;
                case "pose":
                    parameter = Action.InitializePoseParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, ActionParameter.GetValue<string>(value));
                    break;
                case "joints":
                    parameter = Action.InitializeJointsParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, ActionParameter.GetValue<string>(value));
                    break;
                case "string_enum":
                    parameter = Action.InitializeStringEnumParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, ActionParameter.GetValue<string>(value));
                    break;
                case "integer_enum":
                    parameter = Action.InitializeIntegerEnumParameter(actionParameterMetadata, layoutGroupToBeDisabled, canvasRoot, handler, ActionParameter.GetValue<int?>(value));
                    break;
                case "integer":
                    parameter = Action.InitializeIntegerParameter(actionParameterMetadata, handler, ActionParameter.GetValue<int?>(value));
                    break;
                case "double":
                    parameter = Action.InitializeDoubleParameter(actionParameterMetadata, handler, ActionParameter.GetValue<double?>(value));
                    break;

            }
            if (parameter == null) {
                return null;
            } else {
                parameter.GetComponent<IActionParameter>().SetLabel(actionParameterMetadata.Name, actionParameterMetadata.Description);
                return parameter;
            }

        }

        public static bool CheckIfAllValuesValid(List<IActionParameter> actionParameters) {
            foreach (IActionParameter actionParameter in actionParameters) {
                if (actionParameter.GetValue() == null) {
                    Base.Notifications.Instance.ShowNotification("Invalid parameter value", actionParameter.GetName() + " parameter value is not valid");
                    return false;
                }
            }
            return true;
        }

        public static Tuple<string, string> ParseActionType(string type) {
            if (!type.Contains("/"))
                throw new FormatException("Action type has to be in format action_provider_id/action_type");
            return new Tuple<string, string>(type.Split('/')[0], type.Split('/')[1]);
        }

        public static string BuildActionType(string actionProviderId, string actionType) {
            return actionProviderId + "/" + actionType;
        }

        private static void ValidateIntegerParameter(LabeledInput input, ActionParameterMetadata actionMetadata, int newValue) {
            Debug.LogError(actionMetadata.ParameterExtra);
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

        private static void ValidateDoubleParameter(LabeledInput input, ActionParameterMetadata actionMetadata, double newValue) {
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
