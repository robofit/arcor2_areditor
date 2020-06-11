using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;
using System.Collections.Generic;
using Base;

public class MainMenu : MonoBehaviour, IMenu {
    public GameObject ButtonPrefab, ServiceButtonPrefab;
    public GameObject ProjectControlButtons, ActionObjectsContent, ActionObjects,
        SceneControlButtons, MainControlButtons, Services, ServicesContent, RunningProjectControls;
    public GameObject PauseBtn, ResumeBtn;

    [SerializeField]
    private ButtonWithTooltip CloseProjectBtn, CloseSceneBtn, BuildAndRunBtn, BuildBtn;

    public OpenProjectDialog OpenProjectDialog;
    public OpenSceneDialog OpenSceneDialog;
    public CloseProjectDialog CloseProjectDialog;
    public CloseSceneDialog CloseSceneDialog;
    public ServiceSettingsDialog ServiceSettingsDialog;
    public AutoAddObjectDialog AutoAddObjectDialog;
    public AddSerivceDialog AddNewServiceDialog;
    public NewProjectDialog NewProjectDialog;
    public NewSceneDialog NewSceneDialog;

    private GameObject debugTools;

    private Dictionary<string, ServiceButton> serviceButtons = new Dictionary<string, ServiceButton>();

    [SerializeField]
    private InputDialog inputDialog;

    // Start is called before the first frame update
    private void Start() {
        Debug.Assert(ButtonPrefab != null);
        Debug.Assert(ServiceButtonPrefab != null);
        Debug.Assert(ProjectControlButtons != null);
        Debug.Assert(ActionObjectsContent != null);
        Debug.Assert(ActionObjects != null);
        Debug.Assert(SceneControlButtons != null);
        Debug.Assert(MainControlButtons != null);
        Debug.Assert(Services != null);
        Debug.Assert(ServicesContent != null);
        Debug.Assert(RunningProjectControls != null);
        Debug.Assert(OpenProjectDialog != null);
        Debug.Assert(OpenSceneDialog != null);
        Debug.Assert(CloseProjectDialog != null);
        Debug.Assert(CloseSceneDialog != null);
        Debug.Assert(ServiceSettingsDialog != null);
        Debug.Assert(AutoAddObjectDialog != null);
        Debug.Assert(AddNewServiceDialog != null);
        Debug.Assert(NewProjectDialog != null);
        Debug.Assert(NewSceneDialog != null);
        Debug.Assert(inputDialog != null);
        Debug.Assert(ResumeBtn != null);
        Debug.Assert(PauseBtn != null);


        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.SceneManager.Instance.OnServicesUpdated += ServicesUpdated;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.ActionsManager.Instance.OnServiceMetadataUpdated += ServiceMetadataUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        //Base.GameManager.Instance.OnProjectStateChanged += ProjectStateChanged;
        Base.GameManager.Instance.OnRunPackage += OnOpenProjectRunning;
        Base.GameManager.Instance.OnPausePackage += OnPausePackage;
        Base.GameManager.Instance.OnResumePackage += OnResumePackage;
        Base.GameManager.Instance.OnOpenSceneEditor += OnOpenSceneEditor;
        Base.GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;
        //Base.GameManager.Instance.OnOpenMainScreen += OnOpenMainScreen;
        Base.GameManager.Instance.OnDisconnectedFromServer += OnOpenDisconnectedScreen;

        HideEverything();
        OnOpenDisconnectedScreen(this, EventArgs.Empty);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.MainMenu);

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
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

    private void OnOpenMainScreen(object sender, EventArgs eventArgs) {
        MainControlButtons.SetActive(true);
    }

    private void OnOpenSceneEditor(object sender, EventArgs eventArgs) {
        SceneControlButtons.SetActive(true);
        ActionObjects.SetActive(true);
        ServicesUpdated(null, new Base.ServiceEventArgs(null));
        Services.SetActive(true);
    }

    private void OnOpenProjectEditor(object sender, EventArgs eventArgs) {
        ProjectControlButtons.SetActive(true);
        ServicesUpdated(null, new Base.ServiceEventArgs(null));
        Services.SetActive(true);
        if (ProjectManager.Instance.Project.HasLogic) {
            BuildAndRunBtn.SetInteractivity(true);
        } else {
            BuildAndRunBtn.SetInteractivity(false, "Project without defined logic could not be run from editor");

        }
    }

    

    private void OnOpenDisconnectedScreen(object sender, EventArgs eventArgs) {
       
    }


    private void HideEverything() {
        ProjectControlButtons.SetActive(false);
        ActionObjects.SetActive(false);
        SceneControlButtons.SetActive(false);
        MainControlButtons.SetActive(false);
        Services.SetActive(false);
        RunningProjectControls.SetActive(false);
    }

