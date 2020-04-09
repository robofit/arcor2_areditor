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
            if (this.GetType() == typeof(Base.PuckInput)) {
                Data.Default = "start";
            } else {
                Data.Default = "end";
            }
        }

        public Connection GetConnection() {
            return Connection;
        }

        public override async void OnClick(Click type) {
            if (!ConnectionManagerArcoro.Instance.ConnectionsActive) {
                return; 
            }
            if (type == Click.MOUSE_LEFT_BUTTON || type == Click.TOUCH) {
                if (ConnectionManagerArcoro.Instance.IsConnecting()) {
                    if (Connection == null) {
                        Connection = ConnectionManagerArcoro.Instance.ConnectVirtualConnectionToObject(gameObject);
                        GameObject connectedPuck = ConnectionManagerArcoro.Instance.GetConnectedTo(Connection, gameObject);
                        if (connectedPuck != null && connectedPuck.name != "VirtualPointer") {
                            // TODO backup and restore if request failed
                            Data.Default = connectedPuck.transform.GetComponentInParent<Base.Action>().Data.Id;
                            Base.InputOutput theOtherOne = connectedPuck.GetComponent<Base.InputOutput>();
                            theOtherOne.Data.Default = transform.GetComponentInParent<Base.Action>().Data.Id;
                           
                            bool success1 = await GameManager.Instance.UpdateActionLogic(Action.Data.Id, new List<IO.Swagger.Model.ActionIO>() { Action.Input.Data }, new List<IO.Swagger.Model.ActionIO>() { Action.Output.Data });
                            bool success2 = await GameManager.Instance.UpdateActionLogic(theOtherOne.Action.Data.Id, new List<IO.Swagger.Model.ActionIO>() { theOtherOne.Action.Input.Data }, new List<IO.Swagger.Model.ActionIO>() { theOtherOne.Action.Output.Data });
                           
                        } else {
                            InitData();
                        }
                        
                        //GameManager.Instance.UpdateProject();
                        // TODO - implement using RPC
                    }

                } else {
                    if (Connection == null) {
                        Connection = ConnectionManagerArcoro.Instance.CreateConnectionToMouse(gameObject);
                    } else {
                        Connection = ConnectionManagerArcoro.Instance.AttachConnectionToMouse(Connection, gameObject);
                        InitData();
                        Connection = null;
                    }
                }
            }
        }
    }

}

