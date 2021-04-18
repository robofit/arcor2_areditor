using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using IO.Swagger.Model;
using Newtonsoft.Json;
using static Base.Clickable;
using UnityEngine.Events;
using RosSharp.RosBridgeClient.MessageTypes.Nav;

namespace Base {
    [RequireComponent(typeof(OutlineOnClick))]
    public abstract class InputOutput : InteractiveObject {
        public Action Action;
        private List<string> logicItemIds = new List<string>();
        [SerializeField]
        private OutlineOnClick outlineOnClick;

        public object ifValue;


        public void AddLogicItem(string logicItemId) {
            Debug.Assert(logicItemId != null);
            logicItemIds.Add(logicItemId);
        }

        public void RemoveLogicItem(string logicItemId) {
            Debug.Assert(logicItemIds.Contains(logicItemId));
            logicItemIds.Remove(logicItemId);
        }

        public List<LogicItem> GetLogicItems() {
            Debug.Assert(logicItemIds.Count > 0);
            List<LogicItem> items = new List<LogicItem>();
            foreach (string itemId in logicItemIds)
                if (ProjectManager.Instance.LogicItems.TryGetValue(itemId, out LogicItem logicItem)) {
                    items.Add(logicItem);
                } else {
                    throw new ItemNotFoundException("Logic item with ID " + itemId + " does not exists");
                }
            return items;
        }

        protected bool CheckClickType(Click type) {
           
            if (!ControlBoxManager.Instance.ConnectionsToggle.isOn) {
                return false;
            }
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                return false;
            }
            if (type != Click.MOUSE_LEFT_BUTTON && type != Click.TOUCH) {
                return false;
            }
            return true;
        }

        public override async void OnClick(Click type) {
            if (!CheckClickType(type))
                return;
            
            if (ConnectionManagerArcoro.Instance.IsConnecting()) {
                if (typeof(PuckOutput) == GetType() && Action.Data.Id != "START" && Action.Metadata.Returns.Count > 0 && Action.Metadata.Returns[0] == "boolean") {
                    ShowOutputTypeDialog(() => GameManager.Instance.ObjectSelected(this));
                } else {
                    GameManager.Instance.ObjectSelected(this);
                }
                
            } else {
                if (logicItemIds.Count == 0) {
                    if (typeof(PuckOutput) == GetType() && Action.Data.Id != "START" && Action.Metadata.Returns.Count > 0 && Action.Metadata.Returns[0] == "boolean") {
                        ShowOutputTypeDialog(async () => await CreateNewConnection());
                    } else {
                        await CreateNewConnection();
                    }
                } else {
                    // For output:
                    // if there is "Any" connection, no new could be created and this one should be selected
                    // if there are both "true" and "false" connections, no new could be created

                    // For input:
                    // every time show new connection button
                    bool showNewConnectionButton = true;
                    bool conditionValue = false;

                    int howManyConditions = 0;

                    // kterej connection chci, případně chci vytvořit novej
                    Dictionary<string, LogicItem> items = new Dictionary<string, LogicItem>();
                    foreach (string itemId in logicItemIds) {
                        if (ProjectManager.Instance.LogicItems.TryGetValue(itemId, out LogicItem logicItem)) {
                            Action start = ProjectManager.Instance.GetAction(logicItem.Data.Start);
                            Action end = ProjectManager.Instance.GetAction(logicItem.Data.End);
                            string label = start.Data.Name + " -> " + end.Data.Name;
                            if (!(logicItem.Data.Condition is null)) {
                                label += " (" + logicItem.Data.Condition.Value + ")";
                                ++howManyConditions;
                                conditionValue = Parameter.GetValue<bool>(logicItem.Data.Condition.Value);
                            }
                            items.Add(label, logicItem);
                        } else {
                            throw new ItemNotFoundException("Logic item with ID " + itemId + " does not exists");
                        }
                        
                    }
                    if (GetType() == typeof(PuckOutput)) {
                        if (howManyConditions == 2) {// both true and false are filled
                            showNewConnectionButton = false;
                        }
                        else if(items.Count == 1 && howManyConditions == 0) { // the "any" connection already exists
                            await SelectedConnection(items.Values.First());
                            return;
                        }
                    }
                    MenuManager.Instance.ConnectionSelectorDialog.Open(items, showNewConnectionButton, this);

                    /*GameObject theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedTo(GetLogicItems().GetConnection(), gameObject);
                        
                    try {
                        await WebsocketManager.Instance.RemoveLogicItem(logicItemIds);
                        ConnectionManagerArcoro.Instance.CreateConnectionToPointer(theOtherOne);
                        if (typeof(PuckOutput) == GetType()) {
                            theOtherOne.GetComponent<PuckInput>().GetOutput();
                        } else {
                            theOtherOne.GetComponent<PuckOutput>().GetInput();
                        }
                    } catch (RequestFailedException ex) {
                        Debug.LogError(ex);
                        Notifications.Instance.SaveLogs("Failed to remove connection");
                        ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                    }*/
                }
            }
            
        }


