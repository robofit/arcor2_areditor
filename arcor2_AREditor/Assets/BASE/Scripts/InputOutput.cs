using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

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
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
                //return;
            }
            if (!ConnectionManagerArcoro.Instance.ConnectionsActive) {
                return;
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


        public void GetInput() {
            Action<object> action = GetInput;
            Func<object, Task<bool>> validateAction = ValidateInput;
            GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionInput, action, "Select input of other action", validateAction);
        }

        public void GetOutput() {
            Action<object> action = GetOutput;
            Func<object, Task<bool>> validateAction = ValidateOutput;
            GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionOutput, action, "Select output of other action", validateAction);
        }

        private async Task<bool> ValidateInput(object selectedInput) {
            PuckInput input = (PuckInput) selectedInput;
            return await ConnectionManagerArcoro.Instance.ValidateConnection(this, input);
        }

        private async Task<bool> ValidateOutput(object selectedOutput) {
            PuckOutput output = (PuckOutput) selectedOutput;
            return await ConnectionManagerArcoro.Instance.ValidateConnection(output, this);
        }

        private async void GetInput(object selectedInput) {
            PuckInput input = (PuckInput) selectedInput;
            if (input == null)
                return;
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
            if (output == null)
                return;
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
            /*if (!ConnectionManagerArcoro.Instance.ConnectionsActive) {
                return;
            }*/
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                Notifications.Instance.ShowNotification("Not allowed", "Editation of connections only allowed in project editor");
                return;
            }

            outlineOnClick.Highlight();
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
            base.Disable();
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

