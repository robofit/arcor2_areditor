using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.Design;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using TMPro;
using TrilleonAutomation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Debug = UnityEngine.Debug;

namespace Base {
    /// <summary>
    /// Main controller of application. It is responsible for management of different screens
    /// (landing screen, main screen, editor screens) and for management of application states.
    /// </summary>
    public class GameManager : Singleton<GameManager> {

        /// <summary>
        /// Advanced mode of editor
        /// </summary>
        public bool ExpertMode = true;

        /// <summary>
        /// Called when project was saved
        /// </summary>
        public event EventHandler OnSaveProject;
        /// <summary>
        /// Called when package is running. Contains id and name of package 
        /// </summary>
        public event EventHandler<ProjectMetaEventArgs> OnRunPackage;
        /// <summary>
        /// Called when package is stopped
        /// </summary>
        public event EventHandler OnStopPackage;
        /// <summary>
        /// Called when package is paused. Contains id and name of package 
        /// </summary>
        public event EventHandler<ProjectMetaEventArgs> OnPausePackage;
        /// <summary>
        /// Called when package is resumed. Contains id and name of package
        /// </summary>
        public event EventHandler<ProjectMetaEventArgs> OnResumePackage;
        /// <summary>
        /// Called when project is closed
        /// </summary>
        public event EventHandler OnCloseProject;
        /// <summary>
        /// Called when scene is closed
        /// </summary>
        public event EventHandler OnCloseScene;
        /// <summary>
        /// Called when list of projects is changed (new project, removed project, renamed project)
        /// </summary>
        public event EventHandler OnProjectsListChanged;
        /// <summary>
        /// Called when list of packages is changed (new package, removed package, renamed package)
        /// </summary>
        public event EventHandler OnPackagesListChanged;
        /// <summary>
        /// Called when list of scenes is changed (new, removed, renamed)
        /// </summary>
        public event EventHandler OnScenesListChanged;
        /// <summary>
        /// Called when editor connected to server. Contains server URI
        /// </summary>
        public event EventHandler<StringEventArgs> OnConnectedToServer;
        /// <summary>
        /// Called when editor is trying to connect to server. Contains server URI
        /// </summary>
        public event EventHandler<StringEventArgs> OnConnectingToServer;
        /// <summary>
        /// Called when disconected from server
        /// </summary>
        public event EventHandler OnDisconnectedFromServer;
        /// <summary>
        /// Called when some element of scene changed (action object)
        /// </summary>
        public event EventHandler OnSceneChanged;
        /// <summary>
        /// Called when some action object changed
        /// </summary>
        public event EventHandler OnActionObjectsChanged;
        /// <summary>
        /// Invoked when in SceneEditor or ProjectEditor state and no menus are opened
        /// </summary>
        public event EventHandler OnSceneInteractable;
        /// <summary>
        /// Invoked when any menu is opened
        /// </summary>
        public event EventHandler OnSceneNotInteractable; 
        /// <summary>
        /// Invoked when game state changed. Contains new state
        /// </summary>
        public event EventHandler<GameStateEventArgs> OnGameStateChanged;
        /// <summary>
        /// Invoked when editor state changed. Contains new state
        /// </summary>
        public event EventHandler<EditorStateEventArgs> OnEditorStateChanged;
        /// <summary>
        /// Invoked when project editor is opened
        /// </summary>
        public event EventHandler OnOpenProjectEditor;
        /// <summary>
        /// Invoked when scene editor is opened
        /// </summary>
        public event EventHandler OnOpenSceneEditor;
        /// <summary>
        /// Invoked when main screen is opened
        /// </summary>
        public event EventHandler OnOpenMainScreen;
        /// <summary>
        /// Invoked upon action execution. Contains ID of executed action
        /// </summary>
        public event EventHandler<ActionExecutionEventArgs> OnActionExecution;
        /// <summary>
        /// Invoked when action execution finished
        /// </summary>
        public event EventHandler OnActionExecutionFinished;
        /// <summary>
        /// Invoked when action execution was canceled
        /// </summary>
        public event EventHandler OnActionExecutionCanceled;

        /// <summary>
        /// Holds current application state (opened screen)
        /// </summary>
        private GameStateEnum gameState;
        /// <summary>
        /// Holds current editor state
        /// </summary>
        private EditorStateEnum editorState;
        /// <summary>
        /// Prefab for transform gizmo
        /// </summary>
        public GameObject GizmoPrefab;
        /// <summary>
        /// Loading screen with animation
        /// </summary>
        public LoadingScreen LoadingScreen;
        /// <summary>
        /// Canvas group of main menu button (hamburger menu in editor screen)
        /// </summary>
        public CanvasGroup MainMenuBtnCG;
        /// <summary>
        /// Standard button prefab
        /// </summary>
        public GameObject ButtonPrefab;
        /// <summary>
        /// Service button prefab - with green or red strip on the left side (joints buttons)
        /// </summary>
        public GameObject ServiceButtonPrefab;
        /// <summary>
        /// Tooltip gameobject
        /// </summary>
        public GameObject Tooltip;
        /// <summary>
        /// Gameobject for floating point number input (with label)
        /// </summary>
        public GameObject LabeledFloatInput;
        /// <summary>
        /// Text component of tooltip
        /// </summary>
        public TextMeshProUGUI Text;
        /// <summary>
        /// Temp storage for delayed project
        /// </summary>
        private Project newProject;
        /// <summary>
        /// Temp storage for delayed scene
        /// </summary>
        private Scene newScene;
        /// <summary>
        /// Temp storage for delayed package
        /// </summary>
        private PackageStateData newPackageState, nextPackageState;

        /// <summary>
        /// Indicates that project should be opened with delay (waiting for scene or action objects)
        /// </summary>
        private bool openProject = false;
        /// <summary>
        /// Indicates that scene should be opened with delay (waiting for action objects)
        /// </summary>
        private bool openScene = false;
        /// <summary>
        /// Indicates that package should be opened with delay (waiting for scene or action objects)
        /// </summary>
        private bool openPackage = false;
        /// <summary>
        /// Id of action which runs when initializing package
        /// </summary>
        public string ActionRunningOnStartupId;

        /// <summary>
        /// Holds ID of currently executing action. Null if there is no such action
        /// </summary>
        public string ExecutingAction = null;
        /// <summary>
        /// Api version
        /// </summary>        
        public const string ApiVersion = "1.0.0";
        /// <summary>
        /// List of projects metadata
        /// </summary>
        public List<ListProjectsResponseData> Projects = new();
        /// <summary>
        /// List of packages metadata
        /// </summary>
        public List<PackageSummary> Packages = new();
        /// <summary>
        /// List of scenes metadata
        /// </summary>
        public List<ListScenesResponseData> Scenes = new();
        /// <summary>
        /// 
        /// </summary>
        public TMP_Text MessageBox;
        /// <summary>
        /// Connection info component in main menu
        /// </summary>
        public TMP_Text ConnectionInfo;
        /// <summary>
        /// Server version info component in main menu
        /// </summary>
        public TMP_Text ServerVersion;

        /// <summary>
        /// GameObject which is currently manipulated by gizmo
        /// </summary>
        public GameObject ObjectWithGizmo;
        /// <summary>
        /// GameObject of scene
        /// </summary>
        public GameObject Scene;

        /// <summary>
        /// Canvas for headUp info (notifications, tooltip, loading screen etc.
        /// </summary>
        [SerializeField]
        private Canvas headUpCanvas;

        /// <summary>
        /// Info box for selecting of objects
        /// </summary>
        [SerializeField]
        private SelectObjectInfo SelectObjectInfo;

        /// <summary>
        /// Holds info about server (version, supported RPCs, supported parameters etc.)
        /// </summary>
        public SystemInfoResponseData SystemInfo;
        /// <summary>
        /// Holds info about currently running package
        /// </summary>
        public PackageInfoData PackageInfo;

