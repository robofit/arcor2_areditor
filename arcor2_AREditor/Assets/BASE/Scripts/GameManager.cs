using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using IO.Swagger.Model;

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
        public IO.Swagger.Model.PackageState Data {
            get; set;
        }

        public ProjectStateEventArgs(IO.Swagger.Model.PackageState data) {
            Data = data;
        }
    }

    public class GameManager : Singleton<GameManager> {

        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void ProjectStateEventHandler(object sender, ProjectStateEventArgs args);

        public event EventHandler OnSaveProject;
        public event EventHandler OnLoadProject;
        
        public event EventHandler OnRunPackage;
        public event EventHandler OnStopProject;
        public event EventHandler OnPauseProject;
        public event EventHandler OnResumeProject;
        public event EventHandler OnCloseProject;
        public event EventHandler OnCloseScene;
        public event EventHandler OnProjectsListChanged;
        public event EventHandler OnPackagesListChanged;
        public event EventHandler OnSceneListChanged;
        public event StringEventHandler OnConnectedToServer;
        public event StringEventHandler OnConnectingToServer;
        public event EventHandler OnDisconnectedFromServer;
        public event EventHandler OnSceneChanged;
        public event EventHandler OnActionObjectsChanged;
        public event EventHandler OnActionPointsChanged;
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
        private IO.Swagger.Model.PackageState packageState = null;

        public bool ProjectChanged = false, ProjectRunning = false;

        public const string ApiVersion = "0.6.2";

        public readonly string EditorVersion = "0.6.0-alpha.1";
        public List<IO.Swagger.Model.ListProjectsResponseData> Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
        public List<IO.Swagger.Model.PackageSummary> Packages = new List<IO.Swagger.Model.PackageSummary>();
        public List<IO.Swagger.Model.IdDesc> Scenes = new List<IO.Swagger.Model.IdDesc>();

        public TMPro.TMP_Text VersionInfo, MessageBox, EditorInfo, ConnectionInfo, ServerVersion;

        public Image GizmoOverlay;

        public GameObject objectWithGizmo, Scene;

        public IO.Swagger.Model.SystemInfoData SystemInfo;
        public PackageInfo PackageInfo;

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
            PackageRunning
        }

        private ConnectionStatusEnum connectionStatus;

        private async Task Update() {
            if (ConnectionStatus != ConnectionStatusEnum.Connected)
                return;
            // new or updated scene arrived from server
            if (newScene != null) {
                if (SceneManager.Instance.Scene == null) {
                    bool success = await SceneManager.Instance.CreateScene(newScene);
                    newScene = null;
                } else if (SceneManager.Instance.Scene.Id == newScene.Id) {
                    bool success = await SceneManager.Instance.UpdateScene(newScene);
                    newScene = null;
                } else {
                    Debug.LogError("Arrived scene with different id than currently loaded scene");
                    Debug.LogError("Arrived scene: " + newScene.ToString());
                    Debug.LogError("Loaded scene: " + SceneManager.Instance.Scene.ToString());
                    SceneManager.Instance.DestroyScene();
                }
            }
        }

        public ConnectionStatusEnum ConnectionStatus {
            get => connectionStatus; set {
                if (connectionStatus != value) {                    
                    OnConnectionStatusChanged(connectionStatus);
                }
            }
        }


        //TODO: use onvalidate in all scripts to check if everything sets correctly - it allows to check in editor
        private void OnValidate() {
            Debug.Assert(LoadingScreen != null);
        }

        public GameStateEnum GetGameState() {
            return gameState;
        }

        public void SetGameState(GameStateEnum value) {
            gameState = value;
            OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));
        }

        public void SetPackageState(IO.Swagger.Model.PackageState state) {
            packageState = state;
            OnProjectStateChanged?.Invoke(this, new ProjectStateEventArgs(state));
        }

        private void ProjectStateChanged(object sender, Base.ProjectStateEventArgs args) {
            if (GetGameState() == GameStateEnum.PackageRunning &&
                args.Data.State == PackageState.StateEnum.Stopped) {
                OpenProjectEditor();
            } else if (GetGameState() == GameStateEnum.ProjectEditor &&
                args.Data.State != PackageState.StateEnum.Stopped) {
                OpenPackageRunningScreen();
            }
            if (args.Data.State != PackageState.StateEnum.Stopped)
                ProjectRunning = true;
        }

        public IO.Swagger.Model.PackageState GetPackageState() {
            return packageState;
        }

        private void Awake() {
            loadedScene = "";
            sceneReady = false;
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            OpenDisconnectedScreen();
        }

        private void Start() {
            VersionInfo.text = EditorVersion;
            Scene.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
            OnProjectStateChanged += ProjectStateChanged;
            
            OnLoadProject += ProjectLoaded;
            SceneManager.Instance.OnLoadScene += SceneLoaded;
            EndLoading(); // GameManager is executed after all other scripts, set in Edit | Project Settings | Script Execution Order
        }

        

        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            StartLoading();
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    try {
                        IO.Swagger.Model.SystemInfoData systemInfo = await WebsocketManager.Instance.GetSystemInfo();
                        if (!await CheckApiVersion(systemInfo)) {
                            throw new RequestFailedException();
                        }
                        SystemInfo = systemInfo;
                        ServerVersion.text = "Editor version: " + EditorVersion +
                        "\nServer version: " + systemInfo.Version;
                    } catch (RequestFailedException ex) {
                        DisconnectFromSever();
                        EndLoading();
                        Notifications.Instance.ShowNotification("Connection failed", "");
                        return;
                    }
                        
                    ConnectionInfo.text = WebsocketManager.Instance.APIDomainWS;
                    MenuManager.Instance.DisableAllMenus();
                    
                    
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));

                    await UpdateActionObjects();
                    await UpdateServices();


                    try {
                        await Task.Run(() => ActionsManager.Instance.WaitUntilActionsReady(15000));
                    } catch (TimeoutException e) {
                        Notifications.Instance.ShowNotification("Connection failed", "Some actions were not loaded within timeout");
                        DisconnectFromSever();
                        ActionsManager.Instance.Init();
                        return;
                    }

                    /*
                    if (newScene != null) {
                        if (!await SceneManager.Instance.CreateScene(newScene)) {
                            Notifications.Instance.ShowNotification("Failed to initialize scene", "");
                        }
                    }



                    if (!sceneReady && CurrentProject == null) {
                        await OpenMainScreen();
                    } else if (ProjectRunning) {
                        OpenPackageRunningScreen();
                    }*/
                    Scene.SetActive(true);
                    connectionStatus = newState;
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OpenDisconnectedScreen();
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();

                    CurrentProject = null;
                    loadedScene = "";
                    ProjectManager.Instance.DestroyProject();
                    SceneManager.Instance.DestroyScene();
                    Scene.SetActive(false);
                    Init();
                    connectionStatus = newState;
                    break;
            }
            EndLoading();
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

        internal void ProjectSaved() {
            ProjectChanged = false;
        }

        public void DisconnectFromSever() {
            WebsocketManager.Instance.DisconnectFromSever();
        }

        private void OnActionsLoaded(object sender, EventArgs e) {
            EndLoading();
            MenuManager.Instance.EnableAllWindows();
        }

        public async Task UpdateActionObjects(string highlighteObject = null) {
            try {
                List<IO.Swagger.Model.ObjectTypeMeta> objectTypeMetas = await WebsocketManager.Instance.GetObjectTypes();
                await ActionsManager.Instance.UpdateObjects(objectTypeMetas, highlighteObject);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, GameManager.Instance.CurrentProject, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
            
        }

        public async Task UpdateServices() {
            await ActionsManager.Instance.UpdateServicesMetadata(await WebsocketManager.Instance.GetServices());
        }

        /// <summary>
        /// Sends request to the server to create a new Action Object of user specified type and id. Id has to be generated here in the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(string type, string name) {
            StartLoading();
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
            EndLoading();
            return await WebsocketManager.Instance.AddObjectToScene(name, type, pose);
        }

        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string type) {
            return await WebsocketManager.Instance.AutoAddObjectToScene(type);
        }

        public async void AddServiceToScene(string type, string configId = "") {
            StartLoading();
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(type: type, configurationId: configId);
            try {
                await WebsocketManager.Instance.AddServiceToScene(sceneService: sceneService);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Add service failed", e.Message);
            } finally {
                EndLoading();
            }            
            
        }

        internal void ProjectAdded(Project data) {
            ProjectUpdated(data);
        }

        public void SceneAdded(IO.Swagger.Model.Scene scene) {
            //SceneUpdated(scene);
            newScene = scene;
        }

        
        public async void SceneBaseUpdated(IO.Swagger.Model.Scene scene) {
            if (GetGameState() == GameStateEnum.SceneEditor)
                SceneManager.Instance.SceneBaseUpdated(scene);
            else if (GetGameState() == GameStateEnum.MainScreen) {
                await LoadScenes();
            }
        }

        public async void ProjectBaseUpdated(Project data) {
            if (GetGameState() == GameStateEnum.ProjectEditor) {
                CurrentProject.Desc = data.Desc;
                CurrentProject.HasLogic = data.HasLogic;
                CurrentProject.Modified = data.Modified;
                CurrentProject.Name = data.Name;
            } else if (GetGameState() == GameStateEnum.MainScreen) {
                await LoadProjects();
            }
        }

        // SceneUpdated is called from server, when another GUI makes some change.
        public async Task SceneUpdated(IO.Swagger.Model.Scene scene) {
            StartLoading();
            bool sceneOpened = false;
            
            sceneReady = false;
            newScene = null;
            
            if (loadedScene != scene.Id)
                sceneOpened = true;
            if (!ActionsManager.Instance.ActionsReady) {
                newScene = scene;
                return;
            }

            // Set current loaded swagger scene
            if (SceneManager.Instance.Scene == null) {
                SceneManager.Instance.Scene = scene;
                OpenSceneEditor();
            } else {
                SceneManager.Instance.Scene = scene;
            }

            // if another scene was loaded, remove everything from current scene
            if (sceneOpened) {
                SceneManager.Instance.RemoveActionObjects();
                loadedScene = scene.Id;
                SceneManager.Instance.LoadSettings(scene.Id);
            }

            await SceneManager.Instance.UpdateActionObjects();
            await SceneManager.Instance.UpdateServices();

            sceneReady = true;
            //if (sceneOpened)
                //SceneManager.Instance.OnLoadScene?.Invoke(this, EventArgs.Empty);
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            
            if (newProject != null) {
                ProjectUpdated(newProject);
            }
            EndLoading();
        }

        internal void HandleProjectException(ProjectExceptionEventData data) {
            Notifications.Instance.ShowNotification("Project exception", data.Message);
        }

        internal void HandleActionResult(ActionResult data) {
            if (data.Error != null)
                Notifications.Instance.ShowNotification("Action execution failed", data.Error);
            else {
                string res = "";
                if (data.Result != null)
                    res = "Result: " + data.Result;
                Notifications.Instance.ShowNotification("Action execution finished sucessfully", res);
            }
                
        }

        public void ActionUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = SceneManager.Instance.GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdate(projectAction, true);
        }

        public void ActionBaseUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = SceneManager.Instance.GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdateBaseData(projectAction);
        }

        public void ActionAdded(IO.Swagger.Model.Action projectAction, string parentId) {
            ActionPoint actionPoint = SceneManager.Instance.GetActionPoint(parentId);
            IActionProvider actionProvider = SceneManager.Instance.GetActionProvider(Action.ParseActionType(projectAction.Type).Item1);
            Base.Action action = SceneManager.Instance.SpawnAction(projectAction.Id, projectAction.Name, Action.ParseActionType(projectAction.Type).Item2, actionPoint, actionProvider);
            // updates name of the action
            action.ActionUpdateBaseData(projectAction);
            // updates parameters of the action
            action.ActionUpdate(projectAction);
        }


        public void ActionRemoved(IO.Swagger.Model.Action action) {
            SceneManager.Instance.RemoveAction(action.Id);
        }


        public void ActionPointUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPoint(projectActionPoint.Id);
                actionPoint.UpdateActionPoint(projectActionPoint);
                // TODO - update orientations, joints etc.
            } catch (KeyNotFoundException ex) {
                Debug.LogError("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }
        }

        public void ActionPointBaseUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPoint(projectActionPoint.Id);
                actionPoint.ActionPointBaseUpdate(projectActionPoint);
                OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
            } catch (KeyNotFoundException ex) {
                Debug.Log("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }

        }

        public void ActionPointAdded(ProjectActionPoint projectActionPoint) {
            if (projectActionPoint.Parent == null || projectActionPoint.Parent == "") {
                SceneManager.Instance.SpawnActionPoint(projectActionPoint, null);
            } else {
                try {
                    IActionPointParent actionPointParent = SceneManager.Instance.GetActionPointParent(projectActionPoint.Parent);
                    SceneManager.Instance.SpawnActionPoint(projectActionPoint, actionPointParent);
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }
                
            }
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);


        }


        public void ActionPointRemoved(ProjectActionPoint projectActionPoint) {
            SceneManager.Instance.RemoveActionPoint(projectActionPoint.Id);
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }


        public void SceneObjectUpdated(SceneObject sceneObject) {
            ActionObject actionObject = SceneManager.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                actionObject.ActionObjectUpdate(sceneObject, SceneManager.Instance.ActionObjectsVisible, SceneManager.Instance.ActionObjectsInteractive);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
        }

        public void SceneObjectBaseUpdated(SceneObject sceneObject) {
            ActionObject actionObject = SceneManager.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
        }

        public async Task SceneObjectAdded(SceneObject sceneObject) {
            ActionObject actionObject = await SceneManager.Instance.SpawnActionObject(sceneObject.Id, sceneObject.Type, false, sceneObject.Name);
            actionObject.ActionObjectUpdate(sceneObject, SceneManager.Instance.ActionObjectsVisible, SceneManager.Instance.ActionObjectsInteractive);
        }


        public void SceneObjectRemoved(SceneObject sceneObject) {
            ActionObject actionObject = SceneManager.Instance.GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                SceneManager.Instance.ActionObjects.Remove(sceneObject.Id);
                Destroy(actionObject.gameObject);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }            
        }


        public void ActionPointOrientationUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.UpdateOrientation(orientation); 
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationBaseUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.BaseUpdateOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationAdded(NamedOrientation orientation, string actionPointIt) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPoint(actionPointIt);
                actionPoint.AddOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationRemoved(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.RemoveOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointJointsUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.UpdateJoints(joints); 
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsBaseUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.BaseUpdateJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsAdded(ProjectRobotJoints joints, string actionPointIt) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPoint(actionPointIt);
                actionPoint.AddJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point joints", ex.Message);
                return;
            }
        }


        public void ActionPointJointsRemoved(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = SceneManager.Instance.GetActionPointWithJoints(joints.Id);
                actionPoint.RemoveJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point joints", ex.Message);
                return;
            }
        }


        // ProjectUpdated is called from server, when another GUI makes some changes
        public async void ProjectUpdated(IO.Swagger.Model.Project project) {
            StartLoading();
            bool projectOpened = false;
            if (CurrentProject == null && project != null)
                projectOpened = true;
            if (project == null) {
                CurrentProject = null;
                SceneManager.Instance.RemoveActionPoints();
                SceneManager.Instance.Scene = null;
                if (GetGameState() == GameStateEnum.ProjectEditor) {
                    await OpenMainScreen();
                }
                EndLoading();
                return;
            }

            if (project.SceneId != loadedScene || !sceneReady) {
                newProject = project;
                return;
            }

            newProject = null;

            CurrentProject = project;
            SceneManager.Instance.SetAPSize(PlayerPrefsHelper.LoadFloat("project/" + CurrentProject.Id + "/APSize", 0.5f));


            SceneManager.Instance.UpdateActionPoints(CurrentProject);
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);

            if (projectOpened) {
                OnLoadProject?.Invoke(this, EventArgs.Empty);
               
            }
                

            EndLoading();
        }

        

        public string GetSceneId(string name) {
            foreach (IdDesc scene in Scenes) {
                if (name == scene.Name)
                    return scene.Id;
            }
            throw new RequestFailedException("No scene with name: " + name);
        }

        public async Task LoadScenes() {
            try {
                Scenes = await WebsocketManager.Instance.LoadScenes();
                OnSceneListChanged?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, GameManager.Instance.CurrentProject, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
        }

        public async Task LoadProjects() {
            try {
                Projects = await WebsocketManager.Instance.LoadProjects();
                OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, GameManager.Instance.CurrentProject, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
        }

        public async Task LoadPackages() {
            try {
                Packages = await WebsocketManager.Instance.LoadPackages();
                OnPackagesListChanged?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, GameManager.Instance.CurrentProject, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
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
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open project failed", "Failed to load project");
            } finally {
                EndLoading();
            }
        }

        public async Task OpenScene(string id) {
            try {
                await WebsocketManager.Instance.OpenScene(id);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Open scene failed", e.Message);
                return;
            }    
            try {
                await Task.Run(() => WaitForSceneReady(2000));
                return;
            } catch (TimeoutException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", "Failed to open selected scene");
            }
           
        }

        public async Task<bool> RunPackage(string packageId) {
            try {
                StartLoading();
                await WebsocketManager.Instance.RunPackage(packageId);
                OpenPackageRunningScreen();
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
                return false;
            } finally {
                EndLoading();
            }
        }

        internal async Task<bool> TestRunProject() {
            Debug.Assert(CurrentProject != null);
            if (ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                return false;
            }
            try {

                string packageId = await BuildPackage(Guid.NewGuid().ToString());
                return await RunPackage(packageId);
            } catch (RequestFailedException ex) {
                Debug.Log(ex);
                NotificationsModernUI.Instance.ShowNotification("Failed to run project", ex.Message);
                return false;
            }
        }

        public async Task<string> BuildPackage(string name) {
            Debug.Assert(CurrentProject != null);
            if (ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                throw new RequestFailedException("Unsaved changes");
            }
            try {
                StartLoading();
                return await WebsocketManager.Instance.BuildPackage(CurrentProject.Id, name);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to build package", ex.Message);
                throw;
            } finally {
                EndLoading();
            }
        }

        public async Task<bool> BuildAndRunPackage(string name) {
            Debug.Assert(CurrentProject != null);
            if (ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                return false;
            }
            try {
                StartLoading();
                string packageId = await WebsocketManager.Instance.BuildPackage(CurrentProject.Id, name);
                await WebsocketManager.Instance.RunPackage(packageId);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to build package", ex.Message);
                return false;
            } finally {
                EndLoading();
            }
        }

        public async void StopProject() {
            try {
                await WebsocketManager.Instance.StopPackage();
                OnStopProject?.Invoke(this, EventArgs.Empty);
                OpenProjectEditor();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to stop project", ex.Message);
            }
        }

        public async void PauseProject() {
            try {
                await WebsocketManager.Instance.PausePackage();
                OnPauseProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to pause project", ex.Message);
            }
        }


        public async void ResumeProject() {
            try {
                await WebsocketManager.Instance.ResumePackage();
                OnResumeProject?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to resume project", ex.Message);
            }
        }


        public async Task<bool> CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            try {
                await WebsocketManager.Instance.CreateNewObjectType(objectType, false);
                await UpdateActionObjects(objectType.Type);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to create new object type", ex.Message);
                return false;
            }
        }

        public void ExitApp() => Application.Quit();

        public async void UpdateActionPointPositionUsingRobot(string actionPointId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointUsingRobot(actionPointId, robotId, endEffectorId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionPointOrientationUsingRobot(string actionPointId, string robotId, string endEffectorId, string orientationId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointOrientationUsingRobot(actionPointId, robotId, endEffectorId, orientationId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionPointJoints(string robotId, string jointsId) {

            try {
                await WebsocketManager.Instance.UpdateActionPointJoints(robotId, jointsId);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        public async void UpdateActionObjectPoseUsingRobot(string actionObjectId, string robotId, string endEffectorId) {

            try {
                await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(actionObjectId, robotId, endEffectorId);
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

        public async Task NewProject(string name, string sceneId, bool hasLogic) {
            Debug.Assert(sceneId != null && sceneId != "");
            Debug.Assert(name != null && name != "");
            StartLoading();
            
            try {
                await WebsocketManager.Instance.OpenScene(sceneId);
            } catch (RequestFailedException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", e.Message);
                return;
            }           
            
            try {
                await Task.Run(() => WaitForSceneReady(5000));
            } catch (TimeoutException e) {
                EndLoading();
                Notifications.Instance.ShowNotification("Open scene failed", "Scene " + sceneId + " could not be loaded (unknown reason).");
                return;
            }
            //IO.Swagger.Model.Project project = new IO.Swagger.Model.Project(id: Guid.NewGuid().ToString(), name: name, objects: new List<IO.Swagger.Model.ProjectObject>(), sceneId: sceneId, hasLogic: generateLogic);
            //WebsocketManager.Instance.UpdateProject(project);
            //ProjectUpdated(project);
            try {
                await WebsocketManager.Instance.CreateProject(name, sceneId, "", hasLogic, false);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to create project", e.Message);
            } finally {
                EndLoading();
            }
            
        }


        public async Task<bool> NewScene(string name) {
            if (name == "") {
                Notifications.Instance.ShowNotification("Failed to create new scene", "Scane name to defined");
                return false;
            }
            try {
                await WebsocketManager.Instance.CreateScene(name, "");
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to create new scene", e.Message);
            }
            return true;
        }

        public async Task<RemoveFromSceneResponse> RemoveFromScene(string id) {
            return await WebsocketManager.Instance.RemoveFromScene(id, false);
        }

        public async Task<bool> CloseScene(bool force) {
            loadedScene = "";
            bool success = await WebsocketManager.Instance.CloseScene(force);
            if (success) {
                StartLoading();
                SceneManager.Instance.Scene = null;
            }                
            return success;
        }

        public async Task<bool> CloseProject(bool force) {
            loadedScene = "";
            bool success = await WebsocketManager.Instance.CloseProject(force);
            if (success) {
                OnCloseProject?.Invoke(this, EventArgs.Empty);
                SceneManager.Instance.Scene = null;
            }
            return success;
            
        }

        public async Task<List<ObjectAction>> GetActions(string name) {
            try {
                return await WebsocketManager.Instance.GetActions(name);
            } catch (RequestFailedException e) {
                Debug.LogError("Failed to load action for object/service " + name);
                Notifications.Instance.ShowNotification("Failed to laod actions", "Failed to load action for object/service " + name);
                return null;
            }

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
            return int.Parse(SplitVersionString(versionString)[0]);
        }

        public int GetMinorVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[1]);
        }

        public int GetPatchVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[2]);
        }

        public List<string> SplitVersionString(string versionString) {
            List<string> version = versionString.Split('.').ToList<string>();
            Debug.Assert(version.Count == 3, versionString);
            return version;
        }

        public async Task<bool> CheckApiVersion(IO.Swagger.Model.SystemInfoData systemInfo) {
            
            if (systemInfo.ApiVersion == ApiVersion)
                return true;

            if (GetMajorVersion(systemInfo.ApiVersion) != GetMajorVersion(ApiVersion) ||
                (GetMajorVersion(systemInfo.ApiVersion) == 0 && (GetMinorVersion(systemInfo.ApiVersion) != GetMinorVersion(ApiVersion)))) {
                Notifications.Instance.ShowNotification("Incompatibile api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion);
                return false;
            }
            if ((GetMajorVersion(systemInfo.ApiVersion) > 0 && (GetMinorVersion(systemInfo.ApiVersion) < GetMinorVersion(ApiVersion))) ||
                GetPatchVersion(systemInfo.ApiVersion) < GetPatchVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Different api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion + ". It can casuse problems, you have been warned.");
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
            await LoadPackages();
            SetGameState(GameStateEnum.MainScreen);
            OnOpenMainScreen?.Invoke(this, EventArgs.Empty);
            EditorInfo.text = "";
            EndLoading();
        }

        public void OpenSceneEditor() {
            EditorInfo.text = "Scene: " + SceneManager.Instance.Scene.Name;
            SetGameState(GameStateEnum.SceneEditor);
            OnOpenSceneEditor?.Invoke(this, EventArgs.Empty);
        }

        public void OpenProjectEditor() {
            EditorInfo.text = "Project: " + CurrentProject.Name;
            SetGameState(GameStateEnum.ProjectEditor);
            OnOpenProjectEditor?.Invoke(this, EventArgs.Empty);
        }

        public async void OpenPackageRunningScreen() {
            try {
                WaitUntilPackageReady(5000);
                //TODO - get package name from packages list 
                EditorInfo.text = "Running: " + PackageInfo.PackageId;
                SetGameState(GameStateEnum.PackageRunning);
                await SceneUpdated(null);
                ProjectUpdated(null);
                await SceneUpdated(PackageInfo.Scene);
                ProjectUpdated(PackageInfo.Project);
                OnRunPackage?.Invoke(this, EventArgs.Empty);
            } catch (TimeoutException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to open package run screen", "Package info did not arrived");
            }
            
        }

        public void WaitUntilPackageReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (PackageInfo == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
        }


        public void OpenDisconnectedScreen() {
            SetGameState(GameStateEnum.Disconnected);
            EditorInfo.text = "";
        }

        public void ProjectLoaded(object sender, EventArgs eventArgs) {
            ProjectChanged = false;
            OpenProjectEditor();
        }

        public void SceneLoaded(object sender, EventArgs eventArgs) {
            OpenSceneEditor();
        }

        public Button CreateButton(Transform parent, string label) {
            GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, parent);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = label;
            return btn;
        }

        public async Task<bool> RenameActionObject(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameObject(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename object", e.Message);
                return false;
            }
        }
         public async Task<bool> RenameScene(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameScene(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename scene", e.Message);
                return false;
            }
        }

        internal async Task<bool> RemoveScene(string sceneId) {
            try {
                await WebsocketManager.Instance.RemoveScene(sceneId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to remove scene", e.Message);
                return false;
            }
        }


        public async Task<bool> RenameProject(string id, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameProject(id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename project", e.Message);
                return false;
            }
        }

        internal async Task<bool> RemoveProject(string projectId) {
            try {
                await WebsocketManager.Instance.RemoveProject(projectId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to remove project", e.Message);
                return false;
            }
        }

        public async Task<bool> AddActionPoint(string name, string parent, Position position) {
            try {
                await WebsocketManager.Instance.AddActionPoint(name, parent, position);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> RenameActionPoint(ActionPoint actionPoint, string newUserId) {
            try {
                await WebsocketManager.Instance.RenameActionPoint(actionPoint.Data.Id, newUserId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }
        public async Task<bool> UpdateActionPointParent(ActionPoint actionPoint, string parentId) {
            try {
                await WebsocketManager.Instance.UpdateActionPointParent(actionPoint.Data.Id, parentId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateActionPointPosition(ActionPoint actionPoint, Position newPosition) {
            try {
                await WebsocketManager.Instance.UpdateActionPointPosition(actionPoint.Data.Id, newPosition);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> AddActionPointOrientation(ActionPoint actionPoint, Orientation orientation, string orientationId) {
            try {
                await WebsocketManager.Instance.AddActionPointOrientation(actionPoint.Data.Id, orientation, orientationId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }


        public async Task<bool> AddActionPointJoints(ActionPoint actionPoint, string jointsId, string robotId) {
            try {
                await WebsocketManager.Instance.AddActionPointJoints(actionPoint.Data.Id, robotId, jointsId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }
        
        public async Task<bool> AddAction(string actionPointId, List<IO.Swagger.Model.ActionParameter> actionParameters, string type, string name) {
            try {
                await WebsocketManager.Instance.AddAction(actionPointId, actionParameters, type, name);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action", e.Message);
                return false;
            }
        }

        public async Task<bool> RenameAction(string actionId, string newName) {
            try {
                await WebsocketManager.Instance.RenameAction(actionId, newName);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to rename action", e.Message);
                return false;
            }
        }


        public async Task<bool> RemoveAction(string actionId) {
            try {
                await WebsocketManager.Instance.RemoveAction(actionId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> RemoveActionPoint(string actionPointId) {
            try {
                await WebsocketManager.Instance.RemoveActionPoint(actionPointId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateActionObjectPose(string actionObjectId, IO.Swagger.Model.Pose pose) {
            try {
                await WebsocketManager.Instance.UpdateActionObjectPose(actionObjectId, pose);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action object pose", e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateAction(string actionId, List<IO.Swagger.Model.ActionParameter> parameters) {
            try {
                await WebsocketManager.Instance.UpdateAction(actionId, parameters);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action ", e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateActionLogic(string actionId, List<IO.Swagger.Model.ActionIO> inputs, List<IO.Swagger.Model.ActionIO> outputs) {
            try {
                await WebsocketManager.Instance.UpdateActionLogic(actionId, inputs, outputs);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action ", e.Message + " logic");
                return false;
            }
        }

        public async Task<List<string>> GetProjectsWithScene(string sceneId) {
            try {
                return await WebsocketManager.Instance.GetProjectsWithScene(sceneId);
            } catch (RequestFailedException e) {
                Debug.LogError(e);
                return new List<string>();
            }
        }


        }    

}
