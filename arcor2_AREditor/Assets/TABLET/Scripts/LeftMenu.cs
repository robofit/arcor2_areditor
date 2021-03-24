using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(CanvasGroup))]
public class LeftMenu : Base.Singleton<LeftMenu> {

    protected CanvasGroup CanvasGroup;

    public Button FavoritesButton, RobotButton, AddButton, SettingsButton, HomeButton;
    public Button MoveButton, MoveButton2, RemoveButton, RenameButton, CalibrationButton, ResizeCubeButton,
        AddConnectionButton, AddConnectionButton2; //Buttons with number 2 are duplicates in favorites submenu
    public GameObject FavoritesButtons, HomeButtons, SettingsButtons, AddButtons, MeshPicker, ActionPicker;
    public RenameDialog RenameDialog;
    public TMPro.TMP_Text ProjectName, SelectedObjectText;

    private bool isVisibilityForced = false;
    protected ActionPoint3D selectedActionPoint;
    protected LeftMenuSelection currentSubmenuOpened;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
    }

    private void OnEnable() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
    }

    private void OnDisable() {
        SelectorMenu.Instance.OnObjectSelectedChangedEvent -= OnObjectSelectedChangedEvent;
    }

    private void OnObjectSelectedChangedEvent(object sender, InteractiveObjectEventArgs args) {
        if (!updateButtonsInteractivity)
            UpdateBtns(args.InteractiveObject);
    }

    protected virtual void UpdateBtns(InteractiveObject selectedObject) {
        if (requestingObject || selectedObject == null) {
            SelectedObjectText.text = "";
            MoveButton.interactable = false;
            MoveButton2.interactable = false;
            RemoveButton.interactable = false;
            RenameButton.interactable = false;
            CalibrationButton.interactable = false;            
        } else {
            SelectedObjectText.text = selectedObject.GetName() + "\n" + selectedObject.GetType();

            MoveButton.interactable = selectedObject.Movable();
            MoveButton2.interactable = selectedObject.Movable();
            RemoveButton.interactable = selectedObject.Removable();

            RenameButton.interactable = selectedObject is ActionPoint3D ||
                selectedObject is Action3D;
            CalibrationButton.interactable = selectedObject.GetType() == typeof(Recalibrate) ||
                selectedObject.GetType() == typeof(CreateAnchor);
            
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
        if (!isVisibilityForced)
            UpdateVisibility();

        if (!updateButtonsInteractivity)
            return;

        if (MenuManager.Instance.CheckIsAnyRightMenuOpened()) {
            SetActiveSubmenu(LeftMenuSelection.Favorites);

            RobotButton.interactable = false;
            AddButton.interactable = false;
            SettingsButton.interactable = false;
            HomeButton.interactable = false;
            return;
        }

        RobotButton.interactable = true;
        AddButton.interactable = true;
        SettingsButton.interactable = true;
        HomeButton.interactable = true;

               

        if (SceneManager.Instance.SceneMeta != null)
            ProjectName.text = "Project: \n" + SceneManager.Instance.SceneMeta.Name;
    }

    public void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            GameManager.Instance.GetEditorState() == EditorStateEnum.SelectingActionPointParent ||
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Open) {
            UpdateVisibility(false);
        } else {
            UpdateVisibility(true);
        }
    }

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
        SetActiveSubmenu(LeftMenuSelection.None);
        Notifications.Instance.ShowNotification("Not implemented", "");

    }

    public void AddButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Add);

    }

    public void SettingsButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Settings);


    }

    public void HomeButtonClick() {
        SetActiveSubmenu(LeftMenuSelection.Home);
    }

    #region Add submenu button click methods

    

    


    

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

        if (selectedObject.GetType() == typeof(PuckInput) ||
            selectedObject.GetType() == typeof(PuckOutput)) {
            selectedObject.StartManipulation();
            return;
        }


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
            SetActiveSubmenu(LeftMenuSelection.Settings); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        UpdateVisibility(false, true);
        SelectorMenu.Instance.gameObject.SetActive(false);

        RenameDialog.Init(selectedObject);
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
            case LeftMenuSelection.Settings:
                SettingsButtons.SetActive(active);
                SettingsButton.GetComponent<Image>().enabled = active;
                break;
            case LeftMenuSelection.Home:
                HomeButtons.SetActive(active);
                HomeButton.GetComponent<Image>().enabled = active;
                break;
            
        }
    }

    protected virtual void DeactivateAllSubmenus() {
        SelectorMenu.Instance.gameObject.SetActive(true);
        if (RenameDialog.isActiveAndEnabled)
            RenameDialog.Close();
        TransformMenu.Instance.Hide();

        MeshPicker.SetActive(false);
        ActionPicker.SetActive(false);

        FavoritesButtons.SetActive(false);
        HomeButtons.SetActive(false);
        SettingsButtons.SetActive(false);
        AddButtons.SetActive(false);

        FavoritesButton.GetComponent<Image>().enabled = false;
        RobotButton.GetComponent<Image>().enabled = false;
        AddButton.GetComponent<Image>().enabled = false;
        SettingsButton.GetComponent<Image>().enabled = false;
        HomeButton.GetComponent<Image>().enabled = false;

        MoveButton.GetComponent<Image>().enabled = false;
        MoveButton2.GetComponent<Image>().enabled = false;
        ResizeCubeButton.GetComponent<Image>().enabled = false;
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
    None, Favorites, Add, Settings, Home
}

