using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManagerArcoro : Base.Singleton<ConnectionManagerArcoro>
{

    public GameObject _ConnectionPrefab, VirtualPointer, _CameraManager;
    private Connection VirtualConnectionToMouse;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Connection CreateConnection(GameObject o1, GameObject o2)
    {
        GameObject c = Instantiate(_ConnectionPrefab);
        c.transform.SetParent(transform);
        c.GetComponent<Connection>().target[0] = o1.GetComponent<RectTransform>();
        c.GetComponent<Connection>().target[1] = o2.GetComponent<RectTransform>();
        return c.GetComponent<Connection>();
    }

    public Connection CreateConnectionToMouse(GameObject o)
    {
        if (VirtualConnectionToMouse != null)
            Destroy(VirtualConnectionToMouse.gameObject);
        _CameraManager.GetComponent<CameraMove>().DrawVirtualConnection = true;
        VirtualConnectionToMouse = CreateConnection(o, VirtualPointer);
        
        return VirtualConnectionToMouse;
    }

    public void DestroyConnectionToMouse()
    {
        int i = GetIndexByType(VirtualConnectionToMouse, typeof(InputOutput));
        if (i >= 0)
        {
            VirtualConnectionToMouse.target[i].GetComponent<InputOutput>().Connection = null;
        }
        Destroy(VirtualConnectionToMouse.gameObject);
        _CameraManager.GetComponent<CameraMove>().DrawVirtualConnection = false;
    }

    public Connection ConnectVirtualConnectionToObject(GameObject o)
    {
        if (VirtualConnectionToMouse == null)
            return null;
       
        
        int i = GetIndexOf(VirtualConnectionToMouse, VirtualPointer);
        if (i < 0)
        {
            return null;
        }
        if (VirtualConnectionToMouse.target[1 - i].gameObject.GetComponent<InputOutput>().GetType() != o.GetComponent<InputOutput>().GetType())
        {
            VirtualConnectionToMouse.target[i] = o.GetComponent<RectTransform>();
        } else
        {
            return null;
        }        
        Connection c = VirtualConnectionToMouse;
        VirtualConnectionToMouse = null;
        _CameraManager.GetComponent<CameraMove>().DrawVirtualConnection = false;
        return c;
    }

    public Connection AttachConnectionToMouse(Connection c, GameObject o)
    {
        int i = GetIndexOf(c, o);
        if (i < 0)
            return null;
        c.target[i] = VirtualPointer.GetComponent<RectTransform>();
        o.GetComponent<InputOutput>().Connection = null;
        VirtualConnectionToMouse = c;
        _CameraManager.GetComponent<CameraMove>().DrawVirtualConnection = true;
        return VirtualConnectionToMouse;
    }

    public bool IsConnecting()
    {
        return VirtualConnectionToMouse != null;
    }

    public Connection GetVirtualConnectionToMouse()
    {
        return VirtualConnectionToMouse;
    }

    private int GetIndexOf(Connection c, GameObject o)
    {
        if (c.target[0] != null && c.target[0].gameObject == o)
        {
            return 0;
        }
        else if (c.target[1] != null && c.target[1].gameObject == o)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    private int GetIndexByType(Connection c, System.Type type)
    {
        if (c.target[0] != null && c.target[0].gameObject.GetComponent<InputOutput>() != null && c.target[0].gameObject.GetComponent<InputOutput>().GetType() == type)
            return 0;
        else if (c.target[1] != null && c.target[1].gameObject.GetComponent<InputOutput>() != null && c.target[1].gameObject.GetComponent<InputOutput>().GetType() == type)
            return 1;
        else
            return -1;

    }

    public GameObject GetConnectedTo(Connection c, GameObject o)
    {
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
    public bool ValidateConnection(Connection c)
    {
        if (c == null)
            return false;
        int input = GetIndexByType(c, typeof(PuckInput)), output = GetIndexByType(c, typeof(PuckOutput));
        if (input < 0 || output < 0)
            return false;
        return input + output == 1; 
    }
}
