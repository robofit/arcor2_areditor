using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

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

    public class ProjectStateEventArgs : EventArgs {
        public IO.Swagger.Model.ProjectState Data {
            get; set;
        }

        public ProjectStateEventArgs(IO.Swagger.Model.ProjectState data) {
            Data = data;
        }
    }

    public class GameManager : Singleton<GameManager> {

        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void ProjectStateEventHandler(object sender, ProjectStateEventArgs args);

        public event EventHandler OnSaveProject;
        public event EventHandler OnLoadProject;
        public event EventHandler OnLoadScene;
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
        public event ProjectStateEventHandler OnProjectStateChanged;
        public event EventHandler OnOpenProjectEditor;
        public event EventHandler OnOpenSceneEditor;
        public event EventHandler OnOpenMainScreen;


        private GameStateEnum gameState;

        public GameObject LoadingScreen;
        public GameObject ButtonPrefab;
        public GameObject Tooltip;
        public TMPro.TextMeshProUGUI Text;
        private string loadedScene;
        private IO.Swagger.Model.Project newProject;
        public IO.Swagger.Model.Project CurrentProject = null;
        private IO.Swagger.Model.Scene newScene;
        private bool sceneReady;
        private IO.Swagger.Model.ProjectState projectState = null;

        public const string ApiVersion = "0.2.0";
        public const string ServerVersion = "0.2.0";

        public List<IO.Swagger.Model.ListProjectsResponseData> Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
        public List<IO.Swagger.Model.IdDesc> Scenes = new List<IO.Swagger.Model.IdDesc>();

        public TMPro.TMP_Text ConnectionInfo, MessageBox, EditorInfo;

        public GameObject LandingScreen, MainScreen;

        public Image GizmoOverlay;

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
            ProjectEditor,
            ProjectRunning
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

        public void SetGameState(GameStateEnum value) {
            gameState = value;
            OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));
        }

        public void SetProjectState(IO.Swagger.Model.ProjectState state) {
            projectState = state;
            OnProjectStateChanged?.Invoke(this, new ProjectStateEventArgs(state));
        }

        public IO.Swagger.Model.ProjectState GetProjectState() {
            return projectState;
        }

        private void Awake() {
            loadedScene = "";
            sceneReady = false;
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            OpenDisconnectedScreen();
        }

        private void Start() {
            Scene.Instance.gameObject.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
            OnLoadProject += ProjectLoaded;
            OnLoadScene += SceneLoaded;
            EndLoading(); // GameManager is executed after all other scripts, set in Edit | Project Settings | Script Execution Order
        }

        // Update is called once per frame
        private void Update() {
           
        }


        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    if (!await CheckVersions()) {
                        DisconnectFromSever();
                        EndLoading();
                        return;
                    }
                    
                    ConnectionInfo.text = WebsocketManager.Instance.APIDomainWS;
                    MenuManager.Instance.DisableAllMenus();
                    StartLoading();
                    Scene.Instance.gameObject.SetActive(true);
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));
                    await UpdateActionObjects();
                    await UpdateServices();

                    await Task.Run(() => ActionsManager.Instance.WaitUntilActionsReady(10000));
                    if (newScene != null) {
                        SceneUpdated(newScene);
                    }
                    if (sceneReady && CurrentProject == null) {
                        OnLoadScene?.Invoke(this, EventArgs.Empty);
                    } else if (sceneReady && CurrentProject != null && GetProjectState().State == IO.Swagger.Model.ProjectState.StateEnum.Stopped) {
                        OnLoadProject?.Invoke(this, EventArgs.Empty);
                    } else if (!sceneReady && CurrentProject == null) {
                        await OpenMainScreen();
                    } else {
                        OpenProjectRunningScreen();
                    }
                    EndLoading();
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OpenDisconnectedScreen();
                    ConnectionInfo.text = "Not connected";
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();

                    CurrentProject = null;
                    loadedScene = "";
                    ProjectUpdated(null);
                    SceneUpdated(null);
                    Scene.Instance.gameObject.SetActive(false);
                    ActionsManager.Instance.Clear();
                    break;
            }
        }

        public void StartLoading() {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.SetActive(true);
        }

        public void EndLoading() {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.SetActive(false);
        }

        private void Init() {

        }

        public async void ConnectToSever(string domain, int port) {
            StartLoading();
            OnConnectingToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.GetWSURI(domain, port)));
            if (await WebsocketManager.Instance.ConnectToServer(domain, port)) {
                try {
                    await Task.Run(() => WebsocketManager.Instance.WaitForInitData(5000));
                } catch (TimeoutException e) {
                    Notifications.Instance.ShowNotification("Connection failed", "Connected but failed to fetch required data (scene, project, projectstate)");
                    WebsocketManager.Instance.DisconnectFromSever();
                    EndLoading();
                }                
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
            EndLoading();
            MenuManager.Instance.EnableAllWindows();
        }

        public async Task UpdateActionObjects() {
            List<IO.Swagger.Model.ObjectTypeMeta> objectTypeMetas = await WebsocketManager.Instance.GetObjectTypes();
            await ActionsManager.Instance.UpdateObjects(objectTypeMetas);
        }

        public async Task UpdateServices() {
            ActionsManager.Instance.UpdateServicesMetadata(await WebsocketManager.Instance.GetServices());
        }

        /// <summary>
        /// Sends request to the server to create a new Action Object of user specified type and id. Uuid has to be generated here in the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(string type, string id = "") {
            StartLoading();
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            IO.Swagger.Model.SceneObject sceneObject = new IO.Swagger.Model.SceneObject(id: id, pose: pose, type: type, uuid: Guid.NewGuid().ToString());
            EndLoading();
            return await WebsocketManager.Instance.AddObjectToScene(sceneObject: sceneObject);
        }

        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string type) {
            return await WebsocketManager.Instance.AutoAddObjectToScene(type);
        }

        public async void AddServiceToScene(string type, string configId = "") {
            StartLoading();
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(type: type, configurationId: configId, uuid: Guid.NewGuid().ToString());
            IO.Swagger.Model.AddServiceToSceneResponse response = await WebsocketManager.Instance.AddServiceToScene(sceneService: sceneService);
            EndLoading();
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }
        }

        // SceneUpdated is called from server, when another GUI makes some change.
        public void SceneUpdated(IO.Swagger.Model.Scene scene) {
            StartLoading();
            sceneReady = false;
            newScene = null;
            if (scene == null) {
                Scene.Instance.RemoveActionObjects();
                EndLoading();
                return;
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
            sceneReady = true;
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            
            if (newProject != null) {
                ProjectUpdated(newProject);
            }
            EndLoading();
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
        public void ProjectUpdated(IO.Swagger.Model.Project project) {
            StartLoading();
            if (project == null) {
                CurrentProject = null;
                EndLoading();
                return;
            }

            if (project.SceneId != loadedScene || !sceneReady) {
                newProject = project;
                return;
            }

            newProject = null;

            CurrentProject = project;

            Scene.Instance.UpdateActionPoints(CurrentProject);
            
            EndLoading();
        }


        // UpdateProject updates opened project on the server.
        public void UpdateProject() {
            if (CurrentProject == null)
                return;
            CurrentProject.Objects.Clear();
            CurrentProject.SceneId = Scene.Instance.Data.Id;
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
                CurrentProject.Objects.Add(projectObject);
            }

            WebsocketManager.Instance.UpdateProject(CurrentProject);
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
            StartLoading();
            try {
                await WebsocketManager.Instance.OpenProject(id);
                await Task.Run(() => WaitForProjectReady(5000));
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open project failed", "Failed to load project");
            } finally {
                EndLoading();
            }
        }

        public async void OpenScene(string id) {
            IO.Swagger.Model.OpenSceneResponse response = await WebsocketManager.Instance.OpenScene(id);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }
            OnLoadScene?.Invoke(this, EventArgs.Empty);
        }

        public async void RunProject() {
            if (CurrentProject == null)
                return;
            try {
                await WebsocketManager.Instance.BuildProject(CurrentProject.Id);
                await WebsocketManager.Instance.RunProject(CurrentProject.Id);
                OpenProjectRunningScreen();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
            }
        }

        public async void StopProject() {
            try {
                await WebsocketManager.Instance.StopProject();
                OnStopProject?.Invoke(this, EventArgs.Empty);
                OpenProjectEditor();
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

        public async Task NewProject(string name, string sceneId, bool generateLogic) {
            StartLoading();
            if (name == "" || sceneId == null) {
                EndLoading();
                throw new RequestFailedException("Project name or scene id not specified");
            }
            
            IO.Swagger.Model.OpenSceneResponse openSceneResponse = await WebsocketManager.Instance.OpenScene(sceneId);
            if (!openSceneResponse.Result) {
                EndLoading();
                throw new RequestFailedException("Failed to open scene");
            }
            try {
                await Task.Run(() => WaitForSceneReady(2000));
            } catch (TimeoutException e) {
                EndLoading();
                throw new RequestFailedException("Failed to load selected scene");
            }
            IO.Swagger.Model.Project project = new IO.Swagger.Model.Project(id: name, objects: new List<IO.Swagger.Model.ProjectObject>(), sceneId: sceneId, hasLogic: generateLogic);
            WebsocketManager.Instance.UpdateProject(project);
            ProjectUpdated(project);
            OnLoadProject?.Invoke(this, EventArgs.Empty);
            EndLoading();
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
            OnLoadScene?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<IO.Swagger.Model.RemoveFromSceneResponse> RemoveFromScene(string id) {
            return await WebsocketManager.Instance.RemoveFromScene(id);
        }

        public void CloseScene() {
            loadedScene = "";
            WebsocketManager.Instance.UpdateScene(null);
            SceneUpdated(null);
            OpenMainScreen();
        }

        public void CloseProject() {
            loadedScene = "";
            WebsocketManager.Instance.UpdateProject(null);
            ProjectUpdated(null);
            CloseScene();
            OnCloseProject?.Invoke(this, EventArgs.Empty);
            OpenMainScreen();
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

        public int GetMajorVersion(string versionString) {
            Debug.Assert(versionString.Split('.').Length == 2);
            return int.Parse(versionString.Split('.')[0]);
        }

        public int GetMinorVersion(string versionString) {
            Debug.Assert(versionString.Split('.').Length == 2);
            return int.Parse(versionString.Split('.')[1]);
        }

        public int GetPatchVersion(string versionString) {
            Debug.Assert(versionString.Split('.').Length == 2);
            return int.Parse(versionString.Split('.')[2]);
        }

        public async Task<bool> CheckVersions() {
            IO.Swagger.Model.SystemInfoData systemInfo = await WebsocketManager.Instance.GetSystemInfo();
            if (systemInfo.ApiVersion == ApiVersion && systemInfo.Version == ServerVersion)
                return true;
            if (GetMajorVersion(systemInfo.ApiVersion) != GetMajorVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Incompatibile api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion);
                return false;
            }
            if (GetMajorVersion(systemInfo.Version) != GetMajorVersion(ServerVersion)) {
                Notifications.Instance.ShowNotification("Incompatibile editor and server versions", "Editor version: " + ApiVersion + ", server version: " + systemInfo.ApiVersion);
                return false;
            }
            if (GetMinorVersion(systemInfo.ApiVersion) != GetMinorVersion(ApiVersion) || GetPatchVersion(systemInfo.ApiVersion) != GetPatchVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Different api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion + ". It can casuse problem, you have been warned.");
                return true;
            }
            if (GetMinorVersion(systemInfo.Version) != GetMinorVersion(ServerVersion) || GetPatchVersion(systemInfo.Version) != GetPatchVersion(ServerVersion)) {
                Notifications.Instance.ShowNotification("Different api versions", "Editor version: " + ServerVersion + ", server version: " + systemInfo.Version + ". It can casuse problem, you have been warned.");
                return true;
            }
            return false;
        }

        public void WaitForSceneReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!sceneReady) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public void WaitForProjectReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (CurrentProject == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public async Task OpenMainScreen() {
            StartLoading();
            await LoadScenes();
            await LoadProjects();
            OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
            SetGameState(GameStateEnum.MainScreen);
            OnOpenMainScreen?.Invoke(this, EventArgs.Empty);
            EditorInfo.text = "";
            EndLoading();
        }

        public void OpenSceneEditor() {
            EditorInfo.text = "Scene: " + Scene.Instance.Data.Id;
            SetGameState(GameStateEnum.SceneEditor);
            OnOpenSceneEditor?.Invoke(this, EventArgs.Empty);
        }

        public void OpenProjectEditor() {
            EditorInfo.text = "Project: " + CurrentProject.Id;
            SetGameState(GameStateEnum.ProjectEditor);
            OnOpenProjectEditor?.Invoke(this, EventArgs.Empty);
        }

        public void OpenProjectRunningScreen() {
            EditorInfo.text = "Running: " + CurrentProject.Id;
            SetGameState(GameStateEnum.ProjectRunning);
            OnRunProject?.Invoke(this, EventArgs.Empty);            
        }

        public void OpenDisconnectedScreen() {
            SetGameState(GameStateEnum.Disconnected);
            EditorInfo.text = "";
        }

        public void ProjectLoaded(object sender, EventArgs eventArgs) {
            OpenProjectEditor();
        }

        public void SceneLoaded(object sender, EventArgs eventArgs) {
            OpenSceneEditor();
        }

        public void ActivateGizmoOverlay(bool activate) {
            GizmoOverlay.raycastTarget = activate;
        }

    }    

}