        /// <summary>
        /// Holds whether delayed openning of main screen is requested
        /// </summary>
        private bool openMainScreenRequest = false;

        /// <summary>
        /// Holds info about what part of main screen should be displayd
        /// </summary>
        private ShowMainScreenData openMainScreenData;

        /// <summary>
        /// Holds info abour AR session
        /// </summary>
        [SerializeField]
        private ARSession ARSession;

        /// <summary>
        /// Callback to be invoked when requested object is selected and potentionally validated
        /// </summary>
        private Action<object> ObjectCallback;
        /// <summary>
        /// Callback to be invoked when requested object is selected
        /// </summary>
        private Func<object, Task<RequestResult>> ObjectValidationCallback;

        private bool openPackageRunningScreenFlag = false;


        /// TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! is this still neccassarry? How to use it when there is no menumanager anymore?
        /// <summary>
        /// Checks whether scene is interactable
        /// </summary>
        public bool SceneInteractable {
            get => true;
        }



        /// <summary>
        /// Determines whether the application is in correct state (scene or project editor) and
        /// invokes events saying if the sceen is interactable or not, based on provided parameter
        /// </summary>
        /// <param name="interactable"></param>
        public void InvokeSceneInteractable(bool interactable) {
            if (interactable && (gameState == GameStateEnum.SceneEditor || gameState == GameStateEnum.ProjectEditor)) {
                OnSceneInteractable?.Invoke(this, EventArgs.Empty);
            } else {
                OnSceneNotInteractable?.Invoke(this, EventArgs.Empty);
            }
        }



        /// <summary>
        /// Enum specifying connection states
        /// </summary>
        public enum ConnectionStatusEnum {
            Connected, Disconnected, Connecting
        }

        /// <summary>
        /// Enum specifying aplication states
        /// </summary>
        public enum GameStateEnum {
            /// <summary>
            /// Not connected to server
            /// </summary>
            Disconnected,
            /// <summary>
            /// Screen with list of scenes, projects and packages
            /// </summary>
            MainScreen,
            /// <summary>
            /// Scene editor
            /// </summary>
            SceneEditor,
            /// <summary>
            /// Project editor
            /// </summary>
            ProjectEditor,
            /// <summary>
            /// Visualisation of running package
            /// </summary>
            PackageRunning,
            LoadingScene,
            LoadingProject,
            LoadingPackage,
            ClosingScene,
            ClosingProject,
            ClosingPackage,
            None
        }

        /// <summary>
        /// Enum specifying editor states
        ///
        /// For selecting states - other interaction than selecting of requeste object is disabled
        /// </summary>
        public enum EditorStateEnum {
            /// <summary>
            /// No editor (scene or project) opened
            /// </summary>
            Closed,
            /// <summary>
            /// Normal state
            /// </summary>
            Normal,
            /// <summary>
            /// Indicates that user should select action object
            /// </summary>
            SelectingActionObject,
            /// <summary>
            /// Indicates that user should select action point
            /// </summary>
            SelectingActionPoint,
            /// <summary>
            /// Indicates that user should select action 
            /// </summary>
            SelectingAction,
            /// <summary>
            /// Indicates that user should select action input
            /// </summary>
            SelectingActionInput,
            /// <summary>
            /// Indicates that user should select action output
            /// </summary>
            SelectingActionOutput,
            /// <summary>
            /// Indicates that user should select action object or another action point
            /// </summary>
            SelectingActionPointParent,
            /// <summary>
            /// Indicates that user should select orientation of action point
            /// </summary>
            SelectingAPOrientation,
            /// <summary>
            /// Indicates that user should select end effector
            /// </summary>
            SelectingEndEffector,
            /// <summary>
            /// Indicates that all interaction is disabled
            /// </summary>
            InteractionDisabled
        }

        /// <summary>
        /// Holds info of connection status
        /// </summary>
        private ConnectionStatusEnum connectionStatus;

        /// <summary>
        /// When connected to server, checks for requests for delayd scene, project, package or main screen openning
        /// </summary>
        private async Task Update() {
            // Only when connected to server
            if (ConnectionStatus != ConnectionStatusEnum.Connected)
                return;

            // request for delayed openning of scene to allow loading of action objects and their actions
            if (openScene) {
                openScene = false;
                if (newScene != null) {
                    Scene scene = newScene;
                    newScene = null;
                    await SceneOpened(scene);
                }
                // request for delayed openning of project to allow loading of action objects and their actions
            } else if (openProject) {
                openProject = false;
                if (newProject != null && newScene != null) {
                    Scene scene = newScene;
                    Project project = newProject;
                    newScene = null;
                    newProject = null;
                    ProjectOpened(scene, project);
                }
                // request for delayed openning of package to allow loading of action objects and their actions
            } else if (openPackage) {
                openPackage = false;
                UpdatingPackageState = true;
                UpdatePackageState(newPackageState);
            }
            if (nextPackageState != null && !UpdatingPackageState && (Instance.GetGameState() == GameStateEnum.PackageRunning || Instance.GetGameState() == GameStateEnum.LoadingPackage || Instance.GetGameState() == GameStateEnum.ClosingPackage)) {
                UpdatingPackageState = true;
                UpdatePackageState(nextPackageState);
                nextPackageState = null;
            }
            // request for delayed openning of main screen to allow loading of action objects and their actions
            if (openMainScreenRequest && ActionsManager.Instance.ActionsReady) {
                openMainScreenRequest = false;
                await OpenMainScreen(openMainScreenData.What, openMainScreenData.Highlight);
            }
            if (openPackageRunningScreenFlag && GetGameState() != GameStateEnum.PackageRunning) {
                openPackageRunningScreenFlag = false;
                OpenPackageRunningScreen();
            }

        }

        /// <summary>
        /// Holds connection status and invokes callback when status changed
        /// </summary>
        public ConnectionStatusEnum ConnectionStatus {
            get => connectionStatus; set {
                if (connectionStatus != value) {
                    OnConnectionStatusChanged(value);
                }
            }
        }

        public bool UpdatingPackageState {
            get;
            private set;
        }

        //TODO: use onvalidate in all scripts to check if everything sets correctly - it allows to check in editor
        private void OnValidate() {
            Debug.Assert(LoadingScreen != null);
        }

        /// <summary>
        /// Returns current game state
        /// </summary>
        /// <returns>Current game state</returns>
        public GameStateEnum GetGameState() {
            return gameState;
        }

        /// <summary>
        /// Change game state and invoke coresponding event
        /// </summary>
        /// <param name="value">New game state</param>
        public void SetGameState(GameStateEnum value) {
            gameState = value;            
            OnGameStateChanged?.Invoke(this, new GameStateEventArgs(gameState));            
        }

        /// <summary>
        /// Change editor state and enable / disable UI elements based on the new state
        /// and invoke corresponding event
        /// </summary>
        /// <param name="newState">New state</param>
        public void SetEditorState(EditorStateEnum newState) {
            editorState = newState;
            OnEditorStateChanged?.Invoke(this, new EditorStateEventArgs(newState));
            switch (newState) {
                // when normal state, enable main menu button and status panel
                case EditorStateEnum.Normal:
                    EditorHelper.EnableCanvasGroup(MainMenuBtnCG, true);
                    break;
                // otherwise, disable main menu button and status panel
                default:
                    EditorHelper.EnableCanvasGroup(MainMenuBtnCG, false);
                    break;
            }
        }

        /// <summary>
        /// Returns editor state
        /// </summary>
        /// <returns>Editor state</returns>
        public EditorStateEnum GetEditorState() {
            return editorState;
        }

