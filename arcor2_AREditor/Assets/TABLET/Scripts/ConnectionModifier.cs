using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionModifier : MonoBehaviour {

    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update() {
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
    }
}
