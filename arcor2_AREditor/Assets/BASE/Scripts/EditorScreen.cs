using System;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class EditorScreen : MonoBehaviour {
       
    private CanvasGroup CanvasGroup;

    [SerializeField]
    private ButtonWithTooltip StartStopSceneBtn;
    [SerializeField]
    private Image StartStopSceneIcon;



    private void Start() {
        CanvasGroup = GetComponent<CanvasGroup>();
        GameManager.Instance.OnOpenProjectEditor += ShowEditorWindow;
        GameManager.Instance.OnRunPackage += ShowEditorWindow;
        GameManager.Instance.OnOpenSceneEditor += ShowEditorWindow;
        GameManager.Instance.OnOpenMainScreen += HideEditorWindow;
        GameManager.Instance.OnDisconnectedFromServer += HideEditorWindow;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
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
        if (args.Data.State == SceneStateData.StateEnum.Started) {
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
            var response = await CommunicationManager.Instance.Client.StartSceneAsync();
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Going online failed", string.Join(",", response.Messages));
            }
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Going online failed", e.Message);
        }
    }

    private void StopSceneCallback(Task<StopSceneResponse> response) {
        if (!response.Result.Result)
            Notifications.Instance.ShowNotification("Going offline failed", response.Result.Messages.FirstOrDefault());
    }

    public void StopScene() {
        CommunicationManager.Instance.Client.StopSceneAsync().ContinueWith(StopSceneCallback, TaskScheduler.FromCurrentSynchronizationContext());
    }
}
