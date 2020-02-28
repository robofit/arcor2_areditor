using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainsScreen : MonoBehaviour {
       
    public CanvasGroup CanvasGroup;

    private void Start() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnDisconnectedFromServer += DisconnectedFromServer;
    }


    private void ConnectedToServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
    }

    private void DisconnectedFromServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
    }
}
