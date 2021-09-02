using System;
using UnityEngine.UI;
using Base;
using System.Collections;
using IO.Swagger.Model;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using static Base.GameManager;

public class LeftMenuProject : LeftMenu
{

    public ButtonWithTooltip SetActionPointParentButton, AddActionButton, AddActionButton2, RunButton, RunButton2,
        AddConnectionButton, AddConnectionButton2, BuildPackageButton, AddActionPointUsingRobotButton, AddActionPointButton,
        AddActionPointButton2, CopyButton, ActionPointAimingMenuButton;

    public InputDialog InputDialog;
    public AddNewActionDialog AddNewActionDialog;

    private string selectAPNameWhenCreated = "";
    protected override void Update() {
        base.Update();
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
    private const string COPY_LABEL = "Duplicate object";
    private const string ACTION_POINT_AIMING_LABEL = "Open action point aiming menu";
    private const string ADD_ACTION_POINT_USING_ROBOT_LABEL = "Add action point using robot";
    

    protected override void Start() {
#if !AR_ON
        AddActionPointButton.SetInteractivity(true);
        AddActionPointButton2.SetInteractivity(true);
#endif
        Base.ProjectManager.Instance.OnProjectSavedSatusChanged += OnProjectSavedStatusChanged;
        Base.GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;

        GameManager.Instance.OnActionExecution += OnActionExecutionEvent;
        GameManager.Instance.OnActionExecutionCanceled += OnActionExecutionEvent;
        GameManager.Instance.OnActionExecutionFinished += OnActionExecutionEvent;
    }


    private void OnActionExecutionEvent(object sender, EventArgs args) {
        UpdateBtns();
    }

    protected override void Awake() {
        base.Awake();
       
    }

    protected override void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor) {
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
            }
            else
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
            } else {
                SetActionPointParentButton.SetInteractivity(obj is ActionPoint3D, $"{SET_ACTION_POINT_PARENT_LABEL}\n(selected object is not action point)");
                if (obj is ActionPoint3D) {
                    AddActionButton.SetInteractivity(ProjectManager.Instance.AnyAvailableAction, $"{ADD_ACTION_LABEL}\n(no actions available)");
                    AddActionButton2.SetInteractivity(ProjectManager.Instance.AnyAvailableAction, $"{ADD_ACTION_LABEL}\n(no actions available)");
                } else {
                    AddActionButton.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(selected object is not action point)");
                    AddActionButton2.SetInteractivity(false, $"{ADD_ACTION_LABEL}\n(selected object is not action point)");
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
                if (obj is ActionPoint3D) {
                    CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(checking...)");
                    WebsocketManager.Instance.CopyActionPoint(obj.GetId(), null, obj.GetName(), CopyActionPointDryRunCallback, true);
                } else {
                    CopyButton.SetInteractivity(obj is Base.Action && !(obj is StartEndAction), $"{COPY_LABEL}\n(selected object cannot be duplicated)");
                }
                if (!MainSettingsMenu.Instance.ConnectionsSwitch.IsOn()) {
                    AddConnectionButton.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(connections are hidden)");
                    AddConnectionButton2.SetInteractivity(false, $"{ADD_CONNECTION_LABEL}\n(connections are hidden)");
                } else {
                    AddConnectionButton.SetInteractivity(obj.GetType() == typeof(PuckInput) ||
                        obj.GetType() == typeof(PuckOutput), $"{ADD_CONNECTION_LABEL}\n(selected object is not input or output of an action)");
                    AddConnectionButton2.SetInteractivity(obj.GetType() == typeof(PuckInput) ||
                        obj.GetType() == typeof(PuckOutput), $"{ADD_CONNECTION_LABEL}\n(selected object is not input or output of an action)");
                    if (obj is InputOutput inputOutput) {
                        if (inputOutput.GetLogicItems().Count > 0) {
                            AddConnectionButton.SetDescription(ADD_CONNECTION_LABEL);
                            AddConnectionButton2.SetDescription(ADD_CONNECTION_LABEL);
                        } else {
                            AddConnectionButton.SetDescription(EDIT_CONNECTION_LABEL);
                            AddConnectionButton2.SetDescription(EDIT_CONNECTION_LABEL);
                        }
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
                    RunButton.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_TEMP_PACKAGE_LABEL}\n({runBtnInteractivity})");
                    RunButton2.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), $"{RUN_TEMP_PACKAGE_LABEL}\n({runBtnInteractivity})");
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

    public override void DeactivateAllSubmenus(bool unlock = true) {
        base.DeactivateAllSubmenus(unlock);

        AddActionButton.GetComponent<Image>().enabled = false;
        AddActionButton2.GetComponent<Image>().enabled = false;
        ActionPointAimingMenuButton.GetComponent<Image>().enabled = false;
        if (ActionPickerMenu.Instance.IsVisible())
            ActionPickerMenu.Instance.Hide(unlock);
        if (ActionParametersMenu.Instance.IsVisible())
            ActionParametersMenu.Instance.Hide();
        if (ActionPointAimingMenu.Instance.IsVisible())
            _ = ActionPointAimingMenu.Instance.Hide(unlock);
    }

    private void OnOpenProjectEditor(object sender, EventArgs eventArgs) {
        UpdateBtns();
    }

    

    public void SaveProject() {
        SaveButton.SetInteractivity(false, "Saving project...");
        Base.GameManager.Instance.SaveProject();        
    }

    public async void BuildPackage(string name) {
        try {
            await Base.GameManager.Instance.BuildPackage(name);
            InputDialog.Close();
            Notifications.Instance.ShowToastMessage("Package was built sucessfully.");
        } catch (Base.RequestFailedException ex) {

        }

    }


    public async void RunProject() {
        GameManager.Instance.ShowLoadingScreen("Running project", true);
        try {
            await Base.WebsocketManager.Instance.TemporaryPackage();
            MainMenu.Instance.Close();
        } catch (RequestFailedException ex) {
            Base.Notifications.Instance.ShowNotification("Failed to run temporary package", "");
            Debug.LogError(ex);
            GameManager.Instance.HideLoadingScreen(true);
        }
    }

    public void ShowBuildPackageDialog() {
        InputDialog.Open("Build package",
                         "",
                         "Package name",
                         Base.ProjectManager.Instance.ProjectMeta.Name + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"),
                         () => BuildPackage(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }


    private void OnProjectSavedStatusChanged(object sender, EventArgs e) {
       UpdateBuildAndSaveBtns();
    }
    

    public override async void UpdateBuildAndSaveBtns() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor)
            return;
        if (CurrentSubmenuOpened != LeftMenuSelection.Home)
            return;
        
        BuildPackageButton.SetInteractivity(false, $"Build package\n(checking...)");
        SaveButton.SetInteractivity(false, "Save project\n(checking...)");
        CloseButton.SetInteractivity(false, "Close project\n(checking...)");
        if (SceneManager.Instance.SceneStarted) {
            WebsocketManager.Instance.StopScene(true, StopSceneCallback);
        } else {
            CloseButton.SetInteractivity(true);
        }

        if (!ProjectManager.Instance.ProjectChanged) {
            BuildPackageButton.SetInteractivity(true);            
            SaveButton.SetInteractivity(false, "Save project\n(there are no unsaved changes)");
        } else {
            WebsocketManager.Instance.SaveProject(true, SaveProjectCallback);
            BuildPackageButton.SetInteractivity(false, "Build package\n(there are unsaved changes on project)");
            //RunButton.SetInteractivity(false, "There are unsaved changes on project");
            //RunButton2.SetInteractivity(false, "There are unsaved changes on project");
        }
    }

    private void StopSceneCallback(string _, string data) {
        CloseProjectResponse response = JsonConvert.DeserializeObject<CloseProjectResponse>(data);
        if (response.Messages != null) {
            CloseButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
        } else {
            CloseButton.SetInteractivity(response.Result);
        }
    }

    protected void SaveProjectCallback(string _, string data) {
        SaveProjectResponse response = JsonConvert.DeserializeObject<SaveProjectResponse>(data);
        if (response.Messages != null) {
            SaveButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
        } else {
            SaveButton.SetInteractivity(response.Result);
        }
    }
    private void CopyActionPointDryRunCallback(string _, string data) {
        CopyActionPointResponse response = JsonConvert.DeserializeObject<CopyActionPointResponse>(data);
        if (response.Result) {
            CopyButton.SetInteractivity(true);
        } else {
            CopyButton.SetInteractivity(false, response.Messages.FirstOrDefault());
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

    public void CopyObjectClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            selectAPNameWhenCreated = selectedObject.GetName() + "_copy";
            WebsocketManager.Instance.CopyActionPoint(selectedObject.GetId(), null, selectedObject.GetName(), CopyActionPointCallback);            
        } else if (selectedObject is Base.Action action) {
            AddNewActionDialog.InitFromAction(action);
            AddNewActionDialog.Open();
        }
    }

    private void CopyActionPointCallback(string actionPointName, string data) {
        CopyActionPointResponse response = JsonConvert.DeserializeObject<CopyActionPointResponse>(data);
        if (response.Result) {
            Notifications.Instance.ShowToastMessage($"Action point {actionPointName} was duplicated");
        } else {
            Notifications.Instance.ShowNotification("Failed to duplicate action point", response.Messages.FirstOrDefault());
        }
    }

    public async void AddConnectionClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if ((selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput))) {
            if (!await ((InputOutput) selectedObject).Action.WriteLock(false))
                return;
            
            ((InputOutput) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
    }


    public async void AddActionClick() {
        //was clicked the button in favorites or settings submenu?
        Button clickedButton = AddActionButton.Button;
        if (CurrentSubmenuOpened == LeftMenuSelection.Favorites) {
            clickedButton = AddActionButton2.Button;
        }

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionPickerMenu.Instance.Hide();
        } else {
            if (await ActionPickerMenu.Instance.Show((Base.ActionPoint) selectedObject)) {
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

    public void AddActionPointUsingRobotClick() {
        string armId = null;
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            OpenRobotSelector(AddActionPointUsingRobotClick);
            return;
        }
        if (SceneManager.Instance.SelectedRobot.MultiArm())
            armId = SceneManager.Instance.SelectedArmId;
        CreateGlobalActionPointUsingRobot(ProjectManager.Instance.GetFreeAPName("global"),
            SceneManager.Instance.SelectedRobot.GetId(),
            SceneManager.Instance.SelectedEndEffector.EEId,
            armId);
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

        WebsocketManager.Instance.AddActionPointUsingRobot(name, eeId, robotId, false, AddActionPointUsingRobotCallback, armId);
        selectAPNameWhenCreated = name;
    }


    protected void AddActionPointUsingRobotCallback(string nothing, string data) {
        AddApUsingRobotResponse response = JsonConvert.DeserializeObject<AddApUsingRobotResponse>(data);
        GameManager.Instance.HideLoadingScreen();
        if (response.Result) {
            Notifications.Instance.ShowToastMessage("Action point created");
        } else {
            Notifications.Instance.ShowNotification("Failed to add action point", response.Messages.FirstOrDefault());
        }
    }

    public async void ActionPointAimingClick() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !ActionPointAimingMenuButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (ActionPointAimingMenuButton.GetComponent<Image>().enabled) {
            ActionPointAimingMenuButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            _ = ActionPointAimingMenu.Instance.Hide(true);
        } else {
            bool opened = false;

            if (selectedObject is ActionPoint3D actionPoint) {
                opened = await ActionPointAimingMenu.Instance.Show(actionPoint);
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
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor &&
            MainMenu.Instance.CurrentState() == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
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
        (bool success, _) = await Base.GameManager.Instance.CloseProject(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            ConfirmationDialog.Open("Close project",
                         "Are you sure you want to close current project? Unsaved changes will be lost and system will go offline (if online).",
                         () => CloseProject(),
                         () => ConfirmationDialog.Close());
        }

    }

    public async void CloseProject() {
        if (SceneManager.Instance.SceneStarted)
            WebsocketManager.Instance.StopScene(false, null);
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
                Debug.LogError("START");
                RunProject();
            } else if (selectedObject is Action3D action) {
                action.ActionBeingExecuted = true;
                await WebsocketManager.Instance.ExecuteAction(selectedObject.GetId(), false);
                // TODO: enable stop execution (_ = GameManager.Instance.CancelExecution();)
                action.ActionBeingExecuted = false;
            } else if (selectedObject.GetType() == typeof(APOrientation)) {
                
                //await WebsocketManager.Instance.MoveToActionPointOrientation(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId(), 0.5m, selectedObject.GetId(), false);
            } 
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
            return;
        }
        
    }
}
