using System;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class ConnectionManagerArcoro : Base.Singleton<ConnectionManagerArcoro> {

    public GameObject ConnectionPrefab, CameraManager;
    public List<Connection> Connections = new List<Connection>();
    private Connection virtualConnectionToMouse;
    private GameObject virtualPointer;

    public bool ConnectionsActive = true;

    private void Start() {
        virtualPointer = CameraManager.GetComponent<Base.VirtualConnection>().VirtualPointer;

        Base.GameManager.Instance.OnCloseProject += OnCloseProject;
    }


    public Connection CreateConnection(GameObject o1, GameObject o2) {
        if (!ConnectionsActive)
            return null;
        GameObject c = Instantiate(ConnectionPrefab);
        c.transform.SetParent(transform);
        // Set correct targets. Output has to be always at 0 index, because we are connecting output to input.
        // Output has direction to the east, while input has direction to the west.
        if (o1.GetComponent<Base.InputOutput>().GetType() == typeof(Base.PuckOutput)) {
            c.GetComponent<Connection>().target[0] = o1.GetComponent<RectTransform>();
            c.GetComponent<Connection>().target[1] = o2.GetComponent<RectTransform>();
        } else {
            c.GetComponent<Connection>().target[1] = o1.GetComponent<RectTransform>();
            c.GetComponent<Connection>().target[0] = o2.GetComponent<RectTransform>();
        }
        Connections.Add(c.GetComponent<Connection>());
        return c.GetComponent<Connection>();
    }

    public void CreateConnectionToPointer(GameObject o) {
        if (!ConnectionsActive)
            return;
        if (virtualConnectionToMouse != null)
            Destroy(virtualConnectionToMouse.gameObject);
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = true;
        virtualConnectionToMouse = CreateConnection(o, virtualPointer);
    }

    public void DestroyConnectionToMouse() {
        if (!ConnectionsActive)
            return;
        Destroy(virtualConnectionToMouse.gameObject);
        Connections.Remove(virtualConnectionToMouse);
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = false;
    }

    public bool IsConnecting() {
        return virtualConnectionToMouse != null;
    }

    public Base.Action GetActionConnectedToPointer() {
        Debug.Assert(virtualConnectionToMouse != null);
        GameObject obj = GetConnectedTo(virtualConnectionToMouse, virtualPointer);
        return obj.GetComponent<InputOutput>().Action;
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

    private void OnCloseProject(object sender, EventArgs e) {
        foreach (Connection c in Connections) {
            if (c.gameObject != null) {
                Destroy(c.gameObject);
            }
        }
        Connections.Clear();
    }

    public void DisplayConnections(bool active) {
        foreach (Connection connection in Connections) {
            connection.gameObject.SetActive(active);
        }
        ConnectionsActive = active;
    }

}
