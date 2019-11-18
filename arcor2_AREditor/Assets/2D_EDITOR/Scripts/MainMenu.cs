using UnityEngine;
using UnityEngine.UI;
using System;
using Michsky.UI.ModernUIPack;


public class MainMenu : MonoBehaviour {
    public GameObject ButtonPrefab;
    public GameObject ProjectControlButtons, ConnectionControl, ConnectionStatus, ActionObjectsContent, ActionObjects,
        ProjectsList, SceneList, DomainInput, PortInput, LoadProjectMenu, 
        AddNewObjectDialog, NewProjectDialog, NewSceneDialog, Services, ServicesContent, AddNewServiceDialog, AutoAddObjectsDialog; //defined in inspector


    // Start is called before the first frame update
    void Start() {
        Base.GameManager.Instance.OnProjectsListChanged += UpdateProjects;
        
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnConnectingToServer += ConnectingToServer;
        Base.GameManager.Instance.OnDisconnectedFromServer += DisconnectedFromServer;
        Base.ActionsManager.Instance.OnActionObjectsUpdated += ActionObjectsUpdated;
        Base.ServiceManager.Instance.OnServicesUpdated += ServicesUpdated;
    }

    // Update is called once per frame
    void Update() {

    }


    void ActionObjectsUpdated(object sender, EventArgs e) {

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

        foreach (Button b in ServicesContent.GetComponentsInChildren<Button>()) {
            //if (b.gameObject.tag == "PersistentButton") {
            //    continue;
            //} else {
                Destroy(b.gameObject);
            //}

        }

        foreach (IO.Swagger.Model.ServiceMeta service in Base.ServiceManager.Instance.ServicesMetadata) {
            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(ActionObjectsContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = service.Type;
            btn.onClick.AddListener(() => ShowAddServiceDialog(service.Type));
            btnGO.transform.SetAsLastSibling();
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

    public void ShowNewProjectDialog() {
        NewProjectDialog.GetComponent<ModalWindowManager>().OpenWindow();

    }

     public void ShowNewSceneDialog() {
        NewSceneDialog.GetComponent<ModalWindowManager>().OpenWindow();

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
        LoadProjectMenu.SetActive(true);
        string s = "Connected to: " + e.Data;
        Debug.Log(s);
        ConnectionStatus.GetComponentInChildren<TMPro.TMP_Text>().text = s;
    }

    public void DisconnectedFromServer(object sender, EventArgs e) {
        HideDynamicContent();
        HideProjectControlButtons();
        ShowConnectionControl();
        LoadProjectMenu.SetActive(false);
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

    public async void SaveProject() {
        IO.Swagger.Model.SaveSceneResponse saveSceneResponse = await Base.GameManager.Instance.SaveScene();
        Debug.Log(saveSceneResponse);
        if (!saveSceneResponse.Result) {
            saveSceneResponse.Messages.ForEach(Debug.LogError);
            GUIHelpers2D.Instance.ShowNotification("Failed to save scene" + (saveSceneResponse.Messages.Count > 0 ? ": " + saveSceneResponse.Messages[0] : ""));
            return;
        }
        IO.Swagger.Model.SaveProjectResponse saveProjectResponse = await Base.GameManager.Instance.SaveProject();
        if (!saveProjectResponse.Result) {
            saveProjectResponse.Messages.ForEach(Debug.LogError);
            GUIHelpers2D.Instance.ShowNotification("Failed to save project" + (saveProjectResponse.Messages.Count > 0 ? ": " + saveProjectResponse.Messages[0] : ""));
            return;
        }
        GUIHelpers2D.Instance.ShowNotification("Scene and project was saved successfully" + (saveProjectResponse.Messages.Count > 0 ? ": " + saveProjectResponse.Messages[0] : ""));
    }

    public void UpdateProjects(object sender, EventArgs eventArgs) {
        Dropdown projectsListDropdown = ProjectsList.GetComponent<Dropdown>();
        projectsListDropdown.options.Clear();
        foreach (IO.Swagger.Model.IdDesc project in Base.GameManager.Instance.Projects) {            
            projectsListDropdown.options.Add(new Dropdown.OptionData(project.Id));
        }
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
        Base.GameManager.Instance.LoadProject(ProjectsList.GetComponent<Dropdown>().options[ProjectsList.GetComponent<Dropdown>().value].text);
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
