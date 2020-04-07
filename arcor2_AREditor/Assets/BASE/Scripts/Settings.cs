using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class Settings : Singleton<Settings> {

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
    }
}
