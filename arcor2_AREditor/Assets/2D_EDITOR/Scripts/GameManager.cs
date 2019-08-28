using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Base.Singleton<GameManager> {
    public event EventHandler OnSaveProject;
    public event EventHandler OnLoadProject;
    public event EventHandler OnRunProject;
    public event EventHandler OnStopProject;
    public event EventHandler OnPauseProject;
    public event EventHandler OnResumeProject;


    public GameObject ActionObjects, Scene, SpawnPoint;
    public GameObject ConnectionPrefab, APConnectionPrefab, ActionPointPrefab, PuckPrefab, ButtonPrefab;
    public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;
    private string loadedScene;
    private IO.Swagger.Model.Project newProject, currentProject = new IO.Swagger.Model.Project();
    private IO.Swagger.Model.Scene newScene;
    private bool sceneReady;

    public enum ConnectionStatusEnum {
        Connected, Disconnected
    }

    private ConnectionStatusEnum connectionStatus;

    public ConnectionStatusEnum ConnectionStatus {
        get => connectionStatus; set {
            connectionStatus = value;
            OnConnectionStatusChanged(connectionStatus);
        }
    }

    private void Awake() {
        currentProject.Objects = new List<IO.Swagger.Model.ProjectObject>();
        currentProject.Desc = "";
        currentProject.Id = "JabloPCB";
    }

    private void Start() {
        loadedScene = "";
        sceneReady = false;
        ConnectionStatus = ConnectionStatusEnum.Disconnected;
    }

    // Update is called once per frame
    private void Update() {
        if (newScene != null && ActionsManager.Instance.ActionsReady)
            SceneUpdated(newScene);
       
    }

    public void UpdateScene() {
        Scene.GetComponent<Base.Scene>().Data.Objects.Clear();
        foreach (Base.ActionObject actionObject in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>().ToList()) {
            Scene.GetComponent<Base.Scene>().Data.Objects.Add(actionObject.Data);
        }
        Base.WebsocketManager.Instance.UpdateScene(Scene.GetComponent<Base.Scene>().Data);
    }

    private void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
        switch (newState) {
            case ConnectionStatusEnum.Connected:
                MenuManager.Instance.MainMenu.GetComponent<MainMenu>().ConnectedToServer(Base.WebsocketManager.Instance.APIDomainWS);
                break;
            case ConnectionStatusEnum.Disconnected:
                Scene.SetActive(false);
                MenuManager.Instance.MainMenu.GetComponent<MainMenu>().DisconnectedFromServer();
                break;
        }
    }

    public void ConnectToSever() {
        string domain = MenuManager.Instance.MainMenu.GetComponent<MainMenu>().GetConnectionDomain();
        int port = MenuManager.Instance.MainMenu.GetComponent<MainMenu>().GetConnectionPort();
        MenuManager.Instance.MainMenu.GetComponent<MainMenu>().ConnectingToSever(Base.WebsocketManager.Instance.GetWSURI(domain, port));
        Scene.SetActive(true);
        Base.WebsocketManager.Instance.ConnectToServer(domain, port, true);

    }

    public void DisconnectFromSever() {
        Base.WebsocketManager.Instance.DisconnectFromSever();
    }

    public GameObject SpawnActionObject(string type, bool updateScene = true, string id = "") {
        if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out Base.ActionObjectMetadata aom)) {
            return null;
        }
        GameObject obj;
        switch (type) {
            case "Robot":
            case "KinaliRobot":
                obj = Instantiate(RobotPrefab, ActionObjects.transform);
                break;
            case "Box":
                obj = Instantiate(BoxPrefab, ActionObjects.transform);
                break;
            case "Tester":
                obj = Instantiate(TesterPrefab, ActionObjects.transform);
                break;
            case "Workspace":
                obj = Instantiate(WorkspacePrefab, ActionObjects.transform);
                break;
            default:
                obj = Instantiate(UnknownPrefab, ActionObjects.transform);
                break;
        }

        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        obj.transform.position = SpawnPoint.transform.position;
        obj.GetComponent<Base.ActionObject>().Data.Type = type;
        if (id == "")
            obj.GetComponent<Base.ActionObject>().Data.Id = GetFreeIOName(type);
        else
            obj.GetComponent<Base.ActionObject>().Data.Id = id;
        //obj.GetComponent<Base.ActionObject>().Data.Pose = DataHelper.CreatePose(obj.GetComponent<Base.ActionObject>().GetScenePosition, )
        obj.GetComponent<Base.ActionObject>().SetScenePosition(transform.position);
        obj.GetComponent<Base.ActionObject>().SetSceneOrientation(transform.rotation);


        obj.GetComponent<Base.ActionObject>().ActionObjectMetadata = aom;
        if (updateScene)
            UpdateScene();
        return obj;
    }

    private string GetFreeIOName(string ioType) {
        int i = 1;
        bool hasFreeName;
        string freeName = ioType;
        do {
            hasFreeName = true;
            foreach (Base.ActionObject io in ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
                if (io.Data.Id == freeName) {
                    hasFreeName = false;
                }
            }
            if (!hasFreeName)
                freeName = ioType + i++.ToString();
        } while (!hasFreeName);

        return freeName;
    }

    public GameObject SpawnPuck(string action_id, Base.ActionPoint ap, Base.ActionObject actionObject, bool generateData, bool updateProject = true) {
        if (!actionObject.ActionObjectMetadata.ActionsMetadata.TryGetValue(action_id, out Base.ActionMetadata am)) {
            Debug.LogError("Action " + action_id + " not supported by action object " + ap.ActionObject.name);
            return null;
        }
        GameObject puck = Instantiate(PuckPrefab);
        puck.transform.SetParent(ap.transform.Find("Pucks"));
        puck.transform.position = ap.transform.position + new Vector3(0f, ap.GetComponent<Base.ActionPoint>().PuckCounter++ * 0.8f + 1f, 0f);
        const string glyphs = "0123456789";
        string newId = action_id;
        for (int j = 0; j < 4; j++) {
            newId += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
        }
        puck.GetComponent<Puck2D>().Init(newId, am, ap, actionObject, generateData, updateProject);

        puck.transform.localScale = new Vector3(1f, 1f, 1f);
        if (updateProject) {
            UpdateProject();
        }
        return puck;
    }

    public GameObject SpawnActionPoint(Base.ActionObject actionObject, bool updateProject = true) {
        GameObject AP = Instantiate(ActionPointPrefab, actionObject.transform.Find("ActionPoints"));
        AP.transform.position = actionObject.transform.Find("ActionPoints").position + new Vector3(1f, 0f, 0f);
        GameObject c = Instantiate(ConnectionPrefab);
        c.GetComponent<LineRenderer>().enabled = true;
        c.transform.SetParent(ConnectionManager.Instance.transform);
        c.GetComponent<Connection>().target[0] = actionObject.GetComponent<RectTransform>();
        c.GetComponent<Connection>().target[1] = AP.GetComponent<RectTransform>();
        AP.GetComponent<Base.ActionPoint>().ConnectionToIO = c.GetComponent<Connection>();
        AP.GetComponent<Base.ActionPoint>().SetActionObject(actionObject);
        AP.GetComponent<Base.ActionPoint>().SetScenePosition(transform.position);
        AP.GetComponent<Base.ActionPoint>().SetSceneOrientation(transform.rotation);
        if (updateProject)
            UpdateProject();
        return AP;
    }

    public void SceneUpdated(IO.Swagger.Model.Scene scene) {
        sceneReady = false;
        newScene = null;
        if (!ActionsManager.Instance.ActionsReady) {
            newScene = scene;
            return;
        }
        Dictionary<string, Base.ActionObject> actionObjects = new Dictionary<string, Base.ActionObject>();
        if (loadedScene != scene.Id) {
            foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
                Destroy(ao.gameObject);
            }
            loadedScene = scene.Id;
        } else {
            foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
                actionObjects[ao.Data.Id] = ao;
            }
        }

        foreach (IO.Swagger.Model.SceneObject actionObject in scene.Objects) {
            if (actionObjects.TryGetValue(actionObject.Id, out Base.ActionObject ao)) {
                if (actionObject.Type != ao.Data.Type) {
                    // type has changed, what now? delete object and create a new one?
                    Destroy(ao.gameObject);
                    // TODO: create a new one with new type
                }

                ao.Data = actionObject;
                ao.gameObject.transform.position = ao.GetScenePosition();
                ao.gameObject.transform.rotation = DataHelper.OrientationToQuaternion(actionObject.Pose.Orientation);
            } else {
                GameObject new_ao = SpawnActionObject(actionObject.Type, false, actionObject.Id);
                new_ao.transform.localRotation = DataHelper.OrientationToQuaternion(actionObject.Pose.Orientation);
                new_ao.GetComponent<Base.ActionObject>().Data = actionObject;
                new_ao.gameObject.transform.position = new_ao.GetComponent<Base.ActionObject>().GetScenePosition();
            }
        }


        sceneReady = true;
        if (newProject != null) {
            ProjectUpdated(newProject);

        }


    }

    public void ProjectUpdated(IO.Swagger.Model.Project project) {
        if (project.SceneId != loadedScene || !sceneReady) {
            newProject = project;
            return;
        }
        
        currentProject = project;

        Dictionary<string, Base.ActionObject> actionObjects = new Dictionary<string, Base.ActionObject>();

        foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
            actionObjects[ao.Data.Id] = ao;
        }

        Dictionary<string, string> connections = new Dictionary<string, string>();

        foreach (IO.Swagger.Model.ProjectObject projectObject in currentProject.Objects) {
            if (actionObjects.TryGetValue(projectObject.Id, out Base.ActionObject actionObject)) {

                foreach (Base.ActionPoint ap in actionObject.transform.GetComponentsInChildren<Base.ActionPoint>()) {
                    ap.DeleteAP(false);
                }
                foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in projectObject.ActionPoints) {
                    GameObject actionPoint = SpawnActionPoint(actionObject, false);
                    actionPoint.GetComponent<Base.ActionPoint>().Data = DataHelper.ProjectActionPointToActionPoint(projectActionPoint);

                    actionPoint.transform.position = actionPoint.GetComponent<Base.ActionPoint>().GetScenePosition();

                    foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                        string originalIOName = projectAction.Type.Split('/').First();
                        string action_type = projectAction.Type.Split('/').Last();
                        if (actionObjects.TryGetValue(originalIOName, out Base.ActionObject originalActionObject)) {
                            GameObject action = SpawnPuck(action_type, actionPoint.GetComponent<Base.ActionPoint>(), originalActionObject, false, false);
                            action.GetComponent<Base.Action>().Data = projectAction;
                            
                            foreach (IO.Swagger.Model.ActionParameter projectActionParameter in projectAction.Parameters) {
                                if (action.GetComponent<Base.Action>().Metadata.Parameters.TryGetValue(projectActionParameter.Id, out Base.ActionParameterMetadata actionParameterMetadata)) {
                                    Base.ActionParameter actionParameter = new Base.ActionParameter {
                                        ActionParameterMetadata = actionParameterMetadata,
                                        Data = projectActionParameter
                                    };
                                    action.GetComponent<Base.Action>().Parameters.Add(actionParameter.Data.Id, actionParameter);
                                }
                            }

                            foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Inputs) {
                                if (actionIO.Default != "start") {
                                    connections[projectAction.Id] = actionIO.Default;
                                }
                                action.GetComponentInChildren<Base.PuckInput>().Data = actionIO;
                            }

                            foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                                action.GetComponentInChildren<Base.PuckOutput>().Data = actionIO;
                            }

                        }

                    }
                }


            } else {
                //object not exist? 
            }

        }
        foreach (KeyValuePair<string, string> connection in connections) {
            Base.PuckInput input = FindPuck(connection.Key).transform.GetComponentInChildren<Base.PuckInput>();
            Base.PuckOutput output = FindPuck(connection.Value).transform.GetComponentInChildren<Base.PuckOutput>();
            GameObject c = Instantiate(ConnectionPrefab);
            c.transform.SetParent(ConnectionManager.Instance.transform);
            c.GetComponent<Connection>().target[0] = input.gameObject.GetComponent<RectTransform>();
            c.GetComponent<Connection>().target[1] = output.gameObject.GetComponent<RectTransform>();
            //input.GetComponentInParent<Base.Action>().Data.
            input.Connection = c.GetComponent<Connection>();
            output.Connection = c.GetComponent<Connection>();
        }
    }

    public GameObject FindPuck(string id) {

        foreach (Base.Action action in ActionObjects.GetComponentsInChildren<Base.Action>()) {
            Debug.Log(action.Data.Id);
            if (action.Data.Id == id)
                return action.gameObject;
        }
        return new GameObject();
    }



    public void UpdateProject() {
        List<Base.ActionObject> list = new List<Base.ActionObject>();
        list.AddRange(ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>());
        currentProject.Objects.Clear();
        currentProject.SceneId = Scene.GetComponent<Base.Scene>().Data.Id;
        foreach (Base.ActionObject actionObject in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
            IO.Swagger.Model.ProjectObject projectObject = DataHelper.SceneObjectToProjectObject(actionObject.Data);
            foreach (Base.ActionPoint actionPoint in actionObject.ActionPoints.GetComponentsInChildren<Base.ActionPoint>()) {
                IO.Swagger.Model.ProjectActionPoint projectActionPoint = DataHelper.ActionPointToProjectActionPoint(actionPoint.Data);
                foreach (Base.Action action in actionPoint.GetComponentsInChildren<Base.Action>()) {
                    IO.Swagger.Model.Action projectAction = action.Data;
                    projectAction.Parameters = new List<IO.Swagger.Model.ActionParameter>();
                    foreach (Base.ActionParameter parameter in action.Parameters.Values) {
                        IO.Swagger.Model.ActionParameter projectParameter = parameter.Data;
                        projectAction.Parameters.Add(projectParameter);
                    }
                    projectAction.Inputs = new List<IO.Swagger.Model.ActionIO>();
                    projectAction.Outputs = new List<IO.Swagger.Model.ActionIO>();
                    foreach (Base.InputOutput inputOutput in action.GetComponentsInChildren<Base.InputOutput>()) {
                        if (inputOutput.GetType() == typeof(Base.PuckInput)) {
                            projectAction.Inputs.Add(inputOutput.Data);
                        } else {
                            projectAction.Outputs.Add(inputOutput.Data);
                        }
                    }

                    projectActionPoint.Actions.Add(projectAction);
                }
                projectObject.ActionPoints.Add(projectActionPoint);
            }
            currentProject.Objects.Add(projectObject);
        }


        Base.WebsocketManager.Instance.UpdateProject(currentProject);
    }

    public void SaveProject() {
        Base.WebsocketManager.Instance.SaveProject();
        OnSaveProject?.Invoke(this, EventArgs.Empty);
    }

    public void LoadProject() {
        Base.WebsocketManager.Instance.LoadProject();
        OnLoadProject?.Invoke(this, EventArgs.Empty);
    }

    public void RunProject() {
        Base.WebsocketManager.Instance.RunProject();
        OnRunProject?.Invoke(this, EventArgs.Empty);
    }

    public void StopProject() {
        Base.WebsocketManager.Instance.StopProject();
        OnStopProject?.Invoke(this, EventArgs.Empty);
    }

    public void PauseProject() {
        Base.WebsocketManager.Instance.PauseProject();
        OnPauseProject?.Invoke(this, EventArgs.Empty);
    }


    public void ResumeProject() {
        Base.WebsocketManager.Instance.ResumeProject();
        OnResumeProject?.Invoke(this, EventArgs.Empty);
    }

    public void ExitApp() => Application.Quit();

    public void UpdateActionPointPosition(Base.ActionPoint ap, string robotId, string endEffectorId) => Base.WebsocketManager.Instance.UpdateActionPointPosition(ap.Data.Id, robotId, endEffectorId);
    public void UpdateActionObjectPosition(Base.ActionObject ao, string robotId, string endEffectorId) => Base.WebsocketManager.Instance.UpdateActionObjectPosition(ao.Data.Id, robotId, endEffectorId);



}
