using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(CanvasGroup))]
public abstract class LeftMenu : MonoBehaviour {

    public CanvasGroup CanvasGroup;

    public Button FavoritesButton, RobotButton, AddButton, UtilityButton, HomeButton;
    public ButtonWithTooltip MoveButton, MoveButton2, RemoveButton, RenameButton, CalibrationButton,
        OpenMenuButton, RobotSelectorButton, RobotSteppingButton, CloseButton, SaveButton, MainSettingsButton; //Buttons with number 2 are duplicates in favorites submenu
    public GameObject FavoritesButtons, HomeButtons, UtilityButtons, AddButtons, RobotButtons;
    public RenameDialog RenameDialog;
    public RobotSelectorDialog RobotSelector;
    public TMPro.TMP_Text EditorInfo, SelectedObjectText;

    private bool isVisibilityForced = false;
    protected ActionPoint3D selectedActionPoint;
    public LeftMenuSelection CurrentSubmenuOpened;

    public ConfirmationDialog ConfirmationDialog;

    protected InteractiveObject selectedObject = null;
    protected bool selectedObjectUpdated = true, previousUpdateDone = true;

    private string MOVE_BTN_LABEL = "Move object";
    private string REMOVE_BTN_LABEL = "Remove object"; 
    private string RENAME_BTN_LABEL = "Rename object"; 
    private string CALIBRATION_BTN_LABEL = "Calibrate"; 
    private string OPEN_MENU_BTN_LABEL = "Open menu";
    private string ROBOT_STEPPING_MENU_BTN_LABEL = "Robot stepping menu";
    private string ROBOT_SELECTOR_MENU_BTN_LABEL = "Select robot";

    protected virtual void Start() {
        LockingEventsCache.Instance.OnObjectLockingEvent += OnObjectLockingEvent;
        SceneManager.Instance.OnRobotSelected += OnRobotSelected;
        if (MoveButton != null && MoveButton2 != null) {
            MoveButton.SetDescription(MOVE_BTN_LABEL);
            MoveButton2.SetDescription(MOVE_BTN_LABEL);
        }
        if (RemoveButton != null)
            RemoveButton.SetDescription(REMOVE_BTN_LABEL);
        if (RenameButton != null)
            RenameButton.SetDescription(RENAME_BTN_LABEL);
        if (CalibrationButton != null)
            CalibrationButton.SetDescription(CALIBRATION_BTN_LABEL);
        if (OpenMenuButton != null)
            OpenMenuButton.SetDescription(OPEN_MENU_BTN_LABEL);
        if (RobotSteppingButton != null)
            RobotSteppingButton.SetDescription(ROBOT_STEPPING_MENU_BTN_LABEL);
        if (RobotSelectorButton != null)
            RobotSelectorButton.SetDescription(ROBOT_SELECTOR_MENU_BTN_LABEL);
    }

