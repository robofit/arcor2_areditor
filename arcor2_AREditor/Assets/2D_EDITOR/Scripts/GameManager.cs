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
    public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab, ButtonPrefab;
    public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;
    private string loadedScene;
    private JSONObject projectJSON, newScene;
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
       // if (newScene != null && ActionsManager.Instance.ActionsReady)
           // SceneUpdated(newScene);
    }

    public void UpdateScene() {
        List<Base.ActionObject> list = new List<Base.ActionObject>();
        list.AddRange(ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>());
        Base.WebsocketManager.Instance.UpdateScene(list);

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

    public GameObject SpawnPuck(string action_id, Base.ActionPoint ap, Base.ActionObject actionObject, bool updateProject = true) {
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
        puck.GetComponent<Puck2D>().Init(newId, am, ap, updateProject);

        puck.transform.localScale = new Vector3(1f, 1f, 1f);
        if (updateProject)
            UpdateProject();
        return puck;
    }

    public GameObject SpawnActionPoint(Base.ActionObject ActionObject, bool updateProject = true) {
        GameObject AP = Instantiate(ActionPointPrefab, ActionObject.transform.Find("ActionPoints"));
        AP.transform.position = ActionObject.transform.Find("ActionPoints").position + new Vector3(1f, 0f, 0f);
        GameObject c = Instantiate(ConnectionPrefab);
        c.transform.SetParent(ConnectionManager.Instance.transform);
        c.GetComponent<Connection>().target[0] = ActionObject.GetComponent<RectTransform>();
        c.GetComponent<Connection>().target[1] = AP.GetComponent<RectTransform>();
        AP.GetComponent<Base.ActionPoint>().ConnectionToIO = c.GetComponent<Connection>();
        AP.GetComponent<Base.ActionPoint>().SetActionObject(ActionObject);
        if (updateProject)
            UpdateProject();
        return AP;
    }

    public void SceneUpdated(IO.Swagger.Model.Scene scene) {
        sceneReady = false;
        newScene = null;
        if (!ActionsManager.Instance.ActionsReady) {
            newScene = data;
            return;
        }


        /*
        string scene_id;
        JSONObject objects;
        try {
            scene_id = data["id"].str;
            objects = data["objects"];
        } catch (NullReferenceException e) {
            Debug.Log(data.ToString(true));
            Debug.Log("Parse error in SceneUpdated()");
            return;
        }

        Dictionary<string, Base.ActionObject> interactiveObjects = new Dictionary<string, Base.ActionObject>();
        if (loadedScene != scene_id) {
            foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
                Destroy(ao.gameObject);
            }
            loadedScene = scene_id;
        } else {
            foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
                interactiveObjects[ao.Data.Id] = ao;
            }
        }

        foreach (JSONObject aojson in objects.list) {
            string id;
            string type;
            Vector3 position;
            try {
                id = aojson["id"].str;
                type = aojson["type"].str;
                JSONObject pjson = aojson["pose"];
                if (!JSONHelper.TryGetPose(pjson, out position, out Quaternion orientation)) {
                    throw new NullReferenceException();
                }
            } catch (NullReferenceException e) {
                Debug.Log("Parse error in SceneUpdated()");
                return;
            }
            if (interactiveObjects.TryGetValue(id, out Base.ActionObject ao)) {
                if (type != ao.Data.Type) {
                    // type has changed, what now? delete object and create a new one?
                    Destroy(ao.gameObject);
                    // TODO: create a new one with new type
                }
                ao.gameObject.transform.localPosition = position;
            } else {
                GameObject new_ao = SpawnActionObject(type, false, id);
                new_ao.transform.localPosition = position;

            }
        }
        sceneReady = true;
        if (projectJSON != null) {
            ProjectUpdated(projectJSON);
        }*/
    }

    public void ProjectUpdated(JSONObject data) {
        /*projectJSON = null;
        JSONObject objects;
        string scene_id;
        try {
            objects = data["objects"];
            scene_id = data["scene_id"].str;
        } catch (NullReferenceException e) {
            Debug.Log(data);
            Debug.Log("Parse error in ProjectUpdated()");
            return;
        }

        if (scene_id != loadedScene || !sceneReady) {
            projectJSON = data;
            return;
        }

        Dictionary<string, Base.ActionObject> actionObjects = new Dictionary<string, Base.ActionObject>();
        foreach (Base.ActionObject ao in ActionObjects.transform.GetComponentsInChildren<Base.ActionObject>()) {
            actionObjects[ao.Id] = ao;
        }

        Dictionary<string, string> connections = new Dictionary<string, string>();

        foreach (JSONObject iojson in objects.list) {
            string id;

            try {
                id = iojson["id"].str;
            } catch (NullReferenceException e) {
                Debug.Log("Parse error in ProjectUpdated()");
                return;
            }
            if (actionObjects.TryGetValue(id, out Base.ActionObject ao)) {
                Dictionary<string, Base.ActionPoint> actionPoints = new Dictionary<string, Base.ActionPoint>();
                JSONObject action_points;
                foreach (Base.ActionPoint ap in ao.transform.Find("ActionPoints").GetComponentsInChildren<Base.ActionPoint>()) {
                    actionPoints[ap.Data.Id] = ap;
                }
                string io_id;
                try {
                    io_id = iojson["id"].str;
                    action_points = iojson["action_points"];
                } catch (NullReferenceException e) {
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
                    } catch (NullReferenceException e) {
                        Debug.Log(apjson.ToString(true));
                        Debug.Log("Parse error in ProjectUpdated()");
                        return;
                    }

                    if (!actionPoints.TryGetValue(ap_id, out Base.ActionPoint ap)) {
                        ap = SpawnActionPoint(ao, false).GetComponent<Base.ActionPoint>();
                    }

                    ap.SetScenePosition(ap_position);


                    Dictionary<string, Base.Action> pucks = new Dictionary<string, Base.Action>();

                    foreach (Base.Action a in ap.transform.Find("Pucks").GetComponentsInChildren<Base.Action>()) {
                        pucks[a.Data.Id] = a;
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
                        } catch (NullReferenceException e) {
                            Debug.Log(actionjson.ToString(true));
                            Debug.Log("Parse error in ProjectUpdated()");
                            return;
                        }

                        if (!pucks.TryGetValue(puck_id, out Base.Action action)) {
                            Debug.Log("Should spawn puck");
                            GameObject puckGO = SpawnPuck(puck_type, ap, ao, false);
                            if (puckGO == null) {
                                continue;
                            } else {
                                action = puckGO.GetComponent<Base.Action>();
                            }

                        }
                        Debug.Log("got all data");
                        action.Data.Id = puck_id;
                        Debug.Log(inputs);
                        foreach (JSONObject inputs_json in inputs) {
                            string def;
                            try {
                                def = inputs_json["default"].str;

                            } catch (NullReferenceException e) {
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

                                switch (Base.ActionParameterMetadata.StringToType(param_type)) {
                                    case Base.ActionParameterMetadata.Types.Integer:
                                        param_value.AddField("value", json_param["value"].i);
                                        break;
                                    case Base.ActionParameterMetadata.Types.String:
                                    case Base.ActionParameterMetadata.Types.ActionPoint:
                                        param_value.AddField("value", json_param["value"].str);
                                        break;
                                    case Base.ActionParameterMetadata.Types.Bool:
                                        param_value.AddField("value", json_param["value"].b);
                                        break;
                                }
                            } catch (NullReferenceException e) {
                                Debug.Log(actionjson.ToString(true));
                                Debug.Log("Parse error in ProjectUpdated()");
                                continue;
                            }
                            if (!action.Parameters.TryGetValue(param_id, out Base.ActionParameter parameter))
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
            Base.PuckInput input = FindPuck(connection.Key).transform.GetComponentInChildren<Base.PuckInput>();
            Base.PuckOutput output = FindPuck(connection.Value).transform.GetComponentInChildren<Base.PuckOutput>();
            GameObject c = Instantiate(ConnectionPrefab);
            c.transform.SetParent(ConnectionManager.Instance.transform);
            c.GetComponent<Connection>().target[0] = input.gameObject.GetComponent<RectTransform>();
            c.GetComponent<Connection>().target[1] = output.gameObject.GetComponent<RectTransform>();
            input.Connection = c.GetComponent<Connection>();
            output.Connection = c.GetComponent<Connection>();
        }*/

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

        Base.WebsocketManager.Instance.UpdateProject(list, Scene);
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

    public void UpdateActionPointPosition(Base.ActionPoint ap, string robotId) => Base.WebsocketManager.Instance.UpdateActionPointPosition(ap.Data.Id, robotId);



}