        public async Task SelectedConnection(LogicItem logicItem) {
            MenuManager.Instance.ConnectionSelectorDialog.Close();
            if (logicItem == null) {
                if (typeof(PuckOutput) == GetType() && Action.Metadata.Returns.Count > 0 && Action.Metadata.Returns[0] == "boolean") {
                    ShowOutputTypeDialog(async () => await CreateNewConnection());
                } else {
                    await CreateNewConnection();
                }
            } else {
            GameObject theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedTo(logicItem.GetConnection(), gameObject);

            try {
                await WebsocketManager.Instance.RemoveLogicItem(logicItem.Data.Id);
                ConnectionManagerArcoro.Instance.CreateConnectionToPointer(theOtherOne);
                if (typeof(PuckOutput) == GetType()) {
                    await theOtherOne.GetComponent<PuckInput>().GetOutput();
                } else {
                        await theOtherOne.GetComponent<PuckOutput>().GetInput();
                }
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to remove connection");
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
            }
            }
        }


        private void ShowOutputTypeDialog(UnityAction callback) {
            if (logicItemIds.Count == 2) {
                Notifications.Instance.ShowNotification("Failed", "Cannot create any other connection.");
                return;
            } else if (logicItemIds.Count == 1) {
                List<LogicItem> items = GetLogicItems();
                Debug.Assert(items.Count == 1, "There must be exactly one valid logic item!");
                LogicItem item = items[0];
                if (item.Data.Condition is null) {
                    Notifications.Instance.ShowNotification("Failed", "There is already connection which serves all results");
                    return;
                } else {
                    bool condition = JsonConvert.DeserializeObject<bool>(item.Data.Condition.Value);
                    MenuManager.Instance.OutputTypeDialog.Open(this, callback, false, !condition, condition);                
                    return;
                }
            }
            MenuManager.Instance.OutputTypeDialog.Open(this, callback, true, true, true);
        }

        private async Task CreateNewConnection() {
            ConnectionManagerArcoro.Instance.CreateConnectionToPointer(gameObject);
            if (typeof(PuckOutput) == GetType()) {
                await GetInput();
            } else {
                await GetOutput();
            }
        }