        /// <summary>
        /// Switch editor to one of selecting modes (based on request type) and promts user
        /// to select object / AP / etc. 
        /// </summary>
        /// <param name="requestType">Determines what the user should select</param>
        /// <param name="callback">Action which is called when object is selected and (optionaly) validated</param>
        /// <param name="message">Message displayed to the user</param>
        /// <param name="validationCallback">Action to be called when user selects object. If returns true, callback is called,
        /// otherwise waits for selection of another object</param>
        public async Task RequestObject(EditorStateEnum requestType, Action<object> callback, string message, Func<object, Task<RequestResult>> validationCallback = null, UnityAction onCancelCallback = null) {
            // only for "selection" requests
            Debug.Assert(requestType != EditorStateEnum.Closed &&
                requestType != EditorStateEnum.Normal &&
                requestType != EditorStateEnum.InteractionDisabled);
            SetEditorState(requestType);

            SelectorMenu.Instance.PointsToggle.SetInteractivity(false);
            SelectorMenu.Instance.ActionsToggle.SetInteractivity(false);
            SelectorMenu.Instance.IOToggle.SetInteractivity(false);
            SelectorMenu.Instance.ObjectsToggle.SetInteractivity(false);
            SelectorMenu.Instance.OthersToggle.SetInteractivity(false);
            SelectorMenu.Instance.RobotsToggle.SetInteractivity(false);

            // "disable" non-relevant elements to simplify process for the user
            switch (requestType) {
                case EditorStateEnum.SelectingActionObject:
                    SelectorMenu.Instance.RobotsToggle.SetInteractivity(true);
                    SelectorMenu.Instance.ObjectsToggle.SetInteractivity(true);
                    SceneManager.Instance.EnableAllActionObjects(true, true);
                    ProjectManager.Instance.EnableAllActionPoints(false);
                    ProjectManager.Instance.EnableAllActions(false);
                    ProjectManager.Instance.EnableAllOrientations(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(false);
                    break;
                case EditorStateEnum.SelectingActionOutput:
                    ProjectManager.Instance.EnableAllActionPoints(true);
                    ProjectManager.Instance.EnableAllActions(true);
                    SceneManager.Instance.EnableAllActionObjects(false);
                    ProjectManager.Instance.EnableAllOrientations(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(false);
                    break;
                case EditorStateEnum.SelectingActionInput:
                    ProjectManager.Instance.EnableAllActionPoints(true);
                    ProjectManager.Instance.EnableAllActions(true);
                    SceneManager.Instance.EnableAllActionObjects(false);
                    ProjectManager.Instance.EnableAllOrientations(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(false);
                    break;
                case EditorStateEnum.SelectingActionPointParent:
                    SelectorMenu.Instance.RobotsToggle.SetInteractivity(true);
                    SelectorMenu.Instance.ObjectsToggle.SetInteractivity(true);
                    SelectorMenu.Instance.PointsToggle.SetInteractivity(true);
                    ProjectManager.Instance.EnableAllActions(false);
                    ProjectManager.Instance.EnableAllOrientations(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(false);
                    SceneManager.Instance.EnableAllActionObjects(true, true);
                    ProjectManager.Instance.EnableAllActionPoints(true);
                    break;
                case EditorStateEnum.SelectingAPOrientation:
                    ProjectManager.Instance.EnableAllActions(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(false);
                    SceneManager.Instance.EnableAllActionObjects(true, true);
                    ProjectManager.Instance.EnableAllActionPoints(true);
                    ProjectManager.Instance.EnableAllOrientations(true);
                    break;
                case EditorStateEnum.SelectingEndEffector:
                    ProjectManager.Instance.EnableAllActions(false);
                    if (SceneManager.Instance.SceneStarted)
                        await ProjectManager.Instance.EnableAllRobotsEE(true);
                    SceneManager.Instance.EnableAllActionObjects(false, false);
                    SceneManager.Instance.EnableAllRobots(true);
                    ProjectManager.Instance.EnableAllActionPoints(false);
                    ProjectManager.Instance.EnableAllOrientations(false);
                    break;
            }
            ObjectCallback = callback;
            ObjectValidationCallback = validationCallback;
            // display info for user and bind cancel callback,


            if (onCancelCallback == null) {
                SelectObjectInfo.Show(message, () => CancelSelection());
            } else {

                SelectObjectInfo.Show(message,
                    () => {
                        onCancelCallback();
                        CancelSelection();
                    });
            }
        }

        /// <summary>
        /// Method called to cancel selection process. Calls selection callback with null to inform
        /// that nothing was selected
        /// </summary>
        public void CancelSelection() {
            if (ObjectCallback != null) {
                // invoke selection callbeck with null to inform "caller" that nothing was selected
                ObjectCallback.Invoke(null);
                ObjectCallback = null;
            }
            SelectorMenu.Instance.PointsToggle.SetInteractivity(true);
            SelectorMenu.Instance.ActionsToggle.SetInteractivity(true);
            SelectorMenu.Instance.IOToggle.SetInteractivity(true);
            SelectorMenu.Instance.ObjectsToggle.SetInteractivity(true);
            SelectorMenu.Instance.OthersToggle.SetInteractivity(true);
            SelectorMenu.Instance.RobotsToggle.SetInteractivity(true);
            SetEditorState(EditorStateEnum.Normal);
            SelectObjectInfo.gameObject.SetActive(false);
            RestoreFilters();
        }

        /// <summary>
        /// Enables / disables interactive objects which are not part of scene or project
        /// </summary>
        /// <param name="enable"></param>
        public void EnableServiceInteractiveObjects(bool enable) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            VRModeManager.Instance.ARCameraVis.GetComponent<InteractiveObject>().Enable(enable);
#endif
        }

        /// <summary>
        /// The object which was selected calls this method to inform game manager about it.
        /// Validation and potentionally selection callbacks are called and editor is set to normal state.
        /// </summary>
        /// <param name="selectedObject"></param>
        public async void ObjectSelected(object selectedObject) {
            // if validation callbeck is specified, check if this object is valid
            if (ObjectValidationCallback != null) {
                RequestResult result = await ObjectValidationCallback.Invoke(selectedObject);
                if (!result.Success) {
                    Notifications.Instance.ShowNotification(result.Message, "");
                    return;
                }
                
            }
            SelectorMenu.Instance.PointsToggle.SetInteractivity(true);
            SelectorMenu.Instance.ActionsToggle.SetInteractivity(true);
            SelectorMenu.Instance.IOToggle.SetInteractivity(true);
            SelectorMenu.Instance.ObjectsToggle.SetInteractivity(true);
            SelectorMenu.Instance.OthersToggle.SetInteractivity(true);
            SelectorMenu.Instance.RobotsToggle.SetInteractivity(true);
            SetEditorState(EditorStateEnum.Normal);
            // hide selection info 
            SelectObjectInfo.gameObject.SetActive(false);
            RestoreFilters();
            // invoke selection callback
            if (ObjectCallback != null)
                ObjectCallback.Invoke(selectedObject);
            ObjectCallback = null;
        }

        /// <summary>
        /// Enables all visual elements (objects, actions etc.)
        /// </summary>
        private void RestoreFilters() {
            SelectorMenu.Instance.UpdateFilters();
        }

        /// <summary>
        /// Sets framerate to default value (30fps)
        /// </summary>
        public void SetDefaultFramerate() {
            Application.targetFrameRate = 30;
        }

        /// <summary>
        /// Sets framerate to higher value (120fps) for demanding operations
        /// </summary>
        public void SetTurboFramerate() {
            Application.targetFrameRate = 120;
        }

        /// <summary>
        /// Sets initial state of app
        /// </summary>
        private void Awake() {
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            OpenDisconnectedScreen();
        }

        /// <summary>
        /// Binds events and sets initial state of app
        /// </summary>
        private void Start() {
            SetDefaultFramerate();
            UpdatingPackageState = false;
            nextPackageState = null;
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            ARSession.enabled = false;
#endif
            Scene.SetActive(false);
            if (Application.isEditor || Debug.isDebugBuild) {
                AutomationMaster.Initialize();
            }
            ActionsManager.Instance.OnActionsLoaded += OnActionsLoaded;
            CommunicationManager.Instance.Client.ConnectionOpened += CommunicationManager.SafeEventHandler(OnConnected);
            CommunicationManager.Instance.Client.ConnectionClosed += CommunicationManager.SafeEventHandler<WebSocketCloseEventArgs>(OnDisconnected);
            CommunicationManager.Instance.Client.ShowMainScreen += CommunicationManager.SafeEventHandler<ShowMainScreenEventArgs>(OnShowMainScreen);
            CommunicationManager.Instance.Client.ProjectRemoved += CommunicationManager.SafeEventHandler<BareProjectEventArgs>(OnProjectRemoved);
            CommunicationManager.Instance.Client.ProjectBaseUpdated += CommunicationManager.SafeEventHandler<BareProjectEventArgs>(OnProjectBaseUpdated);
            CommunicationManager.Instance.Client.SceneRemoved += CommunicationManager.SafeEventHandler<BareSceneEventArgs>(OnSceneRemoved);
            CommunicationManager.Instance.Client.SceneBaseUpdated +=
                CommunicationManager.SafeEventHandler<BareSceneEventArgs>(OnSceneBaseUpdated);
        }

        /// <summary>
        /// Waits until websocket is disconnected and calls callback method (because after application pause disconnecting isn't finished completely)
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator WaitUntilWebsocketFullyDisconnected(UnityAction callback) {
            yield return new WaitWhile(() => CommunicationManager.Instance.Client.ConnectionState != WebSocketState.Closed);
            callback();
        }

#if (UNITY_ANDROID || UNITY_IOS)

        /// <summary>
        /// Manages connection to server when app is paused or gains focus again
        /// </summary>
        /// <param name="pause"></param>
        private void OnApplicationPause(bool pause) {
            if (pause) {
                if (connectionStatus == ConnectionStatusEnum.Connected) {
                    CommunicationManager.Instance.Client.CloseAsync();
                }
            } else { //automatically connect again
                StartCoroutine(WaitUntilWebsocketFullyDisconnected(() => LandingScreen.Instance.ConnectToServer(false)));
            }
        }
#endif


        private void OnSceneBaseUpdated(object sender, BareSceneEventArgs args) {
            foreach (ListScenesResponseData s in Scenes) {
                if (s.Id == args.Data.Id) {
                    s.Name = args.Data.Name;
                    s.Modified = args.Data.Modified;
                    break;
                }
            }
        }


        private void OnSceneRemoved(object sender, BareSceneEventArgs args) {
            int i = 0;
            foreach (ListScenesResponseData s in Scenes) {
                if (s.Id == args.Data.Id) {
                    Scenes.RemoveAt(i);
                    break;
                }
                i++;
            }
        }


        private void OnProjectBaseUpdated(object sender, BareProjectEventArgs args) {
            foreach (ListProjectsResponseData p in Projects) {
                if (p.Id == args.Data.Id) {
                    p.Name = args.Data.Name;
                    p.Modified = args.Data.Modified;
                    break;
                }
            }            
        }

        /// <summary>
        /// Invoked when project removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">ID of removed object</param>
        private void OnProjectRemoved(object sender, BareProjectEventArgs args) {
            int i = 0;
            foreach (ListProjectsResponseData p in Projects) {
                if (p.Id == args.Data.Id) {
                    Projects.RemoveAt(i);
                    break;
                }
                i++;
            }
        }

        /// <summary>
        /// Event called when request to open main screen come from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void OnShowMainScreen(object sender, ShowMainScreenEventArgs args) {
            if (ActionsManager.Instance.ActionsReady)
                await OpenMainScreen(args.Data.What, args.Data.Highlight);
            else {
                openMainScreenRequest = true;
                openMainScreenData = args.Data;
            }
                
        }

        /// <summary>
        /// Event called when disconnected from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDisconnected(object sender, EventArgs e) {
            
        }

        /// <summary>
        /// Event called when connected to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnConnected(object sender, EventArgs args) {
            // initialize when connected to the server
            ExecutingAction = null;
            ConnectionStatus = ConnectionStatusEnum.Connected;
        }

        /// <summary>
        /// Event called when connections status chanched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnConnectionStatusChanged(ConnectionStatusEnum newState) {
            switch (newState) {
                case ConnectionStatusEnum.Connected:
                    SystemInfoResponseData systemInfo;
                    try {
                        systemInfo = (await CommunicationManager.Instance.Client.GetSystemInfoAsync()).Data;
                        await CommunicationManager.Instance.Client.RegisterUserAsync(new RegisterUserRequestArgs(LandingScreen.Instance.Username.text));
                    } catch (Arcor2ConnectionException ex) {
                        DisconnectFromSever();
                        Notifications.Instance.ShowNotification("Connection failed", ex.Message);
                        return;
                    }
                    if (!CheckApiVersion(systemInfo)) {
                        DisconnectFromSever();
                        return;
                    }

                    SystemInfo = systemInfo;
                    ServerVersion.text = "Editor version: " + Application.version +
                        "\nServer version: " + systemInfo.VarVersion;
                    ConnectionInfo.text = CommunicationManager.Instance.Client.Uri!.ToString();
                    MainMenu.Instance.gameObject.SetActive(false);


                    OnConnectedToServer?.Invoke(this, new StringEventArgs(CommunicationManager.Instance.Client.Uri!.ToString()));

                    await UpdateActionObjects();
                    await UpdateRobotsMeta();

                    try {
                        await Task.Run(() => ActionsManager.Instance.WaitUntilActionsReady(15000));
                    } catch (TimeoutException e) {
                        Notifications.Instance.ShowNotification("Connection failed", "Some actions were not loaded within timeout");
                        DisconnectFromSever();
                        return;
                    }

                    connectionStatus = newState;
                    break;
                case ConnectionStatusEnum.Disconnected:
                    connectionStatus = ConnectionStatusEnum.Disconnected;
                    OpenDisconnectedScreen();
                    OnDisconnectedFromServer?.Invoke(this, EventArgs.Empty);
                    Projects = new List<ListProjectsResponseData>();
                    Scenes = new List<ListScenesResponseData>();

                    ProjectManager.Instance.DestroyProject();
                    SceneManager.Instance.DestroyScene();
                    Scene.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Shows loading screen
        /// </summary>
        /// <param name="text">Optional text for user</param>
        /// <param name="forceToHide">Sets if HideLoadingScreen needs to be run with force flag to
        /// hide loading screen. Used to avoid flickering when several actions with own loading
        /// screen management are chained.</param>
        public void ShowLoadingScreen(string text = "Loading...", bool forceToHide = false) {
            Debug.Assert(LoadingScreen != null);
            // HACK to make loading screen in foreground
            // TODO - find better way
            headUpCanvas.enabled = false;
            headUpCanvas.enabled = true;
            LoadingScreen.Show(text, forceToHide);
        }

        /// <summary>
        /// Hides loading screen
        /// </summary>
        /// <param name="force">Specify if hiding has to be forced. More details in ShowLoadingScreen</param>
        public void HideLoadingScreen(bool force = false) {
            Debug.Assert(LoadingScreen != null);
            LoadingScreen.Hide(force);
        }

        /// <summary>
        /// Connects to server
        /// </summary>
        /// <param name="domain">hostname or IP address</param>
        /// <param name="port">Port of ARServer</param>
        public async void ConnectToSever(string domain, int port) {
            CommunicationManager.Instance.ReloadClient();
            Instance.ConnectionStatus = ConnectionStatusEnum.Connecting;
            ShowLoadingScreen("Connecting to server");
            OnConnectingToServer?.Invoke(this, new StringEventArgs($"ws://{domain}:{port}"));
            try {
                await CommunicationManager.Instance.Client.ConnectAsync(domain, Convert.ToUInt16(port));
            } catch (UriFormatException ex) {
                Notifications.Instance.ShowNotification("Failed to parse domain", ex.Message);
                Instance.ConnectionStatus = ConnectionStatusEnum.Disconnected;
                Instance.HideLoadingScreen(true);
            } catch (Exception ex) { // 99% bad connection related
                Notifications.Instance.ShowNotification("Error while connecting:", ex.Message);
                Instance.ConnectionStatus = ConnectionStatusEnum.Disconnected;
                Instance.HideLoadingScreen(true);
            }
        }

        /// <summary>
        /// Disconnects from server
        /// </summary>
        public void DisconnectFromSever() {
            ConnectionStatus = ConnectionStatusEnum.Disconnected;
            CommunicationManager.Instance.Client.CloseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// When actions are loaded, enables all menus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActionsLoaded(object sender, EventArgs e) {
            MainMenu.Instance.gameObject.SetActive(true);
        }

        /// <summary>
        /// Updates action objects and their actions from server
        /// </summary>
        /// <param name="highlightedObject">When set, object with this ID will gets highlighted for a few seconds in menu
        /// to inform user about it</param>
        /// <returns></returns>
        public async Task UpdateActionObjects() {
            try {
                var result = await CommunicationManager.Instance.Client.GetObjectTypesAsync();
                if (!result.Result) {
                    Notifications.Instance.SaveLogs("Failed to update action objects");
                    Instance.DisconnectFromSever();
                    return;
                }
                ActionsManager.Instance.UpdateObjects(result.Data);
            } catch (Arcor2ConnectionException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to update action objects");
                Instance.DisconnectFromSever();
            }
            
        }

        /// <summary>
        /// Updates robot metadata from server
        /// </summary>
        /// <returns></returns>
        private async Task UpdateRobotsMeta() {
            ActionsManager.Instance.UpdateRobotsMetadata((await CommunicationManager.Instance.Client.GetRobotMetaAsync()).Data);
        }

      
        /// <summary>
        /// When package runs failed with exception, show notification to the user
        /// </summary>
        /// <param name="data"></param>
        internal void HandleProjectException(ProjectExceptionData data) {
            Notifications.Instance.ShowNotification("Project exception", data.Message);
        }

        /// <summary>
        /// Display result of called action to the user
        /// </summary>
        /// <param name="data"></param>
        internal void HandleActionResult(ActionResultData data) {
            if (data.Error != null)
                Notifications.Instance.ShowNotification("Action execution failed", data.Error);
            else {
                string res = "";
                if (data.Results != null && data.Results.Count > 0) {
                    res = "Result: " + data.Results[0];
                }
                Notifications.Instance.ShowNotification("Action execution finished sucessfully", res);
            }
            ExecutingAction = null;
            OnActionExecutionFinished?.Invoke(this, EventArgs.Empty);
            // Stop previously running action (change its color to default)
            if (ActionsManager.Instance.CurrentlyRunningAction != null)
                ActionsManager.Instance.CurrentlyRunningAction.StopAction();
        }

        /// <summary>
        /// Inform the user that action execution was canceled
        /// </summary>
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

        /// <summary>
        /// Highlights currently executed action and invoke coresponding event
        /// </summary>
        /// <param name="actionId"></param>
        internal void HandleActionExecution(string actionId) {
            ExecutingAction = actionId;
            OnActionExecution?.Invoke(this, new ActionExecutionEventArgs(new ActionExecutionData(ExecutingAction)));
            Action puck = ProjectManager.Instance.GetAction(actionId);
            if (puck == null)
                return;

            ActionsManager.Instance.CurrentlyRunningAction = puck;
            // Run current action (set its color to running)
            puck.RunAction();
        }

        /// <summary>
        /// Create visual elements of opened scene and open scene editor
        /// </summary>
        /// <param name="scene">Scene desription from the server</param>
        /// <returns></returns>
        internal async Task SceneOpened(Scene scene) {
            SetGameState(GameStateEnum.LoadingScene);
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

        /// <summary>
        /// Create visual elements of opened scene and project and open project editor
        /// </summary>
        /// <param name="project">Project desription from the server</param>
        /// <returns></returns>
        internal async void ProjectOpened(Scene scene, Project project) {
            var state = GetGameState();
            if (!ActionsManager.Instance.ActionsReady) {
                newProject = project;
                newScene = scene;
                openProject = true;
                return;
            }
            if (GetGameState() == GameStateEnum.SceneEditor) {
                SetEditorState(EditorStateEnum.InteractionDisabled);
                SceneManager.Instance.DestroyScene();
            }
            SetGameState(GameStateEnum.LoadingProject);
            try {
                if (!await SceneManager.Instance.CreateScene(scene, true)) {
                    Notifications.Instance.SaveLogs(scene, project, "Failed to initialize scene");
                    Debug.LogError("wft");
                    HideLoadingScreen();
                    return;
                }
                if (await ProjectManager.Instance.CreateProject(project, true)) {
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

        /// <summary>
        /// Callback called when state of currently executed package change
        /// </summary>
        /// <param name="state">New state:
        /// - running - the package is runnnig
        /// - paused - the package was paused
        /// - stopped - the package was stopped</param>
        public void PackageStateUpdated(PackageStateData state) {
            if (!UpdatingPackageState) {
                nextPackageState = null;
                UpdatingPackageState = true;
                UpdatePackageState(state);
            }
            else
                nextPackageState = state;
        }

        public async void UpdatePackageState(PackageStateData state) {

            if (state.State == PackageStateData.StateEnum.Running ||
                state.State == PackageStateData.StateEnum.Paused) {
                if (!ActionsManager.Instance.ActionsReady || PackageInfo == null) {
                    newPackageState = state;
                    openPackage = true;
                    return;
                }
                Debug.LogError(state);
                if (!ProjectManager.Instance.Valid) {
                    try {
                        SetGameState(GameStateEnum.LoadingPackage);
                        WaitUntilPackageReady(5000);
                        if (PackageInfo == null) {
                            UpdatingPackageState = false;
                            return;
                        }
                        if (!await SceneManager.Instance.CreateScene(PackageInfo.Scene, false, PackageInfo.CollisionModels)) {
                            Notifications.Instance.SaveLogs(PackageInfo.Scene, PackageInfo.Project, "Failed to initialize scene");
                            UpdatingPackageState = false;
                            return;
                        }
                        if (PackageInfo == null) {
                            UpdatingPackageState = false;
                            return;
                        }
                        if (!await ProjectManager.Instance.CreateProject(PackageInfo.Project, false)) {
                            Notifications.Instance.SaveLogs(PackageInfo.Scene, PackageInfo.Project, "Failed to initialize project");
                        }
                        if (PackageInfo == null) {
                            UpdatingPackageState = false;
                            return;
                        }
                        Debug.LogError("done");
                        openPackageRunningScreenFlag = true;
                        if (state.State == PackageStateData.StateEnum.Paused) {
                            OnPausePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, PackageInfo.PackageName));
                        } else {
                            OnResumePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, PackageInfo.PackageName));
                        }
                        if (!string.IsNullOrEmpty(ActionRunningOnStartupId)) {
                            try {
                                Action action = ProjectManager.Instance.GetAction(ActionRunningOnStartupId);
                                ActionsManager.Instance.CurrentlyRunningAction = action;
                                action.RunAction();
                            } catch (ItemNotFoundException) {

                            } finally {
                                ActionRunningOnStartupId = null;
                            }

                        }
                    } catch (TimeoutException ex) {
                        Debug.LogError(ex);
                        Notifications.Instance.SaveLogs(null, null, "Failed to initialize project");
                    } finally {
                        UpdatingPackageState = false;
                    }
                } else if (state.State == PackageStateData.StateEnum.Paused) {
                    OnPausePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, PackageInfo.PackageName));
                    HideLoadingScreen();
                    UpdatingPackageState = false;
                } else if (state.State == PackageStateData.StateEnum.Running) {
                    OnResumePackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, PackageInfo.PackageName));
                    HideLoadingScreen();
                    UpdatingPackageState = false;
                }


            } else if (state.State == PackageStateData.StateEnum.Stopping) {

                UpdatingPackageState = false;
            } else if (state.State == PackageStateData.StateEnum.Stopped) {
                SetGameState(GameStateEnum.ClosingPackage);
                PackageInfo = null;
                ShowLoadingScreen("Stopping package...");
                ProjectManager.Instance.DestroyProject();
                SceneManager.Instance.DestroyScene();
                UpdatingPackageState = false;
                OnStopPackage?.Invoke(this, new EventArgs());
                UpdatingPackageState = false;
                SetGameState(GameStateEnum.None);
                Debug.LogError("stopped");
            }

