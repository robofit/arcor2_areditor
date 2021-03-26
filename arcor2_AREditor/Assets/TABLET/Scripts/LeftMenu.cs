using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(CanvasGroup))]
public abstract class LeftMenu : MonoBehaviour {

    public CanvasGroup CanvasGroup;

    public Button FavoritesButton, RobotButton, AddButton, UtilityButton, HomeButton;
    public Button MoveButton, MoveButton2, RemoveButton, RenameButton, CalibrationButton, OpenMenuButton, RobotSelectorButton, RobotSteppingButton; //Buttons with number 2 are duplicates in favorites submenu
    public GameObject FavoritesButtons, HomeButtons, UtilityButtons, AddButtons, RobotButtons;
    public RenameDialog RenameDialog;
    public RobotSelectorDialog RobotSelector;
    public TMPro.TMP_Text EditorInfo, SelectedObjectText;

    private bool isVisibilityForced = false;
    protected ActionPoint3D selectedActionPoint;
    protected LeftMenuSelection currentSubmenuOpened;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        MenuManager.Instance.MainMenu.onStateChanged.AddListener(() => OnGameStateChanged(this, null));
    }

    private void OnEnable() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        
    }

    private void OnDisable() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent -= OnObjectSelectedChangedEvent;
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged -= OnEditorStateChanged;
    }

    private void OnObjectSelectedChangedEvent(object sender, InteractiveObjectEventArgs args) {
        if (updateButtonsInteractivity)
            UpdateBtns(args.InteractiveObject);
    }

    private void OnGameStateChanged(object sender, GameStateEventArgs _) {
        if (!isVisibilityForced)
            UpdateVisibility();
    }

    protected virtual void UpdateBtns(InteractiveObject selectedObject) {
        RobotSteppingButton.interactable = SceneManager.Instance.SceneStarted;
        RobotSelectorButton.interactable = SceneManager.Instance.SceneStarted;
        if (requestingObject || selectedObject == null) {
            SelectedObjectText.text = "";
            MoveButton.interactable = false;
            MoveButton2.interactable = false;
            RemoveButton.interactable = false;
            RenameButton.interactable = false;
            CalibrationButton.interactable = false;
            OpenMenuButton.interactable = false;
        } else {
            SelectedObjectText.text = selectedObject.GetName() + "\n" + selectedObject.GetType();
            MoveButton.interactable = selectedObject.Movable();
            MoveButton2.interactable = selectedObject.Movable();
            RemoveButton.interactable = selectedObject.Removable();
            RenameButton.interactable = selectedObject.GetType() == typeof(ActionPoint3D) ||
                selectedObject.GetType() == typeof(Action3D) || selectedObject.GetType() == typeof(ActionObject3D) ||
                selectedObject.GetType() == typeof(APOrientation);
            CalibrationButton.interactable = selectedObject.GetType() == typeof(Recalibrate) ||
                selectedObject.GetType() == typeof(CreateAnchor);
            OpenMenuButton.interactable = selectedObject.HasMenu();            
        }
    }

    private bool updateButtonsInteractivity = false;

    protected bool requestingObject = false;

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
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
                requestingObject = true;
                break;
        }
    }

    protected virtual void Update() {

        if (!updateButtonsInteractivity)
            return;

        if (MenuManager.Instance.CheckIsAnyRightMenuOpened()) {
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

               

        if (SceneManager.Instance.SceneMeta != null)
            EditorInfo.text = "Project: \n" + SceneManager.Instance.SceneMeta.Name;
    }

    public void OpenMenuButtonClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        selectedObject.OpenMenu();

        //SetActiveSubmenu(LeftMenuSelection.None);
    }

    public abstract void UpdateVisibility();

    public void UpdateVisibility(bool visible, bool force = false) {
        isVisibilityForced = force;

        CanvasGroup.interactable = visible;
        CanvasGroup.blocksRaycasts = visible;
        CanvasGroup.alpha = visible ? 1 : 0;
    }



    public void FavoritesButtonClick() {
        MenuManager.Instance.HideAllMenus();
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
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot selector", "Scene offline");
            return;
        }
        if (RobotSelectorButton.GetComponent<Image>().enabled) {
            SelectorMenu.Instance.gameObject.SetActive(true);
            RobotSelector.Close();
        } else {
            SelectorMenu.Instance.gameObject.SetActive(false);
            UpdateVisibility(false, true);
            RobotSelector.Open(UpdateVisibility);
        }
    }



    public void RobotSteppingButtonClick() {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot manipulation menu", "Scene offline");
            return;
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Notifications.Instance.ShowNotification("Failed to open robot manipulation menu", "Robot or EE not selected");
            return;
        }
        if (!SelectorMenu.Instance.gameObject.activeSelf && !RobotSteppingButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(currentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (RobotSteppingButton.GetComponent<Image>().enabled) {
            RobotSteppingButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            RobotSteppingMenu.Instance.Hide();
        } else {
            RobotSteppingButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            RobotSteppingMenu.Instance.Show();
        }
    }

    #endregion

    #region Settings submenu button click methods

    public void SetActionPointParentClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null || !(selectedObject is ActionPoint3D))
            return;

        if (!SelectorMenu.Instance.gameObject.activeSelf) { //other menu/dialog opened
            SetActiveSubmenu(LeftMenuSelection.None); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        selectedActionPoint = (ActionPoint3D) selectedObject;
        Action<object> action = AssignToParent;
        GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionPointParent, action,
            "Select new parent (action object)", ValidateParent, UntieActionPointParent);
    }

    public void MoveClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        
        //was clicked the button in favorites or settings submenu?
        Button clickedButton = MoveButton;
        if (currentSubmenuOpened == LeftMenuSelection.Favorites)
            clickedButton = MoveButton2;

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(currentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            TransformMenu.Instance.Hide();
        } else {
            clickedButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            //selectedObject.StartManipulation();
            TransformMenu.Instance.Show(selectedObject);
        }

    }

    public void RenameClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        if (!SelectorMenu.Instance.gameObject.activeSelf) { //other menu/dialog opened
            SetActiveSubmenu(LeftMenuSelection.Utility); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        UpdateVisibility(false, true);
        SelectorMenu.Instance.gameObject.SetActive(false);

        RenameDialog.Init(selectedObject, UpdateVisibility);
        RenameDialog.Open();
    }

    public void RemoveClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;

        selectedObject.Remove();
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
        }

        SetActiveSubmenu(LeftMenuSelection.None);
    }

    public async void RunButtonClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null) {
            GameManager.Instance.ShowLoadingScreen("Running project", true);
            try {
                await Base.WebsocketManager.Instance.TemporaryPackage();
                MenuManager.Instance.MainMenu.Close();
            } catch (RequestFailedException ex) {
                Base.Notifications.Instance.ShowNotification("Failed to run temporary package", "");
                Debug.LogError(ex);
                GameManager.Instance.HideLoadingScreen(true);
            }
        } else if (selectedObject.GetType() == typeof(Action3D)) {
            try {
                await WebsocketManager.Instance.ExecuteAction(selectedObject.GetId(), false);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
                return;
            }
        } else if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            string robotId = "";
            foreach (IRobot r in SceneManager.Instance.GetRobots()) {
                robotId = r.GetId();
            }
            NamedOrientation o = ((ActionPoint3D) selectedObject).GetFirstOrientation();
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            await WebsocketManager.Instance.MoveToActionPointOrientation(robot.GetId(), (await robot.GetEndEffectorIds())[0], 0.5m, o.Id, false);
        }            
        
    }

    #endregion

    public void SetActiveSubmenu(LeftMenuSelection which, bool active = true) {
        DeactivateAllSubmenus();
        currentSubmenuOpened = which;
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
                break;
            case LeftMenuSelection.Robot:
                RobotButtons.SetActive(active);
                RobotButton.GetComponent<Image>().enabled = active;
                break;
            
        }
    }

    protected virtual void DeactivateAllSubmenus() {
        SelectorMenu.Instance.gameObject.SetActive(true);
        if (RenameDialog.isActiveAndEnabled)
            RenameDialog.Close();
        TransformMenu.Instance.Hide();
        RobotSteppingMenu.Instance.Hide();

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

        MoveButton.GetComponent<Image>().enabled = false;
        MoveButton2.GetComponent<Image>().enabled = false;
        OpenMenuButton.GetComponent<Image>().enabled = false;
        RobotSelectorButton.GetComponent<Image>().enabled = false;
        RobotSteppingButton.GetComponent<Image>().enabled = false;
        RobotSelector.Close(false);
    }

    private async Task<RequestResult> ValidateParent(object selectedParent) {
        IActionPointParent parent = (IActionPointParent) selectedParent;
        RequestResult result = new RequestResult(true, "");
        if (parent.GetId() == selectedActionPoint.GetId()) {
            result.Success = false;
            result.Message = "Action point cannot be its own parent!";
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
        if (result) {
            //
        }
    }

    private async void UntieActionPointParent() {
        Debug.Assert(selectedActionPoint != null);
        if (selectedActionPoint.GetParent() == null)
            return;

        if (await Base.GameManager.Instance.UpdateActionPointParent(selectedActionPoint, "")) {
            Notifications.Instance.ShowToastMessage("Parent of action point untied");
        }
    }
}

public enum LeftMenuSelection{
    None, Favorites, Add, Utility, Home, Robot
}

