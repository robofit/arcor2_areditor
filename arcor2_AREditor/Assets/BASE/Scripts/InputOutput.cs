using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Base {
    [RequireComponent(typeof(OutlineOnClick))]
    public class InputOutput : Clickable {
        public Action Action;
        private string logicItemId;
        private OutlineOnClick outlineOnClick;

        private void Awake() {
            outlineOnClick = GetComponent<OutlineOnClick>();
        }

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
                return;
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
                        InputOutput theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedToPointer().GetComponent<InputOutput>();
                        // check if this connection is valid
                        bool result;
                        if (GetType() == typeof(PuckInput)) {
                            result = await ConnectionManagerArcoro.Instance.ValidateConnection(theOtherOne, this);
                        } else {
                            result = await ConnectionManagerArcoro.Instance.ValidateConnection(this, theOtherOne);
                        }
                        if (!result)
                            return;
                        try {
                            if (typeof(PuckInput) == GetType()) {
                                await WebsocketManager.Instance.AddLogicItem(theOtherOne.Action.Data.Id, Action.Data.Id, false);
                            } else {
                                await WebsocketManager.Instance.AddLogicItem(Action.Data.Id, theOtherOne.Action.Data.Id, false);

                            }
                            ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                        } catch (RequestFailedException ex) {
                            Debug.LogError(ex);
                            Notifications.Instance.SaveLogs("Failed to add connection");
                        }
                        
                    }

                } else {
                    if (string.IsNullOrEmpty(logicItemId)) {
                        ConnectionManagerArcoro.Instance.CreateConnectionToPointer(gameObject);
                    } else {
                        GameObject theOtherOne = ConnectionManagerArcoro.Instance.GetConnectedTo(GetLogicItem().GetConnection(), gameObject);
                        ConnectionManagerArcoro.Instance.CreateConnectionToPointer(theOtherOne);
                        try {
                            await WebsocketManager.Instance.RemoveLogicItem(logicItemId);
                        } catch (RequestFailedException ex) {
                            Debug.LogError(ex);
                            Notifications.Instance.SaveLogs("Failed to remove connection");
                            ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                        }
                        
                        
                    }
                }
            }
        }

        public async override void OnHoverStart() {
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
    }

}