    protected virtual void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        MainMenu.Instance.AddListener(() => OnGameStateChanged(this, null));
    }

    private void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        UpdateBuildAndSaveBtns();
    }

    private void OnRobotSelected(object sender, EventArgs e) {
        UpdateBtns(selectedObject);
    }

    protected virtual void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (args.Event.State == SceneStateData.StateEnum.Stopping) {

            if (TransformMenu.Instance.CanvasGroup.alpha == 1 && TransformMenu.Instance.RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Left) {
                MoveButton.GetComponent<Image>().enabled = false;
                MoveButton2.GetComponent<Image>().enabled = false;
                TransformMenu.Instance.Hide();
                SelectorMenu.Instance.gameObject.SetActive(true);
            }
            if (RobotSteppingMenu.Instance.CanvasGroup.alpha == 1) {
                Debug.LogError(RobotSteppingButton.GetComponent<Image>().enabled);
                RobotSteppingButton.GetComponent<Image>().enabled = false;
                Debug.LogError(RobotSteppingButton.GetComponent<Image>().enabled);
                RobotSteppingMenu.Instance.Hide();
                SelectorMenu.Instance.gameObject.SetActive(true);
            }
            if (RobotSelector.Opened()) {
                RobotSelector.Close(false);
                RobotSelectorButton.GetComponent<Image>().enabled = false;
            }
            UpdateVisibility();
        } else if (args.Event.State == SceneStateData.StateEnum.Started || args.Event.State == SceneStateData.StateEnum.Stopped) {
            UpdateBuildAndSaveBtns();
            UpdateRobotSelectorAndSteppingButtons();
        }
    }



    protected void OnObjectSelectedChangedEvent(object sender, InteractiveObjectEventArgs args) {
        selectedObject = args.InteractiveObject;
        selectedObjectUpdated = true;
    }

    protected void OnGameStateChanged(object sender, GameStateEventArgs args) {
        if (!isVisibilityForced)
            UpdateVisibility();
        if (args != null) {
            if (args.Data == GameStateEnum.SceneEditor || args.Data == GameStateEnum.ProjectEditor || args.Data == GameStateEnum.PackageRunning) {
                UpdateBuildAndSaveBtns();
            } else {
                DeactivateAllSubmenus();
                SetActiveSubmenu(LeftMenuSelection.Favorites);
                isVisibilityForced = false;
                UpdateVisibility();
            }            
        }

    }

    public void UpdateBtns() {
        _ = UpdateBtns(selectedObject);
    }

    protected async virtual Task UpdateBtns(InteractiveObject obj) {

        if (CanvasGroup.alpha == 0)
            return;
        UpdateRobotSelectorAndSteppingButtons();
        if (requestingObject || obj == null) {
            SelectedObjectText.text = "";
            MoveButton.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(no object selected)");
            MoveButton2.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(no object selected)");
            RemoveButton.SetInteractivity(false, $"{REMOVE_BTN_LABEL}\n(no object selected)");
            RenameButton.SetInteractivity(false, $"{RENAME_BTN_LABEL}\n(no object selected)");
            CalibrationButton.SetInteractivity(false, $"{CALIBRATION_BTN_LABEL}\n(no object selected)");
            OpenMenuButton.SetInteractivity(false, $"{OPEN_MENU_BTN_LABEL}\n(no object selected)");
        } else if (obj.IsLocked && obj.LockOwner != LandingScreen.Instance.GetUsername()) {
            SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
            MoveButton.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(object is used by {obj.LockOwner})");
            MoveButton2.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(object is used by {obj.LockOwner})");
            RemoveButton.SetInteractivity(false, $"{REMOVE_BTN_LABEL}\n(object is used by {obj.LockOwner})");
            RenameButton.SetInteractivity(false, $"{RENAME_BTN_LABEL}\n(object is used by {obj.LockOwner})");
            CalibrationButton.SetInteractivity(false, $"{CALIBRATION_BTN_LABEL}\n(object is used by {obj.LockOwner})");
            OpenMenuButton.SetInteractivity(false, $"{OPEN_MENU_BTN_LABEL}\n(object is used by {obj.LockOwner})");
        } else {
            SelectedObjectText.text = obj.GetName() + "\n" + obj.GetObjectTypeName();
            MoveButton.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(loading...)");
            MoveButton2.SetInteractivity(false, $"{MOVE_BTN_LABEL}\n(loading...)");
            RemoveButton.SetInteractivity(false, $"{REMOVE_BTN_LABEL}\n(loading...)");
            Task<RequestResult> tMove = Task.Run(() => obj.Movable());
            Task<RequestResult> tRemove = Task.Run(() => obj.Removable());
            UpdateMoveAndRemoveBtns(selectedObject.GetId(), tMove, tRemove);

            RenameButton.SetInteractivity(obj.GetType() == typeof(ActionPoint3D) ||
                obj.GetType() == typeof(Action3D) || (obj.GetType().IsSubclassOf(typeof(ActionObject)) && !SceneManager.Instance.SceneStarted &&
                GameManager.Instance.GetGameState() == GameStateEnum.SceneEditor) ||
                obj.GetType() == typeof(APOrientation), $"{RENAME_BTN_LABEL}\n(selected object could not be renamed)");
            CalibrationButton.SetInteractivity(obj.GetType() == typeof(Recalibrate) ||
                obj.GetType() == typeof(CreateAnchor) || obj.GetType() == typeof(RecalibrateUsingServer), $"{CALIBRATION_BTN_LABEL}\n(selected object is not calibration cube)");
            if (obj is Action3D action) {
                OpenMenuButton.SetInteractivity(action.Parameters.Count > 0, "Open action parameters menu\n(action has no parameters)");
            } else {
                OpenMenuButton.SetInteractivity(obj.HasMenu(), $"{OPEN_MENU_BTN_LABEL}\n(selected object has no menu)");
            }
        }
    }


    private void UpdateRobotSelectorAndSteppingButtons() {
        if (SceneManager.Instance.IsRobotAndEESelected()) {
            RobotSteppingButton.SetInteractivity(SceneManager.Instance.SceneStarted &&
                    !SceneManager.Instance.GetActionObject(SceneManager.Instance.SelectedRobot.GetId()).IsLockedByOtherUser,
                    SceneManager.Instance.SceneStarted ? $"{ROBOT_STEPPING_MENU_BTN_LABEL}\n(robot used by {SceneManager.Instance.SelectedRobot.LockOwner()})" : $"{ROBOT_STEPPING_MENU_BTN_LABEL}\n(scene offline)");
        } else {
            RobotSteppingButton.SetInteractivity(SceneManager.Instance.SceneStarted, $"{ROBOT_STEPPING_MENU_BTN_LABEL}\n(scene offline)");
        }
        RobotSelectorButton.SetInteractivity(SceneManager.Instance.SceneStarted, $"{ROBOT_SELECTOR_MENU_BTN_LABEL}\n(scene offline)");
    }

    private async void UpdateMoveAndRemoveBtns(string objId, Task<RequestResult> movable, Task<RequestResult> removable) {
        RequestResult move = await movable;
        RequestResult remove = await removable;

        if (selectedObject != null && objId != selectedObject.GetId()) // selected object was updated in the meantime
            return; 
        MoveButton.SetInteractivity(move.Success, $"{MOVE_BTN_LABEL}\n({move.Message})");
        MoveButton2.SetInteractivity(move.Success, $"{MOVE_BTN_LABEL}\n({move.Message})");
        RemoveButton.SetInteractivity(remove.Success, $"{REMOVE_BTN_LABEL}\n({remove.Message})");
    }

    private bool updateButtonsInteractivity = false;

    protected bool requestingObject = false;

    protected void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        switch (args.Data) {
            case GameManager.EditorStateEnum.Normal:
                requestingObject = false;
                updateButtonsInteractivity = true;
                break;
            case GameManager.EditorStateEnum.InteractionDisabled:
                updateButtonsInteractivity = false;
                break;
            case GameManager.EditorStateEnum.Closed:
                updateButtonsInteractivity = false;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingActionPointParent:
            case EditorStateEnum.SelectingAPOrientation:
            case EditorStateEnum.SelectingEndEffector:
                requestingObject = true;
                break;
        }
    }

    protected virtual void Update() {

        if (!updateButtonsInteractivity)
            return;

        /*if (MenuManager.Instance.CheckIsAnyRightMenuOpened()) {
            SetActiveSubmenu(LeftMenuSelection.Favorites);
            RobotButton.interactable = false;
            AddButton.interactable = false;
            UtilityButton.interactable = false;
            HomeButton.interactable = false;
            return;
        }

        RobotButton.interactable = true;
        AddButton.interactable = true;
        UtilityButton.interactable = true;
        HomeButton.interactable = true;
        */

    }

    private async void LateUpdate() {
        if (CanvasGroup.alpha > 0 && selectedObjectUpdated && previousUpdateDone) {
            selectedObjectUpdated = false;
            previousUpdateDone = false;
            await UpdateBtns(selectedObject);
        }
    }

    public void OpenMenuButtonClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        if (!SelectorMenu.Instance.gameObject.activeSelf && !OpenMenuButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (OpenMenuButton.GetComponent<Image>().enabled) {
            OpenMenuButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            selectedObject.CloseMenu();
        } else {
            OpenMenuButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            selectedObject.OpenMenu();
        }
        

    }

    public abstract void UpdateVisibility();

    public virtual void UpdateVisibility(bool visible, bool force = false) {
        isVisibilityForced = force;
        CanvasGroup.interactable = visible;
        CanvasGroup.blocksRaycasts = visible;
        CanvasGroup.alpha = visible ? 1 : 0;
        
        if (visible)
            UpdateBtns();
    }

    public abstract void UpdateBuildAndSaveBtns();

    public void FavoritesButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Favorites);

    }

    public void RobotButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Robot);

    }

    public void AddButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Add);

    }

    public void UtilityButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Utility);
    }

    public void HomeButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Home);
    }

    #region Add submenu button click methods








    #endregion

    #region Robot buttons methods

    public void RobotSelectorButtonClick() {
        OpenRobotSelector();
    }

    protected async void OpenRobotSelector(UnityAction afterSelectionCallback = null) {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot selector", "Scene offline");
            return;
        }

        if (!SelectorMenu.Instance.gameObject.activeSelf && !RobotSelectorButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: true); //close all other opened menus/dialogs and take care of red background of buttons
        }

        Action<object> callback;
        if (afterSelectionCallback == null) {
            callback = selectedObject => SelectEndEffector(selectedObject);
        } else {
            InteractiveObject tempSelectedObject = SelectorMenu.Instance.GetSelectedObject();
            bool manuallySelected = SelectorMenu.Instance.ManuallySelected;
            callback = (returnedObject) => {                
                SelectEndEffector(returnedObject);
                if (tempSelectedObject != null)
                    SelectorMenu.Instance.SetSelectedObject(tempSelectedObject, manuallySelected);
                if (returnedObject != null) {
                    afterSelectionCallback.Invoke();
                }
            };
        }
        System.Collections.Generic.List<RobotEE> EEs = await SceneManager.Instance.GetAllRobotsEEs();
        if (EEs.Count == 0) {
            Notifications.Instance.ShowNotification("Failed to open EE selector", "There is no robot with EE in the scene");
            return;
        } else if (EEs.Count == 1) {
            callback(EEs[0]);
        } else {
            _ = GameManager.Instance.RequestObject(EditorStateEnum.SelectingEndEffector, callback, "Select End Effector", ValidateEndEffector);
        }
    }

    private void SelectEndEffector(object selectedObject) {
        if (selectedObject == null)
            return;
        RobotEE endEffector = (RobotEE) selectedObject;
        SceneManager.Instance.SelectRobotAndEE(endEffector);
        

        Notifications.Instance.ShowToastMessage($"Selected EE {endEffector.GetName()} on robot {SceneManager.Instance.SelectedRobot.GetName()}" + (SceneManager.Instance.SelectedRobot.MultiArm() ? $" (arm {SceneManager.Instance.SelectedArmId})" : ""));
    }

    private async Task<RequestResult> ValidateEndEffector(object selectedInput) {
        if (selectedInput is RobotEE) {
            return new RequestResult(true);
        } else {
            return new RequestResult(false, "Selected object is not end effector");
        }
    }



    public async void RobotSteppingButtonClick() {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot manipulation menu", "Scene offline");
            return;
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            OpenRobotSelector(RobotSteppingButtonClick);
            return;
        }
        if (!SelectorMenu.Instance.gameObject.activeSelf && !RobotSteppingButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: true); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (RobotSteppingButton.GetComponent<Image>().enabled) { //hide menu
            RobotSteppingButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            RobotSteppingMenu.Instance.Hide();
        } else { //open menu
            ActionObject robot = SceneManager.Instance.GetActionObject(SceneManager.Instance.SelectedRobot.GetId());
            if (await robot.WriteLock(false)) {
                RobotSteppingButton.GetComponent<Image>().enabled = true;
                SelectorMenu.Instance.gameObject.SetActive(false);
                RobotSteppingMenu.Instance.Show();
            }
        }
    }

    #endregion

    #region Settings submenu button click methods

    public async void SetActionPointParentClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null || !(selectedObject is ActionPoint3D))
            return;

        if (!SelectorMenu.Instance.gameObject.activeSelf) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        selectedActionPoint = (ActionPoint3D) selectedObject;
        Action<object> action = AssignToParent;
        await selectedActionPoint.WriteLock(false);
        GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionPointParent, action,
            "Select new parent (action object)", ValidateParent, UntieActionPointParent);
    }

    public async void MoveClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        
        //was clicked the button in favorites or settings submenu?
        Button clickedButton;
        if (CurrentSubmenuOpened == LeftMenuSelection.Favorites)
            clickedButton = MoveButton2.Button;
        else
            clickedButton = MoveButton.Button;

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            if (selectedObject.GetType().IsSubclassOf(typeof(StartEndAction))) {
                TransformGizmo.Instance.ClearTargets();
            } else {
                TransformMenu.Instance.Hide();
            }
        } else {
            clickedButton.GetComponent<Image>().enabled = true;
            
            if (selectedObject.GetType().IsSubclassOf(typeof(StartEndAction))) {
                selectedObject.StartManipulation();
            } else {
                if (await selectedObject.WriteLock(true)) {
                    TransformMenu.Instance.Show(selectedObject);
                } else {
                    SetActiveSubmenu(CurrentSubmenuOpened);
                    return;
                }
            }
            SelectorMenu.Instance.gameObject.SetActive(false);
        }

    }

    public void RenameClick() {
        RenameClick(false);   
    }

    public void RenameClick(bool removeOnCancel, UnityAction confirmCallback = null) {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        if (!SelectorMenu.Instance.gameObject.activeSelf) { //other menu/dialog opened
            SetActiveSubmenu(LeftMenuSelection.Utility, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        UpdateVisibility(false, true);
        SelectorMenu.Instance.gameObject.SetActive(false);
        if (removeOnCancel)
            RenameDialog.Init(selectedObject, UpdateVisibility, true, () => selectedObject.Remove(), confirmCallback);
        else
            RenameDialog.Init(selectedObject, UpdateVisibility, false, null, confirmCallback);
        RenameDialog.Open();
    }

    public void RemoveClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        SetActiveSubmenu(CurrentSubmenuOpened);
        ConfirmationDialog.Open($"Remove {selectedObject.GetObjectTypeName().ToLower()}",
            $"Do you want to remove {selectedObject.GetName()}",
            () => RemoveObject(selectedObject),
            null);
    }

    private void RemoveObject(InteractiveObject obj) {
        obj.Remove();
        ConfirmationDialog.Close();
    }

    #endregion

    #region Home submenu button click methods

    public void CalibrationButtonClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(Recalibrate)) {
            ((Recalibrate) selectedObject).OnClick(Clickable.Click.TOUCH);
        } else if (selectedObject.GetType() == typeof(CreateAnchor)) {
            ((CreateAnchor) selectedObject).OnClick(Clickable.Click.TOUCH);
        } else if (selectedObject.GetType() == typeof(RecalibrateUsingServer)) {
            ((RecalibrateUsingServer) selectedObject).OnClick(Clickable.Click.TOUCH);
        }

        //SetActiveSubmenu(LeftMenuSelection.None);
    }

    public void MainSettingsButtonClick() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !MainSettingsButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (MainSettingsButton.GetComponent<Image>().enabled) {
            MainSettingsButton.GetComponent<Image>().enabled = false;
            MainSettingsMenu.Instance.Hide();
            SelectorMenu.Instance.gameObject.SetActive(true);
            //ActionPicker.SetActive(false);
        } else {
            MainSettingsButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            MainSettingsMenu.Instance.Show();

        }
    }

    #endregion

    public void SetActiveSubmenu(LeftMenuSelection which, bool active = true, bool unlock = true) {
        DeactivateAllSubmenus(unlock);
        CurrentSubmenuOpened = which;
        if (!active)
            return;
        switch (which) {
            case LeftMenuSelection.None:
                break;
            case LeftMenuSelection.Favorites:
                FavoritesButtons.SetActive(active);
                FavoritesButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Add:
                AddButtons.SetActive(active);
                AddButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Utility:
                UtilityButtons.SetActive(active);
                UtilityButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Home:
                HomeButtons.SetActive(active);
                HomeButton.GetComponent<Image>().enabled = active;
                UpdateBuildAndSaveBtns();
                break;
            case LeftMenuSelection.Robot:
                RobotButtons.SetActive(active);
                RobotButton.GetComponent<Image>().enabled = active;
                break;
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unlock">Unlock objects?</param>
    public virtual void DeactivateAllSubmenus(bool unlock = true) {
        SelectorMenu.Instance.gameObject.SetActive(true);
        if (RenameDialog.Visible)
            RenameDialog.Cancel();
        TransformMenu.Instance.Hide(unlock);
        RobotSteppingMenu.Instance.Hide();

        ActionParametersMenu.Instance.Hide(unlock);

        MainSettingsMenu.Instance.Hide();
        ActionObjectMenu.Instance.Hide();

        FavoritesButtons.SetActive(false);
        HomeButtons.SetActive(false);
        UtilityButtons.SetActive(false);
        AddButtons.SetActive(false);
        RobotButtons.SetActive(false);

        FavoritesButton.GetComponent<Image>().enabled = false;
        RobotButton.GetComponent<Image>().enabled = false;
        AddButton.GetComponent<Image>().enabled = false;
        UtilityButton.GetComponent<Image>().enabled = false;
        HomeButton.GetComponent<Image>().enabled = false;

        MainSettingsButton.GetComponent<Image>().enabled = false;
        MoveButton.GetComponent<Image>().enabled = false;
        MoveButton2.GetComponent<Image>().enabled = false;
        OpenMenuButton.GetComponent<Image>().enabled = false;
        RobotSelectorButton.GetComponent<Image>().enabled = false;
        RobotSteppingButton.GetComponent<Image>().enabled = false;
        RobotSelector.Close(false);
        if (TransformGizmo.Instance != null)
            TransformGizmo.Instance.ClearTargets();
    }

    private async Task<RequestResult> ValidateParent(object selectedParent) {
        RequestResult result;
        
        if (selectedParent is IActionPointParent parent) {
            result = new RequestResult(true, "");
            if (selectedActionPoint.GetId() == parent.GetId()) {
                result.Success = false;
                result.Message = "AP could not be its own parent.";
            } else {
                try {
                    GameManager.Instance.ShowLoadingScreen("Checking hierarchy...");
                    await WebsocketManager.Instance.UpdateActionPointParent(selectedActionPoint.GetId(), parent.GetId(), true);
                    GameManager.Instance.HideLoadingScreen();
                } catch (RequestFailedException ex) {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }            
        } else {
            result = new RequestResult(false, "This object could not be parent of AP");
        }
        

        return result;
    }
    private async void AssignToParent(object selectedObject) {
        IActionPointParent parent = (IActionPointParent) selectedObject;
        
        if (parent == null)
            return;
        string id = "";
        id = parent.GetId();
        bool result = await Base.GameManager.Instance.UpdateActionPointParent(selectedActionPoint, id);
        if (!result) {
            selectedActionPoint.WriteUnlock();
        }
    }

    private async void UntieActionPointParent() {
        Debug.Assert(selectedActionPoint != null);
        if (selectedActionPoint.GetParent() == null) {
            selectedActionPoint.WriteUnlock();
            return;
        }

        if (await Base.GameManager.Instance.UpdateActionPointParent(selectedActionPoint, "")) {
            Notifications.Instance.ShowToastMessage("Parent of action point untied");
        } else {
            selectedActionPoint.WriteUnlock();
        }
    }
}

public enum LeftMenuSelection{
    None, Favorites, Add, Utility, Home, Robot
}

