using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineConnection : MonoBehaviour {

    public Transform[] targets = new Transform[2];
    private LineRenderer lineRenderer;
    private bool connectionActive;

    public Material ClickMaterial;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start() {
        CreateConnection(targets[0], targets[1]);
    }

    private void Update() {
        if (connectionActive) {
            if (targets[0].hasChanged || targets[1].hasChanged) {
                UpdateLine();
            }
        }
    }

    public void CreateConnection(Transform input, Transform output) {
        lineRenderer.SetPositions(new Vector3[2]{ input.position, output.position });
        connectionActive = true;
    }

    public void UpdateLine() {
        lineRenderer.SetPositions(new Vector3[2] { targets[0].position, targets[1].position });
    }
}
