using UnityEngine;
using System.Collections.Generic;

namespace Base {
    public class InputOutput : Clickable {
        public Action Action;
        private string logicItemId;

        protected virtual void Awake() {
        }

        protected virtual void Start() {
        }

        // Update is called once per frame
        protected virtual void Update() {

        }

        public void Init(string logicItemId) {
            this.logicItemId = logicItemId;
        }

        private string GetDefaultValue() {
            if (this.GetType() == typeof(Base.PuckInput)) {
                return "start";
            } else {
                return "end";
            }
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
                        if (GetType() == theOtherOne.GetType()) {
                            Notifications.Instance.ShowNotification("Connection failed", "You cannot connect two arrows of same type");
                            return;
                        }
                        try {
                            if (typeof(PuckInput) == GetType()) {
                                await WebsocketManager.Instance.AddLogicItem(theOtherOne.Action.Data.Id, Action.Data.Id);
                            } else {
                                await WebsocketManager.Instance.AddLogicItem(Action.Data.Id, theOtherOne.Action.Data.Id);
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
                        try {
                            await WebsocketManager.Instance.RemoveLogicItem(logicItemId);
                        } catch (RequestFailedException ex) {
                            Debug.LogError(ex);
                            Notifications.Instance.SaveLogs("Failed to add connection");
                        }
                        ConnectionManagerArcoro.Instance.CreateConnectionToPointer(theOtherOne);
                        
                    }
                }
            }
        }
    }

}

