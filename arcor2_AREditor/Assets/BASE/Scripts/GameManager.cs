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

        public GameObject LoadingScreen;
        public GameObject ButtonPrefab;
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

        public GameStateEnum GetGameState() {
            return gameState;
        }

        public async void SetGameState(GameStateEnum value) {
            gameState = value;
            OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));
            if (gameState == GameStateEnum.MainScreen) {
                await LoadScenes();
                await LoadProjects();
            }
        }


        private void Awake() {
            loadedScene = "";
            sceneReady = false;
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            SetGameState(GameStateEnum.Disconnected);
        }

        private void Start() {
            Scene.Instance.gameObject.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
        }

        // Update is called once per frame
        private void Update() {
            if (newScene != null && ActionsManager.Instance.ActionsReady)
                SceneUpdated(newScene);
        }

        
        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    MenuManager.Instance.DisableAllMenus();
                    LoadingScreen.SetActive(true);
                    Scene.Instance.gameObject.SetActive(true);
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));                   
                    OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
                    UpdateActionObjects();                    
                    UpdateServices();
                    SetGameState(GameStateEnum.MainScreen);
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();
                    SetGameState(GameStateEnum.Disconnected);
                    currentProject = null;
                    loadedScene = "";
                    ProjectUpdated(null);
                    SceneUpdated(null);
                    Scene.Instance.gameObject.SetActive(false);
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
                Notifications.Instance.ShowNotification("Connection failed", "Failed to connect to remote server. Is it running?");
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

        /// <summary>
        /// Sends request to the server to create a new Action Object of user specified type and id. Uuid has to be generated here in the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(string type, string id = "") {
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            IO.Swagger.Model.SceneObject sceneObject = new IO.Swagger.Model.SceneObject(id: id, pose: pose, type: type, uuid: Guid.NewGuid().ToString());
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

        // SceneUpdated is called from server, when another GUI makes some change.
        public void SceneUpdated(IO.Swagger.Model.Scene scene) {
            sceneReady = false;
            newScene = null;
            if (scene == null) {
                if (GetGameState() == GameStateEnum.SceneEditor || GetGameState() == GameStateEnum.ProjectEditor) {
                    SetGameState(GameStateEnum.MainScreen);
                }
                Scene.Instance.RemoveActionObjects();
                return;
            } else if (GetGameState() != GameStateEnum.SceneEditor) {
                SetGameState(GameStateEnum.SceneEditor);
            }
            
            if (!ActionsManager.Instance.ActionsReady) {
                newScene = scene;

                return;
            }
            
            // Set current loaded swagger scene
            Scene.Instance.Data = scene;

            // if another scene was loaded, remove everything from current scene
            if (loadedScene != scene.Id) {
                Scene.Instance.RemoveActionObjects();
                loadedScene = scene.Id;
            }

            Scene.Instance.UpdateActionObjects();
            
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            sceneReady = true;
            if (newProject != null) {
                ProjectUpdated(newProject);
            }
        }


        // UpdateScene updates scene on the server.
        public void UpdateScene() {
            Scene.Instance.Data.Objects.Clear();
            foreach (ActionObject actionObject in Scene.Instance.ActionObjects.Values) {
                Scene.Instance.Data.Objects.Add(actionObject.Data);
            }
            WebsocketManager.Instance.UpdateScene(Scene.Instance.Data);
        }    

        // ProjectUpdated is called from server, when another GUI makes some changes
        public async void ProjectUpdated(IO.Swagger.Model.Project project) {
            if (project == null) {
                if (GetGameState() == GameStateEnum.ProjectEditor) {
                    SetGameState(GameStateEnum.MainScreen);
                }
                currentProject = null;
                return;
            } else if (GetGameState() != GameStateEnum.ProjectEditor) {
                SetGameState(GameStateEnum.ProjectEditor);
            }

            if (project.SceneId != loadedScene || !sceneReady) {
                newProject = project;
                return;
            }

            //HACK: close all opened windows when project is updated, to avoid missing references to objects/points etc.
            //TODO: find better solution
            MenuManager.Instance.HideAllMenus();


            newProject = null;

            currentProject = project;
           
            Scene.Instance.UpdateActionPoints(currentProject);           
        }


        // UpdateProject updates opened project on the server.
        public void UpdateProject() {
            if (currentProject == null)
                return;
            currentProject.Objects.Clear();
            currentProject.SceneId = Scene.Instance.Data.Id;
            foreach (ActionObject actionObject in Scene.Instance.ActionObjects.Values) {
                IO.Swagger.Model.ProjectObject projectObject = DataHelper.SceneObjectToProjectObject(actionObject.Data);
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    actionPoint.UpdatePositionsOfPucks();
                    IO.Swagger.Model.ProjectActionPoint projectActionPoint = actionPoint.Data;
                    projectActionPoint.Actions.Clear();
                    foreach (Action action in actionPoint.Actions.Values) {
                        IO.Swagger.Model.Action projectAction = action.Data;
                        projectAction.Parameters = new List<IO.Swagger.Model.ActionParameter>();
                        foreach (ActionParameter parameter in action.Parameters.Values) {
                            IO.Swagger.Model.ActionParameter projectParameter = parameter;
                            projectAction.Parameters.Add(projectParameter);
                        }

                        // TODO Discuss and solve multiple inputs/outputs possibility in Action (currently only 1 input and 1 output)
                        projectAction.Inputs = new List<IO.Swagger.Model.ActionIO>();
                        projectAction.Outputs = new List<IO.Swagger.Model.ActionIO>();

                        projectAction.Inputs.Add(action.Input.Data);
                        projectAction.Outputs.Add(action.Output.Data);

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
            return response;
        }

        public async Task<IO.Swagger.Model.SaveProjectResponse> SaveProject() {
            IO.Swagger.Model.SaveProjectResponse response = await WebsocketManager.Instance.SaveProject();
            OnSaveProject?.Invoke(this, EventArgs.Empty);
            return response;
        }

        public async void OpenProject(string id) {
            try {
                await WebsocketManager.Instance.OpenProject(id);
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
            }
            
        }

        public async void OpenScene(string id) {
            IO.Swagger.Model.OpenSceneResponse response = await WebsocketManager.Instance.OpenScene(id);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }            
            OnLoadProject?.Invoke(this, EventArgs.Empty);
        }

        public async void RunProject() {
            if (currentProject == null)
                return;
            try {
                await WebsocketManager.Instance.RunProject(currentProject.Id);
                OnRunProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
            }
        }

        public async void StopProject() {
            try {
                await WebsocketManager.Instance.StopProject();
                OnStopProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to stop project", ex.Message);
            }
        }

        public async void PauseProject() {
            try {
                await WebsocketManager.Instance.PauseProject();
                OnPauseProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to pause project", ex.Message);
            }
        }


        public async void ResumeProject() {
            try {
                await WebsocketManager.Instance.ResumeProject();
                OnResumeProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to resume project", ex.Message);
            }
        }


        public async void CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            try {
                await WebsocketManager.Instance.CreateNewObjectType(objectType);
                UpdateActionObjects();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to create new object type", ex.Message);
            }
        }

        public void ExitApp() => Application.Quit();

        public async void UpdateActionPointPosition(string actionPointId, string robotId, string endEffectorId, string orientationId, bool updatePosition) {
            
            try {
                await WebsocketManager.Instance.UpdateActionPointPosition(actionPointId, robotId, endEffectorId, orientationId, updatePosition);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }
        
         public async void UpdateActionPointJoints(string actionPointId, string robotId, string jointsId) {
            
            try {
                await WebsocketManager.Instance.UpdateActionPointJoints(actionPointId, robotId, jointsId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }
        
         public async void UpdateActionObjectPosition(string actionObjectId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionObjectPosition(actionObjectId, robotId, endEffectorId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action object", ex.Message);
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

        public async Task<bool> ExecuteAction(string actionId) {
            try {
                await WebsocketManager.Instance.ExecuteAction(actionId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
                return false;
            }
            return true;
        }


    }

}
