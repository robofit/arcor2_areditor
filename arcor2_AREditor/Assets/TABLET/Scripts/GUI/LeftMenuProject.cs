using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;
using Action = Base.Action;

public class LeftMenuProject : LeftMenu {

    public ButtonWithTooltip SetActionPointParentButton, AddActionButton, AddActionButton2, RunButton, RunButton2,
        AddConnectionButton, AddConnectionButton2, BuildPackageButton, AddActionPointUsingRobotButton, AddActionPointButton,
        AddActionPointButton2, ActionPointAimingMenuButton, RunDebugButton;

    public InputDialog InputDialog;

    private string selectAPNameWhenCreated = "";
    protected void Update() {
        if (ProjectManager.Instance.ProjectMeta != null)
            EditorInfo.text = "Project: \n" + ProjectManager.Instance.ProjectMeta.Name;
    }



    private const string SET_ACTION_POINT_PARENT_LABEL = "Set action point parent";
    private const string ADD_ACTION_LABEL = "Add action";
    private const string ADD_CONNECTION_LABEL = "Add connection";
    private const string EDIT_CONNECTION_LABEL = "Edit connection";
    private const string RUN_ACTION_LABEL = "Execute action";
    private const string RUN_ACTION_OR_PACKAGE_LABEL = "Execute action or temporary package";
    private const string RUN_TEMP_PACKAGE_LABEL = "Create and execute temporary package";
    private const string ADD_ACTION_POINT_GLOBAL_LABEL = "Add global action point";
    private const string ADD_ACTION_POINT_LABEL = "Add action point";
    private const string ACTION_POINT_AIMING_LABEL = "Open action point aiming menu";
    private const string ADD_ACTION_POINT_USING_ROBOT_LABEL = "Add action point using robot";
    private const string RUN_DEBUG_LABEL = "Run package in debug mode";
    private const string RUN_DEBUG_OR_TRIGGER_BREAKPOINT_LABEL = "Run package in debug mode or trigger breakpoint on AP";
    private const string TRIGGER_BREAKPOINT_ON_LABEL = "Add breakpoint";
    private const string TRIGGER_BREAKPOINT_OFF_LABEL = "Remove breakpoint";


    protected override void Start() {
#if !AR_ON
        AddActionPointButton.SetInteractivity(true);
        AddActionPointButton2.SetInteractivity(true);
#endif
        ProjectManager.Instance.OnProjectSavedSatusChanged += OnProjectSavedStatusChanged;
        GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;

        GameManager.Instance.OnActionExecution += OnActionExecutionEvent;
        GameManager.Instance.OnActionExecutionCanceled += OnActionExecutionEvent;
        GameManager.Instance.OnActionExecutionFinished += OnActionExecutionEvent;

        base.Start();
    }


    private void OnActionExecutionEvent(object sender, EventArgs args) {
        UpdateBtns();
    }

    protected override void Awake() {
        base.Awake();

    }

