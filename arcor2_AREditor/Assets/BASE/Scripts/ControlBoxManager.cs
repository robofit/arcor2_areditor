using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.UI;

public class ControlBoxManager : Singleton<ControlBoxManager> {

    [SerializeField]
    private InputDialog InputDialog;
    [SerializeField]

    public Toggle TrackablesToggle;
    public Toggle ConnectionsToggle;
    public Toggle VRModeToggle;
    public Toggle CalibrationElementsToggle;
    public Toggle AutoCalibToggle;

    public AddActionPointUsingRobotDialog AddActionPointUsingRobotDialog;

    



 



    


    

    


    

    private void OnDestroy() {
#if UNITY_ANDROID && AR_ON
        PlayerPrefsHelper.SaveBool("control_box_display_trackables", TrackablesToggle.isOn);
        PlayerPrefsHelper.SaveBool("control_box_autoCalib", AutoCalibToggle.isOn);
#endif
        PlayerPrefsHelper.SaveBool("control_box_display_connections", ConnectionsToggle.isOn);
    }
}
