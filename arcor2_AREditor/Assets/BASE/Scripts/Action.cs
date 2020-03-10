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

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action("", new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionParameter>(), "", "");

        public delegate void OnChangeParameterHandlerDelegate(string parameterId, object newValue);
        public delegate DropdownParameter GetDropdownParameterDelegate(string parameterId, GameObject parentParam);

        public async Task Init(string uuid, string id, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider, bool updateProject = true) {

            ActionPoint = ap;
            this.metadata = metadata;
            this.ActionProvider = actionProvider;

            Data.Uuid = uuid;

            if (!GameManager.Instance.CurrentProject.HasLogic) {
                InputArrow.gameObject.SetActive(false);
                OutputArrow.gameObject.SetActive(false);
            }


            UpdateId(id, false);
            //UpdateUuid(Guid.NewGuid().ToString());
            UpdateType();
            foreach (InputOutput io in GetComponentsInChildren<InputOutput>()) {
                io.InitData();
            }

            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }


        }

        public virtual void ActionUpdate(IO.Swagger.Model.Action aData = null) {
            if (aData != null)
                Data = aData;
        }

        public void UpdateType() {
            Data.Type = GetActionType();
        }        

        public virtual void UpdateId(string newId, bool updateProject = true) {
            Data.Id = newId;

            // update changed IDs in connected actions
            Action actionOnInput = Scene.Instance.GetActionById(Input.Data.Default);
            if (actionOnInput != null) {
                actionOnInput.Output.Data.Default = newId;
            }
            Action actionOnOutput = Scene.Instance.GetActionById(Output.Data.Default);
            if (actionOnOutput != null) {
                actionOnOutput.Input.Data.Default = newId;
            }

            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public string GetActionType() {
            return ActionProvider.GetProviderName() + "/" + metadata.Name; //TODO: AO|Service/Id
        }

        public void DeleteAction(bool updateProject = true) {
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

            ActionPoint.Actions.Remove(Data.Uuid);

            if (updateProject)
                GameManager.Instance.UpdateProject();
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
                input = InitializeDropdownParameter(actionParameterMetadata, new List<string>(), selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler);
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

        public static GameObject InitializeDropdownParameter(ActionParameterMetadata actionParameterMetadata, List<string> data, string selectedValue, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler) {
            DropdownParameter dropdownParameter = Instantiate(ActionsManager.Instance.ParameterDropdownPrefab).GetComponent<DropdownParameter>();
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
            return InitializeDropdownParameter(actionParameterMetadata, data, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler);
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
            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValueString, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler);
        }

        public static GameObject InitializePoseParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value) {
            List<string> options = new List<string>();
            foreach (Base.ActionPoint ap in Base.Scene.Instance.GetAllActionPoints()) {
                foreach (string poseKey in ap.GetPoses().Keys) {
                    options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + poseKey);
                }
            }
            string selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<string>();
            }

            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler);
        }

        public static GameObject InitializeJointsParameter(ActionParameterMetadata actionParameterMetadata, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, string value) {
            List<string> options = new List<string>();
            foreach (Base.ActionPoint ap in Base.Scene.Instance.GetAllActionPoints()) {
                foreach (string jointsId in ap.GetJoints().Keys) {
                    options.Add(ap.ActionObject.GetComponent<Base.ActionObject>().Data.Id + "." + ap.Data.Id + "." + jointsId);
                }
            }
            string selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<string>();
            }
            return InitializeDropdownParameter(actionParameterMetadata, options, selectedValue, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler);
        }

        public static GameObject InitializeIntegerParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, int? value) {
            GameObject input = Instantiate(ActionsManager.Instance.ParameterInputPrefab);
            int? selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<int>();
            }
            input.GetComponent<LabeledInput>().SetType(actionParameterMetadata.Type);
            input.GetComponent<LabeledInput>().Input.text = selectedValue != null ? selectedValue.ToString() : "0";
            input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, int.Parse(newValue)));
            return input;
        }

        public static GameObject InitializeDoubleParameter(ActionParameterMetadata actionParameterMetadata, OnChangeParameterHandlerDelegate onChangeParameterHandler, double? value) {
            GameObject input = Instantiate(ActionsManager.Instance.ParameterInputPrefab);
            input.GetComponent<LabeledInput>().SetType(actionParameterMetadata.Type);
            double? selectedValue = null;
            if (value != null) {
                selectedValue = value;
            } else if (actionParameterMetadata.DefaultValue != null) {
                selectedValue = actionParameterMetadata.GetDefaultValue<double>();
            }
            input.GetComponent<LabeledInput>().Input.text = selectedValue != null ? selectedValue.ToString() : "0";
            input.GetComponent<LabeledInput>().Input.onEndEdit.AddListener((string newValue)
                => onChangeParameterHandler(actionParameterMetadata.Name, ParseDouble(newValue)));
            return input;
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
                () => onChangeParameterHandler(parameterId, dropdownParameter.Dropdown.selectedText.text));
            if (selectedValue == "" || selectedValue == null) {
                string value;
                if (dropdownParameter.Dropdown.dropdownItems.Count == 0)
                    value = "";
                else
                    value = dropdownParameter.Dropdown.selectedText.text;

                onChangeParameterHandler(parameterId, value);
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
                        Debug.LogError("Parrent param has no value, this should never happen!");
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

        public static async Task<List<IActionParameter>> InitParameters(string actionProviderId, List<ActionParameterMetadata> parameter_metadatas, GameObject parentObject, OnChangeParameterHandlerDelegate handler, VerticalLayoutGroup DynamicContentLayout, GameObject canvasRoot) {
            List<Tuple<DropdownParameter, ActionParameterMetadata>> dynamicDropdowns = new List<Tuple<DropdownParameter, ActionParameterMetadata>>();
            List<IActionParameter> actionParameters = new List<IActionParameter>();
            foreach (ActionParameterMetadata parameterMetadata in parameter_metadatas) {
                GameObject paramGO = InitializeParameter(parameterMetadata, handler, DynamicContentLayout, canvasRoot, null);
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


    }

}
