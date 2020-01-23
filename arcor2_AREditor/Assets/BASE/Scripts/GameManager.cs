using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace Base {

    public class StringEventArgs : EventArgs {
        public string Data {
            get; set;
        }

        public StringEventArgs(string data) {
            Data = data;
        }
    }

    public class GameStateEventArgs : EventArgs {
        public GameManager.GameStateEnum Data {
            get; set;
        }

        public GameStateEventArgs(GameManager.GameStateEnum data) {
            Data = data;
        }
    }

    public class GameManager : Singleton<GameManager> {

        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);

        public event EventHandler OnSaveProject;
        public event EventHandler OnLoadProject;
        public event EventHandler OnRunProject;
        public event EventHandler OnStopProject;
        public event EventHandler OnPauseProject;
        public event EventHandler OnResumeProject;
        public event EventHandler OnCloseProject;
        public event EventHandler OnProjectsListChanged;
        public event EventHandler OnSceneListChanged;
        public event StringEventHandler OnConnectedToServer;
        public event StringEventHandler OnConnectingToServer;
        public event EventHandler OnDisconnectedFromServer;
        public event EventHandler OnSceneChanged;
        public event EventHandler OnActionObjectsChanged;
        public event EventHandler OnServicesChanged;
        public event GameStateEventHandler OnGameStateChanged;

        private GameStateEnum gameState;

        

        public GameObject ActionObjects, Scene, SpawnPoint, LoadingScreen;
        public GameObject ConnectionPrefab, APConnectionPrefab, ActionPointPrefab, PuckPrefab, ButtonPrefab;
        public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;
        public GameObject Tooltip;
        public TMPro.TextMeshProUGUI Text;
        private string loadedScene;
        private IO.Swagger.Model.Project newProject, currentProject = null;
        private IO.Swagger.Model.Scene newScene;
        private bool sceneReady;

        public List<IO.Swagger.Model.ListProjectsResponseData> Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
        public List<IO.Swagger.Model.IdDesc> Scenes = new List<IO.Swagger.Model.IdDesc>();


        public bool SceneInteractable {
            get => !MenuManager.Instance.IsAnyMenuOpened();
        }

        public enum ConnectionStatusEnum {
            Connected, Disconnected, Connecting
        }

        public enum GameStateEnum {
            Disconnected,
            MainScreen,
            SceneEditor,
            ProjectEditor
        }

        private ConnectionStatusEnum connectionStatus;

        public ConnectionStatusEnum ConnectionStatus {
            get => connectionStatus; set {
                if (connectionStatus != value) {
                    connectionStatus = value;
                    OnConnectionStatusChanged(connectionStatus);
                }
            }
        }

        public GameStateEnum GameState {
            get => gameState;
            set {
                gameState = value;
                OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));
            }
        }

        private void Awake() {
            loadedScene = "";
            sceneReady = false;
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            gameState = GameStateEnum.Disconnected;
        }

        private void Start() {
            Scene.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
        }

        // Update is called once per frame
        private void Update() {
            if (newScene != null && ActionsManager.Instance.ActionsReady)
                SceneUpdated(newScene);

        }

        public void UpdateScene() {
            Scene.GetComponent<Scene>().Data.Objects.Clear();
            foreach (ActionObject actionObject in ActionObjects.transform.GetComponentsInChildren<ActionObject>().ToList()) {
                Scene.GetComponent<Scene>().Data.Objects.Add(actionObject.Data);
            }
            WebsocketManager.Instance.UpdateScene(Scene.GetComponent<Scene>().Data);
            //UpdateProject();
        }
        
        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    MenuManager.Instance.DisableAllMenus();
                    LoadingScreen.SetActive(true);
                    Scene.SetActive(true);
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));
                    LoadScenes();
                    Projects = await WebsocketManager.Instance.LoadProjects();
                    OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
                    UpdateActionObjects();                    
                    UpdateServices();
                    GameState = GameStateEnum.MainScreen;
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();
                    GameState = GameStateEnum.Disconnected;
                    currentProject = null;
                    loadedScene = "";
                    ProjectUpdated(null);
                    SceneUpdated(null);
                    Scene.SetActive(false);
                    ActionsManager.Instance.Clear();
                    break;
            }
        }

       
        public async void ConnectToSever(string domain, int port) {
            OnConnectingToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.GetWSURI(domain, port)));
            if (await WebsocketManager.Instance.ConnectToServer(domain, port)) {                
               ConnectionStatus = GameManager.ConnectionStatusEnum.Connected;
            } else {
                ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
                NotificationsModernUI.Instance.ShowNotification("Connection failed", "Failed to connect to remote server. Is it running?");
            }

        }

        public void DisconnectFromSever() {
            WebsocketManager.Instance.DisconnectFromSever();
        }

        private void OnActionsLoaded(object sender, EventArgs e) {
            LoadingScreen.SetActive(false);
            MenuManager.Instance.EnableAllWindows();
        }

        public async void UpdateActionObjects() {
            List<IO.Swagger.Model.ObjectTypeMeta> objectTypeMetas = await WebsocketManager.Instance.GetObjectTypes();
            ActionsManager.Instance.UpdateObjects(objectTypeMetas);
        }

        public async void UpdateServices() {
            ActionsManager.Instance.UpdateServicesMetadata(await WebsocketManager.Instance.GetServices());
        }

        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(string type, string id = "") {
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            IO.Swagger.Model.SceneObject sceneObject = new IO.Swagger.Model.SceneObject(id: id, pose: pose, type: type);
            return await WebsocketManager.Instance.AddObjectToScene(sceneObject: sceneObject);
        }

        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string type) {
            //IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            //IO.Swagger.Model.SceneObject sceneObject = new IO.Swagger.Model.SceneObject(id: id, pose: pose, type: type);
            return await WebsocketManager.Instance.AutoAddObjectToScene(type);
        }

        public async void AddServiceToScene(string type, string configId = "") {
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(type: type, configurationId: configId);
            IO.Swagger.Model.AddServiceToSceneResponse response = await WebsocketManager.Instance.AddServiceToScene(sceneService: sceneService);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }
        }

        public GameObject SpawnActionObject(string type, bool updateScene = true, string id = "") {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                obj = Instantiate(RobotPrefab, ActionObjects.transform);
            } else {
                switch (type) {
                    case "Box":
                        obj = Instantiate(BoxPrefab, ActionObjects.transform);
                        break;
                    case "Box2":
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
            }
            

            
            //obj.transform.position = SpawnPoint.transform.position;
            obj.GetComponentInChildren<ActionObject>().Data.Type = type;
            if (id == "")
                obj.GetComponentInChildren<ActionObject>().Data.Id = GetFreeIOName(type);
            else
                obj.GetComponentInChildren<ActionObject>().Data.Id = id;
            obj.GetComponentInChildren<ActionObject>().SetScenePosition(obj.transform.localPosition);
            obj.GetComponentInChildren<ActionObject>().SetSceneOrientation(obj.transform.localRotation);


            obj.GetComponentInChildren<ActionObject>().ActionObjectMetadata = aom;
            if (aom.Robot) {
                obj.GetComponentInChildren<ActionObject>().LoadEndEffectors();
            }
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
                foreach (ActionObject io in ActionObjects.GetComponentsInChildren<ActionObject>()) {
                    if (io.Data.Id == freeName) {
                        hasFreeName = false;
                    }
                }
                if (!hasFreeName)
                    freeName = ioType + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public GameObject SpawnPuck(string action_id, ActionPoint ap, bool generateData, IActionProvider actionProvider, bool updateProject = true, string puck_id = "") {
            ActionMetadata actionMetadata;

            try {
                actionMetadata = actionProvider.GetActionMetadata(action_id);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                return null; //TODO: throw exception
            }

            if (actionMetadata == null) {
                Debug.LogError("Actions not ready");
                return null; //TODO: throw exception
            }            
            
            GameObject puck = Instantiate(PuckPrefab, ap.Actions.transform);
            const string glyphs = "0123456789";
            string newId = puck_id;
            if (newId == "") {
                newId = action_id;
                for (int j = 0; j < 4; j++) {
                    newId += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
                }
            }
            puck.GetComponent<Action>().Init(newId, actionMetadata, ap, generateData, actionProvider, false);

            puck.transform.localScale = new Vector3(1f, 1f, 1f);
            if (updateProject) {
                UpdateProject();
            }
            return puck;
        }

        public GameObject SpawnActionPoint(ActionObject actionObject, IO.Swagger.Model.ActionPoint apData, bool updateProject = true) {
            GameObject AP = Instantiate(ActionPointPrefab, actionObject.transform.Find("ActionPoints"));
            AP.transform.localPosition = new Vector3(0, 0, 0);   
            AP.transform.localScale = new Vector3(1f, 1f, 1f);

            //GameObject c = Instantiate(ConnectionPrefab);
            //c.GetComponent<LineRenderer>().enabled = true;
            //c.transform.SetParent(ConnectionManager.Instance.transform);
            //c.GetComponent<Connection>().target[0] = actionObject.GetComponent<RectTransform>();
            //c.GetComponent<Connection>().target[1] = AP.GetComponent<RectTransform>();
            //AP.GetComponent<ActionPoint>().ConnectionToIO = c.GetComponent<Connection>();

            AP.GetComponent<ActionPoint>().InitAP(actionObject, apData);
            if (apData == null) {
                AP.GetComponent<ActionPoint>().SetScenePosition(transform.localPosition);
                AP.GetComponent<ActionPoint>().SetSceneOrientation(transform.rotation);
            }
            if (updateProject)
                UpdateProject();
            return AP;
        }

        public void SceneUpdated(IO.Swagger.Model.Scene scene) {
            sceneReady = false;
            newScene = null;
            if (scene == null) {
                if (GameState == GameStateEnum.SceneEditor || GameState == GameStateEnum.ProjectEditor) {
                    GameState = GameStateEnum.MainScreen;
                }
                foreach (ActionObject ao in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
                    //TODO probably replace with something more convenient
                    Base.Scene.Instance.ActionObjects.Remove(ao.gameObject);

                    Destroy(ao.gameObject);
                }
                return;
            } else if (GameState != GameStateEnum.SceneEditor) {
                GameState = GameStateEnum.SceneEditor;
            }
            
            if (!ActionsManager.Instance.ActionsReady) {
                newScene = scene;

                return;
            }
            

            Scene.GetComponent<Scene>().Data = scene;
            Dictionary<string, ActionObject> actionObjects = new Dictionary<string, ActionObject>();
            if (loadedScene != scene.Id) {
                foreach (ActionObject ao in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
                    //TODO probably replace with something more convenient
                    Base.Scene.Instance.ActionObjects.Remove(ao.gameObject);

                    Destroy(ao.gameObject);
                }
                loadedScene = scene.Id;
            } else {
                foreach (ActionObject ao in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
                    actionObjects[ao.Data.Id] = ao;
                }
            }

            foreach (IO.Swagger.Model.SceneObject actionObject in scene.Objects) {
                if (actionObjects.TryGetValue(actionObject.Id, out ActionObject ao)) {
                    if (actionObject.Type != ao.Data.Type) {
                        //TODO probably replace with something more convenient
                        Base.Scene.Instance.ActionObjects.Remove(ao.gameObject);


                        // type has changed, what now? delete object and create a new one?
                        Destroy(ao.gameObject);
                        // TODO: create a new one with new type
                    }

                    ao.Data = actionObject;
                    ao.gameObject.transform.localPosition = ao.GetScenePosition();
                    ao.gameObject.transform.localRotation = ao.GetSceneOrientation();
                    actionObjects.Remove(actionObject.Id);
                } else {
                    GameObject new_ao = SpawnActionObject(actionObject.Type, false, actionObject.Id);
                    new_ao.GetComponentInChildren<ActionObject>().Data = actionObject;
                    new_ao.transform.localRotation = new_ao.GetComponentInChildren<ActionObject>().GetSceneOrientation();
                    new_ao.transform.localPosition = new_ao.GetComponentInChildren<ActionObject>().GetScenePosition();

                    //TODO probably replace with something more convenient
                    Base.Scene.Instance.ActionObjects.Add(new_ao, new List<GameObject>());
                }
            }

            // remove leftovers
            foreach (ActionObject ao in actionObjects.Values) {
                //TODO probably replace with something more convenient
                Base.Scene.Instance.ActionObjects.Remove(ao.gameObject);

                Destroy(ao.gameObject);
            }

            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            sceneReady = true;
            if (newProject != null) {
                ProjectUpdated(newProject);

            }


        }


        /**
         *  TODO: create update method for all models - action object, action point, action - and call it instead of updating like this..
         *
         *
         *
         **/
        /*
       public void ProjectUpdated(IO.Swagger.Model.Project project) {
           if (project == null) {
               if (GameState == GameStateEnum.ProjectEditor) {
                   GameState = GameStateEnum.MainScreen;
               }
               currentProject = null;
               return;
           } else if (GameState != GameStateEnum.ProjectEditor) {
               GameState = GameStateEnum.ProjectEditor;
           }

           if (project.SceneId != loadedScene || !sceneReady) {
               newProject = project;
               return;
           }
           newProject = null;


           currentProject = project;

           Dictionary<string, ActionObject> actionObjects = new Dictionary<string, ActionObject>();


           foreach (ActionObject ao in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
               actionObjects[ao.Data.Id] = ao;
           }

           Dictionary<string, string> connections = new Dictionary<string, string>();

           foreach (IO.Swagger.Model.ProjectObject projectObject in currentProject.Objects) {
               if (actionObjects.TryGetValue(projectObject.Id, out ActionObject actionObject)) {

                   Dictionary<string, ActionPoint> actionPoints = new Dictionary<string, ActionPoint>();

                   foreach (ActionPoint ap in actionObject.transform.GetComponentsInChildren<ActionPoint>()) {
                       //ap.DeleteAP(false);
                       actionPoints.Add(ap.Data.Id, ap);
                   }

                   foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in projectObject.ActionPoints) {
                       if (actionPoints.TryGetValue(projectActionPoint.Id, out ActionPoint actionPoint)) {
                           actionPoint.Data = DataHelper.ProjectActionPointToActionPoint(projectActionPoint);
                       } else {
                           actionPoint = SpawnActionPoint(actionObject,
                               DataHelper.ProjectActionPointToActionPoint(projectActionPoint), false).GetComponent<ActionPoint>();
                       }

                       actionPoint.transform.localPosition = actionPoint.GetScenePosition();

                       Dictionary<string, Action> actions = new Dictionary<string, Action>();

                       foreach (Action action in actionPoint.Actions.transform.GetComponentsInChildren<Action>()) {
                           actions.Add(action.Data.Id, action);
                       }

                       foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                           string providerName = projectAction.Type.Split('/').First();
                           string action_type = projectAction.Type.Split('/').Last();
                           IActionProvider actionProvider;
                           if (actionObjects.TryGetValue(providerName, out ActionObject originalActionObject)) {
                               actionProvider = originalActionObject;
                           } else if (ActionsManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                               actionProvider = originalService;
                           } else {
                               continue; //TODO: throw exception
                           }

                           if (!actions.TryGetValue(projectAction.Id, out Action action)) {
                               action = SpawnPuck(action_type, actionPoint, false, actionProvider, false, projectAction.Id).GetComponent<Action>();
                           }
                           action.GetComponent<Action>().Data = projectAction;

                           foreach (IO.Swagger.Model.ActionParameter projectActionParameter in projectAction.Parameters) {
                               try {
                                   IO.Swagger.Model.ObjectActionArg actionMetadata = action.GetComponent<Action>().Metadata.GetParamMetadata(projectActionParameter.Id);

                                   ActionParameter actionParameter = new ActionParameter(actionMetadata, projectActionParameter);
                                   action.GetComponent<Action>().Parameters.Add(actionParameter.Id, actionParameter);
                               } catch (ItemNotFoundException ex) {
                                   Debug.LogError(ex);
                               }


                           }

                           foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Inputs) {
                               if (actionIO.Default != "start") {
                                   connections[projectAction.Id] = actionIO.Default;
                               }
                               action.GetComponentInChildren<PuckInput>().Data = actionIO;
                           }

                           foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                               action.GetComponentInChildren<PuckOutput>().Data = actionIO;
                           }



                       }
                       actionPoint.UpdatePositionsOfPucks();
                   }



               } else {
                   //object not exist? 
               }

           }
           foreach (KeyValuePair<string, string> connection in connections) {
               try {
                   PuckInput input = FindPuck(connection.Key).Input;
                   PuckOutput output = FindPuck(connection.Value).Output;
                   if (input == null || output == null) {
                       Debug.LogError("Conection does not exists");
                       continue;

                   }
                   GameObject c = Instantiate(ConnectionPrefab);
                   c.transform.SetParent(ConnectionManager.Instance.transform);
                   c.GetComponent<Connection>().target[0] = input.gameObject.GetComponent<RectTransform>();
                   c.GetComponent<Connection>().target[1] = output.gameObject.GetComponent<RectTransform>();

                   input.Connection = c.GetComponent<Connection>();
                   output.Connection = c.GetComponent<Connection>();
               } catch (KeyNotFoundException ex) {
                   Debug.LogError(ex);
               }                
           }


       } */

        public void ProjectUpdated(IO.Swagger.Model.Project project) {
            if (project == null) {
                if (GameState == GameStateEnum.ProjectEditor) {
                    GameState = GameStateEnum.MainScreen;
                }
                currentProject = null;
                return;
            } else if (GameState != GameStateEnum.ProjectEditor) {
                GameState = GameStateEnum.ProjectEditor;
            }

            if (project.SceneId != loadedScene || !sceneReady) {
                newProject = project;
                return;
            }
            newProject = null;


            currentProject = project;

            Dictionary<string, ActionObject> actionObjects = new Dictionary<string, ActionObject>();

            foreach (ActionObject ao in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
                actionObjects[ao.Data.Id] = ao;
            }

            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.ProjectObject projectObject in currentProject.Objects) {
                if (actionObjects.TryGetValue(projectObject.Id, out ActionObject actionObject)) {

                    foreach (ActionPoint ap in actionObject.transform.GetComponentsInChildren<ActionPoint>()) {
                        ap.DeleteAP(false);
                    }
                    foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in projectObject.ActionPoints) {
                        GameObject actionPoint = SpawnActionPoint(actionObject, DataHelper.ProjectActionPointToActionPoint(projectActionPoint), false);
                        actionPoint.transform.localPosition = actionPoint.GetComponent<ActionPoint>().GetScenePosition();

                        //TODO probably replace with something more convenient
                        Base.Scene.Instance.ActionObjects[actionObject.gameObject].Add(actionPoint);

                        foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                            string providerName = projectAction.Type.Split('/').First();
                            string action_type = projectAction.Type.Split('/').Last();
                            IActionProvider actionProvider;
                            if (actionObjects.TryGetValue(providerName, out ActionObject originalActionObject)) {
                                actionProvider = originalActionObject;
                            } else if (ActionsManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                                actionProvider = originalService;
                            } else {
                                continue; //TODO: throw exception
                            }
                            GameObject action = SpawnPuck(action_type, actionPoint.GetComponent<ActionPoint>(), false, actionProvider, false, projectAction.Id);
                            action.GetComponent<Action>().Data = projectAction;

                            foreach (IO.Swagger.Model.ActionParameter projectActionParameter in projectAction.Parameters) {
                                try {
                                    IO.Swagger.Model.ActionParameterMeta actionMetadata = action.GetComponent<Action>().Metadata.GetParamMetadata(projectActionParameter.Id);

                                    ActionParameter actionParameter = new ActionParameter(actionMetadata, projectActionParameter);
                                    action.GetComponent<Action>().Parameters.Add(actionParameter.Id, actionParameter);
                                } catch (ItemNotFoundException ex) {
                                    Debug.LogError(ex);
                                }


                            }

                            foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Inputs) {
                                if (actionIO.Default != "start") {
                                    connections[projectAction.Id] = actionIO.Default;
                                }
                                action.GetComponentInChildren<PuckInput>().Data = actionIO;
                            }

                            foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                                action.GetComponentInChildren<PuckOutput>().Data = actionIO;
                            }



                        }
                        actionPoint.GetComponent<ActionPoint>().UpdatePositionsOfPucks();
                    }



                } else {
                    //object not exist? 
                }

            }
            foreach (KeyValuePair<string, string> connection in connections) {
                try {
                    PuckInput input = FindPuck(connection.Key).Input;
                    PuckOutput output = FindPuck(connection.Value).Output;
                    if (input == null || output == null) {
                        Debug.LogError("Conection does not exists");
                        continue;

                    }
                    GameObject c = Instantiate(ConnectionPrefab);
                    c.transform.SetParent(ConnectionManager.instance.transform);
                    // We are always connecting output to input.
                    c.GetComponent<Connection>().target[0] = output.gameObject.GetComponent<RectTransform>();
                    c.GetComponent<Connection>().target[1] = input.gameObject.GetComponent<RectTransform>();

                    input.Connection = c.GetComponent<Connection>();
                    output.Connection = c.GetComponent<Connection>();
                    ConnectionManagerArcoro.Instance.Connections.Add(c.GetComponent<Connection>());
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }
            }


        }


        public Action FindPuck(string id) {            
            foreach (Action action in ActionObjects.GetComponentsInChildren<Action>()) {
                if (action.Data.Id == id)
                    return action;
            }
            throw new KeyNotFoundException("Action " + id + " not found!");
        }



        public void UpdateProject() {
            List<ActionObject> list = new List<ActionObject>();
            list.AddRange(ActionObjects.transform.GetComponentsInChildren<ActionObject>());
            if (currentProject == null)
                return;
            currentProject.Objects.Clear();
            currentProject.SceneId = Scene.GetComponent<Scene>().Data.Id;
            foreach (ActionObject actionObject in ActionObjects.transform.GetComponentsInChildren<ActionObject>()) {
                IO.Swagger.Model.ProjectObject projectObject = DataHelper.SceneObjectToProjectObject(actionObject.Data);
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.GetComponentsInChildren<ActionPoint>()) {
                    actionPoint.UpdatePositionsOfPucks();
                    IO.Swagger.Model.ProjectActionPoint projectActionPoint = DataHelper.ActionPointToProjectActionPoint(actionPoint.Data);
                    foreach (Action action in actionPoint.GetComponentsInChildren<Action>()) {
                        IO.Swagger.Model.Action projectAction = action.Data;
                        projectAction.Parameters = new List<IO.Swagger.Model.ActionParameter>();
                        foreach (ActionParameter parameter in action.Parameters.Values) {
                            IO.Swagger.Model.ActionParameter projectParameter = parameter;
                            projectAction.Parameters.Add(projectParameter);
                        }
                        projectAction.Inputs = new List<IO.Swagger.Model.ActionIO>();
                        projectAction.Outputs = new List<IO.Swagger.Model.ActionIO>();
                        foreach (InputOutput inputOutput in action.GetComponentsInChildren<InputOutput>()) {
                            if (inputOutput.GetType() == typeof(PuckInput)) {
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


            WebsocketManager.Instance.UpdateProject(currentProject);
        }
        public async Task LoadScenes() {
            Scenes = await WebsocketManager.Instance.LoadScenes();
            OnSceneListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task LoadProjects() {
            Projects = await WebsocketManager.Instance.LoadProjects();
            OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<IO.Swagger.Model.SaveSceneResponse> SaveScene() {
            IO.Swagger.Model.SaveSceneResponse response = await WebsocketManager.Instance.SaveScene();
            LoadScenes();
            return response;
        }

        public async Task<IO.Swagger.Model.SaveProjectResponse> SaveProject() {
            IO.Swagger.Model.SaveProjectResponse response = await WebsocketManager.Instance.SaveProject();
            OnSaveProject?.Invoke(this, EventArgs.Empty);
            LoadProjects();
            return response;
        }

        public void OpenProject(string id) {
            WebsocketManager.Instance.OpenProject(id);
            OnLoadProject?.Invoke(this, EventArgs.Empty);
        }

        public async void OpenScene(string id) {
            IO.Swagger.Model.OpenSceneResponse response = await WebsocketManager.Instance.OpenScene(id);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }            
            OnLoadProject?.Invoke(this, EventArgs.Empty);
        }

        public void RunProject() {
            if (currentProject == null)
                return;
            WebsocketManager.Instance.RunProject(currentProject.Id);
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


        public async void CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            try {
                await WebsocketManager.Instance.CreateNewObjectType(objectType);
                UpdateActionObjects();
            } catch (RequestFailedException ex) {
                NotificationsModernUI.Instance.ShowNotification("Failed to create new object type", ex.Message);
            }
        }

        public void ExitApp() => Application.Quit();

        public async void UpdateActionPointPosition(string actionPointId, string robotId, string endEffectorId, string orientationId, bool updatePosition) {
            
            try {
                await WebsocketManager.Instance.UpdateActionPointPosition(actionPointId, robotId, endEffectorId, orientationId, updatePosition);
            } catch (RequestFailedException ex) {
                NotificationsModernUI.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }
        
         public async void UpdateActionPointJoints(string actionPointId, string robotId, string jointsId) {
            
            try {
                await WebsocketManager.Instance.UpdateActionPointJoints(actionPointId, robotId, jointsId);
            } catch (RequestFailedException ex) {
                NotificationsModernUI.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }
        
         public async void UpdateActionObjectPosition(string actionObjectId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionObjectPosition(actionObjectId, robotId, endEffectorId);
            } catch (RequestFailedException ex) {
                NotificationsModernUI.Instance.ShowNotification("Failed to update action object", ex.Message);
            }
        }
        
        public async Task StartObjectFocusing(string objectId, string robotId, string endEffector) {
            await WebsocketManager.Instance.StartObjectFocusing(objectId, robotId, endEffector);
        }

        public async Task SavePosition(string objectId, int pointIdx) {
            await WebsocketManager.Instance.SavePosition(objectId, pointIdx);
        }

        public async Task FocusObjectDone(string objectId) {
            await WebsocketManager.Instance.FocusObjectDone(objectId);
        } 

        public async void NewProject(string name, string sceneId, bool generateLogic) {
            LoadingScreen.SetActive(true);
            if (name == "") {
                LoadingScreen.SetActive(false);
                throw new RequestFailedException("Project name not specified");
            }
            if (sceneId == null) {
                // if no scene defined, create a new one with the name of the project
                NewScene(sceneId);
            }
            IO.Swagger.Model.OpenSceneResponse openSceneResponse = await WebsocketManager.Instance.OpenScene(sceneId);
            if (!openSceneResponse.Result) {
                LoadingScreen.SetActive(false);
                throw new RequestFailedException("Failed to open scene");
            }
            IO.Swagger.Model.Project project = new IO.Swagger.Model.Project(id: name, objects: new List<IO.Swagger.Model.ProjectObject>(), sceneId: sceneId, hasLogic: generateLogic);
            WebsocketManager.Instance.UpdateProject(project);
            ProjectUpdated(project);
            LoadingScreen.SetActive(false);
        }


        public bool NewScene(string name) {
            if (name == "") {
                return false;
            }
            foreach (IO.Swagger.Model.IdDesc idDesc in Scenes) {
                if (idDesc.Id == name)
                    return false; // scene already exist
            }
            IO.Swagger.Model.Scene scene = new IO.Swagger.Model.Scene(id: name, objects: new List<IO.Swagger.Model.SceneObject>(), services: new List<IO.Swagger.Model.SceneService>());
            WebsocketManager.Instance.UpdateScene(scene);
            SceneUpdated(scene);
            return true;
        }

        public async Task<IO.Swagger.Model.RemoveFromSceneResponse> RemoveFromScene(string id) {
            return await WebsocketManager.Instance.RemoveFromScene(id);
        }

        public void CloseScene() {
            loadedScene = "";
            WebsocketManager.Instance.UpdateScene(null);
            
            SceneUpdated(null);
        }

        public void CloseProject() {
            loadedScene = "";
            WebsocketManager.Instance.UpdateProject(null);
            ProjectUpdated(null);
            CloseScene();
            OnCloseProject?.Invoke(this, EventArgs.Empty);
        }

        public async Task<List<IO.Swagger.Model.ObjectAction>> GetActions(string name) {
            return await WebsocketManager.Instance.GetActions(name);
        }

        public async Task<List<string>> GetActionParamValues(string actionProviderId, string param_id, List<IO.Swagger.Model.IdValue> parent_params) {
            return await WebsocketManager.Instance.GetActionParamValues(actionProviderId, param_id, parent_params);
        }


    }

}