        public async Task GetInput() {
            List<Action> actionList = ProjectManager.Instance.GetAllActions();
            actionList.Add(ProjectManager.Instance.StartAction);
            actionList.Add(ProjectManager.Instance.EndAction);
            /*foreach (Action a in actionList) {
                if (!await ConnectionManagerArcoro.Instance.ValidateConnection(this, a.Input)) {
                    a.Input.Disable();
                }
            }*/
            await GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionInput, GetInput,
                "Select input of other action", ValidateInput, async() => {
                    try {
                        await WebsocketManager.Instance.WriteUnlock(Action.GetId());
                    } catch (RequestFailedException ex) {
                        Notifications.Instance.ShowNotification("Failed to unlock action", ex.Message);
                    }
                });
        }

        public async Task GetOutput() {
            List<Action> actionList = ProjectManager.Instance.GetAllActions();
            actionList.Add(ProjectManager.Instance.StartAction);
            actionList.Add(ProjectManager.Instance.EndAction);
            /*foreach (Action a in actionList) {
                if (!await ConnectionManagerArcoro.Instance.ValidateConnection(a.Output, this)) {
                    a.Output.Disable();
                }
            }*/
            await GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionOutput, GetOutput, "Select output of other action", ValidateOutput);
        }

        private async Task<RequestResult> ValidateInput(object selectedInput) {
            InputOutput input;
            try {
                input = (InputOutput) selectedInput;
            } catch (InvalidCastException) {
                return new RequestResult(false, "Wrong object type selected");
            } 
            
            RequestResult result = new RequestResult(true, "");
            if (!await ConnectionManagerArcoro.Instance.ValidateConnection(this, input, GetProjectLogicIf())) {
                result.Success = false;
                result.Message = "Invalid connection";
            }
            return result;
        }

        private async Task<RequestResult> ValidateOutput(object selectedOutput) {
            PuckOutput output;
            try {
                output = (PuckOutput) selectedOutput;
            } catch (InvalidCastException) {
                return new RequestResult(false, "Wrong object type selected");
            }
            RequestResult result = new RequestResult(true, "");
            if (!await ConnectionManagerArcoro.Instance.ValidateConnection(output, this, output.GetProjectLogicIf())) {
                result.Success = false;
                result.Message = "Invalid connection";
            }
            return result;
        }

        protected async virtual void GetInput(object selectedInput) {
            InputOutput input = (InputOutput) selectedInput;
            
            if (selectedInput == null || input == null) {
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                return;
            }
            try {
                await WebsocketManager.Instance.AddLogicItem(Action.Data.Id, input.Action.Data.Id, GetProjectLogicIf(), false);
                ifValue = null;
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to add connection");
            }

        }

        private async void GetOutput(object selectedOutput) {
            PuckOutput output = (PuckOutput) selectedOutput;
            
            if (selectedOutput == null || output == null) {
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                return;
            }
            try {
                await WebsocketManager.Instance.AddLogicItem(output.Action.Data.Id, Action.Data.Id, output.GetProjectLogicIf(), false);
                ifValue = null;
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();                
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to add connection");
            }
        }

        private IO.Swagger.Model.ProjectLogicIf GetProjectLogicIf() {
            if (ifValue is null)
                return null;
            List<Flow> flows = Action.GetFlows();
            string flowName = flows[0].Type.GetValueOrDefault().ToString().ToLower();
            IO.Swagger.Model.ProjectLogicIf projectLogicIf = new ProjectLogicIf(JsonConvert.SerializeObject(ifValue), Action.Data.Id + "/" + flowName + "/0");
            return projectLogicIf;
        }

        public async override void OnHoverStart() {
            if (!Enabled)
                return;
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                return;
            }
            outlineOnClick.Highlight();
            Action.NameText.gameObject.SetActive(true);
            if (!ConnectionManagerArcoro.Instance.IsConnecting())
                return;
            InputOutput theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedToPointer().GetComponent<InputOutput>();
            bool result;
            if (GetType() == typeof(PuckInput)) {
                result = await ConnectionManagerArcoro.Instance.ValidateConnection(theOtherOne, this, theOtherOne.GetProjectLogicIf());
            } else {
                result = await ConnectionManagerArcoro.Instance.ValidateConnection(this, theOtherOne, null);
            }
            if (!result)
                ConnectionManagerArcoro.Instance.DisableConnectionToMouse();
                
        }

        public override void OnHoverEnd() {
            outlineOnClick.UnHighlight();
            Action.NameText.gameObject.SetActive(false);
            if (!ConnectionManagerArcoro.Instance.IsConnecting())
                return;
            ConnectionManagerArcoro.Instance.EnableConnectionToMouse();
        }

        public override void Enable(bool enable) {
            base.Enable(enable);
            if (enable)
                foreach (Renderer renderer in outlineOnClick.Renderers) {
                    if (Action.Data.Id == "START")
                        renderer.material.color = Color.green;
                    else if (Action.Data.Id == "END")
                        renderer.material.color = Color.red;
                    else
                        renderer.material.color = new Color(0.9f, 0.84f, 0.27f);
                }
            else {
                foreach (Renderer renderer in outlineOnClick.Renderers)
                    renderer.material.color = Color.gray;
            }
                
        }

        public override string GetName() {
            if (typeof(PuckOutput) == GetType()) {
                return "Output of " + Action.Data.Name;
            } else {
                return "Input of " + Action.Data.Name;
            }
        }

        public override string GetId() {
            return GetName();
        }

        public override void OpenMenu() {
            throw new NotImplementedException();
        }

        public override bool HasMenu() {
            return false;
        }

        public async override Task<RequestResult> Movable() {
            return new RequestResult(false, "Input / output could not be moved");
        }

        public override void StartManipulation() {
            throw new NotImplementedException();
        }

        public async override Task<RequestResult> Removable() {
            return new RequestResult(false, "Input / output could not be removed");
        }

        public override void Remove() {
            throw new NotImplementedException();
        }

        public override void Rename(string name) {
            throw new NotImplementedException();
        }
    }

}

