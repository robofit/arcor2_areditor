using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesBtnGroup : MonoBehaviour
{
    public CoordBtn X, Y, Z;
    private CoordBtn selectedBtn;


    private void Awake() {
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
        TransformMenu.Instance.ResetTransformWheel();
        btn.Select();
    }

    public string GetSelectedAxis() {
        return selectedBtn.Axis;
    }
}
