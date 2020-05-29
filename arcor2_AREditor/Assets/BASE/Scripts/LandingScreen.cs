using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Base;

[RequireComponent(typeof(CanvasGroup))]
public class LandingScreen : MonoBehaviour
{
    public TMPro.TMP_InputField Domain, Port;
    public Toggle KeepConnected;
    public CanvasGroup CanvasGroup;
    [SerializeField]
    private TMPro.TMP_Text Version;

    private void Start() {
        Debug.Assert(Domain != null);
        Debug.Assert(Port != null);
        Debug.Assert(KeepConnected != null);
        Debug.Assert(CanvasGroup != null);
        Debug.Assert(Version != null);
        bool keepConnected = PlayerPrefs.GetInt("arserver_keep_connected", 0) == 1 ? true : false;
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnDisconnectedFromServer += DisconnectedFromServer;
        Domain.text = PlayerPrefs.GetString("arserver_domain", "");
        Port.text = PlayerPrefs.GetInt("arserver_port", 6789).ToString();
        KeepConnected.isOn = keepConnected;
        Version.text = Base.GameManager.Instance.EditorVersion;
        if (keepConnected) {
            ConnectToServer();
        }
    }

    public void ConnectToServer() {
        string domain = Domain.text;
        int port = int.Parse(Port.text);
        PlayerPrefs.SetString("arserver_domain", domain);
        PlayerPrefs.SetInt("arserver_port", port);
        PlayerPrefs.SetInt("arserver_keep_connected", KeepConnected.isOn ? 1 : 0);
        PlayerPrefs.Save();
        Base.GameManager.Instance.ConnectToSever(domain, port);
    }

    private void ConnectedToServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }

    private void DisconnectedFromServer(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }

    public void SaveLogs() {
        Notifications.Instance.SaveLogs();
    }
}
