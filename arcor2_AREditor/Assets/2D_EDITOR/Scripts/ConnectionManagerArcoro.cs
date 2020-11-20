using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class ConnectionManagerArcoro : Base.Singleton<ConnectionManagerArcoro> {

    public GameObject ConnectionPrefab;
    public List<Connection> Connections = new List<Connection>();
    private Connection virtualConnectionToMouse;
    private GameObject virtualPointer;
    [SerializeField]
    private Material EnabledMaterial, DisabledMaterial;

    private void Start() {
        virtualPointer = VirtualConnectionOnTouch.Instance.VirtualPointer;

    }


    public Connection CreateConnection(GameObject o1, GameObject o2) {
        Connection c = Instantiate(ConnectionPrefab).GetComponent<Connection>();
        c.transform.SetParent(transform);
        // Set correct targets. Output has to be always at 0 index, because we are connecting output to input.
        // Output has direction to the east, while input has direction to the west.
        if (o1.GetComponent<Base.InputOutput>().GetType() == typeof(Base.PuckOutput)) {
            c.target[0] = o1.GetComponent<RectTransform>();
            c.target[1] = o2.GetComponent<RectTransform>();
        } else {
            c.target[1] = o1.GetComponent<RectTransform>();
            c.target[0] = o2.GetComponent<RectTransform>();
        }
        Connections.Add(c);
        if (!ControlBoxManager.Instance.ConnectionsToggle.isOn)
            c.gameObject.SetActive(false);
        
        return c;
    }

    public void CreateConnectionToPointer(GameObject o) {
        if (virtualConnectionToMouse != null)
            Destroy(virtualConnectionToMouse.gameObject);
        VirtualConnectionOnTouch.Instance.DrawVirtualConnection = true;
        virtualConnectionToMouse = CreateConnection(o, virtualPointer);
    }

    public void DestroyConnectionToMouse() {
        Destroy(virtualConnectionToMouse.gameObject);
        Connections.Remove(virtualConnectionToMouse);
        VirtualConnectionOnTouch.Instance.DrawVirtualConnection = false;
    }

    public bool IsConnecting() {
        return virtualConnectionToMouse != null;
    }

    public Base.Action GetActionConnectedToPointer() {
        Debug.Assert(virtualConnectionToMouse != null);
        GameObject obj = GetConnectedTo(virtualConnectionToMouse, virtualPointer);
        return obj.GetComponent<InputOutput>().Action;
    }

    public GameObject GetConnectedToPointer() {
        Debug.Assert(virtualConnectionToMouse != null);
        return GetConnectedTo(virtualConnectionToMouse, virtualPointer);
    }

     public Base.Action GetActionConnectedTo(Connection c, GameObject o) {        
        return GetConnectedTo(c, o).GetComponent<InputOutput>().Action;
    }

    private int GetIndexOf(Connection c, GameObject o) {
        if (c.target[0] != null && c.target[0].gameObject == o) {
            return 0;
        } else if (c.target[1] != null && c.target[1].gameObject == o) {
            return 1;
        } else {
            return -1;
        }
    }

    private int GetIndexByType(Connection c, System.Type type) {
        if (c.target[0] != null && c.target[0].gameObject.GetComponent<Base.InputOutput>() != null && c.target[0].gameObject.GetComponent<Base.InputOutput>().GetType().IsSubclassOf(type))
            return 0;
        else if (c.target[1] != null && c.target[1].gameObject.GetComponent<Base.InputOutput>() != null && c.target[1].gameObject.GetComponent<Base.InputOutput>().GetType().IsSubclassOf(type))
            return 1;
        else
            return -1;

    }

    public GameObject GetConnectedTo(Connection c, GameObject o) {
        if (c == null || o == null)
            return null;
        int i = GetIndexOf(c, o);
        if (i < 0)
            return null;
        return c.target[1 - i].gameObject;
    }

    /**
     * Checks that there is input on one end of connection and output on the other side
     */
    public bool ValidateConnection(Connection c) {
        if (c == null)
            return false;
        int input = GetIndexByType(c, typeof(Base.PuckInput)), output = GetIndexByType(c, typeof(Base.PuckOutput));
        if (input < 0 || output < 0)
            return false;
        return input + output == 1;
    }

    public async Task<bool> ValidateConnection(InputOutput output, InputOutput input) {
        string[] startEnd = new[] { "START", "END" };
        if (output.GetType() == input.GetType() ||
            output.Action.Data.Id.Equals(input.Action.Data.Id) ||
            (startEnd.Contains(output.Action.Data.Id) && startEnd.Contains(input.Action.Data.Id))) {
            return false;
        }
        try {
            await WebsocketManager.Instance.AddLogicItem(output.Action.Data.Id, input.Action.Data.Id, true);
        } catch (RequestFailedException) {
            return false;
        }
        return true;
    }

    public void Clear() {
        foreach (Connection c in Connections) {
            if (c != null && c.gameObject != null) {
                Destroy(c.gameObject);
            }
        }
        Connections.Clear();
    }

    public void DisplayConnections(bool active) {
        foreach (Connection connection in Connections) {
            connection.gameObject.SetActive(active);
        }
    }

    public void DisableConnectionToMouse() {
        if (virtualConnectionToMouse != null)
            virtualConnectionToMouse.GetComponent<LineRenderer>().material = DisabledMaterial;
    }

    public void EnableConnectionToMouse() {
        if (virtualConnectionToMouse != null)
            virtualConnectionToMouse.GetComponent<LineRenderer>().material = EnabledMaterial;
    }

}
