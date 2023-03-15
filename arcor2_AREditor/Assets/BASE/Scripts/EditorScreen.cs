using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Base;
using UnityEngine.UI;
using IO.Swagger.Model;
using Newtonsoft.Json;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class EditorScreen : MonoBehaviour {
       
    private CanvasGroup CanvasGroup;

    [SerializeField]
    private ButtonWithTooltip StartStopSceneBtn;
    [SerializeField]
    private Image StartStopSceneIcon;



    private void Start() {
        CanvasGroup = GetComponent<CanvasGroup>();
        Base.GameManager.Instance.OnOpenProjectEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnRunPackage += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenSceneEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenMainScreen += HideEditorWindow;
        Base.GameManager.Instance.OnDisconnectedFromServer += HideEditorWindow;
        Base.SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void ShowEditorWindow(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }

    private void HideEditorWindow(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (args.Event.State == IO.Swagger.Model.SceneStateData.StateEnum.Started) {
            StartStopSceneIcon.sprite = AREditorResources.Instance.SceneOnline;
            StartStopSceneBtn.SetDescription("Go offline");
        } else {
            StartStopSceneIcon.sprite = AREditorResources.Instance.SceneOffline;
            StartStopSceneBtn.SetDescription("Go online");
        }
    }

    public void SwitchSceneState() {
        if (SceneManager.Instance.SceneStarted)
            StopScene();
        else
            StartScene();
    }

    public async void StartScene() {
        try {
            await WebsocketManager.Instance.StartScene(false);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Going online failed", e.Message);
        }
    }

    private void StopSceneCallback(string _, string data) {
        CloseProjectResponse response = JsonConvert.DeserializeObject<CloseProjectResponse>(data);
        if (!response.Result)
            Notifications.Instance.ShowNotification("Going offline failed", response.Messages.FirstOrDefault());
    }

    public void StopScene() {
        WebsocketManager.Instance.StopScene(false, StopSceneCallback);
    }
}