            UpdatingPackageState = false;
        }

        /// <summary>
        /// Callback when scene was closed
        /// </summary>
        internal void SceneClosed() {
            SetGameState(GameStateEnum.ClosingScene);
            ShowLoadingScreen();
            SceneManager.Instance.DestroyScene();
            OnCloseScene?.Invoke(this, EventArgs.Empty);
            SetGameState(GameStateEnum.None);
        }

        /// <summary>
        /// Callback when project was closed
        /// </summary>
        internal void ProjectClosed() {
            SetGameState(GameStateEnum.ClosingProject);
            ShowLoadingScreen();
            ProjectManager.Instance.DestroyProject();
            SceneManager.Instance.DestroyScene();
            OnCloseProject?.Invoke(this, EventArgs.Empty);
            SetGameState(GameStateEnum.None);
        }

        /// <summary>
        /// Get scene id based on its name
        /// </summary>
        /// <param name="name">Name of scene</param>
        /// <returns>Scene ID</returns>
        public string GetSceneId(string name) {
            foreach (ListScenesResponseData scene in Scenes) {
                if (name == scene.Name)
                    return scene.Id;
            }
            throw new Arcor2ConnectionException("No scene with name: " + name);
        }

        /// <summary>
        /// Get project id based on its name
        /// </summary>
        /// <param name="name">Name of project</param>
        /// <returns>Project ID</returns>
        public string GetProjectId(string name) {
            foreach (ListProjectsResponseData project in Projects) {
                if (name == project.Name)
                    return project.Id;
            }
            throw new Arcor2ConnectionException("No project with name: " + name);
        }

        public void InvokeScenesListChanged() {
            OnScenesListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void InvokeProjectsListChanged() {
            OnProjectsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void InvokePackagesListChanged() {
            OnPackagesListChanged?.Invoke(this, EventArgs.Empty);
        }
             
        /// <summary>
        /// Gets package by ID
        /// </summary>
        /// <param name="id">Id of package</param>
        /// <returns>Package with corresponding ID</returns>
        public PackageSummary GetPackage(string id) {
            foreach (PackageSummary package in Packages) {
                if (id == package.Id)
                    return package;
            }
            throw new ItemNotFoundException("Package does not exist");
        }

        /// <summary>
        /// Asks server to save scene
        /// </summary>
        /// <returns></returns>
        public async Task<SaveSceneResponse> SaveScene() {
            ShowLoadingScreen("Saving scene...");
            SaveSceneResponse response = await CommunicationManager.Instance.Client.SaveSceneAsync();
            HideLoadingScreen();
            return response;
        }

        /// <summary>
        /// Asks server to save project
        /// </summary>
        /// <returns></returns>
        public async Task SaveProject() {
            ShowLoadingScreen("Saving project...");
            var response = await CommunicationManager.Instance.Client.SaveProjectAsync();
            HideLoadingScreen();
            if (response.Result) {
                OnSaveProject?.Invoke(this, EventArgs.Empty);
            } else {
                response.Messages.ForEach(Debug.LogError);
                Notifications.Instance.ShowNotification("Failed to save project", response.Messages.Count > 0 ? response.Messages[0] : "");
                return;
            }
        }

        /// <summary>
        /// Asks server to open project
        /// </summary>
        /// <param name="id">Project id</param>
        public async void OpenProject(string id) {
            ShowLoadingScreen();
            try {
                await CommunicationManager.Instance.Client.OpenProjectAsync(new IdArgs(id));
                await Task.Run(() => WaitForProjectReady(5000));
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to open project", ex.Message);
                HideLoadingScreen();
            } catch (TimeoutException e) {
                Notifications.Instance.ShowNotification("Open project failed", "Failed to load project");
                HideLoadingScreen();
            } 
        }

        /// <summary>
        /// Asks server to open scene
        /// </summary>
        /// <param name="id">Scene id</param>
        public async Task OpenScene(string id) {
            ShowLoadingScreen();
            try {
                await CommunicationManager.Instance.Client.OpenSceneAsync(new IdArgs(id));
            } catch (Arcor2ConnectionException e) {
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

        /// <summary>
        /// Asks server to run package
        /// </summary>
        /// <param name="id">Project id</param>
        public async Task<bool> RunPackage(string packageId) {
            ShowLoadingScreen();
            try {
                await CommunicationManager.Instance.Client.RunPackageAsync(new RunPackageRequestArgs(packageId));
                return true;
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to run project", ex.Message);
                HideLoadingScreen();
                return false;
            } 
        }

        /// <summary>
        /// Builds package from currently opened project
        /// </summary>
        /// <param name="name">Name of the new package</param>
        /// <returns></returns>
        public async Task<string> BuildPackage(string name) {
            ShowLoadingScreen();
            Debug.Assert(ProjectManager.Instance.ProjectMeta != null);
            if (ProjectManager.Instance.ProjectChanged) {
                Notifications.Instance.ShowNotification("Unsaved changes", "There are some unsaved changes in project. Save it before build the package.");
                HideLoadingScreen();
                throw new Arcor2ConnectionException("Unsaved changes");
            }
            try {
                var response = await CommunicationManager.Instance.Client.BuildProjectAsync(new BuildProjectRequestArgs(ProjectManager.Instance.ProjectMeta!.Id, name));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to build package", string.Join(',', response.Messages));
                    throw new Arcor2ConnectionException();
                }
                return response.Data.PackageId;
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to build package", ex.Message);
                throw;
            } finally {
                HideLoadingScreen();
            }
        }

        

        /// <summary>
        /// Asks server to pause running package
        /// </summary>
        public async Task<bool> PausePackage() {
            ShowLoadingScreen();
            try {
                var response = await CommunicationManager.Instance.Client.PausePackageAsync();
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to pause project", string.Join(',', response.Messages));
                    HideLoadingScreen();
                    return false;
                }

                return true;
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to pause project", ex.Message);
                HideLoadingScreen();
                return false;
            }
        }


        /// <summary>
        /// Asks server to resume paused package
        /// </summary>
        public async Task<bool> ResumePackage() {
            ShowLoadingScreen();
            try {
                var response = await CommunicationManager.Instance.Client.ResumePackageAsync();
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to resume project", string.Join(',', response.Messages));
                    HideLoadingScreen();
                    return false;
                }

                return true;
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to resume project", ex.Message);
                HideLoadingScreen();
                return false;
            }
        }

        /// <summary>
        /// Asks server to create new object type
        /// </summary>
        /// <param name="objectType">Description of object type</param>
        /// <returns></returns>
        public async Task<bool> CreateNewObjectType(ObjectTypeMeta objectType) {
            ShowLoadingScreen();
            try {
                var response = await CommunicationManager.Instance.Client.AddNewObjectTypeAsync(objectType, false);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to create new object type", string.Join(',', response.Messages));
                    return false;
                }

                return true;
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to create new object type", ex.Message);
                return false;
            } finally {
                HideLoadingScreen();
            }
        }

        /// <summary>
        /// Will quit the app
        /// </summary>
        public void ExitApp() => Application.Quit();

        public async void UpdateActionPointPositionUsingRobot(string actionPointId, string robotId, string arm_id, string endEffectorId) {

            try {
                var response = await CommunicationManager.Instance.Client.UpdateActionPointUsingRobotAsync(new UpdateActionPointUsingRobotRequestArgs(actionPointId, new RobotArg(robotId, endEffectorId, arm_id)));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to update action point", string.Join(',', response.Messages));
                }

            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to update action point", ex.Message);
            }
        }

        /// <summary>
        /// Asks server to create new project
        /// </summary>
        /// <param name="name">Name of the new project</param>
        /// <param name="sceneId">Id of scene (UUID)</param>
        /// <param name="hasLogic">Whether or not to allow user to define connections of actions in editor
        /// and thus define logical flow of the progeam
        /// <returns></returns>
        public async Task NewProject(string name, string sceneId, bool hasLogic) {
            ShowLoadingScreen("Creating new project...");
            Debug.Assert(sceneId != null && sceneId != "");
            Debug.Assert(name != null && name != "");
            
            try {
                var response = await CommunicationManager.Instance.Client.AddNewProjectAsync(new NewProjectRequestArgs(sceneId, name, "", hasLogic), false);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to create project", string.Join(',', response.Messages));
                    HideLoadingScreen();
                }

            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to create project", e.Message);
                HideLoadingScreen();
            } finally {
            }        
        }

        /// <summary>
        /// Asks server to create new scene
        /// </summary>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if scene was successfully created, false otherwise</returns>
        public async Task<bool> NewScene(string name) {
            ShowLoadingScreen("Creating new scene...");

            if (name == "") {
                Notifications.Instance.ShowNotification("Failed to create new scene", "Scene name not defined");
                HideLoadingScreen();
                return false;
            }
            try {
                var response = await CommunicationManager.Instance.Client.AddNewSceneAsync(new NewSceneRequestArgs(name, ""));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to create new scene", string.Join(',', response.Messages));
                    HideLoadingScreen();
                }
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to create new scene", e.Message);
                HideLoadingScreen();
            }
            return true;
        }
        /// <summary>
        /// Asks server to close currently opened scene
        /// </summary>
        /// <param name="force">True if the server should close scene with unsaved changes</param>
        /// <param name="dryRun">Only check if the scene could be closed without forcing</param>
        /// <returns>True if request was successfull. If not, message describing error is attached</returns>
        public async Task<RequestResult> CloseScene(bool force, bool dryRun = false) {
            if (!dryRun)
                ShowLoadingScreen();
            try {
                var response = await CommunicationManager.Instance.Client.CloseSceneAsync(new CloseSceneRequestArgs(force), dryRun);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to close scene", string.Join(',', response.Messages));
                    HideLoadingScreen();
                    return (false, string.Join(',', response.Messages));
                }
                return (true, "");
            } catch (Arcor2ConnectionException ex) {
                if (!dryRun && force) {
                    Notifications.Instance.ShowNotification("Failed to close scene", ex.Message);
                    HideLoadingScreen();                   
                }
                return (false, ex.Message);
            }          
            
        }

        /// <summary>
        /// Asks server to close currently opened project
        /// </summary>
        /// <param name="force">True if the server should close project with unsaved changes</param>
        /// <param name="dryRun">Only check if the project could be closed without forcing</param>
        /// <returns></returns>
        public async Task<RequestResult> CloseProject(bool force, bool dryRun = false) {
            if (!dryRun)
                ShowLoadingScreen("Closing project");
            try {
                var response = await CommunicationManager.Instance.Client.CloseProjectAsync(new CloseProjectRequestArgs(force), dryRun);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to close project", string.Join(',', response.Messages));
                    HideLoadingScreen();
                    return (false, string.Join(',', response.Messages));
                }
                return (true, "");
            } catch (Arcor2ConnectionException ex) {
                if (!dryRun && force) {
                    Notifications.Instance.ShowNotification("Failed to close project", ex.Message);
                    HideLoadingScreen();
                }                
                return (false, ex.Message);
            }           
            
        }

        /// <summary>
        /// Asks server to cancel exection of action
        /// </summary>
        /// <returns>True if request successfull</returns>
        public async Task<bool> CancelExecution() {
            try {
                var response = await CommunicationManager.Instance.Client.CancelActionAsync();
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to cancel action", string.Join(',', response.Messages));
                    return false;
                }
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to cancel action", ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses version string and returns major version
        /// </summary>
        /// <param name="versionString">Version string in format 0.0.0 (major, minor, patch)</param>
        /// <returns>First number (major version)</returns>
        public int GetMajorVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[0]);
        }

        /// <summary>
        /// Parses version string and returns minor version
        /// </summary>
        /// <param name="versionString">Version string in format 0.0.0 (major, minor, patch)</param>
        /// <returns>Second number (minor version)</returns>
        public int GetMinorVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[1]);
        }

        /// <summary>
        /// Parses version string and returns patch version
        /// </summary>
        /// <param name="versionString">Version string in format 0.0.0 (major, minor, patch)</param>
        /// <returns>Last number (patch version)</returns>
        public int GetPatchVersion(string versionString) {
            return int.Parse(SplitVersionString(versionString)[2]);
        }

        /// <summary>
        /// Splits version string and returns list of components
        /// </summary>
        /// <param name="versionString">Version string in format 0.0.0 (major.minor.patch)</param>
        /// <returns>List of components of the version string</returns>
        public List<string> SplitVersionString(string versionString) {
            List<string> version = Enumerable.ToList<string>(versionString.Split('.'));
            Debug.Assert(version.Count == 3, versionString);
            return version;
        }

        /// <summary>
        /// Checks if api version of the connected server is compatibile with editor
        /// </summary>
        /// <param name="systemInfo">Version string in format 0.0.0 (major.minor.patch)</param>
        /// <returns>True if versions are compatibile</returns>
        public bool CheckApiVersion(SystemInfoResponseData systemInfo) {
            
            if (systemInfo.ApiVersion == ApiVersion)
                return true;

            if (GetMajorVersion(systemInfo.ApiVersion) != GetMajorVersion(ApiVersion) ||
                (GetMajorVersion(systemInfo.ApiVersion) == 0 && GetMinorVersion(systemInfo.ApiVersion) != GetMinorVersion(ApiVersion))) {
                Notifications.Instance.ShowNotification("Incompatibile api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion);
                return false;
            }
            Notifications.Instance.ShowNotification("Different api versions", "Editor API version: " + ApiVersion + ", server API version: " + systemInfo.ApiVersion + ". It can cause problems, you have been warned.");

            return true;
        }

        /// <summary>
        /// Waits until scene is loaded
        /// </summary>
        /// <param name="timeout">TimeoutException is thrown after timeout ms when scene is not loaded</param>
        public void WaitForSceneReady(int timeout) {
            Stopwatch sw = new();
            sw.Start();
            while (SceneManager.Instance.SceneMeta == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(100);
            }
            return;
        }

        /// <summary>
        /// Waits until project is loaded
        /// </summary>
        /// <param name="timeout">TimeoutException is thrown after timeout ms when project is not loaded</param>
        public void WaitForProjectReady(int timeout) {
            Stopwatch sw = new();
            sw.Start();
            while (ProjectManager.Instance.ProjectMeta == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(100);
            }
            return;
        }

        /// <summary>
        /// Opens main screen
        /// </summary>
        /// <param name="what">Defines what list should be displayed (scenes/projects/packages)</param>
        /// <param name="highlight">ID of element to highlight (e.g. when scene is closed, it is highlighted for a few seconds</param>
        /// <returns></returns>
        public async Task OpenMainScreen(ShowMainScreenData.WhatEnum what, string highlight) {

#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            ARSession.enabled = false;
#endif
            Scene.SetActive(false);
            MainMenu.Instance.Close();
            switch (what) {
                case ShowMainScreenData.WhatEnum.PackagesList:
                    MainScreen.Instance.SwitchToPackages();
                    
                    break;
                case ShowMainScreenData.WhatEnum.ScenesList:
                    MainScreen.Instance.SwitchToScenes();
                    break;
                case ShowMainScreenData.WhatEnum.ProjectsList:
                    MainScreen.Instance.SwitchToProjects();
                    break;
            }
            if (!string.IsNullOrEmpty(highlight)) {
                MainScreen.Instance.HighlightTile(highlight);
            }
            SetGameState(GameStateEnum.MainScreen);
            OnOpenMainScreen?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Closed);
            HideLoadingScreen();
        }

        /// <summary>
        /// Opens scene editor
        /// </summary>
        public void OpenSceneEditor() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            ARSession.enabled = true;
            if (CalibrationManager.Instance.Calibrated) {
                Scene.SetActive(true);
            }
