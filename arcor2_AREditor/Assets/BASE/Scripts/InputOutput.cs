using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Base.Clickable;

namespace Base {
    public abstract class InputOutput : MonoBehaviour, ISubItem {
        public Action Action;
        private List<string> logicItemIds = new();
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
            List<LogicItem> items = new();
            foreach (string itemId in logicItemIds)
                if (ProjectManager.Instance.LogicItems.TryGetValue(itemId, out LogicItem logicItem)) {
                    items.Add(logicItem);
                } else {
                    throw new ItemNotFoundException("Logic item with ID " + itemId + " does not exists");
                }
            return items;
        }

        protected bool CheckClickType(Click type) {
           
            if (!(bool) MainSettingsMenu.Instance.ConnectionsSwitch.GetValue()) {
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
/*
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
                    AREditorResources.Instance.ConnectionSelectorDialog.Open(items, showNewConnectionButton, this, () => Action.WriteUnlock());

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
                    }*//*
                }
            }
            
        }
*/

        


        
        
        
/*
        public async override void OnHoverStart() {
            if (!Enabled)
                return;
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                return;
            }
            outlineOnClick.Highlight();
            Action.NameText.gameObject.SetActive(true);
            DisplayOffscreenIndicator(true);

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
        }*/
/*
        public override void OnHoverEnd() {
            outlineOnClick.UnHighlight();
            Action.NameText.gameObject.SetActive(false);
            DisplayOffscreenIndicator(false);

            if (!ConnectionManagerArcoro.Instance.IsConnecting())
                return;
            ConnectionManagerArcoro.Instance.EnableConnectionToMouse();
        }

        public override string GetName() {
            if (typeof(PuckOutput) == GetType()) {
                return "Output";
            } else {
                return "Input";
            }
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

        public override Task Rename(string name) {
            throw new NotImplementedException();
        }*/
        public InteractiveObject GetParentObject() {
            return Action;
        }

        public bool AnyConnection() {
            return ConnectionCount() > 0;
        }

        public int ConnectionCount() {
            return logicItemIds.Count();
        }
    }

}

