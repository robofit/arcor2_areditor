using UnityEngine;

namespace Base {
    public class InputOutput : Clickable {
        public Connection Connection;
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

        public override void OnClick(Click type) {
            if (type == Click.MOUSE_LEFT_BUTTON || type == Click.TOUCH) {
                if (ConnectionManagerArcoro.Instance.IsConnecting()) {
                    if (Connection == null) {
                        Connection = ConnectionManagerArcoro.Instance.ConnectVirtualConnectionToObject(gameObject);
                        GameObject connectedPuck = ConnectionManagerArcoro.Instance.GetConnectedTo(Connection, gameObject);
                        if (connectedPuck != null && connectedPuck.name != "VirtualPointer") {
                            Data.Default = connectedPuck.transform.GetComponentInParent<Base.Action>().Data.Id;
                            connectedPuck.GetComponent<Base.InputOutput>().Data.Default = transform.GetComponentInParent<Base.Action>().Data.Id;
                        } else {
                            InitData();
                        }

                        GameManager.Instance.UpdateProject();
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

