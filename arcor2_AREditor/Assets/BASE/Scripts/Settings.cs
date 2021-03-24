using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Settings : Singleton<Settings> {

    public bool DontTurnOffScreenOnTablet = true;

    private bool useCloudAnchors = false;
    public bool UseCloudAnchors {
        get {
            return useCloudAnchors;
        }
        set {
            useCloudAnchors = value;
            PlayerPrefsHelper.SaveBool("use_cloud_anchors", useCloudAnchors);
        }
    }

    private void Start() {
        useCloudAnchors = PlayerPrefsHelper.LoadBool("use_cloud_anchors", false);

#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        if (DontTurnOffScreenOnTablet) {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
#endif
    }
}
