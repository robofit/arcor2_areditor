using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SimpleSideMenu))]
public class MainMenu : Singleton<MainMenu>, IMenu {
    private GameObject debugTools;   

    [SerializeField]
    private SimpleSideMenu menu, notificationsMenu;


    // Start is called before the first frame update
    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseSceneOrProject;
        GameManager.Instance.OnCloseScene += OnCloseSceneOrProject;

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
    }
    private void OnCloseSceneOrProject(object sender, EventArgs e) {
        menu.Close();
        notificationsMenu.Close();
    }


    public void DisconnectFromSever() {
        GameManager.Instance.DisconnectFromSever();
    }

    public void ExitApp() {
        GameManager.Instance.ExitApp();
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
            menu.Open();
        }
    }

    public void SetHeader(string header) {
        //nothing to do.. yet
    }

    public void SaveLogs() {
        Notifications.Instance.SaveLogs(SceneManager.Instance.GetScene(), ProjectManager.Instance.GetProject());
    }

    public void ShowLogs() {
        notificationsMenu.Open();
    }


    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        CalibrationManager.Instance.Recalibrate(showNotification:true);
#endif
    }

    public void Close() {
        menu.Close();
    }

    public SimpleSideMenu.State CurrentState() {
        return menu.CurrentState;
    }

    public void AddListener(UnityAction callback) {
        menu.onStateChanged.AddListener(callback);
    }

}
