using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Recalibrate : Clickable {


    public override void OnClick(Click type) {
        Debug.Log("CLICK: " + type);

        if (type == Click.LONG_TOUCH) {
            Calibrate();
        }
    }

    public void Calibrate() {
        CalibrationManager.Instance.Recalibrate();
    }
}
