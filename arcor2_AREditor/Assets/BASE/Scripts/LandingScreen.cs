using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LandingScreen : MonoBehaviour
{
    public TMPro.TMP_InputField Domain, Port;
    public CanvasGroup CanvasGroup;

    private void Start() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnDisconnectedFromServer += DisconnectedFromServer;
        Domain.text = PlayerPrefs.GetString("arserver_domain", "");
        Port.text = PlayerPrefs.GetInt("arserver_port", 6789).ToString(); 
    }

    public void ConnectToServer() {
        string domain = Domain.text;
        int port = int.Parse(Port.text);
        PlayerPrefs.SetString("arserver_domain", domain);
        PlayerPrefs.SetInt("arserver_port", port);
        PlayerPrefs.Save();
        Base.GameManager.Instance.ConnectToSever(domain, port);
    }

    private void ConnectedToServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
    }

    private void DisconnectedFromServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
    }
}
