using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : Base.Singleton<ConnectionManager> {

    [SerializeField] Connection connectionPrefab;
    [SerializeField] List<Connection> connections = new List<Connection>();

    public static Connection FindConnection(RectTransform t1, RectTransform t2) {
        if (!Instance)
            return null;

        foreach (Connection connection in Instance.connections) {
            if (connection == null)
                continue;

            if (connection.Match(t1, t2)) {
                return connection;
            }
        }

        return null;
    }

    public static List<Connection> FindConnections(RectTransform transform) {
        List<Connection> found = new List<Connection>();
        if (!Instance)
            return found;

        foreach (Connection connection in Instance.connections) {
            if (connection == null)
                continue;

            if (connection.Contains(transform)) {
                found.Add(connection);
            }
        }

        return found;
    }

    public static void AddConnection(Connection c) {
        if (c == null || !Instance)
            return;

        if (!Instance.connections.Contains(c)) {
            c.transform.SetParent(Instance.transform);
            Instance.connections.Add(c);
        }
    }

    public static void RemoveConnection(Connection c) {
        //don't use the property here. We don't want to create an instance when the scene loads
        if (c != null && Instance != null)
            Instance.connections.Remove(c);
    }

    public static void SortConnections() {
        if (!Instance)
            return;

        Instance.connections.Sort((Connection c1, Connection c2) => { return string.Compare(c1.name, c2.name); });
        for (int i = 0; i < Instance.connections.Count; i++) {
            Instance.connections[i].transform.SetSiblingIndex(i);
        }
    }

    public static void CleanConnections() {
        if (!Instance)
            return;

        //fist clean any null entries
        Instance.connections.RemoveAll((Connection c) => { return c == null; });

        //copy list because OnDestroy messages will modify the original
        List<Connection> copy = new List<Connection>(Instance.connections);
        foreach (Connection c in copy) {
            if (c && !c.isValid) {
                DestroyImmediate(c.gameObject);
            }
        }
    }

    public static void CreateConnection(RectTransform t1, RectTransform t2 = null) {
        if (!Instance)
            return;

        Connection conn;

        if (Instance.connectionPrefab) {
            conn = Instantiate<Connection>(Instance.connectionPrefab);
        } else {
            conn = new GameObject("new connection").AddComponent<Connection>();
        }

        conn.SetTargets(t1, t2);
    }
}
