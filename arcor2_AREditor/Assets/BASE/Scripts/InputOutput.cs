using UnityEngine;

namespace Base {
    public class InputOutput : MonoBehaviour {
        protected ConnectionManagerArcoro _ConnectionManagerArcoro;
        public Connection Connection;
        GameManager GameManager;
        public IO.Swagger.Model.ActionIO Data = new IO.Swagger.Model.ActionIO();

        protected virtual void Awake() {
            InitData();
        }

        protected virtual void Start() {
            _ConnectionManagerArcoro = GameObject.Find("_ConnectionManager").GetComponent<ConnectionManagerArcoro>();
            GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
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

        protected virtual void Touch() {


        }

        private void OnMouseUp() {
            if (_ConnectionManagerArcoro.IsConnecting()) {

                if (Connection == null) {
                    Connection = _ConnectionManagerArcoro.ConnectVirtualConnectionToObject(gameObject);
                    GameObject connectedPuck = ConnectionManagerArcoro.Instance.GetConnectedTo(Connection, gameObject);
                    if (connectedPuck != null && connectedPuck.name != "VirtualPointer") {
                        Data.Default = connectedPuck.transform.GetComponentInParent<Base.Action>().Data.Id;
                        connectedPuck.GetComponent<Base.InputOutput>().Data.Default = transform.GetComponentInParent<Base.Action>().Data.Id;
                    } else {
                        InitData();
                        connectedPuck.GetComponent<Base.InputOutput>().InitData();
                    }

                    GameManager.UpdateProject();
                }

            } else {
                if (Connection == null) {
                    Connection = _ConnectionManagerArcoro.CreateConnectionToMouse(gameObject);
                } else {
                    Connection = _ConnectionManagerArcoro.AttachConnectionToMouse(Connection, gameObject);
                    InitData();
                    Connection = null;
                }
            }

        }

        public Connection GetConneciton() {
            return Connection;
        }
    }

}

