using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CreateAnchor : Base.Clickable {

    public override void OnClick(Click type) {
        CalibrationManager.Instance.CreateAnchor(transform);
    }

}
