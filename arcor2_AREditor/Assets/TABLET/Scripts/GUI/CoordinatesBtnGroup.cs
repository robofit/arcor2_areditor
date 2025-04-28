using System;
using Base;
using UnityEngine;

public class CoordinatesBtnGroup : MonoBehaviour {


    public CoordBtn X, Y, Z;
    private CoordBtn selectedBtn;

    public event EventHandler<GizmoAxisEventArgs> OnAxisChanged;


    private void Start() {
        X.Select();
        selectedBtn = X;
    }

    public void DisableAll() {
        X.Deselect();
        Y.Deselect();
        Z.Deselect();
    }

    public void Enable(CoordBtn btn) {
        DisableAll();
        selectedBtn = btn;
        btn.Select();
        OnAxisChanged?.Invoke(this, new GizmoAxisEventArgs(selectedBtn.Axis));
    }

    public Gizmo.Axis GetSelectedAxis() {
        return selectedBtn.Axis;
    }
}
