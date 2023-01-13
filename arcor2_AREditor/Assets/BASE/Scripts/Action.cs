using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using IO.Swagger.Model;
using TMPro;
using System.Linq;

namespace Base {
    public abstract class Action : InteractiveObject {
        // Metadata of this Action
        private ActionMetadata metadata;
        // Dictionary of all action parameters for this Action
        private Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
        
        public InputOutput Input;
        public PuckOutput Output;
        public IActionProvider ActionProvider;

        public ActionPoint ActionPoint;

        public IO.Swagger.Model.Action Data = null;

        public TextMeshPro NameText;


        public bool ActionBeingExecuted = false;
        
        public virtual void Init(IO.Swagger.Model.Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider) {

            ActionPoint = ap;
            this.metadata = metadata;
            ActionProvider = actionProvider;
            Data = projectAction;
            UpdateName(Data.Name);
            if (actionProvider != null)
                UpdateType();
            SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
        }

        public virtual void ActionUpdateBaseData(IO.Swagger.Model.BareAction action) {
            Data.Name = action.Name;
            SelectorItem.SetText(action.Name);
        }

        public virtual void ActionUpdate(IO.Swagger.Model.Action action, bool updateConnections = false) {

            // Updates (or creates new) parameters of current action
            foreach (IO.Swagger.Model.ActionParameter projectActionParameter in action.Parameters) {
                try {
                    // If action parameter exist in action dictionary, then just update that parameter value (it's metadata will always be unchanged)
                    if (Parameters.TryGetValue(projectActionParameter.Name, out Parameter actionParameter)) {
                        actionParameter.UpdateActionParameter(DataHelper.ActionParameterToParameter(projectActionParameter));
                    }
                    // Otherwise create a new action parameter, load metadata for it and add it to the dictionary of action
                    else {
                        // Loads metadata of specified action parameter - projectActionParameter. Action.Metadata is created when creating Action.
                        IO.Swagger.Model.ParameterMeta actionParameterMetadata = Metadata.GetParamMetadata(projectActionParameter.Name);

                        actionParameter = new Parameter(actionParameterMetadata, projectActionParameter.Type, projectActionParameter.Value);
                        Parameters.Add(actionParameter.Name, actionParameter);
                    }
                } catch (ItemNotFoundException ex) {
                    Debug.LogError(ex);
                }
            }
            
        }

        public void UpdateType() {
            Data.Type = GetActionType();
        }        

        public virtual void UpdateName(string newName) {
            Data.Name = newName;
            name = newName;
        }

        public string GetActionType() {
            return ActionProvider.GetProviderType() + "/" + metadata.Name; //TODO: AO|Service/Id
        }

        public void DeleteAction() {
            Destroy(gameObject);
            DestroyObject();
            ActionPoint.Actions.Remove(Data.Id);
        }

        public Dictionary<string, Parameter> Parameters {
            get => parameters; set => parameters = value;
        }

        public ActionMetadata Metadata {
            get => metadata; set => metadata = value;
        }

        public virtual void RunAction() {

        }

        public virtual void StopAction() {

        }

        public static Tuple<string, string> ParseActionType(string type) {
            if (!type.Contains("/"))
                throw new FormatException("Action type has to be in format action_provider_id/action_type");
            return new Tuple<string, string>(type.Split('/')[0], type.Split('/')[1]);
        }

        public static string BuildActionType(string actionProviderId, string actionType) {
            return actionProviderId + "/" + actionType;
        }

        public List<Flow> GetFlows() {
            return Data.Flows;
        }

        public override string GetId() {
            return Data.Id;
        }

        public async override Task<RequestResult> Movable() {
            return new RequestResult(false, "Actions could not be moved");
        }

        public override void DestroyObject() {
            base.DestroyObject();
        }

        public async void AddConnection() {
            if (!Output.AnyConnection()) {
                if (GetId() != "START" && Metadata.Returns.Count > 0 && Metadata.Returns[0] == "boolean") {
                    ShowOutputTypeDialog(async () => await CreateNewConnection());
                } else {
                    await CreateNewConnection();
                }
            } else {
                // if there is "Any" connection, no new could be created and this one should be selected
                // if there are both "true" and "false" connections, no new could be created

                bool showNewConnectionButton = true;
                bool conditionValue = false;

                int howManyConditions = 0;

                // kterej connection chci, případně chci vytvořit novej
                Dictionary<string, LogicItem> items = new Dictionary<string, LogicItem>();
                foreach (LogicItem logicItem in Output.GetLogicItems()) {
                    Action start = ProjectManager.Instance.GetAction(logicItem.Data.Start);
                    Action end = ProjectManager.Instance.GetAction(logicItem.Data.End);
                    string label = start.Data.Name + " -> " + end.Data.Name;
                    if (!(logicItem.Data.Condition is null)) {
                        label += " (" + logicItem.Data.Condition.Value + ")";
                        ++howManyConditions;
                        conditionValue = Parameter.GetValue<bool>(logicItem.Data.Condition.Value);
                    }
                    items.Add(label, logicItem);                   

                }
                
                if (howManyConditions == 2) {// both true and false are filled
                    showNewConnectionButton = false;
                } else if (items.Count == 1 && howManyConditions == 0) { // the "any" connection already exists
                    await SelectedConnection(items.Values.First());
                    return;
                }
                
                AREditorResources.Instance.ConnectionSelectorDialog.Open(items, showNewConnectionButton, this, () => WriteUnlock());
            }
        }