#else
            Scene.SetActive(true);
#endif
            AREditorResources.Instance.LeftMenuScene.DeactivateAllSubmenus();
            MainMenu.Instance.Close();
            SetGameState(GameStateEnum.SceneEditor);
            OnOpenSceneEditor?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Normal);
            HideLoadingScreen(true);
        }

        /// <summary>
        /// Opens project editor
        /// </summary>
        public void OpenProjectEditor() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            ARSession.enabled = true;
            if (CalibrationManager.Instance.Calibrated) {
                Scene.SetActive(true);
            }
#else
            Scene.SetActive(true);
#endif
            AREditorResources.Instance.LeftMenuProject.DeactivateAllSubmenus();
            MainMenu.Instance.Close();
            SetGameState(GameStateEnum.ProjectEditor);
            OnOpenProjectEditor?.Invoke(this, EventArgs.Empty);
            SetEditorState(EditorStateEnum.Normal);
            HideLoadingScreen(true);
        }

        /// <summary>
        /// Opens package running screen
        /// </summary>
        public async void OpenPackageRunningScreen() {
            openPackageRunningScreenFlag = false;
            try {
                MainMenu.Instance.Close();
                SetGameState(GameStateEnum.PackageRunning);
                SetEditorState(EditorStateEnum.InteractionDisabled);
                EditorHelper.EnableCanvasGroup(MainMenuBtnCG, true);
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
                ARSession.enabled = true;
                if (CalibrationManager.Instance.Calibrated) {
                    Scene.SetActive(true);
                }
#else
                Scene.SetActive(true);
#endif

                if (PackageInfo == null)
                    return;
                OnRunPackage?.Invoke(this, new ProjectMetaEventArgs(PackageInfo.PackageId, PackageInfo.PackageName));
            } catch (TimeoutException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to open package run screen", "Package info did not arrived");
            } finally {
                HideLoadingScreen(true);
            }
        }

        /// <summary>
        /// Waits until package is loaded
        /// </summary>
        /// <param name="timeout">TimeoutException is thrown after timeout ms when package is not loaded</param>
        public void WaitUntilPackageReady(int timeout) {
            Stopwatch sw = new();
            sw.Start();
            while (PackageInfo == null) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Opens disconnected screen
        /// </summary>
        public void OpenDisconnectedScreen() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
            ARSession.enabled = false;
#endif
            MainMenu.Instance.Close();
            Scene.SetActive(false);
            SetGameState(GameStateEnum.Disconnected);
            HideLoadingScreen(true);
        }

        /// <summary>
        /// Activates/Disactivates the Scene and calls all necessary methods (Selector menu update).
        /// </summary>
        /// <param name="active"></param>
        public void SceneSetActive(bool active) {
            Scene.SetActive(active);
        }

        /// <summary>
        /// Helper method to create button
        /// </summary>
        /// <param name="parent">Parent GUI element</param>
        /// <param name="label">Label of the button</param>
        /// <returns>Created button</returns>
        public Button CreateButton(Transform parent, string label) {
            GameObject btnGO = Instantiate(Instance.ButtonPrefab, parent);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMP_Text>().text = label;
            return btn;
        }

        /// <summary>
        /// Adds action point to the project
        /// </summary>
        /// <param name="name">Name of new action point</param>
        /// <param name="parent">Parent object (global AP if parent is null)</param>
        /// <returns></returns>
        public async Task<bool> AddActionPoint(string name, IActionPointParent parent) {
            try {
                Vector3 point;
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
                if (parent == null) {
                    point = TransformConvertor.UnityToROS(Scene.transform.InverseTransformPoint(ray.GetPoint(0.5f)));
                } else {
                    point = TransformConvertor.UnityToROS(parent.GetTransform().InverseTransformPoint(ray.GetPoint(0.5f)));
                }                
                Position position = DataHelper.Vector3ToPosition(point);
                var response = await CommunicationManager.Instance.Client.AddActionPointAsync(new AddActionPointRequestArgs(name, position, parent == null ? "" : parent.GetId()));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to add action point", string.Join(',', response.Messages));
                    return false;
                }

                return true;
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to add action point", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates parent of action point
        /// </summary>
        /// <param name="actionPoint">Action point to be updated</param>
        /// <param name="parentId">ID of new parent</param>
        /// <returns>True if renamed, false otherwise</returns>
        public async Task<bool> UpdateActionPointParent(ActionPoint actionPoint, string parentId) {
            try {
                var response = await CommunicationManager.Instance.Client.UpdateActionPointParentAsync(new UpdateActionPointParentRequestArgs(actionPoint.Data.Id, parentId));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to update action point parent", string.Join(',', response.Messages));
                    return false;
                }

                return true;
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to update action point parent", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets name of project based on its ID
        /// </summary>
        /// <param name="projectId">ID of project</param>
        /// <returns>Name of project</returns>
        public string GetProjectName(string projectId) {
            foreach (ListProjectsResponseData project in Projects) {
                if (project.Id == projectId)
                    return project.Name;
            }
            throw new ItemNotFoundException("Project with id: " + projectId + " not found");
        }

        /// <summary>
        /// Gets name of scene based on its ID
        /// </summary>
        /// <param name="sceneId">ID of scene</param>
        /// <returns>Name of scene</returns>
        public string GetSceneName(string sceneId) {
            foreach (ListScenesResponseData scene in Scenes) {
                if (scene.Id == sceneId)
                    return scene.Name;
            }
            throw new ItemNotFoundException("Scene with id: " + sceneId + " not found");
        }

        public List<InteractiveObject> GetAllInteractiveObjects() {
            return FindObjectsOfType<InteractiveObject>().OrderBy(o => o.GetName()).ToList();
        }

    }

    /// <summary>
    /// Universal struct for getting result of requests. 
    /// </summary>
    public struct RequestResult {
        /// <summary>
        /// Whether the request was successfull or not
        /// </summary>
        public bool Success;
        /// <summary>
        /// Empty when success is true, otherwise contains error description
        /// </summary>
        public string Message;

        public RequestResult(bool success, string message) {
            Success = success;
            Message = message;
        }

        public RequestResult(bool success) {
            Success = success;
            Message = "";
        }

        public override bool Equals(object obj) {
            return obj is RequestResult other &&
                   Success == other.Success &&
                   Message == other.Message;
        }

        public override int GetHashCode() {
            int hashCode = 151515764;
            hashCode = hashCode * -1521134295 + Success.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            return hashCode;
        }

        public void Deconstruct(out bool success, out string message) {
            success = Success;
            message = Message;
        }

        public static implicit operator (bool success, string message)(RequestResult value) {
            return (value.Success, value.Message);
        }

        public static implicit operator RequestResult((bool success, string message) value) {
            return new RequestResult(value.success, value.message);
        }

        
    }
}
