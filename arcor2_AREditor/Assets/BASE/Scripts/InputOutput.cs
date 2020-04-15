using UnityEngine;
using System.Collections.Generic;

namespace Base {
    public class InputOutput : Clickable {
        public Connection Connection;
        public Action Action;
        public IO.Swagger.Model.ActionIO Data = new IO.Swagger.Model.ActionIO("");

        protected virtual void Awake() {
        }

        protected virtual void Start() {
        }

        // Update is called once per frame
        protected virtual void Update() {

        }

        public void InitData() {
            Data.Default = GetDefaultValue();
        }

        private string GetDefaultValue() {
            if (this.GetType() == typeof(Base.PuckInput)) {
                return "start";
            } else {
                return "end";
            }
        }

        public Connection GetConnection() {
            return Connection;
        }

        private void AddConnections(GameObject connectedGameObject) {
            Debug.Assert(connectedGameObject != null);
            UpdateConnection(connectedGameObject, connectedGameObject.transform.GetComponentInParent<Base.Action>().Data.Id, Action.Data.Id);
        }

        private void RemoveConnection(GameObject connectedGameObject) {
            Debug.Assert(connectedGameObject != null);
            InputOutput connectedIO = connectedGameObject.GetComponent<Base.InputOutput>();
            UpdateConnection(connectedGameObject, GetDefaultValue(), connectedIO.GetDefaultValue());
        }

        private async void UpdateConnection(GameObject connectedGameObject, string localValue, string remoteValue) {
            InputOutput connectedIO = connectedGameObject.GetComponent<Base.InputOutput>();

            string originalLocalValue = Data.Default, originalRemoveValue = connectedIO.Data.Default;
            Data.Default = localValue;
            connectedIO.Data.Default = remoteValue;

            bool success1 = await GameManager.Instance.UpdateActionLogic(Action.Data.Id, new List<IO.Swagger.Model.ActionIO>() { Action.Input.Data }, new List<IO.Swagger.Model.ActionIO>() { Action.Output.Data });
            bool success2 = await GameManager.Instance.UpdateActionLogic(connectedIO.Action.Data.Id, new List<IO.Swagger.Model.ActionIO>() { connectedIO.Action.Input.Data }, new List<IO.Swagger.Model.ActionIO>() { connectedIO.Action.Output.Data });

            if (!success1 || !success2) {
                Data.Default = originalLocalValue;
                connectedIO.Data.Default = originalRemoveValue;
            }

        }

        public override void OnClick(Click type) {
            if (!ConnectionManagerArcoro.Instance.ConnectionsActive) {
                return; 
            }
            if (type == Click.MOUSE_LEFT_BUTTON || type == Click.TOUCH) {
                if (ConnectionManagerArcoro.Instance.IsConnecting()) {
                    if (Connection == null) {
                        Connection = ConnectionManagerArcoro.Instance.ConnectVirtualConnectionToObject(gameObject);
                        GameObject connectedGameObject = ConnectionManagerArcoro.Instance.GetConnectedTo(Connection, gameObject);
                        if (connectedGameObject != null && connectedGameObject.name != "VirtualPointer") {
                            // TODO backup and restore if request failed
                            AddConnections(connectedGameObject);
                        } else {
                            InitData();
                        }
                        
                    }

                } else {
                    if (Connection == null) {
                        Connection = ConnectionManagerArcoro.Instance.CreateConnectionToPointer(gameObject);
                    } else {
                        GameObject connectedGameObject = ConnectionManagerArcoro.Instance.GetConnectedTo(Connection, gameObject);
                        Connection = ConnectionManagerArcoro.Instance.AttachConnectionToPointer(Connection, gameObject);
                        RemoveConnection(connectedGameObject);
                        Connection = null;
                    }
                }
            }
        }
    }

}

