using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Recalibrate : Clickable {


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        if (type == Click.LONG_TOUCH) {
            Calibrate();
        }
    }

    public void Calibrate() {
        CalibrationManager.Instance.Recalibrate();
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
