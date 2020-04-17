using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MainScreenMenu : MonoBehaviour
{
    public TMPro.TMP_Text ConnectionString;

    public Toggle CloudAnchorToggle;

    private void Start() {
        Debug.Assert(ConnectionString != null);
        CloudAnchorToggle.isOn = Settings.Instance.UseCloudAnchors;
        Base.GameManager.Instance.OnConnectedToServer += OnConnectedToServer;
    }

    private void OnConnectedToServer(object sender, Base.StringEventArgs eventArgs) {
        ConnectionString.text = eventArgs.Data;
    }
}