        private void ShowOutputTypeDialog(UnityAction callback) {
            if (Output.ConnectionCount() == 2) {
                Notifications.Instance.ShowNotification("Failed", "Cannot create any other connection.");
                return;
            } else if (Output.ConnectionCount() == 1) {
                List<LogicItem> items = Output.GetLogicItems();
                Debug.Assert(items.Count == 1, "There must be exactly one valid logic item!");
                LogicItem item = items[0];
                if (item.Data.Condition is null) {
                    Notifications.Instance.ShowNotification("Failed", "There is already connection which serves all results");
                    return;
                } else {
                    bool condition = JsonConvert.DeserializeObject<bool>(item.Data.Condition.Value);
                    AREditorResources.Instance.OutputTypeDialog.Open(Output, callback, false, !condition, condition);
                    return;
                }
            }
            AREditorResources.Instance.OutputTypeDialog.Open(Output, callback, true, true, true);
        }

        private async Task CreateNewConnection() {
            ConnectionManagerArcoro.Instance.CreateConnectionToPointer(Output.gameObject);
            
            await GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingAction, GetOtherAction,
                "Select input of other action", ValidateInput, async () => await WriteUnlock());
        }

        protected async virtual void GetOtherAction(object otherAction) {
            Base.Action input = (Base.Action) otherAction;

            if (otherAction == null || input == null) {
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                return;
            }
            try {
                await WebsocketManager.Instance.AddLogicItem(GetId(), input.GetId(), GetProjectLogicIf(), false);
                Output.ifValue = null;
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to add connection");
            }

        }

        private async Task<RequestResult> ValidateInput(object selectedInput) {
            if (selectedInput is Base.Action action) {
                RequestResult result = new RequestResult(true, "");
                if (!await ConnectionManagerArcoro.Instance.ValidateConnection(Output, action.Input, GetProjectLogicIf())) {
                    result.Success = false;
                    result.Message = "Invalid connection";
                }
                return result;
            } else {
                return new RequestResult(false, "Wrong object type selected");
            }
            
        }

        private IO.Swagger.Model.ProjectLogicIf GetProjectLogicIf() {
            if (Output.ifValue is null)
                return null;
            List<Flow> flows = GetFlows();
            string flowName = flows[0].Type.GetValueOrDefault().ToString().ToLower();
            IO.Swagger.Model.ProjectLogicIf projectLogicIf = new ProjectLogicIf(JsonConvert.SerializeObject(Output.ifValue), $"{GetId()}/{flowName}/0");
            return projectLogicIf;
        }

        public async Task SelectedConnection(LogicItem logicItem) {
            AREditorResources.Instance.ConnectionSelectorDialog.Close();
            if (logicItem == null) {
                if (Metadata.Returns.Count > 0 && Metadata.Returns[0] == "boolean") {
                    ShowOutputTypeDialog(async () => await CreateNewConnection());
                } else {
                    await CreateNewConnection();
                }
            } else {
                try {
                    Action otherAction;
                    if (GetId() == logicItem.Data.Start)
                        otherAction = ProjectManager.Instance.GetAction(logicItem.Data.End);
                    else
                        otherAction = ProjectManager.Instance.GetAction(logicItem.Data.Start);
                    GameManager.Instance.ShowLoadingScreen("Removing old connection...");
                    await WebsocketManager.Instance.RemoveLogicItem(logicItem.Data.Id);
                    GameManager.Instance.HideLoadingScreen();
                    if (!await otherAction.WriteLock(false)) {
                        return;
                    }
                    AddConnection();
                    
                } catch (RequestFailedException ex) {
                    GameManager.Instance.HideLoadingScreen();
                    Notifications.Instance.ShowNotification("Failed to remove connection", ex.Message);
                    
                }
            }
        }

        public void UpdateRotation() {
            if (Output.AnyConnection()) {
                LogicItem c = Output.GetLogicItems()[0];
                UpdateRotation(c.Input.Action);
            } else {
                UpdateRotation(null);
            }                
        }

        public void UpdateRotation(Base.Action otherAction) {
            if (otherAction != null && (otherAction.transform.position - transform.position).magnitude > 0.0001) {
                transform.rotation = Quaternion.LookRotation(otherAction.transform.position - transform.position);
                transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
            } else {
                transform.rotation = GameManager.Instance.Scene.transform.rotation;
                transform.Rotate(-90 * GameManager.Instance.Scene.transform.up);
            }
        }

    }

    

}
