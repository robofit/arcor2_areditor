using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using IO.Swagger.Model;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Events;

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

    public class EditorStateEventArgs : EventArgs {
        public GameManager.EditorStateEnum Data {
            get; set;
        }

        public EditorStateEventArgs(GameManager.EditorStateEnum data) {
            Data = data;
        }
    }

    public class ProjectMetaEventArgs : EventArgs {
        public string Name {
            get; set;
        }

        public string Id {
            get; set;
        }

        public ProjectMetaEventArgs(string id, string name) {
            Id = id;
            Name = name;
        }
    }




    public class GameManager : Singleton<GameManager> {

        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void EditorStateEventHandler(object sender, EditorStateEventArgs args);
        public delegate void ProjectMetaEventHandler(object sender, ProjectMetaEventArgs args);

        public event EventHandler OnSaveProject;
        
        public event ProjectMetaEventHandler OnRunPackage;
        public event EventHandler OnStopPackage;
        public event ProjectMetaEventHandler OnPausePackage;
        public event ProjectMetaEventHandler OnResumePackage;
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
        public event EventHandler OnServicesChanged;
        public event GameStateEventHandler OnGameStateChanged;
        public event EditorStateEventHandler OnEditorStateChanged;
        public event EventHandler OnOpenProjectEditor;
        public event EventHandler OnOpenSceneEditor;
        public event EventHandler OnOpenMainScreen;
        public event StringEventHandler OnActionExecution;
        public event EventHandler OnActionExecutionFinished;
        public event EventHandler OnActionExecutionCanceled;


        private GameStateEnum gameState;
        private EditorStateEnum editorState;

        public GameObject LoadingScreen, MainMenuBtn, StatusPanel;
        public GameObject ButtonPrefab;
        public GameObject Tooltip;
        public TMPro.TextMeshProUGUI Text;
        private IO.Swagger.Model.Project newProject;
        private IO.Swagger.Model.Scene newScene;
        private PackageState newPackageState;

        private bool openProject = false;
        private bool openScene = false;
        private bool openPackage = false;

        public string ExecutingAction = null;

        public const string ApiVersion = "0.7.0";

        public readonly string EditorVersion = "0.6.0-beta.3";
        public List<IO.Swagger.Model.ListProjectsResponseData> Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
        public List<IO.Swagger.Model.PackageSummary> Packages = new List<IO.Swagger.Model.PackageSummary>();
        public List<IO.Swagger.Model.IdDesc> Scenes = new List<IO.Swagger.Model.IdDesc>();

        public TMPro.TMP_Text VersionInfo, MessageBox, EditorInfo, ConnectionInfo, ServerVersion;

        public Image GizmoOverlay;

        public GameObject objectWithGizmo, Scene;

        [SerializeField]
        private Canvas headUpCanvas;

        [SerializeField]
        private SelectObjectInfo SelectObjectInfo;

        public IO.Swagger.Model.SystemInfoData SystemInfo;
        public PackageInfo PackageInfo;

        private string reopenProjectId = null;

        
        [SerializeField]
        private ARSession ARSession;

        private Action<object> ObjectCallback;
        // sets to true when OpenProjec, OpenScene or PackageStatus == Running upon startup
        bool openSceneProjectPackage = false;

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

        public enum EditorStateEnum {
            Closed,
            Normal,
            SelectingActionObject,
            SelectingActionPoint,
            SelectingAction,
            InteractionDisabled
        }

        private ConnectionStatusEnum connectionStatus;

        private async Task Update() {
            if (ConnectionStatus != ConnectionStatusEnum.Connected)
                return;
            if (openScene) {
                openScene = false;
                if (newScene != null) {
                    Scene scene = newScene;
                    newScene = null;
                    await SceneOpened(scene);
                }

            } else if (openProject) {
                openProject = false;
                if (newProject != null && newScene != null) {
                    Scene scene = newScene;
                    Project project = newProject;
                    newScene = null;
                    newProject = null;
                    ProjectOpened(scene, project);
                }
            } else if (openPackage) {
                openPackage = false;
                PackageStateUpdated(newPackageState);
            }

            
        }

        public ConnectionStatusEnum ConnectionStatus {
            get => connectionStatus; set {
                if (connectionStatus != value) {                    
                    OnConnectionStatusChanged(value);
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

        private void SetEditorState(EditorStateEnum newState) {
            editorState = newState;
            OnEditorStateChanged?.Invoke(this, new EditorStateEventArgs(newState));
            switch (newState) {
                case EditorStateEnum.Normal:
                    MainMenuBtn.SetActive(true);
                    StatusPanel.SetActive(true);
                    break;
                default:
                    MainMenuBtn.SetActive(false);
                    MenuManager.Instance.HideAllMenus();
                    StatusPanel.SetActive(false);
                    break;
            }
        }

        public EditorStateEnum GetEditorState() {
            return editorState;
        }

        public void RequestObject(EditorStateEnum requestType, Action<object> callback, string message) {
            Debug.Assert(requestType != EditorStateEnum.Closed && requestType != EditorStateEnum.Normal);
            SetEditorState(requestType);
            ObjectCallback = callback;
            SelectObjectInfo.Show(message, () => CancelSelection());
        }

        public void CancelSelection() {
            if (ObjectCallback != null) {
                ObjectCallback.Invoke(null);
                ObjectCallback = null;
            }
            SetEditorState(EditorStateEnum.Normal);
        }

        public void ObjectSelected(object selectedObject) {
            if (ObjectCallback != null)
                ObjectCallback.Invoke(selectedObject);
            ObjectCallback = null;
            SetEditorState(EditorStateEnum.Normal);
            SelectObjectInfo.gameObject.SetActive(false);
        }

        private void Awake() {
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            OpenDisconnectedScreen();
        }

        private void Start() {

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = false;
#endif
            VersionInfo.text = EditorVersion;
            Scene.SetActive(false);
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
        }

        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    IO.Swagger.Model.SystemInfoData systemInfo;
                    try {
                        systemInfo = await WebsocketManager.Instance.GetSystemInfo();                        
                    } catch (RequestFailedException ex) {
                        DisconnectFromSever();
                        Notifications.Instance.ShowNotification("Connection failed", "");
                        return;
                    }
                    if (!CheckApiVersion(systemInfo)) {
                        return;
                    }

                    SystemInfo = systemInfo;
                    ServerVersion.text = "Editor version: " + EditorVersion +
                        "\nServer version: " + systemInfo.Version;
                    ConnectionInfo.text = WebsocketManager.Instance.APIDomainWS;
                    MenuManager.Instance.DisableAllMenus();
                    
                    
                    OnConnectedToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.APIDomainWS));

                    await UpdateActionObjects();
                    await UpdateServices();
                    await UpdateRobotsMeta();

                    try {
                        await Task.Run(() => ActionsManager.Instance.WaitUntilActionsReady(15000));
                    } catch (TimeoutException e) {
                        Notifications.Instance.ShowNotification("Connection failed", "Some actions were not loaded within timeout");
                        DisconnectFromSever();
                        ActionsManager.Instance.Init();
                        return;
                    }

                    await LoadScenes();
                    await LoadProjects();
                    await LoadPackages();

                    if (!openSceneProjectPackage) {
                        await OpenMainScreen(false);
                    }
                    connectionStatus = newState;
                    break;
                case ConnectionStatusEnum.Disconnected:
                    OpenDisconnectedScreen();
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<IO.Swagger.Model.ListProjectsResponseData>();
                    Scenes = new List<IO.Swagger.Model.IdDesc>();

                    ProjectManager.Instance.DestroyProject();
                    SceneManager.Instance.DestroyScene();
                    Scene.SetActive(false);
                    Init();
                    connectionStatus = newState;
                    break;
            }
        }


        public void ShowLoadingScreen() {
            Debug.Assert(LoadingScreen != null);
            // HACK to make loading screen in foreground
            // TODO - find better way
            headUpCanvas.enabled = false;
            headUpCanvas.enabled = true;
            LoadingScreen.SetActive(true);
        }

        public void HideLoadingScreen() {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.SetActive(false);
        }

        private void Init() {
            openSceneProjectPackage = false;
        }

        public async void ConnectToSever(string domain, int port) {
            ShowLoadingScreen();
            OnConnectingToServer?.Invoke(this, new StringEventArgs(WebsocketManager.Instance.GetWSURI(domain, port)));
            if (await WebsocketManager.Instance.ConnectToServer(domain, port)) {
                try {
                    await Task.Run(() => WebsocketManager.Instance.WaitForInitData(5000));
                    ConnectionStatus = GameManager.ConnectionStatusEnum.Connected;
                } catch (TimeoutException e) {
                    Notifications.Instance.ShowNotification("Connection failed", "Connected but failed to fetch required data (scene, project, projectstate)");
                    WebsocketManager.Instance.DisconnectFromSever();
                }
            
            } else {
                ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
                
                Notifications.Instance.ShowNotification("Connection failed", "Failed to connect to remote server. Is it running?");
                WebsocketManager.Instance.DisconnectFromSever();
            }

        }

        
        public void DisconnectFromSever() {
            WebsocketManager.Instance.DisconnectFromSever();
        }

        private void OnActionsLoaded(object sender, EventArgs e) {
            MenuManager.Instance.EnableAllWindows();
        }

        public async Task UpdateActionObjects(string highlighteObject = null) {
            try {
                List<IO.Swagger.Model.ObjectTypeMeta> objectTypeMetas = await WebsocketManager.Instance.GetObjectTypes();
                await ActionsManager.Instance.UpdateObjects(objectTypeMetas, highlighteObject);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, ProjectManager.Instance.Project, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
            
        }

        public async Task UpdateServices() {
            await ActionsManager.Instance.UpdateServicesMetadata(await WebsocketManager.Instance.GetServices());
        }


        private async Task UpdateRobotsMeta() {
            ActionsManager.Instance.UpdateRobotsMetadata(await WebsocketManager.Instance.GetRobotMeta());

        }

        /// <summary>
        /// Sends request to the server to create a new Action Object of user specified type and id. Id has to be generated here in the client.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> AddObjectToScene(string type, string name) {
            try {
                IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(new Vector3(0, 0, 0)), orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
                await WebsocketManager.Instance.AddObjectToScene(name, type, pose);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add object to scene", ex.Message);
                return false;
            }
            return true;
        }

        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string type) {
            return await WebsocketManager.Instance.AutoAddObjectToScene(type);
        }

        public async void AddServiceToScene(string type, string configId = "") {
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(type: type, configurationId: configId);
            try {
                await WebsocketManager.Instance.AddServiceToScene(sceneService: sceneService);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Add service failed", e.Message);
            } finally {
            }            
            
        }


        public void SceneAdded(IO.Swagger.Model.Scene scene) {
            newScene = scene;
        }

        
        public async void SceneBaseUpdated(IO.Swagger.Model.Scene scene) {
            if (GetGameState() == GameStateEnum.SceneEditor)
                SceneManager.Instance.SceneBaseUpdated(scene);
            else if (GetGameState() == GameStateEnum.MainScreen) {
                await LoadScenes();
            }
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
            ExecutingAction = null;
            OnActionExecutionFinished?.Invoke(this, EventArgs.Empty);
            // Stop previously running action (change its color to default)
            if (ActionsManager.Instance.CurrentlyRunningAction != null)
                ActionsManager.Instance.CurrentlyRunningAction.StopAction();
        }

        internal void HandleActionCanceled() {
            try {
                Action action = ProjectManager.Instance.GetAction(ExecutingAction);                
                Notifications.Instance.ShowNotification("Action execution canceled", "Action " + action.Data.Name + " was cancelled");
            } catch (ItemNotFoundException ex) {
                Notifications.Instance.ShowNotification("Action execution canceled", "Unknown action was cancelled");
            } finally {
                ExecutingAction = null;
                OnActionExecutionCanceled?.Invoke(this, EventArgs.Empty);
                
                // Stop previously running action (change its color to default)
                if (ActionsManager.Instance.CurrentlyRunningAction != null)
                    ActionsManager.Instance.CurrentlyRunningAction.StopAction();

            }

        }

        internal void HandleActionExecution(string actionId) {
            ExecutingAction = actionId;
            OnActionExecution?.Invoke(this, new StringEventArgs(ExecutingAction));
            Action puck = ProjectManager.Instance.GetAction(actionId);
            if (puck == null)
                return;

            ActionsManager.Instance.CurrentlyRunningAction = puck;
            // Run current action (set its color to running)
            puck.RunAction();
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
            ActionObject actionObject = await SceneManager.Instance.SpawnActionObject(sceneObject.Id, sceneObject.Type);
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


        
        internal async Task SceneOpened(Scene scene) {
            openSceneProjectPackage = true;
            if (!ActionsManager.Instance.ActionsReady) {
                newScene = scene;
                openScene = true;
                return;
            }
            try {
                if (await SceneManager.Instance.CreateScene(scene, true)) {                    
                    OpenSceneEditor();                    
                } else {
                    Notifications.Instance.SaveLogs(scene, null, "Failed to initialize scene");
                    HideLoadingScreen();
                }
            } catch (TimeoutException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(scene, null, "Failed to initialize scene");
                HideLoadingScreen();
            }
            

        }

        internal async void ProjectOpened(Scene scene, Project project) {
            openSceneProjectPackage = true;
            if (!ActionsManager.Instance.ActionsReady) {
                newProject = project;
                newScene = scene;
                openProject = true;
                return;
            }
            try {
                if (!await SceneManager.Instance.CreateScene(scene, true)) {
                    Notifications.Instance.SaveLogs(scene, project, "Failed to initialize scene");
                    HideLoadingScreen();
                    return;
                }
                if (ProjectManager.Instance.CreateProject(project, true)) {
                    OpenProjectEditor();
                } else {
                    Notifications.Instance.SaveLogs(scene, project, "Failed to initialize project");
                    HideLoadingScreen();
                }
            } catch (TimeoutException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(scene, project, "Failed to initialize project");
                HideLoadingScreen();
            }
        }

        public async void PackageStateUpdated(IO.Swagger.Model.PackageState state) {
            if (state.State == PackageState.StateEnum.Running ||
                state.State == PackageState.StateEnum.Paused) {
                openSceneProjectPackage = true;
                if (!ActionsManager.Instance.ActionsReady || PackageInfo == null) {
                    newPackageState = state;
                    openPackage = true;
                    return;
                }
                if (GetGameState() != GameStateEnum.PackageRunning) {
                    try {
                        WaitUntilPackageReady(5000);
                        if (!await SceneManager.Instance.CreateScene(PackageInfo.Scene, false, PackageInfo.CollisionModels)) {
                            Notifications.Instance.SaveLogs(PackageInfo.Scene, PackageInfo.Project, "Failed to initialize scene");
                            return;
                        }
                        if (!ProjectManager.Instance.CreateProject(PackageInfo.Project, false)) {
                            Notifications.Instance.SaveLogs(PackageInfo.Scene, PackageInfo.Project, "Failed to initialize project");
                        }
                        OpenPackageRunningScreen();
                        if (state.State == PackageState.StateEnum.Paused) {
                            OnPausePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, GetPackageName(PackageInfo.PackageId)));
                        }
                    } catch (TimeoutException ex) {
                        Debug.LogError(ex);
                        Notifications.Instance.SaveLogs(null, null, "Failed to initialize project");
                    }
                } else if (state.State == PackageState.StateEnum.Paused) {
                    OnPausePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, GetPackageName(PackageInfo.PackageId)));
                    HideLoadingScreen();
                } else if (state.State == PackageState.StateEnum.Running) {
                    OnResumePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, GetPackageName(PackageInfo.PackageId)));
                    HideLoadingScreen();
                }
                
                
            } else if (state.State == PackageState.StateEnum.Stopped) {
                if (!ActionsManager.Instance.ActionsReady) {
                    newPackageState = state;
                    openPackage = true;
                    return;
                }
                
                if (!string.IsNullOrEmpty(reopenProjectId)) {
                    ProjectManager.Instance.DestroyProject();
                    SceneManager.Instance.DestroyScene();
                    OpenProject(reopenProjectId);
                    reopenProjectId = null;
                } else {
                    if (newProject == null &&
                        newScene == null &&
                        SceneManager.Instance.Scene == null &&
                        ProjectManager.Instance.Project == null) {
                        await OpenMainScreen();
                    } else if (GetGameState() == GameStateEnum.PackageRunning) {
                        ProjectManager.Instance.DestroyProject();
                        SceneManager.Instance.DestroyScene();
                        await OpenMainScreen();
                    }
                            
                }                
            }
        }





        internal void SceneClosed() {
            ShowLoadingScreen();
            SceneManager.Instance.DestroyScene();
            _ = OpenMainScreen();
        }

        internal void ProjectClosed() {
            ShowLoadingScreen();
            ProjectManager.Instance.DestroyProject();
            SceneManager.Instance.DestroyScene();
            _ = OpenMainScreen();
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
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, Base.ProjectManager.Instance.Project, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
        }

        public async Task LoadProjects() {
            try {
                Projects = await WebsocketManager.Instance.LoadProjects();
                OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, Base.ProjectManager.Instance.Project, "Failed to update action objects");
                GameManager.Instance.DisconnectFromSever();
            }
        }

        public async Task LoadPackages() {
            try {
                Packages = await WebsocketManager.Instance.LoadPackages();
                OnPackagesListChanged?.Invoke(this, EventArgs.Empty);
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs(SceneManager.Instance.Scene, Base.ProjectManager.Instance.Project, "Failed to update action objects");
                DisconnectFromSever();
            }
        }

        public PackageSummary GetPackage(string id) {
            foreach (PackageSummary package in Packages) {
                if (id == package.Id)
                    return package;
            }
            throw new ItemNotFoundException("Package does not exist");
        }

        public string GetPackageName(string id) {
            return GetPackage(id).Name;
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
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.OpenProject(id);
                await Task.Run(() => WaitForProjectReady(5000));
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
                HideLoadingScreen();
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open project failed", "Failed to load project");
                HideLoadingScreen();
            } 
        }

        public async Task OpenScene(string id) {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.OpenScene(id);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Open scene failed", e.Message);
                HideLoadingScreen();
                return;
            }    
            try {
                await Task.Run(() => WaitForSceneReady(5000));
                return;
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open scene failed", "Failed to open selected scene");
                HideLoadingScreen();
            }
           
        }

        public async Task<bool> RunPackage(string packageId) {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.RunPackage(packageId);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
                HideLoadingScreen();
                return false;
            } 
        }

        internal async Task<bool> TestRunProject() {
            Debug.Assert(Base.ProjectManager.Instance.Project != null);
            if (ProjectManager.Instance.ProjectChanged) {
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
            ShowLoadingScreen();
            Debug.Assert(Base.ProjectManager.Instance.Project != null);
            if (ProjectManager.Instance.ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                HideLoadingScreen();
                throw new RequestFailedException("Unsaved changes");
            }
            try {
                return await WebsocketManager.Instance.BuildPackage(Base.ProjectManager.Instance.Project.Id, name);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to build package", ex.Message);
                throw;
            } finally {
                HideLoadingScreen();
            }
        }

        public async Task<bool> BuildAndRunPackage(string name) {
            ShowLoadingScreen();
            Debug.Assert(Base.ProjectManager.Instance.Project != null);
            if (ProjectManager.Instance.ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                return false;
            }
            try {
                string packageId = await WebsocketManager.Instance.BuildPackage(Base.ProjectManager.Instance.Project.Id, name);
                reopenProjectId = ProjectManager.Instance.Project.Id;
                if (!await CloseProject(false)) {
                    Notifications.Instance.ShowNotification("Failed to build and run package", "");
                    reopenProjectId = null;
                    return false;
                }
                await LoadPackages();
                await WebsocketManager.Instance.RunPackage(packageId);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to build and run package", ex.Message);
                return false;
            } finally {
            }
        }

        public async void StopProject() {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.StopPackage();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to stop project", ex.Message);
                HideLoadingScreen();
            }
        }

        public async void PauseProject() {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.PausePackage();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to pause project", ex.Message);
                HideLoadingScreen();
            }
        }


        public async void ResumeProject() {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.ResumePackage();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to resume project", ex.Message);
                HideLoadingScreen();
            }
        }


        public async Task<bool> CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            ShowLoadingScreen();
            try {
                await WebsocketManager.Instance.CreateNewObjectType(objectType, false);
                await UpdateActionObjects(objectType.Type);
                return true;
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to create new object type", ex.Message);
                return false;
            } finally {
                HideLoadingScreen();
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
            
            try {
                await WebsocketManager.Instance.CreateProject(name, sceneId, "", hasLogic, false);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to create project", e.Message);
            } finally {
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
            ShowLoadingScreen();
            bool success = await WebsocketManager.Instance.CloseScene(force);
            if (success) {
                SceneManager.Instance.Scene = null;
            } else {
                HideLoadingScreen();
            }          
            return success;
        }

        public async Task<bool> CloseProject(bool force) {
            ShowLoadingScreen();
            bool success = await WebsocketManager.Instance.CloseProject(force);
            if (success) {
                OnCloseProject?.Invoke(this, EventArgs.Empty);
                SceneManager.Instance.Scene = null;
            } else {
                HideLoadingScreen();
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

        public async Task<bool> CancelExecution() {
            try {
                await WebsocketManager.Instance.CancelExecution();
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to cancel action", ex.Message);
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

        public bool CheckApiVersion(IO.Swagger.Model.SystemInfoData systemInfo) {
            
            if (systemInfo.ApiVersion == ApiVersion)
                return true;

            if (GetMajorVersion(systemInfo.ApiVersion) != GetMajorVersion(ApiVersion) ||
                (GetMajorVersion(systemInfo.ApiVersion) == 0 && (GetMinorVersion(systemInfo.ApiVersion) != GetMinorVersion(ApiVersion)))) {
                Notifications.Instance.ShowNotification("Incompatibile api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion);
                return false;
            }
            if ((GetMajorVersion(systemInfo.ApiVersion) > 0 && (GetMinorVersion(systemInfo.ApiVersion) < GetMinorVersion(ApiVersion))) ||
                GetPatchVersion(systemInfo.ApiVersion) < GetPatchVersion(ApiVersion)) {
                Notifications.Instance.ShowNotification("Different api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion + ". It can cause problems, you have been warned.");
                return true;
            }
            
            return false;
        }

        public void WaitForSceneReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (SceneManager.Instance.Scene == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public void WaitForProjectReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (Base.ProjectManager.Instance.Project == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        public async Task OpenMainScreen(bool updateResources = true) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = false;
#endif
            Scene.SetActive(false);
            if (updateResources) {
                await LoadScenes();
                await LoadProjects();
                await LoadPackages();
            }            
            SetGameState(GameStateEnum.MainScreen);
            OnOpenMainScreen?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Closed);
            EditorInfo.text = "";
            HideLoadingScreen();
        }

        public void OpenSceneEditor() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = true;
#endif
            EditorInfo.text = "Scene: " + SceneManager.Instance.Scene.Name;
            SetGameState(GameStateEnum.SceneEditor);
            Scene.SetActive(true);
            OnOpenSceneEditor?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Normal);
            HideLoadingScreen();
        }

        public void OpenProjectEditor() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = true;
#endif
            EditorInfo.text = "Project: " + Base.ProjectManager.Instance.Project.Name;
            SetGameState(GameStateEnum.ProjectEditor);
            Scene.SetActive(true);
            OnOpenProjectEditor?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Normal);
            HideLoadingScreen();
        }

        public async void OpenPackageRunningScreen() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = true;
#endif
            try {
                EditorInfo.text = "Running: " + PackageInfo.PackageId;
                SetGameState(GameStateEnum.PackageRunning);
                SetEditorState(EditorStateEnum.InteractionDisabled);
                Scene.SetActive(true);
                OnRunPackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, GetPackageName(PackageInfo.PackageId)));
            } catch (TimeoutException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to open package run screen", "Package info did not arrived");
            } finally {
                HideLoadingScreen();
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
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            ARSession.enabled = false;
#endif
            Scene.SetActive(false);
            SetGameState(GameStateEnum.Disconnected);
            EditorInfo.text = "";
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

        internal async Task<bool> RemovePackage(string packageId) {
            try {
                await WebsocketManager.Instance.RemovePackage(packageId);
                return true;
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to remove package", e.Message);
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
            Debug.Assert(ProjectManager.Instance.AllowEdit);
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
