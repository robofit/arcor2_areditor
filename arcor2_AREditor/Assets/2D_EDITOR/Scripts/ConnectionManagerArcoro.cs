using System;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManagerArcoro : Base.Singleton<ConnectionManagerArcoro> {

    public GameObject ConnectionPrefab, CameraManager;
    public List<Connection> Connections = new List<Connection>();
    private Connection virtualConnectionToMouse;
    private GameObject virtualPointer;

    // Start is called before the first frame update
    void Start() {
        virtualPointer = CameraManager.GetComponent<Base.VirtualConnection>().VirtualPointer;

        Base.GameManager.Instance.OnCloseProject += OnCloseProject;
    }

    // Update is called once per frame
    void Update() {

    }

    public Connection CreateConnection(GameObject o1, GameObject o2) {
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

    public Connection CreateConnectionToMouse(GameObject o) {
        if (virtualConnectionToMouse != null)
            Destroy(virtualConnectionToMouse.gameObject);
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = true;
        virtualConnectionToMouse = CreateConnection(o, virtualPointer);

        return virtualConnectionToMouse;
    }

    public void DestroyConnectionToMouse() {
        int i = GetIndexByType(virtualConnectionToMouse, typeof(Base.InputOutput));
        if (i >= 0) {
            virtualConnectionToMouse.target[i].GetComponent<Base.InputOutput>().Connection = null;
            virtualConnectionToMouse.target[i].GetComponent<Base.InputOutput>().InitData();
        }
        Destroy(virtualConnectionToMouse.gameObject);
        Connections.Remove(virtualConnectionToMouse);
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = false;
    }

    public Connection ConnectVirtualConnectionToObject(GameObject o) {
        if (virtualConnectionToMouse == null)
            return null;


        int i = GetIndexOf(virtualConnectionToMouse, virtualPointer);
        if (i < 0) {
            return null;
        }
        if (virtualConnectionToMouse.target[1 - i].gameObject.GetComponent<Base.InputOutput>().GetType() != o.GetComponent<Base.InputOutput>().GetType()) {
            virtualConnectionToMouse.target[i] = o.GetComponent<RectTransform>();
        } else {
            return null;
        }
        Connection c = virtualConnectionToMouse;
        virtualConnectionToMouse = null;
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = false;
        return c;
    }

    public Connection AttachConnectionToMouse(Connection c, GameObject o) {
        int i = GetIndexOf(c, o);
        if (i < 0)
            return null;
        c.target[i] = virtualPointer.GetComponent<RectTransform>();
        o.GetComponent<Base.InputOutput>().Connection = null;
        o.GetComponent<Base.InputOutput>().InitData();
        virtualConnectionToMouse = c;
        CameraManager.GetComponent<Base.VirtualConnection>().DrawVirtualConnection = true;
        return virtualConnectionToMouse;
    }

    public bool IsConnecting() {
        return virtualConnectionToMouse != null;
    }

    public Connection GetVirtualConnectionToMouse() {
        return virtualConnectionToMouse;
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

}
