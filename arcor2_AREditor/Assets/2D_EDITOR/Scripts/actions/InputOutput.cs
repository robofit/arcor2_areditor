using UnityEngine;

public class InputOutput : MonoBehaviour {
    protected ConnectionManagerArcoro _ConnectionManagerArcoro;
    public Connection Connection;
    GameManager GameManager;

    protected virtual void Start() {
        _ConnectionManagerArcoro = GameObject.Find("_ConnectionManager").GetComponent<ConnectionManagerArcoro>();
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    protected virtual void Update() {

    }

    protected virtual void Touch() {


    }

    private void OnMouseUp() {
        if (_ConnectionManagerArcoro.IsConnecting()) {

            if (Connection == null) {
                Connection = _ConnectionManagerArcoro.ConnectVirtualConnectionToObject(gameObject);
                GameManager.UpdateProject();
            }

        } else {
            if (Connection == null) {
                Connection = _ConnectionManagerArcoro.CreateConnectionToMouse(gameObject);
            } else {
                Connection = _ConnectionManagerArcoro.AttachConnectionToMouse(Connection, gameObject);
                Connection = null;
            }
        }

    }

    public Connection GetConneciton() {
        return Connection;
    }
}
