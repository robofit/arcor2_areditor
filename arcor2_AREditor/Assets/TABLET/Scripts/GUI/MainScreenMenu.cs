using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainScreenMenu : MonoBehaviour
{
    public TMPro.TMP_Text ConnectionString;

    private void Start() {
        Base.GameManager.Instance.OnConnectedToServer += OnConnectedToServer;
    }

    private void OnConnectedToServer(object sender, Base.StringEventArgs eventArgs) {
        ConnectionString.text = eventArgs.Data;
    }
}