    private void ActionObjectsUpdated(object sender, Base.StringEventArgs eventArgs) {

        foreach (Button b in ActionObjectsContent.GetComponentsInChildren<Button>()) {
            if (b.gameObject.tag == "PersistentButton") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }
        foreach (Base.ActionObjectMetadata actionObjectMetadata in Base.ActionsManager.Instance.ActionObjectMetadata.Values) {
            if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(actionObjectMetadata.Type, out Base.ActionObjectMetadata actionObject)) {
                if (actionObject.Abstract) {
                    continue;
                }
            } else {
                continue;
            }

            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(ActionObjectsContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = actionObjectMetadata.Type;
            btn.onClick.AddListener(() => AddObjectToScene(actionObjectMetadata.Type));
            btnGO.transform.SetAsFirstSibling();
            
            if (eventArgs.Data == actionObjectMetadata.Type) {
                btn.GetComponent<ActionButton>().Highlight(2f);
            }
            btn.interactable = !actionObjectMetadata.Disabled;

        }

    }

    public void ServicesUpdated(object sender, Base.ServiceEventArgs eventArgs) {
        if (eventArgs.Data != null) {
            if (serviceButtons.TryGetValue(eventArgs.Data.Data.Type, out ServiceButton btn)) {
                UpdateServiceButton(btn);
            }
        } else {
            foreach (ServiceButton serviceButton in serviceButtons.Values) {
                UpdateServiceButton(serviceButton);
            }
        }
        
        
    }

