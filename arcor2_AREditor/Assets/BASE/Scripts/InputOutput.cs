using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using IO.Swagger.Model;

namespace Base {
    [RequireComponent(typeof(OutlineOnClick))]
    public class InputOutput : Clickable {
        public Action Action;
        private string logicItemId;
        [SerializeField]
        private OutlineOnClick outlineOnClick;


        public void Init(string logicItemId) {
            this.logicItemId = logicItemId;
        }

        public LogicItem GetLogicItem() {
            Debug.Assert(logicItemId != null);
            if (ProjectManager.Instance.LogicItems.TryGetValue(logicItemId, out LogicItem logicItem)) {
                return logicItem;
            } else {
                throw new ItemNotFoundException("Logic item with ID " + logicItemId + " does not exists");
            }
        }

        public override async void OnClick(Click type) {
            if (!ControlBoxManager.Instance.ConnectionsToggle.isOn) {
                Notifications.Instance.ShowNotification("Cannot manipulate connections", "When connections are disabled, they cannot be manipulated");
                return;
            }
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
                //return;
            }
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                Notifications.Instance.ShowNotification("Not allowed", "Editation of connections only allowed in project editor");
                return;
            }
            if (type == Click.MOUSE_LEFT_BUTTON || type == Click.TOUCH) {
                if (ConnectionManagerArcoro.Instance.IsConnecting()) {
                    if (string.IsNullOrEmpty(logicItemId)) {
                        GameManager.Instance.ObjectSelected(this);
                    }
                } else {
                    if (string.IsNullOrEmpty(logicItemId)) {
                        ConnectionManagerArcoro.Instance.CreateConnectionToPointer(gameObject);
                        if (typeof(PuckOutput) == GetType()) {
                            GetInput();
                        } else {
                            GetOutput();
                        }
                    } else {
                        GameObject theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedTo(GetLogicItem().GetConnection(), gameObject);
                        
                        try {
                            await WebsocketManager.Instance.RemoveLogicItem(logicItemId);
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
                        }
                    }
                }
            }
        }


        public async void GetInput() {
            List<Action> actionList = ProjectManager.Instance.GetAllActions();
            actionList.Add(ProjectManager.Instance.StartAction);
            actionList.Add(ProjectManager.Instance.EndAction);
            /*foreach (Action a in actionList) {
                if (!await ConnectionManagerArcoro.Instance.ValidateConnection(this, a.Input)) {
                    a.Input.Disable();
                }
            }*/
            GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionInput, GetInput, "Select input of other action", ValidateInput);
        }

        public async void GetOutput() {
            List<Action> actionList = ProjectManager.Instance.GetAllActions();
            actionList.Add(ProjectManager.Instance.StartAction);
            actionList.Add(ProjectManager.Instance.EndAction);
            /*foreach (Action a in actionList) {
                if (!await ConnectionManagerArcoro.Instance.ValidateConnection(a.Output, this)) {
                    a.Output.Disable();
                }
            }*/
            GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionOutput, GetOutput, "Select output of other action", ValidateOutput);
        }

        private async Task<RequestResult> ValidateInput(object selectedInput) {
            PuckInput input;
            try {
                input = (PuckInput) selectedInput;
            } catch (InvalidCastException) {
                return new RequestResult(false, "Wrong object type selected");
            } 
            
            RequestResult result = new RequestResult(true, "");
            if (!await ConnectionManagerArcoro.Instance.ValidateConnection(this, input)) {
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
            if (!await ConnectionManagerArcoro.Instance.ValidateConnection(output, this)) {
                result.Success = false;
                result.Message = "Invalid connection";
            }
            return result;
        }

        private async void GetInput(object selectedInput) {
            PuckInput input = (PuckInput) selectedInput;
            if (selectedInput == null || input == null) {
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                return;
            }
            try {
                await WebsocketManager.Instance.AddLogicItem(Action.Data.Id, input.Action.Data.Id, false);
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
                await WebsocketManager.Instance.AddLogicItem(output.Action.Data.Id, Action.Data.Id, false);
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();                
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to add connection");
            }
        }


        public async override void OnHoverStart() {
            if (!Enabled)
                return;
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                Notifications.Instance.ShowNotification("Not allowed", "Editation of connections only allowed in project editor");
                return;
            }
            outlineOnClick.Highlight();
            Action.NameText.gameObject.SetActive(true);
            if (!ConnectionManagerArcoro.Instance.IsConnecting())
                return;
            InputOutput theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedToPointer().GetComponent<InputOutput>();
            bool result;
            if (GetType() == typeof(PuckInput)) {
                result = await ConnectionManagerArcoro.Instance.ValidateConnection(theOtherOne, this);
            } else {
                result = await ConnectionManagerArcoro.Instance.ValidateConnection(this, theOtherOne);
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

        public override void Disable() {
            base.Disable();
            foreach (Renderer renderer in outlineOnClick.Renderers)
                renderer.material.color = Color.gray;
        }
        public override void Enable() {
            base.Enable();
            foreach (Renderer renderer in outlineOnClick.Renderers) {
                if (logicItemId == "START")
                    renderer.material.color = Color.green;
                else if (logicItemId == "END")
                    renderer.material.color = Color.red;
                else
                    renderer.material.color = new Color(0.9f, 0.84f, 0.27f);
            }
                
        }
    }

}

