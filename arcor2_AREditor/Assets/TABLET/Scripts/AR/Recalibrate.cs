using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Recalibrate : InteractiveObject
    {


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        Calibrate();
    }

    public void Calibrate() {
        CalibrationManager.Instance.Recalibrate();
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }

    public override string GetName() {
        return "ReCalibration cube";
    }

    public override string GetId() {
        return "ReCalibration cube";
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }
}
