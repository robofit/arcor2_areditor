using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesBtnGroup : MonoBehaviour
{
    public class CoordinateSwitchEventArgs : EventArgs {
        public Gizmo.Axis SelectedAxis {
            get; set;
        }

        public CoordinateSwitchEventArgs(Gizmo.Axis selectedAxis) {
            SelectedAxis = selectedAxis;
        }
    }

    public CoordBtn X, Y, Z;
    private CoordBtn selectedBtn;

    public delegate void CoordinateSwitchEventHandler(object sender, CoordinateSwitchEventArgs args);
    public event CoordinateSwitchEventHandler OnAxisChanged;


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
        OnAxisChanged?.Invoke(this, new CoordinateSwitchEventArgs(selectedBtn.Axis));
    }

    public Gizmo.Axis GetSelectedAxis() {
        return selectedBtn.Axis;
    }
}
