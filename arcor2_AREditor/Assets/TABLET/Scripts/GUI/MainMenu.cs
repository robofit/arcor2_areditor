using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;
using System.Collections.Generic;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using System.Threading.Tasks;
using IO.Swagger.Model;
using System.Linq;
using Newtonsoft.Json;

[RequireComponent(typeof(SimpleSideMenu))]
public class MainMenu : MonoBehaviour, IMenu {

    [SerializeField]
    private ButtonWithTooltip StartSceneBtn, StopSceneBtn;

    private GameObject debugTools;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private InputDialogWithToggle inputDialogWithToggle;

    

    private SimpleSideMenu menu;

    [SerializeField]
    private GameObject loadingScreen;


    // Start is called before the first frame update
    private void Start() {
        menu = GetComponent<SimpleSideMenu>();
        Debug.Assert(inputDialog != null);
        Debug.Assert(StartSceneBtn != null);
        Debug.Assert(StopSceneBtn != null);
        Debug.Assert(loadingScreen != null);

        Base.SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
        MenuManager.Instance.ShowMenu(MenuManager.Instance.MainMenu);

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        StartSceneBtn.gameObject.SetActive(!(args.Event.State == SceneStateData.StateEnum.Started));
        StopSceneBtn.gameObject.SetActive(args.Event.State == SceneStateData.StateEnum.Started);
    }

    public void DisconnectFromSever() {
        Base.GameManager.Instance.DisconnectFromSever();
    }

    public void ExitApp() {
        Base.GameManager.Instance.ExitApp();
    }

    public void SetDebugMode() {
        if (debugTools != null) {
            if (debugTools.activeSelf)
                debugTools.SetActive(false);
            else
                debugTools.SetActive(true);
        }
    }

    public async void UpdateMenu() {
        if (menu.CurrentState == SimpleSideMenu.State.Open) {
            menu.Close();
            return;
        } else {
            loadingScreen.SetActive(true);
            menu.Open();
        }

        loadingScreen.SetActive(false);
    }

    public void SetHeader(string header) {
        //nothing to do.. yet
    }

    public void SaveLogs() {
        Base.Notifications.Instance.SaveLogs(Base.SceneManager.Instance.GetScene(), Base.ProjectManager.Instance.GetProject());
    }

    public async void StartScene() {
        try {
            await WebsocketManager.Instance.StartScene(false);
        } catch(RequestFailedException e) {
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


    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        CalibrationManager.Instance.Recalibrate(showNotification:true);
#endif
    }

}
