using System;
using UnityEngine;
using UnityEngine.UI;
using Base;
using static Base.GameManager;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using IO.Swagger.Model;
using UnityEditorInternal;

public class LeftMenuScene : LeftMenu
{

    //public GameObject MeshPicker;

    public ButtonWithTooltip CreateProjectBtn, AddNewObjectTypeButton, ActionObjectAimingMenuButton;

    public InputDialogWithToggle InputDialogWithToggle;

    public ButtonWithTooltip AddActionObjectButton;

    protected override void Awake() {
        base.Awake();
        Base.SceneManager.Instance.OnSceneSavedStatusChanged += OnSceneSavedStatusChanged;

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
    }
    protected override void Update() {
        base.Update();
        if (SceneManager.Instance.SceneMeta != null)
            EditorInfo.text = "Scene: \n" + SceneManager.Instance.SceneMeta.Name;
    }

    protected override void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor)
            base.OnSceneStateEvent(sender, args);

    }


    private void OnSceneSavedStatusChanged(object sender, EventArgs e) {
        UpdateBuildAndSaveBtns();
    }

    protected async override Task UpdateBtns(InteractiveObject obj) {
        try {
        
            if (CanvasGroup.alpha == 0) {
                return;
            }

            await base.UpdateBtns(obj);
            if (!CalibrationManager.Instance.Calibrated && !TrackingManager.Instance.IsDeviceTracking()) {
                ActionObjectAimingMenuButton.SetInteractivity(false, "AR is not calibrated");
            } else if (requestingObject || obj == null) {
                SelectedObjectText.text = "";
                ActionObjectAimingMenuButton.SetInteractivity(false, "No object selected");
            } else if (obj.IsLocked && obj.LockOwner != LandingScreen.Instance.GetUsername()) {
                ActionObjectAimingMenuButton.SetInteractivity(false, "Object is locked");
            } else {
                if (obj is ActionObject actionObject) {
                    if (!SceneManager.Instance.SceneStarted) {
                        ActionObjectAimingMenuButton.SetInteractivity(false, "Only available when online");
                    } else if (!actionObject.ActionObjectMetadata.HasPose) {
                        ActionObjectAimingMenuButton.SetInteractivity(false, "Not available for objects without pose");
                    } else {
                        ActionObjectAimingMenuButton.SetInteractivity(true);
                    }
                } else {
                    ActionObjectAimingMenuButton.SetInteractivity(false, "Selected object is not action object");
                }
            }
            
            previousUpdateDone = true;
        } finally {
            previousUpdateDone = true;
        }
    }

    public override void DeactivateAllSubmenus(bool unlock = true) {
        if (ActionObjectAimingMenu.Instance.Focusing) {
            ConfirmationDialog.Open("Cancel object aiming?",
                "Action object aiming is running, do you want to cancel it?",
                CancelObjectAiming,
                null,
                "Cancel aiming",
                "Keep running");
            return;
        }

                

        base.DeactivateAllSubmenus(unlock);
        AddActionObjectButton.GetComponent<Image>().enabled = false;
        AddNewObjectTypeButton.GetComponent<Image>().enabled = false;
        ActionObjectAimingMenuButton.GetComponent<Image>().enabled = false;

        ActionObjectPickerMenu.Instance.Hide();
        NewObjectTypeMenu.Instance.Hide();
        ActionObjectAimingMenu.Instance.Hide(unlock);
    }

    private void CancelObjectAiming() {
        Notifications.Instance.ShowNotification("Not available at the moment", "Sorry, i'm not sorry");
    }

    public void AddMeshClick() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !AddActionObjectButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (AddActionObjectButton.GetComponent<Image>().enabled) {
            AddActionObjectButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionObjectPickerMenu.Instance.Hide();
        } else {
            AddActionObjectButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            ActionObjectPickerMenu.Instance.Show();
        }

    }

    public override void UpdateVisibility() {
        
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor &&
            MainMenu.Instance.CurrentState() == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
        
    }

    public async void SaveScene() {
        SaveButton.SetInteractivity(false, "Saving scene...");
        IO.Swagger.Model.SaveSceneResponse saveSceneResponse = await Base.GameManager.Instance.SaveScene();
        if (!saveSceneResponse.Result) {
            saveSceneResponse.Messages.ForEach(Debug.LogError);
            Notifications.Instance.ShowNotification("Scene save failed", saveSceneResponse.Messages.Count > 0 ? saveSceneResponse.Messages[0] : "Failed to save scene");
            return;
        } else {
            SaveButton.SetInteractivity(false, "There are no unsaved changes");
            UpdateBuildAndSaveBtns();
        }
    }

    public override async void UpdateBuildAndSaveBtns() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor)
            return;
        if (CurrentSubmenuOpened != LeftMenuSelection.Home)
            return;

        SaveButton.SetInteractivity(false, "Loading...");
        CloseButton.SetInteractivity(false, "Loading...");
        //WebsocketManager.Instance.CloseScene(true, true, CloseSceneCallback);
        if (SceneManager.Instance.SceneStarted) {
            WebsocketManager.Instance.StopScene(true, StopSceneCallback);
        } else {
            CloseButton.SetInteractivity(true);
        }

        if (!SceneManager.Instance.SceneChanged) {
            SaveButton.SetInteractivity(false, "There are no unsaved changes");
        } else {
            WebsocketManager.Instance.SaveScene(true, SaveSceneCallback);
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

    protected void SaveSceneCallback(string nothing, string data) {
        SaveSceneResponse response = JsonConvert.DeserializeObject<SaveSceneResponse>(data);
        if (response.Messages != null) {
            SaveButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
        } else {
            SaveButton.SetInteractivity(response.Result);
        }
    }
    /*
    protected void CloseSceneCallback(string nothing, string data) {
        CloseSceneResponse response = JsonConvert.DeserializeObject<CloseSceneResponse>(data);
        if (response.Messages != null) {
            CloseButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
        } else {
            CloseButton.SetInteractivity(response.Result);
        }
    }*/

    public void ShowNewProjectDialog() {
        InputDialogWithToggle.Open("New project",
            "",
            "Name",
            "",
            () => CreateProject(),
            () => InputDialogWithToggle.Close(),
            validateInput: ValidateProjectName);
    }

    private async Task<Base.RequestResult> ValidateProjectName(string name) {
        try {
            await WebsocketManager.Instance.CreateProject(name,
            SceneManager.Instance.SceneMeta.Id,
            "",
            InputDialogWithToggle.GetToggleValue(),
            true);
        } catch (RequestFailedException ex) {
            return new RequestResult(false, ex.Message);
        }
        return new RequestResult(true, "");
    }


    private async void CreateProject() {
        GameManager.Instance.ShowLoadingScreen("Creating new project", true);
        string nameOfNewProject = InputDialogWithToggle.GetValue();
        if (string.IsNullOrEmpty(nameOfNewProject)) {
            Notifications.Instance.ShowNotification("Failed to create new project", "Name of project must not be empty");
            GameManager.Instance.HideLoadingScreen(true);
            return;
        }
        try {
            await WebsocketManager.Instance.CreateProject(nameOfNewProject,
            SceneManager.Instance.SceneMeta.Id,
            "",
            InputDialogWithToggle.GetToggleValue(),
            false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to create new project", ex.Message);
            GameManager.Instance.HideLoadingScreen(true);
        }
        InputDialogWithToggle.Close();
    }


    public async void ShowCloseSceneDialog() {
        (bool success, _) = await Base.GameManager.Instance.CloseScene(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            ConfirmationDialog.Open("Close scene",
                         "Are you sure you want to close current scene? Unsaved changes will be lost and system will go offline (if online)..",
                         () => CloseScene(),
                         () => ConfirmationDialog.Close());
        }
    }


    public async void CloseScene() {
        if (SceneManager.Instance.SceneStarted)
            WebsocketManager.Instance.StopScene(false, null);
        (bool success, string message) = await Base.GameManager.Instance.CloseScene(true);
        if (success) {

            ConfirmationDialog.Close();
            MainMenu.Instance.Close();
        }
    }


    public void ShowNewObjectTypeMenu() {        
        if (!SelectorMenu.Instance.gameObject.activeSelf && !AddNewObjectTypeButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (AddNewObjectTypeButton.GetComponent<Image>().enabled) {
            AddNewObjectTypeButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            NewObjectTypeMenu.Instance.Hide();
        } else {
            NewObjectTypeMenu.Instance.Show(() => DeactivateAllSubmenus(false));
            AddNewObjectTypeButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
        }
    }

    public void ActionObjectAimingMenuClick() {
        if (ActionObjectAimingMenu.Instance.Focusing) {
            ConfirmationDialog.Open("Cancel object aiming?",
                "Action object aiming is running, do you want to cancel it?",
                CancelObjectAiming,
                null,
                "Cancel aiming",
                "Keep running");
            return;
        }
        if (selectedObject is ActionObject actionObject) {
            if (!SelectorMenu.Instance.gameObject.activeSelf && !ActionObjectAimingMenuButton.GetComponent<Image>().enabled) { //other menu/dialog opened
                SetActiveSubmenu(CurrentSubmenuOpened, unlock: false); //close all other opened menus/dialogs and takes care of red background of buttons
            }
            if (ActionObjectAimingMenuButton.GetComponent<Image>().enabled) {
                ActionObjectAimingMenuButton.GetComponent<Image>().enabled = false;
                SelectorMenu.Instance.gameObject.SetActive(true);
                ActionObjectAimingMenu.Instance.Hide();
            } else {
                ActionObjectAimingMenu.Instance.Show(actionObject);
                ActionObjectAimingMenuButton.GetComponent<Image>().enabled = true;
                SelectorMenu.Instance.gameObject.SetActive(false);
            }
        }        
    }


}