    protected override void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (GameManager.Instance.GetGameState() == GameStateEnum.ProjectEditor) {
            base.OnSceneStateEvent(sender, args);
            UpdateBtns();
        }

    }

    protected void OnEnable() {
        ProjectManager.Instance.OnActionPointAddedToScene += OnActionPointAddedToScene;

    }

    protected void OnDisable() {
        ProjectManager.Instance.OnActionPointAddedToScene -= OnActionPointAddedToScene;
    }

    private void OnActionPointAddedToScene(object sender, ActionPointEventArgs args) {
        if (!string.IsNullOrEmpty(selectAPNameWhenCreated) && args.ActionPoint.GetName().Contains(selectAPNameWhenCreated)) {
            SelectorMenu.Instance.SetSelectedObject(args.ActionPoint, true);
            selectAPNameWhenCreated = "";
            RenameClick(true);
        }
    }


    protected async override Task UpdateBtns(InteractiveObject obj) {
        try {
            if (CanvasGroup.alpha == 0) {
                previousUpdateDone = true;
                return;
            }

            await base.UpdateBtns(obj);
#if UNITY_ANDROID && AR_ON
            if (!CalibrationManager.Instance.Calibrated && !TrackingManager.Instance.IsDeviceTracking()) {
                SetActionPointParentButton.SetInteractivity(false, $"{SET_ACTION_POINT_PARENT_LABEL}\n(AR not calibrated)");
                AddActionButton.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(AR not calibrated)");
                AddActionButton2.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(AR not calibrated)");
                AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(AR not calibrated)");
                AddConnectionButton2.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(AR not calibrated)");
                RunButton.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n(AR not calibrated)");
                RunButton2.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n(AR not calibrated)");
                AddActionPointButton.SetInteractivity(false, $"{ADD_ACTION_POINT_LABEL}\n(AR not calibrated)");
                AddActionPointButton2.SetInteractivity(false, $"{ADD_ACTION_POINT_LABEL}\n(AR not calibrated)");
                CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(AR not calibrated");
                ActionPointAimingMenuButton.SetInteractivity(false, $"{ACTION_POINT_AIMING_LABEL}\n(AR not calibrated)");
            } else
#endif
            if (requestingObject || obj == null) {
                SetActionPointParentButton.SetInteractivity(false, $"{SET_ACTION_POINT_PARENT_LABEL}\n(no action point is selected)");
                AddActionButton.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(no action point is selected)");
                AddActionButton2.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(no action point is selected)");
                AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(no input / output is selected)");
                AddConnectionButton2.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(no input / output is selected)");
                RunButton.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n(select action to execute it or START to run project)");
                RunButton2.SetInteractivity(false, RunButton.GetAlternativeDescription());
                AddActionPointButton.SetInteractivity(true);
                AddActionPointButton2.SetInteractivity(true);
                AddActionPointButton.SetDescription(ADD_ACTION_POINT_GLOBAL_LABEL);
                AddActionPointButton2.SetDescription(ADD_ACTION_POINT_GLOBAL_LABEL);
                CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(no object to duplicate selected)");
                ActionPointAimingMenuButton.SetInteractivity(false, $"{ACTION_POINT_AIMING_LABEL}\n(no action point selected)");
                RunDebugButton.SetInteractivity(false, $"{RUN_DEBUG_OR_TRIGGER_BREAKPOINT_LABEL}\n(select action point to trigger breakpoint or START to run debug mode)");
            } else if (obj.IsLocked && obj.LockOwner != LandingScreen.Instance.GetUsername()) {
                SetActionPointParentButton.SetInteractivity(false, $"{SET_ACTION_POINT_PARENT_LABEL}\n(object is used by {obj.LockOwner})");
                AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(object is used by {obj.LockOwner})");
                AddConnectionButton2.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(object is used by {obj.LockOwner})");
                RunButton.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n(object is used by {obj.LockOwner})");
                RunButton2.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n(object is used by {obj.LockOwner})");
                AddActionButton.SetInteractivity(false, $"{ADD_ACTION_POINT_LABEL}\n(object is used by {obj.LockOwner})");
                AddActionButton2.SetInteractivity(false, $"{ADD_ACTION_POINT_LABEL}\n(object is used by {obj.LockOwner})");
                CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(object is used by {obj.LockOwner})");
                ActionPointAimingMenuButton.SetInteractivity(false, $"{ACTION_POINT_AIMING_LABEL}\n(object is used by {obj.LockOwner})");
                RunDebugButton.SetInteractivity(false, $"{RUN_DEBUG_OR_TRIGGER_BREAKPOINT_LABEL}\n(object is used by {obj.LockOwner})");
            } else {
                RunDebugButton.SetInteractivity(false, $"{RUN_DEBUG_OR_TRIGGER_BREAKPOINT_LABEL}\n(select action point to trigger breakpoint or START to run debug mode)");
                SetActionPointParentButton.SetInteractivity(obj is ActionPoint3D, $"{SET_ACTION_POINT_PARENT_LABEL}\n(selected object is not action point)");
                if (obj is ActionPoint3D ap) {
                    AddActionButton.SetInteractivity(ProjectManager.Instance.AnyAvailableAction, $"{ADD_ACTION_LABEL}\n(no actions available)");
                    AddActionButton2.SetInteractivity(ProjectManager.Instance.AnyAvailableAction, $"{ADD_ACTION_LABEL}\n(no actions available)");
                    CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(checking...)");
                    CommunicationManager.Instance.Client
                        .DuplicateActionPointAsync(new CopyActionPointRequestArgs(obj.GetId(), null), true).ContinueWith(CopyActionPointCallback, TaskScheduler.FromCurrentSynchronizationContext());
                    RunDebugButton.SetInteractivity(true);
                    RunDebugButton.SetDescription(ap.BreakPoint ? TRIGGER_BREAKPOINT_OFF_LABEL : TRIGGER_BREAKPOINT_ON_LABEL);
                } else {
                    AddActionButton.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(selected object is not action point)");
                    AddActionButton2.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(selected object is not action point)");
                    CopyButton.SetInteractivity(obj is Action && !(obj is StartEndAction), $"{COPY_LABEL}\n(selected object cannot be duplicated)");
                }

                ActionPointAimingMenuButton.SetInteractivity(obj is ActionPoint3D || obj is APOrientation, $"{ACTION_POINT_AIMING_LABEL}\n(selected object is not action point or orientation)");
                if (obj is IActionPointParent) {
                    AddActionPointButton.SetDescription($"Add AP relative to {obj.GetName()}");
                    AddActionPointButton.SetInteractivity(true);
                } else {
                    AddActionPointButton.SetInteractivity(false, $"{ADD_ACTION_POINT_LABEL}\n(selected object could not be parent of AP");
                }
                AddActionPointButton2.SetInteractivity(AddActionPointButton.IsInteractive(), $"{ADD_ACTION_POINT_LABEL}\n({AddActionPointButton.GetAlternativeDescription()})");
                AddActionPointButton2.SetDescription(AddActionPointButton.GetDescription());

                if (!MainSettingsMenu.Instance.ConnectionsSwitch.IsOn()) {
                    AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(connections are hidden)");
                    AddConnectionButton2.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(connections are hidden)");
                } else {
                    if (obj is Action) {
                        if (obj is EndAction) {
                            AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(end action could not be connected to anything else)");
                        } else {
                            AddConnectionButton.SetInteractivity(true);
                        }
                        AddConnectionButton2.SetInteractivity(AddConnectionButton.IsInteractive(), AddConnectionButton.GetAlternativeDescription());
                    }


                }
                string runBtnInteractivity = null;

                if (obj.GetType() == typeof(Action3D)) {
                    if (!SceneManager.Instance.SceneStarted)
                        runBtnInteractivity = "scene offline";
                    else if (!string.IsNullOrEmpty(GameManager.Instance.ExecutingAction)) {
                        string actionName = ProjectManager.Instance.GetAction(GameManager.Instance.ExecutingAction).GetName();
                        runBtnInteractivity = $"action '{actionName}' running";
                    }
                    RunButton.SetDescription(RUN_ACTION_LABEL);
                    RunButton2.SetDescription(RUN_ACTION_LABEL);
                    RunButton.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_ACTION_LABEL}\n({runBtnInteractivity})");
                    RunButton2.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_ACTION_LABEL}\n({runBtnInteractivity})");
                } else if (obj.GetType() == typeof(StartAction)) {
                    if (!ProjectManager.Instance.ProjectMeta.HasLogic) {
                        runBtnInteractivity = "project without logic could not be started from editor";
                    } else if (ProjectManager.Instance.ProjectChanged) {
                        runBtnInteractivity = "project has unsaved changes";
                    }
                    RunButton.SetDescription(RUN_TEMP_PACKAGE_LABEL);
                    RunButton2.SetDescription(RUN_TEMP_PACKAGE_LABEL);
                    RunDebugButton.SetDescription(RUN_DEBUG_LABEL);
                    RunButton.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_TEMP_PACKAGE_LABEL}\n({runBtnInteractivity})");
                    RunButton2.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_TEMP_PACKAGE_LABEL}\n({runBtnInteractivity})");
                    RunDebugButton.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_DEBUG_LABEL}\n({runBtnInteractivity})");
                } else {
                    runBtnInteractivity = "select action to execute it or START to run project";
                    RunButton.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n({runBtnInteractivity})");
                    RunButton2.SetInteractivity(false, $"{RUN_ACTION_OR_PACKAGE_LABEL}\n({runBtnInteractivity})");
                }


            }

            if (!SceneManager.Instance.SceneStarted) {
                AddActionPointUsingRobotButton.SetInteractivity(false, $"{ADD_ACTION_POINT_USING_ROBOT_LABEL}\n(scene offline");
            } else {
                AddActionPointUsingRobotButton.SetInteractivity(true);
            }
        } finally {
            previousUpdateDone = true;
        }
    }

    private void CopyActionPointCallback(Task<CopyActionPointResponse> task) {
        CopyActionPointResponse response = task.Result;
        if (response.Result) {
            CopyButton.SetInteractivity(true);
        } else {
            CopyButton.SetInteractivity(false, response.Messages.FirstOrDefault());
        }
    }

    public override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();

        AddActionButton.GetComponent<Image>().enabled = false;
        AddActionButton2.GetComponent<Image>().enabled = false;
        ActionPointAimingMenuButton.GetComponent<Image>().enabled = false;
        if (ActionPickerMenu.Instance.IsVisible)
            ActionPickerMenu.Instance.Hide();
        if (ActionParametersMenu.Instance.IsVisible)
            ActionParametersMenu.Instance.Hide();
        if (ActionPointAimingMenu.Instance.IsVisible)
            ActionPointAimingMenu.Instance.Hide();
    }

    private void OnOpenProjectEditor(object sender, EventArgs eventArgs) {
        UpdateBtns();
    }


    public void SaveProject() {
        SaveButton.SetInteractivity(false, "Saving project...");
        GameManager.Instance.SaveProject();
    }

    public async void BuildPackage(string name) {
        try {
            await GameManager.Instance.BuildPackage(name);
            InputDialog.Close();
            Notifications.Instance.ShowToastMessage("Package was built sucessfully.");
        } catch (Arcor2ConnectionException ex) {

        }

    }


    public async void RunProject() {
        GameManager.Instance.ShowLoadingScreen("Running project", true);
        try {
            var response = await CommunicationManager.Instance.Client.RunTemporaryPackageAsync(new TemporaryPackageRequestArgs(breakpoints:new List<string>()));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to run temporary package", string.Join(',', response.Messages));
                Debug.LogError(string.Join(',', response.Messages));
                GameManager.Instance.HideLoadingScreen(true);
                return;
            }

            MainMenu.Instance.Close();
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to run temporary package", ex.Message);
            Debug.LogError(ex);
            GameManager.Instance.HideLoadingScreen(true);
        }
    }

    public void ShowBuildPackageDialog() {
        InputDialog.Open("Build package",
                         "",
                         "Package name",
                         ProjectManager.Instance.ProjectMeta.Name + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"),
                         () => BuildPackage(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }


    private void OnProjectSavedStatusChanged(object sender, EventArgs e) {
        UpdateBuildAndSaveBtns();
    }


    public override async void UpdateBuildAndSaveBtns() {
        if (GameManager.Instance.GetGameState() != GameStateEnum.ProjectEditor)
            return;
        if (CurrentSubmenuOpened != LeftMenuSelection.Home)
            return;

        BuildPackageButton.SetInteractivity(false, $"Build package\n(checking...)");
        SaveButton.SetInteractivity(false, "Save project\n(checking...)");
        CloseButton.SetInteractivity(false, "Close project\n(checking...)");
        if (SceneManager.Instance.SceneStarted) {
            CommunicationManager.Instance.Client.StopSceneAsync(true).ContinueWith(task => {
                StopSceneResponse response = task.Result;
                if (response.Messages != null) {
                    CloseButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
                } else {
                    CloseButton.SetInteractivity(response.Result);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        } else {
            CloseButton.SetInteractivity(true);
        }

        if (!ProjectManager.Instance.ProjectChanged) {
            BuildPackageButton.SetInteractivity(true);
            SaveButton.SetInteractivity(false, "Save project\n(there are no unsaved changes)");
        } else {
            CommunicationManager.Instance.Client.SaveProjectAsync(true).ContinueWith(task => {
                SaveProjectResponse response = task.Result;
                if (response.Messages != null) {
                    SaveButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
                } else {
                    SaveButton.SetInteractivity(response.Result);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            BuildPackageButton.SetInteractivity(false, "Build package\n(there are unsaved changes on project)");
            //RunButton.SetInteractivity(false, "There are unsaved changes on project");
            //RunButton2.SetInteractivity(false, "There are unsaved changes on project");
        }
    }

    /*
    protected void CloseProjectCallback(string nothing, string data) {
        CloseProjectResponse response = JsonConvert.DeserializeObject<CloseProjectResponse>(data);
        if (response.Messages != null) {
            CloseButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
        } else {
            CloseButton.SetInteractivity(response.Result);
        }
    }*/

    public override void CopyObjectClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            selectAPNameWhenCreated = selectedObject.GetName() + "_copy";
            CommunicationManager.Instance.Client.DuplicateActionPointAsync(new CopyActionPointRequestArgs(selectedObject.GetId(), null!)).ContinueWith(
                (task) => {
                    CopyActionPointResponse response = task.Result;
                    if (response.Result) {
                        Notifications.Instance.ShowToastMessage($"Action point {selectedObject.name} was duplicated");
                    } else {
                        Notifications.Instance.ShowNotification("Failed to duplicate action point", response.Messages.FirstOrDefault());
                    }
                });

        } else if (selectedObject is Action action) {
            ActionPickerMenu.Instance.DuplicateAction(action);
        }
    }


    public async void AddConnectionClick() {
        if (SelectorMenu.Instance.GetSelectedObject() is Action action) {
            action.AddConnection();
        }
    }


    public async void AddActionClick() {
        //was clicked the button in favorites or settings submenu?
        Button clickedButton = AddActionButton.Button;
        if (CurrentSubmenuOpened == LeftMenuSelection.Favorites) {
            clickedButton = AddActionButton2.Button;
        }

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionPickerMenu.Instance.Hide();
        } else {
            if (await ActionPickerMenu.Instance.Show(selectedObject, false)) {
                clickedButton.GetComponent<Image>().enabled = true;
                SelectorMenu.Instance.gameObject.SetActive(false);
            } else {
                Notifications.Instance.ShowNotification("Failed to open action picker", "Could not lock action point");
            }

        }
    }

    public void AddActionPointClick() {
        SetActiveSubmenu(CurrentSubmenuOpened);
        if (selectedObject is IActionPointParent parent) {
            CreateActionPoint(ProjectManager.Instance.GetFreeAPName(parent.GetName()), parent);
        } else {
            CreateActionPoint(ProjectManager.Instance.GetFreeAPName("global"), default);
        }
    }

    public async void AddActionPointUsingRobotClick() {
        string armId = null;

        // if any robot is targeted by the cursor, automatically select its end effector
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject != null) {
            if (selectedObject.GetType() == typeof(RobotActionObject)) {
                RobotActionObject selectedRobot = (RobotActionObject) selectedObject;
                List<RobotEE> eeList = await selectedRobot.GetAllEE();
                if (eeList?.Count > 0) {
                    SceneManager.Instance.SelectRobotAndEE(eeList[0]);
                }
            }
        }

        // if not looking at any robot nor any robot is selected, open selector menu and let the user select the robot manually
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            OpenRobotSelector(AddActionPointUsingRobotClick);
            return;
        } else { // open menu
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            CreateGlobalActionPointUsingRobot(ProjectManager.Instance.GetFreeAPName("global"),
            SceneManager.Instance.SelectedRobot.GetId(),
            SceneManager.Instance.SelectedEndEffector.EEId,
            armId);
        }
    }

    /// <summary>
    /// Creates new action point
    /// </summary>
    /// <param name="name">Name of the new action point</param>
    /// <param name="parentId">Id of AP parent. Global if null </param>
    private async void CreateActionPoint(string name, IActionPointParent parentId = null) {
        Debug.Assert(!string.IsNullOrEmpty(name));
        Debug.Assert(parentId != null);
        selectAPNameWhenCreated = name;
        bool result = await GameManager.Instance.AddActionPoint(name, parentId);
        if (result)
            InputDialog.Close();
        else
            selectAPNameWhenCreated = "";
    }


    private void CreateGlobalActionPointUsingRobot(string name, string robotId, string eeId, string armId) {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(robotId) || string.IsNullOrEmpty(eeId)) {
            Notifications.Instance.ShowNotification("Failed to create new AP", "Some required parameter is missing");
            return;
        }

        GameManager.Instance.ShowLoadingScreen("Adding AP...");

        CommunicationManager.Instance.Client.AddActionPointUsingRobotAsync(
            new AddApUsingRobotRequestArgs(robotId, eeId, name, armId)).ContinueWith((task) => {
            AddApUsingRobotResponse response = task.Result;
            GameManager.Instance.HideLoadingScreen();
            if (response.Result) {
                Notifications.Instance.ShowToastMessage("Action point created");
            } else {
                Notifications.Instance.ShowNotification("Failed to add action point",
                    response.Messages.FirstOrDefault());
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
        selectAPNameWhenCreated = name;
    }

    public async void ActionPointAimingClick() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !ActionPointAimingMenuButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (ActionPointAimingMenuButton.GetComponent<Image>().enabled) {
            ActionPointAimingMenuButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionPointAimingMenu.Instance.Hide();
        } else {
            bool opened = false;

            if (selectedObject is ActionPoint3D actionPoint) {
                opened = await ActionPointAimingMenu.Instance.Show(actionPoint, true);
            } else if (selectedObject is APOrientation orientation) {
                opened = await orientation.OpenDetailMenu();
            }
            if (opened) {
                ActionPointAimingMenuButton.GetComponent<Image>().enabled = true;
                SelectorMenu.Instance.gameObject.SetActive(false);
            } else {
                Notifications.Instance.ShowNotification("Failed to open action picker", "Could not lock action point");
            }


        }
    }


    public override void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameStateEnum.ProjectEditor &&
            MainMenu.Instance.CurrentState() == SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
    }

    public override void UpdateVisibility(bool visible, bool force = false) {
        base.UpdateVisibility(visible, force);
        if (GameManager.Instance.GetGameState() == GameStateEnum.ProjectEditor)
            AREditorResources.Instance.StartStopSceneBtn.gameObject.SetActive(visible);
    }

    public async void ShowCloseProjectDialog() {
        (bool success, _) = await GameManager.Instance.CloseProject(false);
        if (!success) {
            string message = "Are you sure you want to close current project? ";
            if (ProjectManager.Instance.ProjectChanged) {
                message += "Unsaved changes will be lost";
                if (SceneManager.Instance.SceneStarted) {
                    message += " and system will go offline";
                }
                message += ".";
            } else if (SceneManager.Instance.SceneStarted) {
                message += "System will go offline.";
            }
            GameManager.Instance.HideLoadingScreen();
            ConfirmationDialog.Open("Close project",
                         message,
                         () => CloseProject(),
                         () => ConfirmationDialog.Close());
        }

    }

    public async void CloseProject() {
        if (SceneManager.Instance.SceneStarted)
            await CommunicationManager.Instance.Client.StopSceneAsync();
        GameManager.Instance.ShowLoadingScreen("Closing project..");
        _ = await GameManager.Instance.CloseProject(true);
        ConfirmationDialog.Close();
        MainMenu.Instance.Close();
        GameManager.Instance.HideLoadingScreen();
    }

    public async void RunClicked() {
        try {
            InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
            if (selectedObject is null)
                return;
            if (selectedObject is StartAction) {
                RunProject();
            } else if (selectedObject is Action3D action) {
                action.ActionBeingExecuted = true;
                var response = await CommunicationManager.Instance.Client.ExecuteActionAsync(new ExecuteActionRequestArgs(selectedObject.GetId()));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to execute action", string.Join(',', response.Messages));
                    return;
                }
                // TODO: enable stop execution (_ = GameManager.Instance.CancelExecution();)
                action.ActionBeingExecuted = false;
            } else if (selectedObject.GetType() == typeof(APOrientation)) {

                //await WebsocketManager.Instance.MoveToActionPointOrientation(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId(), 0.5m, selectedObject.GetId(), false);
            }
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
            return;
        }

    }

    public void RunDebugClicked() {
        if (selectedObject is StartAction)
            ConfirmationDialog.Open("Debug project", "Do you want to pause execution on the first action?", () => RunDebug(true), () => RunDebug(false), "Yes", "No");
        else if (selectedObject is ActionPoint3D actionPoint) {
            actionPoint.BreakPoint = !actionPoint.BreakPoint;
            RunDebugButton.SetDescription(actionPoint.BreakPoint ? TRIGGER_BREAKPOINT_OFF_LABEL : TRIGGER_BREAKPOINT_ON_LABEL);
            RunDebugButton.ForceUpdate();
        }
    }

    public async void RunDebug(bool pause) {
        try {
            ConfirmationDialog.Close();
            GameManager.Instance.ShowLoadingScreen("Starting...");
            var response = await CommunicationManager.Instance.Client.RunTemporaryPackageAsync(new TemporaryPackageRequestArgs(pause, ProjectManager.Instance.GetAllBreakpoints()));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to debug project", string.Join(',', response.Messages));
                GameManager.Instance.HideLoadingScreen();
            }
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to debug project", ex.Message);
            GameManager.Instance.HideLoadingScreen();
        }
    }
}
