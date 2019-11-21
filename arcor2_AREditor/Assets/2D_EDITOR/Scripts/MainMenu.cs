using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;


public class MainMenu : MonoBehaviour {
    public GameObject ButtonPrefab, ServiceButtonPrefab;
    public GameObject ProjectControlButtons, ConnectionControl, ConnectionStatus, ActionObjectsContent, ActionObjects,
        ProjectsList, SceneList, DomainInput, PortInput, SceneControlButtons, MainControlButtons,
        AddNewObjectDialog, NewProjectDialog, NewSceneDialog, Services, ServicesContent, AddNewServiceDialog, AutoAddObjectsDialog,
        ServiceSettingsDialog, CloseSceneDialog, CloseProjectDialog, OpenSceneDialog, OpenProjectDialog; //defined in inspector


    // Start is called before the first frame update
    void Start() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnConnectingToServer += ConnectingToServer;
        Base.ServiceManager.Instance.OnServicesUpdated += ServicesUpdated;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.ServiceManager.Instance.OnServiceMetadataUpdated += ServiceMetadataUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        HideEverything();
        DisconnectedFromServer(this, EventArgs.Empty);
    }


    private void GameStateChanged(object sender, Base.GameStateEventArgs args) {
        HideEverything();
        switch (args.Data) {
            case Base.GameManager.GameStateEnum.Disconnected:
                DisconnectedFromServer(this, EventArgs.Empty);
                break;
            case Base.GameManager.GameStateEnum.MainScreen:
                SetMainScreen();
                break;
            case Base.GameManager.GameStateEnum.SceneEditor:
                SceneControlButtons.SetActive(true);
                ActionObjects.SetActive(true);
                Services.SetActive(true);
                break;
            case Base.GameManager.GameStateEnum.ProjectEditor:
                ProjectControlButtons.SetActive(true);
                ActionObjects.SetActive(false);
                Services.SetActive(true);
                break;


        }
    }
   

    private void SetMainScreen() {
        MainControlButtons.SetActive(true);
    }

    private void HideEverything() {
        ProjectControlButtons.SetActive(false);
        ConnectionControl.SetActive(false);
        ActionObjects.SetActive(false);
        SceneControlButtons.SetActive(false);
        MainControlButtons.SetActive(false);
        Services.SetActive(false);
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
            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(ActionObjectsContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = ao_name;
            btn.onClick.AddListener(() => AddObjectToScene(ao_name));
            btnGO.transform.SetAsFirstSibling();
        }

    }

    public void ServicesUpdated(object sender, EventArgs e) {
        foreach (ServiceButton serviceButton in ServicesContent.GetComponentsInChildren<ServiceButton>()) {
            if (Base.ServiceManager.Instance.ServiceInScene(serviceButton.ServiceMetadata.Type)) {
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

        foreach (IO.Swagger.Model.ServiceTypeMeta service in Base.ServiceManager.Instance.ServicesMetadata.Values) {
            GameObject serviceButton = Instantiate(ServiceButtonPrefab);
            serviceButton.transform.SetParent(ServicesContent.transform);
            serviceButton.transform.localScale = new Vector3(1, 1, 1);
            serviceButton.GetComponentInChildren<TMPro.TMP_Text>().text = service.Type;
            if (Base.ServiceManager.Instance.ServiceInScene(service.Type)) {
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

    public async void ServiceStateChanged(ServiceButton serviceButton) {
        
        if (!serviceButton.State) {
            ShowAddServiceDialog(serviceButton.ServiceMetadata.Type);
        } else {
            ShowServiceSettingsDialog(serviceButton);
            /*if (!await Base.GameManager.Instance.RemoveFromScene(type)) {
                Base.NotificationsModernUI.Instance.ShowNotification("Remove failed", "Failed to remove service from scene!");

            }*/
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
        CloseSceneDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }
    

    public void ShowCloseProjectDialog(string type) {
        CloseProjectDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }
    

    public void ShowAddObjectDialog(string type) {
        AddNewObjectDialog.GetComponent<AddNewObjectDialog>().ObjectToBeCreated = type;
        AddNewObjectDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }
    

    public void ShowAutoAddObjectDialog(string type) {
        AutoAddObjectsDialog.GetComponent<AutoAddObjectDialog>().ObjectToBeAdded = type;
        AutoAddObjectsDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowAddServiceDialog(string type) {
        AddNewServiceDialog.GetComponent<AddNewServiceDialog>().ServiceToBeAdded = type;
        AddNewServiceDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowServiceSettingsDialog(ServiceButton serviceButton) {
        ServiceSettingsDialog.GetComponent<ServiceSettings>().Type = serviceButton.ServiceMetadata.Type;
        ServiceSettingsDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowNewProjectDialog() {
        NewProjectDialog.GetComponent<ModalWindowManager>().OpenWindow();

    }

     public void ShowNewSceneDialog() {
        NewSceneDialog.GetComponent<ModalWindowManager>().OpenWindow();

    }

    public void ShowOpenProjectDialog() {
        OpenProjectDialog.GetComponent<ModalWindowManager>().OpenWindow();

    }

     public void ShowOpenSceneDialog() {
        OpenSceneDialog.GetComponent<ModalWindowManager>().OpenWindow();

    }

    public void ShowProjectControlButtons() {
        ProjectControlButtons.SetActive(true);
    }

    public void ShowConnectionControl() {
        ConnectionControl.SetActive(true);
        ConnectionControl.GetComponentInChildren<Button>().interactable = true;
    }

    public void ShowConnectionStatus() {
        ConnectionStatus.SetActive(true);
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

    public void HideConnectionStatus() {
        ConnectionStatus.SetActive(false);
    }

    public void HideDynamicContent() {
        ActionObjects.SetActive(false);
    }

    public void ConnectToServer() {
        Base.GameManager.Instance.ConnectToSever(GetConnectionDomain(), GetConnectionPort());
    }


    public void ConnectedToServer(object sender, Base.StringEventArgs e) {

        HideConnectionControl();
        ShowProjectControlButtons();
        ShowDynamicContent();
        string s = "Connected to: " + e.Data;
        Debug.Log(s);
        ConnectionStatus.GetComponentInChildren<TMPro.TMP_Text>().text = s;
    }

    public void DisconnectedFromServer(object sender, EventArgs e) {
        HideDynamicContent();
        HideProjectControlButtons();
        ShowConnectionControl();
        ConnectionStatus.GetComponentInChildren<TMPro.TMP_Text>().text = "Not connected to server";
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
        string s = "Connecting to server: " + e.Data;
        ConnectionStatus.GetComponentInChildren<TMPro.TMP_Text>().text = s;
        Debug.Log(s);
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
            GUIHelpers2D.Instance.ShowNotification("Failed to save project" + (saveProjectResponse.Messages.Count > 0 ? ": " + saveProjectResponse.Messages[0] : ""));
            return;
        }
        Base.NotificationsModernUI.Instance.ShowNotification("Project saved successfully", "");
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

    public void LoadProject() {
        Base.GameManager.Instance.OpenProject(ProjectsList.GetComponent<Dropdown>().options[ProjectsList.GetComponent<Dropdown>().value].text);
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


}
