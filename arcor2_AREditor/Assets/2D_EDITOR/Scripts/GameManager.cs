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


    public GameObject InteractiveObjects, Scene, SpawnPoint;
    public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab, ButtonPrefab;
    public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;
    private string loadedScene;
    private JSONObject projectJSON, sceneJSON;
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

    }

    private void Start() {
        loadedScene = "";
        sceneReady = false;
        ConnectionStatus = ConnectionStatusEnum.Disconnected;
    }

    // Update is called once per frame
    private void Update() {
        if (sceneJSON != null && ActionsManager.Instance.ActionsReady)
            SceneUpdated(sceneJSON);
    }

    public void UpdateScene() {
        List<InteractiveObject> list = new List<InteractiveObject>();
        list.AddRange(InteractiveObjects.transform.GetComponentsInChildren<InteractiveObject>());
        WebsocketManager.Instance.UpdateScene(list);

    }

    private void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
        switch (newState) {
            case ConnectionStatusEnum.Connected:
                MenuManager.Instance.MainMenu.GetComponent<MainMenu>().ConnectedToServer(WebsocketManager.Instance.APIDomainWS);
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
        MenuManager.Instance.MainMenu.GetComponent<MainMenu>().ConnectingToSever(WebsocketManager.Instance.GetWSURI(domain, port));
        Scene.SetActive(true);
        WebsocketManager.Instance.ConnectToServer(domain, port, true);

    }

    public void DisconnectFromSever() {
        WebsocketManager.Instance.DisconnectFromSever();
    }

    public GameObject SpawnInteractiveObject(string type, bool updateScene = true, string id = "") {
        if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
            return null;
        }
        GameObject obj;
        switch (type) {
            case "Robot":
            case "KinaliRobot":
                obj = Instantiate(RobotPrefab, InteractiveObjects.transform);
                break;
            case "Box":
                obj = Instantiate(BoxPrefab, InteractiveObjects.transform);
                break;
            case "Tester":
                obj = Instantiate(TesterPrefab, InteractiveObjects.transform);
                break;
            case "Workspace":
                obj = Instantiate(WorkspacePrefab, InteractiveObjects.transform);
                break;
            default:
                obj = Instantiate(UnknownPrefab, InteractiveObjects.transform);
                break;
        }
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        obj.transform.position = SpawnPoint.transform.position;
        obj.GetComponent<InteractiveObject>().type = type;
        if (id == "")
            obj.GetComponent<InteractiveObject>().Id = GetFreeIOName(type);
        else
            obj.GetComponent<InteractiveObject>().Id = id;


        obj.GetComponent<InteractiveObject>().ActionObjectMetadata = aom;
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
            foreach (InteractiveObject io in InteractiveObjects.GetComponentsInChildren<InteractiveObject>()) {
                if (io.Id == freeName) {
                    hasFreeName = false;
                }
            }
            if (!hasFreeName)
                freeName = ioType + i++.ToString();
        } while (!hasFreeName);

        return freeName;
    }

    public GameObject SpawnPuck(string action_id, GameObject ap, string originalIOName, bool updateProject = true) {
        foreach (InteractiveObject io in InteractiveObjects.GetComponentsInChildren<InteractiveObject>()) {
            Debug.Log(io.Id + " - " + originalIOName);
            if (io.Id == originalIOName) {
                return SpawnPuck(action_id, ap, io, updateProject);
            }
        }
        return null;
    }

    public GameObject SpawnPuck(string action_id, GameObject ap, InteractiveObject originalIO, bool updateProject = true) {
        if (!originalIO.ActionObjectMetadata.ActionsMetadata.TryGetValue(action_id, out ActionMetadata am)) {
            return null;
        }
        GameObject puck = Instantiate(PuckPrefab);

        puck.transform.SetParent(ap.transform.Find("Pucks"));
        puck.transform.position = ap.transform.position + new Vector3(0f, ap.GetComponent<ActionPoint>().PuckCounter++ * 0.8f + 1f, 0f);
        Action action = new Action(am.Name, am, originalIO, ap.GetComponent<ActionPoint>());
        puck.GetComponent<Puck>().Init(action_id, action, updateProject);

        puck.transform.localScale = new Vector3(1f, 1f, 1f);
        if (updateProject)
            UpdateProject();
        return puck;
    }

    public GameObject SpawnActionPoint(InteractiveObject interactiveObject, bool updateProject = true) {
        GameObject aP = Instantiate(ActionPointPrefab, interactiveObject.transform.Find("ActionPoints"));
        aP.transform.position = interactiveObject.transform.Find("ActionPoints").position + new Vector3(1f, 0f, 0f);
        GameObject c = Instantiate(ConnectionPrefab);
        c.transform.SetParent(ConnectionManager.Instance.transform);
        c.GetComponent<Connection>().target[0] = interactiveObject.GetComponent<RectTransform>();
        c.GetComponent<Connection>().target[1] = aP.GetComponent<RectTransform>();
        aP.GetComponent<ActionPoint>().ConnectionToIO = c.GetComponent<Connection>();
        aP.GetComponent<ActionPoint>().SetInteractiveObject(interactiveObject.gameObject);
        if (updateProject)
            UpdateProject();
        return aP;
    }

    public void SceneUpdated(JSONObject data) {
        sceneReady = false;
        sceneJSON = null;
        if (!ActionsManager.Instance.ActionsReady) {
            sceneJSON = data;
            return;
        }

        string scene_id;
        JSONObject objects;

        try {
            scene_id = data["id"].str;
            objects = data["objects"];
        } catch (NullReferenceException) {
            Debug.Log(data.ToString(true));
            Debug.Log("Parse error in SceneUpdated()");
            return;
        }

        Dictionary<string, InteractiveObject> interactiveObjects = new Dictionary<string, InteractiveObject>();
        if (loadedScene != scene_id) {
            foreach (InteractiveObject io in InteractiveObjects.transform.GetComponentsInChildren<InteractiveObject>()) {
                Destroy(io.gameObject);
            }
            loadedScene = scene_id;
        } else {
            foreach (InteractiveObject io in InteractiveObjects.transform.GetComponentsInChildren<InteractiveObject>()) {
                interactiveObjects[io.Id] = io;
            }
        }

        foreach (JSONObject iojson in objects.list) {
            string id;
            string type;
            Vector3 position;
            try {
                id = iojson["id"].str;
                type = iojson["type"].str;
                JSONObject pjson = iojson["pose"];
                if (!JSONHelper.TryGetPose(pjson, out position, out Quaternion orientation)) {
                    throw new NullReferenceException();
                }
            } catch (NullReferenceException) {
                Debug.Log("Parse error in SceneUpdated()");
                return;
            }
            if (interactiveObjects.TryGetValue(id, out InteractiveObject io)) {
                if (type != io.type) {
                    // type has changed, what now? delete object and create a new one?
                    Destroy(io.gameObject);
                    // TODO: create a new one with new type
                }
                io.gameObject.transform.localPosition = position;
            } else {
                GameObject new_io = SpawnInteractiveObject(type, false, id);
                new_io.transform.localPosition = position;

                //new_io.transform.localPosition = new Vector3(position.x, position.y, 0);
                //UpdateScene();
            }
        }
        sceneReady = true;
        if (projectJSON != null) {
            ProjectUpdated(projectJSON);
        }
    }

    public void ProjectUpdated(JSONObject data) {
        projectJSON = null;
        JSONObject objects;
        string scene_id;

        try {
            objects = data["objects"];
            scene_id = data["scene_id"].str;
        } catch (NullReferenceException) {
            Debug.Log(data);
            Debug.Log("Parse error in ProjectUpdated()");
            return;
        }

        if (scene_id != loadedScene || !sceneReady) {
            projectJSON = data;
            return;
        }

        Dictionary<string, InteractiveObject> interactiveObjects = new Dictionary<string, InteractiveObject>();
        foreach (InteractiveObject io in InteractiveObjects.transform.GetComponentsInChildren<InteractiveObject>()) {
            interactiveObjects[io.Id] = io;
        }

        Dictionary<string, string> connections = new Dictionary<string, string>();

        foreach (JSONObject iojson in objects.list) {
            string id;

            try {
                id = iojson["id"].str;
            } catch (NullReferenceException) {
                Debug.Log("Parse error in ProjectUpdated()");
                return;
            }
            if (interactiveObjects.TryGetValue(id, out InteractiveObject io)) {
                Dictionary<string, ActionPoint> actionPoints = new Dictionary<string, ActionPoint>();
                JSONObject action_points;
                foreach (ActionPoint ap in io.transform.Find("ActionPoints").GetComponentsInChildren<ActionPoint>()) {
                    actionPoints[ap.id] = ap;
                }
                string io_id;
                try {
                    io_id = iojson["id"].str;
                    action_points = iojson["action_points"];
                } catch (NullReferenceException) {
                    Debug.Log("Parse error in ProjectUpdated()");
                    return;
                }

                foreach (JSONObject apjson in action_points.list) {
                    string ap_id;
                    JSONObject actions;

                    Vector3 ap_position;
                    try {
                        ap_id = apjson["id"].str;
                        actions = apjson["actions"];
                        if (!JSONHelper.TryGetPose(apjson["pose"], out ap_position, out Quaternion orientation)) {
                            throw new NullReferenceException();
                        }
                    } catch (NullReferenceException) {
                        Debug.Log(apjson.ToString(true));
                        Debug.Log("Parse error in ProjectUpdated()");
                        return;
                    }

                    if (!actionPoints.TryGetValue(ap_id, out ActionPoint ap)) {
                        ap = SpawnActionPoint(io, false).GetComponent<ActionPoint>();
                    }

                    ap.SetScenePosition(ap_position);


                    Dictionary<string, Puck> pucks = new Dictionary<string, Puck>();

                    foreach (Puck p in ap.transform.Find("Pucks").GetComponentsInChildren<Puck>()) {
                        pucks[p.id] = p;
                    }

                    foreach (JSONObject actionjson in actions.list) {
                        Debug.Log(actionjson);
                        string puck_id, puck_type, originalIOName;
                        JSONObject inputs, outputs, parameters;
                        try {
                            puck_id = actionjson["id"].str;
                            originalIOName = actionjson["type"].str.Split('/').First();
                            puck_type = actionjson["type"].str.Split('/').Last();
                            inputs = actionjson["inputs"];
                            outputs = actionjson["outputs"];
                            parameters = actionjson["parameters"];
                        } catch (NullReferenceException) {
                            Debug.Log(actionjson.ToString(true));
                            Debug.Log("Parse error in ProjectUpdated()");
                            return;
                        }

                        if (!pucks.TryGetValue(puck_id, out Puck puck)) {
                            Debug.Log("Should spawn puck");
                            GameObject puckGO = SpawnPuck(puck_type, ap.gameObject, originalIOName, false);
                            if (puckGO == null) {
                                continue;
                            } else {
                                puck = puckGO.GetComponent<Puck>();
                            }

                        }
                        Debug.Log("got all data");
                        puck.id = puck_id;
                        Debug.Log(inputs);
                        foreach (JSONObject inputs_json in inputs) {
                            string def;
                            try {
                                def = inputs_json["default"].str;

                            } catch (NullReferenceException) {
                                Debug.Log(actionjson.ToString(true));
                                Debug.Log("Parse error in ProjectUpdated()");
                                return;
                            }
                            if (def != "start") {
                                connections[puck_id] = def;
                            }

                        }
                        foreach (JSONObject json_param in parameters) {
                            string param_id, param_type;
                            JSONObject param_value = new JSONObject(JSONObject.Type.OBJECT);
                            try {
                                param_id = json_param["id"].str;
                                param_type = json_param["type"].str;

                                switch (ActionParameterMetadata.StringToType(param_type)) {
                                    case ActionParameterMetadata.Types.Integer:
                                        param_value.AddField("value", json_param["value"].i);
                                        break;
                                    case ActionParameterMetadata.Types.String:
                                    case ActionParameterMetadata.Types.ActionPoint:
                                        param_value.AddField("value", json_param["value"].str);
                                        break;
                                    case ActionParameterMetadata.Types.Bool:
                                        param_value.AddField("value", json_param["value"].b);
                                        break;
                                }
                            } catch (NullReferenceException) {
                                Debug.Log(actionjson.ToString(true));
                                Debug.Log("Parse error in ProjectUpdated()");
                                continue;
                            }
                            if (!puck.Action.Parameters.TryGetValue(param_id, out ActionParameter parameter))
                                continue;
                            parameter.Value = param_value;
                        }


                    }

                }

            } else {
                //unknown object
                continue;
            }


        }
        Debug.Log(connections.Count);
        foreach (KeyValuePair<string, string> connection in connections) {
            PuckInput input = FindPuck(connection.Key).transform.GetComponentInChildren<PuckInput>();
            PuckOutput output = FindPuck(connection.Value).transform.GetComponentInChildren<PuckOutput>();
            GameObject c = Instantiate(ConnectionPrefab);
            c.transform.SetParent(ConnectionManager.Instance.transform);
            c.GetComponent<Connection>().target[0] = input.gameObject.GetComponent<RectTransform>();
            c.GetComponent<Connection>().target[1] = output.gameObject.GetComponent<RectTransform>();
            input.Connection = c.GetComponent<Connection>();
            output.Connection = c.GetComponent<Connection>();
        }

    }

    public GameObject FindPuck(string id) {

        foreach (Puck puck in InteractiveObjects.GetComponentsInChildren<Puck>()) {
            Debug.Log(puck.id);
            if (puck.id == id)
                return puck.gameObject;
        }
        return new GameObject();
    }



    public void UpdateProject() {
        List<InteractiveObject> list = new List<InteractiveObject>();
        list.AddRange(InteractiveObjects.transform.GetComponentsInChildren<InteractiveObject>());

        WebsocketManager.Instance.UpdateProject(list, Scene);
    }

    public void SaveProject() {
        WebsocketManager.Instance.SaveProject();
        OnSaveProject?.Invoke(this, EventArgs.Empty);
    }

    public void LoadProject() {
        WebsocketManager.Instance.LoadProject();
        OnLoadProject?.Invoke(this, EventArgs.Empty);
    }

    public void RunProject() {
        WebsocketManager.Instance.RunProject();
        OnRunProject?.Invoke(this, EventArgs.Empty);
    }

    public void StopProject() {
        WebsocketManager.Instance.StopProject();
        OnStopProject?.Invoke(this, EventArgs.Empty);
    }

    public void PauseProject() {
        WebsocketManager.Instance.PauseProject();
        OnPauseProject?.Invoke(this, EventArgs.Empty);
    }


    public void ResumeProject() {
        WebsocketManager.Instance.ResumeProject();
        OnResumeProject?.Invoke(this, EventArgs.Empty);
    }

    public void ExitApp() {
        Application.Quit();
    }

    public void UpdateActionPointPosition(ActionPoint ap, string robotId) {
        WebsocketManager.Instance.UpdateActionPointPosition(ap.id, robotId);
    }



}
