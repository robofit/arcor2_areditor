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

    //[SerializeField]
    //private ButtonWithTooltip StartSceneBtn, StopSceneBtn;

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
        
        Debug.Assert(loadingScreen != null);

        
        MenuManager.Instance.ShowMenu(MenuManager.Instance.MainMenu);

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
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
            menu.Open();
        }
    }

    public void SetHeader(string header) {
        //nothing to do.. yet
    }

    public void SaveLogs() {
        Base.Notifications.Instance.SaveLogs(Base.SceneManager.Instance.GetScene(), Base.ProjectManager.Instance.GetProject());
    }

    


    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        CalibrationManager.Instance.Recalibrate(showNotification:true);
#endif
    }

}
