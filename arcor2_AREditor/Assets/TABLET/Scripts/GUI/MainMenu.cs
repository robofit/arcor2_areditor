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
    public GameObject ActionObjectButtonPrefab;
    public GameObject ActionObjectsContent, ActionObjects,
        RunningProjectControls;
    public GameObject PauseBtn, ResumeBtn;

    [SerializeField]
    private ButtonWithTooltip StartSceneBtn, StopSceneBtn;

    private GameObject debugTools;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private InputDialogWithToggle inputDialogWithToggle;

    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    [SerializeField]
    private AddNewActionObjectDialog addNewActionObjectDialog;

    private SimpleSideMenu menu;

    [SerializeField]
    private GameObject loadingScreen;

    private bool restoreButtons = false;


    // Start is called before the first frame update
    private void Start() {
        menu = GetComponent<SimpleSideMenu>();
        Debug.Assert(ActionObjectButtonPrefab != null);
        Debug.Assert(ActionObjectsContent != null);
        Debug.Assert(ActionObjects != null);
        Debug.Assert(RunningProjectControls != null);
        Debug.Assert(inputDialog != null);
        Debug.Assert(confirmationDialog != null);
        Debug.Assert(ResumeBtn != null);
        Debug.Assert(PauseBtn != null);
        Debug.Assert(StartSceneBtn != null);
        Debug.Assert(StopSceneBtn != null);
        Debug.Assert(loadingScreen != null);


        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        Base.GameManager.Instance.OnRunPackage += OnOpenProjectRunning;
        Base.GameManager.Instance.OnPausePackage += OnPausePackage;
        Base.GameManager.Instance.OnResumePackage += OnResumePackage;
        Base.GameManager.Instance.OnOpenSceneEditor += OnOpenSceneEditor;
        Base.SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;


        HideEverything();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.MainMenu);

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        StartSceneBtn.gameObject.SetActive(!(args.Event.State == SceneStateData.StateEnum.Started));
        StopSceneBtn.gameObject.SetActive(args.Event.State == SceneStateData.StateEnum.Started);
        if (menu.CurrentState == SimpleSideMenu.State.Open) {
            
            UpdateActionButtonState(args.Event.State == SceneStateData.StateEnum.Started);
            //_ = UpdateBuildAndSaveBtns();
        }
    }


    private void OnResumePackage(object sender, ProjectMetaEventArgs args) {
        ResumeBtn.SetActive(false);
        PauseBtn.SetActive(true);
    }

    private void OnPausePackage(object sender, ProjectMetaEventArgs args) {
        PauseBtn.SetActive(false);
        ResumeBtn.SetActive(true);
    }

    private void OnOpenProjectRunning(object sender, ProjectMetaEventArgs args) {
        RunningProjectControls.SetActive(true);
        ResumeBtn.SetActive(false);
        PauseBtn.SetActive(true);
    }

    private void GameStateChanged(object sender, Base.GameStateEventArgs args) {
        HideEverything();
    }


    private void OnOpenSceneEditor(object sender, EventArgs eventArgs) {
        ActionObjects.SetActive(true);
    }

    private void HideEverything() {
        ActionObjects.SetActive(false);
        RunningProjectControls.SetActive(false);
    }

    private void ActionObjectsUpdated(object sender, Base.StringListEventArgs eventArgs) {

        foreach (ActionObjectButton b in ActionObjectsContent.GetComponentsInChildren<ActionObjectButton>()) {
            if (b.gameObject.tag == "PersistentButton") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }
        List<ActionObjectMetadata> orderedList = Base.ActionsManager.Instance.ActionObjectMetadata.Values.ToList();
        orderedList.Sort(
            delegate (ActionObjectMetadata obj1,
            ActionObjectMetadata obj2) {
                return obj2.Type.CompareTo(obj1
                    .Type);
            }
        );
        foreach (Base.ActionObjectMetadata actionObjectMetadata in orderedList) {
            if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(actionObjectMetadata.Type, out Base.ActionObjectMetadata actionObject)) {
                if (actionObject.Abstract) {
                    continue;
                }
            } else {
                continue;
            }

            GameObject btnGO = Instantiate(ActionObjectButtonPrefab, ActionObjectsContent.transform);
            ActionObjectButton btn = btnGO.GetComponent<ActionObjectButton>();
            ButtonWithTooltip btnTooltip = btn.Button.GetComponent<ButtonWithTooltip>();
            btn.SetLabel(actionObjectMetadata.Type);
            btn.Button.onClick.AddListener(() => AddObjectToScene(actionObjectMetadata.Type));
            btn.RemoveBtn.Button.onClick.AddListener(() => ShowRemoveActionObjectDialog(actionObjectMetadata.Type));
            btn.RemoveBtn.SetInteractivity(false, "");
            btnGO.transform.SetAsFirstSibling();

            if (eventArgs.Data.Contains(actionObjectMetadata.Type)) {
                btn.GetComponent<ActionButton>().Highlight(2f);
            }
            if (SceneManager.Instance.SceneStarted)
                btnTooltip.SetInteractivity(false, "Objects could not be added when online");
            else
                btnTooltip.SetInteractivity(!actionObjectMetadata.Disabled, actionObjectMetadata.Problem);

        }

        UpdateRemoveBtns();

    }
    //TODO: remove
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();


    public async void TestWebsockets() {
        /*stopwatch.Reset();
        stopwatch.Start();
        await WebsocketManager.Instance.WriteLock("act_16eef51d091946cebd9025f6e8af8469", false);
        WebsocketManager.Instance.SaveProject(SaveProjectCb, true);
        await WebsocketManager.Instance.RemoveLogicItem("lit_6915fd7bbcb2440990036a0971945bd4ik");
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Debug.LogError("Done in " + elapsedTime);*/
    }

    private void SaveProjectCb(string _, string resp) {
        Debug.LogError(resp);
    }
    
    public void ShowRemoveActionObjectDialog(string type) {
        confirmationDialog.Open("Delete object",
                         "Are you sure you want to delete action object " + type + "?",
                         () => RemoveActionObject(type),
                         () => confirmationDialog.Close());
    }

    public async void RemoveActionObject(string type) {
        try {
            await WebsocketManager.Instance.DeleteObjectType(type);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to remove object type.", ex.Message);
            Debug.LogError(ex);
        } finally {
            confirmationDialog.Close();
        }
    }
    
    private void AddObjectToScene(string type) {
        if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out Base.ActionObjectMetadata actionObjectMetadata)) {
           /* if (actionObjectMetadata.NeedsServices.Count > 0) {
                ShowAutoAddObjectDialog(type);
            } else {*/
                ShowAddObjectDialog(type);
            //}
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", "Object type " + type + " does not exist!");
        }

    }

    


    public void ShowAddObjectDialog(string type) {
        
        if (ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata actionObjectMetadata)) {
            addNewActionObjectDialog.InitFromMetadata(actionObjectMetadata, UpdateRemoveBtns);
            addNewActionObjectDialog.Open();
        } else {
            Notifications.Instance.SaveLogs("Failed to load metadata for object type" + type);
        }

        
    }    

   public void ShowDynamicContent() {
        ActionObjects.SetActive(true);
    }


    public void HideDynamicContent() {
        ActionObjects.SetActive(false);
    }


    public void ConnectedToServer(object sender, Base.StringEventArgs e) {
        ShowDynamicContent();
    }

    public void ProjectRunning(object sender, EventArgs e) {

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

        UpdateActionButtonState();
        UpdateRemoveBtns();
        loadingScreen.SetActive(false);
    }

    private void UpdateActionButtonState(bool sceneStarted) {
        if (sceneStarted) {
            restoreButtons = true;
            foreach (ButtonWithTooltip b in ActionObjectsContent.GetComponentsInChildren<ButtonWithTooltip>()) {
                if (b.gameObject.tag == "ActionObjectButton")
                    b.SetInteractivity(false, "Objects could not be added when online.");
            }
        } else if (restoreButtons) {
            restoreButtons = false;
            ActionObjectsUpdated(this, new StringListEventArgs(new List<string>()));
        }
    }

    private void UpdateActionButtonState() {
        UpdateActionButtonState(SceneManager.Instance.SceneStarted);
    }

    public void UpdateRemoveBtns() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
            return;
        }
        foreach (ActionObjectButton b in ActionObjectsContent.GetComponentsInChildren<ActionObjectButton>()) {
            if (b == null || b.RemoveBtn == null)
                return;
            WebsocketManager.Instance.DeleteObjectTypeDryRun(b.GetLabel(), UpdateRemoveBtnCallback);
        }
    }

    public void UpdateRemoveBtnCallback(string id, string data) {
        IO.Swagger.Model.DeleteObjectTypeResponse deleteObjectTypeResponse =
            JsonConvert.DeserializeObject<IO.Swagger.Model.DeleteObjectTypeResponse>(data);
        foreach (ActionObjectButton b in ActionObjectsContent.GetComponentsInChildren<ActionObjectButton>()) {
            if (b != null && b.RemoveBtn != null && deleteObjectTypeResponse != null && id == b.GetLabel())
                b.RemoveBtn.SetInteractivity(deleteObjectTypeResponse.Result,
                    deleteObjectTypeResponse.Messages != null && deleteObjectTypeResponse.Messages.Count > 0 ? deleteObjectTypeResponse.Messages[0] : "");
        }
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
