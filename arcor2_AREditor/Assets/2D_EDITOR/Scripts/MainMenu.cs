using UnityEngine;
using UnityEngine.UI;
using System;


public class MainMenu : MonoBehaviour {
    public GameObject InteractiveObjects, ButtonPrefab;
    public GameObject ProjectControlButtons, ConnectionControl, ConnectionStatus, ActionObjectsContent, ActionObjects,
        ProjectsList, SceneList, DomainInput, PortInput, NewProjectMenu, LoadProjectMenu, NewProjectName, RobotSystemId; //defined in inspector


    // Start is called before the first frame update
    void Start() {
        Base.GameManager.Instance.OnProjectsListChanged += UpdateProjects;
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        Base.GameManager.Instance.OnConnectingToServer += ConnectingToServer;
        Base.GameManager.Instance.OnDisconnectedFromServer += DisconnectedFromServer;
    }

    // Update is called once per frame
    void Update() {

    }


    void ActionObjectsUpdated() {

        foreach (Button b in ActionObjectsContent.GetComponentsInChildren<Button>()) {
            if (b.gameObject.tag == "PersistentButton") {
                continue;
            } else {
                Destroy(b.gameObject);
            }

        }
        foreach (string ao_name in ActionsManager.Instance.ActionObjectMetadata.Keys) {
            GameObject btnGO = Instantiate(ButtonPrefab);
            btnGO.transform.SetParent(ActionObjectsContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = ao_name;
            btn.onClick.AddListener(() => Base.GameManager.Instance.SpawnActionObject(ao_name));
            btnGO.transform.SetAsFirstSibling();
        }

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
        NewProjectMenu.SetActive(true);
        LoadProjectMenu.SetActive(true);
        string s = "Connected to: " + e.Data;
        Debug.Log(s);
        ConnectionStatus.GetComponentInChildren<Text>().text = s;
    }

    public void DisconnectedFromServer(object sender, EventArgs e) {
        HideDynamicContent();
        HideProjectControlButtons();
        ShowConnectionControl();
        NewProjectMenu.SetActive(false);
        LoadProjectMenu.SetActive(false);
        ConnectionStatus.GetComponentInChildren<Text>().text = "Not connected to server";
    }

    public string GetConnectionDomain() {
        return DomainInput.GetComponentInChildren<InputField>().text;
    }

    public int GetConnectionPort() {
        return int.Parse(PortInput.GetComponentInChildren<InputField>().text);
    }
    public void ShowNewObjectTypeMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.NewObjectTypeMenu);
    }

    public void ConnectingToServer(object sender, Base.StringEventArgs e) {
        ConnectionControl.GetComponentInChildren<Button>().interactable = false;
        string s = "Connecting to server: " + e.Data;
        ConnectionStatus.GetComponentInChildren<Text>().text = s;
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

    public void NewProject() {
        string name = NewProjectName.GetComponent<InputField>().text;
        string scene = SceneList.GetComponent<Dropdown>().options[SceneList.GetComponent<Dropdown>().value].text;
        if (SceneList.GetComponent<Dropdown>().value < 2) {
            scene = null;
        }
        string robotSystemId = RobotSystemId.GetComponent<InputField>().text;
        
        Base.GameManager.Instance.NewProject(name, scene, robotSystemId);
    }

   
}
