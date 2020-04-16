using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineConnectionsManager : MonoBehaviour {

    public List<LineConnection> LineConnections = new List<LineConnection>();

    public void AddConnection(LineConnection connection) {
        LineConnections.Add(connection);
    }

    public void RemoveConnection(LineConnection connection) {
        LineConnections.Remove(connection);
    }
    
    public void EnableConnections(bool enable) {
        foreach (LineConnection connection in LineConnections) {
            connection.enabled = enable;
        }
    }

    public void SetConnectionsTransparency(float alpha) {
        Color color;
        foreach (LineConnection connection in LineConnections) {
            LineRenderer lineRenderer = connection.GetComponent<LineRenderer>();
            color = lineRenderer.material.color;
            color.a = alpha;
            lineRenderer.material.color = color;
        }
    }
}
