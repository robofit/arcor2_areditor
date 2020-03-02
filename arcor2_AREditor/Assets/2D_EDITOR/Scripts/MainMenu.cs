using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;


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

    // Start is called before the first frame update
    void Start() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnConnectingToServer += ConnectingToServer;
        Base.ActionsManager.Instance.OnServicesUpdated += ServicesUpdated;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.ActionsManager.Instance.OnServiceMetadataUpdated += ServiceMetadataUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        Base.GameManager.Instance.OnProjectStateChanged += ProjectStateChanged;
        Base.GameManager.Instance.OnRunProject += OnOpenProjectRunning;
        Base.GameManager.Instance.OnOpenSceneEditor += OnOpenSceneEditor;
        Base.GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;
        Base.GameManager.Instance.OnOpenMainScreen += OnOpenMainScreen;
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

    private void ProjectStateChanged(object sender, Base.ProjectStateEventArgs args) {
        if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectRunning &&
            args.Data.State == IO.Swagger.Model.ProjectState.StateEnum.Stopped) {
            HideEverything();
            OnOpenProjectEditor(this, EventArgs.Empty);
        }        
    }

    private void OnOpenMainScreen(object sender, EventArgs eventArgs) {
        MainControlButtons.SetActive(true);
    }

    private void OnOpenSceneEditor(object sender, EventArgs eventArgs) {
        SceneControlButtons.SetActive(true);
        ActionObjects.SetActive(true);
        Services.SetActive(true);
    }

    private void OnOpenProjectEditor(object sender, EventArgs eventArgs) {
        ProjectControlButtons.SetActive(true);
        ActionObjects.SetActive(false);
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

    private void ActionObjectsUpdated(object sender, EventArgs e) {

        foreach (Button b in ActionObjectsContent.GetComponentsInChildren<Button>()) {
            if (b.gameObject.tag == "PersistentButton") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }
        foreach (string ao_name in Base.ActionsManager.Instance.ActionObjectMetadata.Keys) {
            if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(ao_name, out Base.ActionObjectMetadata actionObject)) {
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
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = ao_name;
            btn.onClick.AddListener(() => AddObjectToScene(ao_name));
            btnGO.transform.SetAsFirstSibling();
        }

    }

    public void ServicesUpdated(object sender, EventArgs e) {
        foreach (ServiceButton serviceButton in ServicesContent.GetComponentsInChildren<ServiceButton>()) {
            if (Base.ActionsManager.Instance.ServiceInScene(serviceButton.ServiceMetadata.Type)) {
                //checked
                serviceButton.State = true;
            } else {
                serviceButton.State = false;
            }
        }
    }

    public void ServiceMetadataUpdated(object sender, EventArgs e) {

        foreach (ServiceButton b in ServicesContent.GetComponentsInChildren<ServiceButton>()) {
            if (b.gameObject.tag == "Persistent") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }

        foreach (IO.Swagger.Model.ServiceTypeMeta service in Base.ActionsManager.Instance.ServicesMetadata.Values) {
            GameObject serviceButton = Instantiate(ServiceButtonPrefab);
            serviceButton.transform.SetParent(ServicesContent.transform);
            serviceButton.transform.localScale = new Vector3(1, 1, 1);
            serviceButton.GetComponentInChildren<TMPro.TMP_Text>().text = service.Type;
            if (Base.ActionsManager.Instance.ServiceInScene(service.Type)) {
                //checked
                serviceButton.GetComponentInChildren<ServiceButton>().State = true;
            } else {
                serviceButton.GetComponentInChildren<ServiceButton>().State = false;
            }
            serviceButton.GetComponent<ServiceButton>().ServiceMetadata = service;
            serviceButton.GetComponentInChildren<Button>().onClick.AddListener(() => ServiceStateChanged(serviceButton.GetComponent<ServiceButton>()));
            //Button btn = btnGO.GetComponent<Button>();
            //btn.GetComponentInChildren<Text>().text = service.Type;
            //btn.onClick.AddListener(() => ShowAddServiceDialog(service.Type));
            serviceButton.transform.SetAsLastSibling();
        }

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



    public void ShowCloseSceneDialog(string type) {
        CloseSceneDialog.WindowManager.OpenWindow();
    }


    public void ShowCloseProjectDialog(string type) {
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