    private static void UpdateServiceButton(ServiceButton serviceButton) {
        serviceButton.SetInteractable(!serviceButton.ServiceMetadata.Disabled);
        
        if (Base.SceneManager.Instance.ServiceInScene(serviceButton.ServiceMetadata.Type)) {
            //checked
            serviceButton.gameObject.SetActive(true);
            serviceButton.State = true;
        } else {
            if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
                serviceButton.gameObject.SetActive(false);
            } else {
                serviceButton.gameObject.SetActive(true);
            }
            serviceButton.State = false;
        }
    }

    public void ServiceMetadataUpdated(object sender, EventArgs e) {

        foreach (ServiceButton b in serviceButtons.Values) {
            Destroy(b.gameObject);
        }

        serviceButtons.Clear();

        foreach (IO.Swagger.Model.ServiceTypeMeta service in Base.ActionsManager.Instance.ServicesMetadata.Values) {
            ServiceButton serviceButton = Instantiate(ServiceButtonPrefab).GetComponent<ServiceButton>();
            serviceButton.transform.SetParent(ServicesContent.transform);
            serviceButton.transform.localScale = new Vector3(1, 1, 1);
            serviceButton.gameObject.GetComponentInChildren<TMPro.TMP_Text>().text = service.Type;
            
            serviceButton.ServiceMetadata = service;
            serviceButton.gameObject.GetComponentInChildren<Button>().onClick.AddListener(() => ServiceStateChanged(serviceButton.GetComponent<ServiceButton>()));
            serviceButton.transform.SetAsLastSibling();
            serviceButtons.Add(service.Type, serviceButton);
        }
        ServicesUpdated(null, new Base.ServiceEventArgs(null));
    }

    public void ServiceStateChanged(ServiceButton serviceButton) {
        if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
            Base.Notifications.Instance.ShowNotification("Failed to update service", "Service state can only be changed in scene editor!");
            return;
        }
        if (!serviceButton.State) {
            ShowAddServiceDialog(serviceButton.ServiceMetadata.Type);
        } else {
            ShowServiceSettingsDialog(serviceButton);
        }
    }

    private void AddObjectToScene(string type) {
        if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out Base.ActionObjectMetadata actionObjectMetadata)) {
            if (actionObjectMetadata.NeedsServices.Count > 0) {
                ShowAutoAddObjectDialog(type);
            } else {
                ShowAddObjectDialog(type);
            }
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", "Object type " + type + " does not exist!");
        }

    }



    public async void CloseScene() {
        (bool success, string message) = await Base.GameManager.Instance.CloseScene(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            CloseSceneDialog.WindowManager.OpenWindow();
        }
    }


    public async void ShowCloseProjectDialog(string type) {
        (bool success, _) = await Base.GameManager.Instance.CloseProject(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            CloseProjectDialog.WindowManager.OpenWindow();
        }
            
    }


    public void ShowAddObjectDialog(string type) {
        inputDialog.Open("Add object of type " + type,
                         "",
                         "Object name",
                         SceneManager.Instance.GetFreeAOName(type),
                         () => AddObject(type, inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void AddObject(string type, string name) {
        if (await Base.GameManager.Instance.AddObjectToScene(type: type, name: name)) {
            inputDialog.Close();
        }
        
    }


    public void ShowAutoAddObjectDialog(string type) {
        AutoAddObjectDialog.ObjectToBeAdded = type;
        AutoAddObjectDialog.WindowManager.OpenWindow();
    }

    public void ShowAddServiceDialog(string type) {
        AddNewServiceDialog.UpdateMenu(type);
        AddNewServiceDialog.WindowManager.OpenWindow();
    }

    public void ShowServiceSettingsDialog(ServiceButton serviceButton) {
        bool sceneEditor = Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor;
        ServiceSettingsDialog.Show(serviceButton.ServiceMetadata.Type, sceneEditor);
    }

    public void ShowNewProjectDialog() {
        NewProjectDialog.WindowManager.OpenWindow();

    }

    public void ShowNewSceneDialog() {
        NewSceneDialog.WindowManager.OpenWindow();

    }

    public void ShowOpenProjectDialog() {
        OpenProjectDialog.WindowManager.OpenWindow();

    }

    public void ShowOpenSceneDialog() {
        OpenSceneDialog.WindowManager.OpenWindow();

    }

    public void ShowProjectControlButtons() {
        ProjectControlButtons.SetActive(true);
    }

    public void ShowDynamicContent() {
        ActionObjects.SetActive(true);
    }

    public void HideProjectControlButtons() {
        ProjectControlButtons.SetActive(false);
    }

    public void HideDynamicContent() {
        ActionObjects.SetActive(false);
    }

    
    public void ConnectedToServer(object sender, Base.StringEventArgs e) {
        ShowProjectControlButtons();
        ShowDynamicContent();
    }

    

    public void ProjectRunning(object sender, EventArgs e) {

    }

    public void ShowNewObjectTypeMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.NewObjectTypeMenu);
    }

    public async void SaveScene() {
        IO.Swagger.Model.SaveSceneResponse saveSceneResponse = await Base.GameManager.Instance.SaveScene();
        if (!saveSceneResponse.Result) {
            saveSceneResponse.Messages.ForEach(Debug.LogError);
            Base.NotificationsModernUI.Instance.ShowNotification("Scene save failed", saveSceneResponse.Messages.Count > 0 ? saveSceneResponse.Messages[0] : "Failed to save scene");
            return;
        }
        Base.NotificationsModernUI.Instance.ShowNotification("Scene save sucessfull", "");
    }

    public async void SaveProject() {
        IO.Swagger.Model.SaveProjectResponse saveProjectResponse = await Base.GameManager.Instance.SaveProject();
        if (!saveProjectResponse.Result) {
            saveProjectResponse.Messages.ForEach(Debug.LogError);
            Base.Notifications.Instance.ShowNotification("Failed to save project", (saveProjectResponse.Messages.Count > 0 ? ": " + saveProjectResponse.Messages[0] : ""));
            return;
        }
        UpdateMenu();
        Base.Notifications.Instance.ShowNotification("Project saved successfully", "");
    }



    public void ShowBuildPackageDialog() {
        inputDialog.Open("Build package",
                         "",
                         "Package name",
                         Base.ProjectManager.Instance.Project.Name + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"),
                         () => BuildPackage(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }
 
    public async void BuildPackage(string name) {
        try {
            await Base.GameManager.Instance.BuildPackage(name);
            inputDialog.Close();
        } catch (Base.RequestFailedException ex) {

        }
        
    }

    public void ShowBuildAndRunPackage() {
        inputDialog.Open("Build and run package",
                         "",
                         "Package name",
                         Base.ProjectManager.Instance.Project.Name + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"),
                         () => BuildAndRunPackage(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void BuildAndRunPackage(string name) {
        inputDialog.Close();
        if (await Base.GameManager.Instance.BuildAndRunPackage(name)) {



        } else {
            Base.Notifications.Instance.ShowNotification("Failed to build and run package", "");
            GameManager.Instance.HideLoadingScreen();
        }
    }

    public void TestRun() {
        Base.GameManager.Instance.TestRunProject();
    }

    public void StopProject() {
        Base.GameManager.Instance.StopProject();
    }


    public void PauseProject() {
        Base.GameManager.Instance.PauseProject();
    }

    public void ResumeProject() {
        Base.GameManager.Instance.ResumeProject();
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
        bool successForce = false;
        string messageForce = "";
        ButtonWithTooltip button = null;
        switch (GameManager.Instance.GetGameState()) {
            case GameManager.GameStateEnum.ProjectEditor:
                (bool success, _) = await GameManager.Instance.CloseProject(false, true);
                (successForce, messageForce) = await GameManager.Instance.CloseProject(true, true);
                button = CloseProjectBtn;
                if (success) {
                    BuildBtn.SetInteractivity(true);
                } else {
                    BuildBtn.SetInteractivity(false, "There are unsaved changes on project");
                }
                break;
            case GameManager.GameStateEnum.SceneEditor:
                (successForce, messageForce) = await GameManager.Instance.CloseScene(true, true);
                button = CloseSceneBtn;
                break;
        }
        if (button != null) {
            if (successForce) {
                button.SetInteractivity(true);
            } else {
                button.SetInteractivity(false, messageForce);
            }
        }
    }

    public void SetHeader(string header) {
        //nothing to do.. yet
    }

    public void SaveLogs() {
        Base.Notifications.Instance.SaveLogs(Base.SceneManager.Instance.Scene, Base.ProjectManager.Instance.Project);
    }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    public void Recalibrate() {
        CalibrationManager.Instance.Recalibrate();
    }
#endif

 
}
