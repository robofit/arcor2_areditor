using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;
using System.Collections.Generic;


public class MainMenu : MonoBehaviour, IMenu {
    public GameObject ButtonPrefab, ServiceButtonPrefab;
    public GameObject ProjectControlButtons, ConnectionControl, ActionObjectsContent, ActionObjects,
        ProjectsList, SceneList, DomainInput, PortInput, SceneControlButtons, MainControlButtons, Services, ServicesContent, RunningProjectControls;

    public OpenProjectDialog OpenProjectDialog;
    public OpenSceneDialog OpenSceneDialog;
    public CloseProjectDialog CloseProjectDialog;
    public CloseSceneDialog CloseSceneDialog;
    public ServiceSettingsDialog ServiceSettingsDialog;
    public AutoAddObjectDialog AutoAddObjectDialog;
    public AddSerivceDialog AddNewServiceDialog;
    public AddNewObjectDialog AddNewObjectDialog;
    public NewProjectDialog NewProjectDialog;
    public NewSceneDialog NewSceneDialog;

    private GameObject debugTools;

    private Dictionary<string, ServiceButton> serviceButtons = new Dictionary<string, ServiceButton>();

    // Start is called before the first frame update
    private void Start() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnConnectingToServer += ConnectingToServer;
        Base.ActionsManager.Instance.OnServicesUpdated += ServicesUpdated;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.ActionsManager.Instance.OnServiceMetadataUpdated += ServiceMetadataUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        //Base.GameManager.Instance.OnProjectStateChanged += ProjectStateChanged;
        Base.GameManager.Instance.OnRunProject += OnOpenProjectRunning;
        Base.GameManager.Instance.OnOpenSceneEditor += OnOpenSceneEditor;
        Base.GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;
        //Base.GameManager.Instance.OnOpenMainScreen += OnOpenMainScreen;
        Base.GameManager.Instance.OnDisconnectedFromServer += OnOpenDisconnectedScreen;

        HideEverything();
        OnOpenDisconnectedScreen(this, EventArgs.Empty);
        DomainInput.GetComponent<TMPro.TMP_InputField>().text = PlayerPrefs.GetString("arserver_domain", "localhost");
        PortInput.GetComponent<TMPro.TMP_InputField>().text = PlayerPrefs.GetInt("arserver_port", 6789).ToString();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.MainMenu);

        debugTools = GameObject.FindGameObjectWithTag("debug_tools");
        if (debugTools != null)
            debugTools.SetActive(false);
    }


    private void GameStateChanged(object sender, Base.GameStateEventArgs args) {
        HideEverything();        
    }

    /*private void ProjectStateChanged(object sender, Base.ProjectStateEventArgs args) {
        if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectRunning &&
            args.Data.State == IO.Swagger.Model.ProjectState.StateEnum.Stopped) {
            HideEverything();
            OnOpenProjectEditor(this, EventArgs.Empty);
        }        
    }*/

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
        ActionObjects.SetActive(false);
        ServicesUpdated(null, new Base.ServiceEventArgs(null));
        Services.SetActive(true);
    }

    private void OnOpenProjectRunning(object sender, EventArgs eventArgs) {
        RunningProjectControls.SetActive(true);
    }

    private void OnOpenDisconnectedScreen(object sender, EventArgs eventArgs) {
        ShowConnectionControl();
    }


    private void HideEverything() {
        ProjectControlButtons.SetActive(false);
        ConnectionControl.SetActive(false);
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
        
        if (Base.ActionsManager.Instance.ServiceInScene(serviceButton.ServiceMetadata.Type)) {
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
        if (! await Base.GameManager.Instance.CloseScene(false))
            CloseSceneDialog.WindowManager.OpenWindow();
    }


    public async void ShowCloseProjectDialog(string type) {
        if (!await Base.GameManager.Instance.CloseProject(false))
            CloseProjectDialog.WindowManager.OpenWindow();
    }


    public void ShowAddObjectDialog(string type) {
        AddNewObjectDialog.Init(type);
        AddNewObjectDialog.WindowManager.OpenWindow();
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

    public void ShowConnectionControl() {
        ConnectionControl.SetActive(true);
        ConnectionControl.GetComponentInChildren<Button>().interactable = true;
    }

    public void ShowDynamicContent() {
        ActionObjects.SetActive(true);
    }

    public void HideProjectControlButtons() {
        ProjectControlButtons.SetActive(false);
    }

    public void HideConnectionControl() {
        ConnectionControl.SetActive(false);
    }

    public void HideDynamicContent() {
        ActionObjects.SetActive(false);
    }

    public void ConnectToServer() {
        PlayerPrefs.SetString("arserver_domain", DomainInput.GetComponent<TMPro.TMP_InputField>().text);
        PlayerPrefs.SetInt("arserver_port", int.Parse(PortInput.GetComponent<TMPro.TMP_InputField>().text));
        PlayerPrefs.Save();
        Base.GameManager.Instance.ConnectToSever(GetConnectionDomain(), GetConnectionPort());
    }


    public void ConnectedToServer(object sender, Base.StringEventArgs e) {

        HideConnectionControl();
        ShowProjectControlButtons();
        ShowDynamicContent();
    }

    

    public void ProjectRunning(object sender, EventArgs e) {

    }

    public string GetConnectionDomain() {
        return DomainInput.GetComponent<TMPro.TMP_InputField>().text;
    }

    public int GetConnectionPort() {
        return int.Parse(PortInput.GetComponentInChildren<TMPro.TMP_InputField>().text);
    }
    public void ShowNewObjectTypeMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.NewObjectTypeMenu);
    }

    public void ConnectingToServer(object sender, Base.StringEventArgs e) {
        ConnectionControl.GetComponentInChildren<Button>().interactable = false;
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
        Base.Notifications.Instance.ShowNotification("Project saved successfully", "");
    }


    public void UpdateScenes(object sender, EventArgs eventArgs) {
        Dropdown sceneListDropdown = SceneList.GetComponent<Dropdown>();
        sceneListDropdown.options.Clear();
        sceneListDropdown.options.Add(new Dropdown.OptionData("Create new scene"));
        sceneListDropdown.options.Add(new Dropdown.OptionData("---"));
        foreach (IO.Swagger.Model.IdDesc scene in Base.GameManager.Instance.Scenes) {
            sceneListDropdown.options.Add(new Dropdown.OptionData(scene.Id));
        }
    }


    public void RunProject() {
        Base.GameManager.Instance.RunProject();
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

    public void UpdateMenu() {
        //nothing to do..
    }

    public void SetHeader(string header) {
        //nothing to do.. yet
    }

    public void Recalibrate() {
        CalibrationManager.Instance.Recalibrate();
    }

 
}
