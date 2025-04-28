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
using Parameter = Base.Parameter;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class LeftMenuScene : LeftMenu {

    //public GameObject MeshPicker;

    public ButtonWithTooltip CreateProjectBtn, AddNewCollisionObjectButton, ActionObjectAimingMenuButton;

    public InputDialogWithToggle InputDialogWithToggle;

    public ButtonWithTooltip AddActionObjectButton;

    private const string AIMING_MENU_BTN_LABEL = "Action object precise aiming";
    private const string ADD_ACTION_OBJECT_BTN_LABEL = "Add new action object to scene";
    private const string ADD_NEW_COLLISION_OBJECT_BTN_LABEL = "Add new collision object to scene";

    protected override void Awake() {
        base.Awake();
        SceneManager.Instance.OnSceneSavedStatusChanged += OnSceneSavedStatusChanged;

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        SelectorMenu.Instance.OnObjectSelectedChangedEvent += OnObjectSelectedChangedEvent;
        ActionObjectAimingMenuButton.SetDescription(AIMING_MENU_BTN_LABEL);
        AddActionObjectButton.SetDescription(ADD_ACTION_OBJECT_BTN_LABEL);
    }
    protected void Update() {
        if (SceneManager.Instance.SceneMeta != null)
            EditorInfo.text = "Scene: \n" + SceneManager.Instance.SceneMeta.Name;
    }

    protected override void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (GameManager.Instance.GetGameState() == GameStateEnum.SceneEditor)
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
            AddNewCollisionObjectButton.SetInteractivity(!SceneManager.Instance.SceneStarted, $"{ADD_NEW_COLLISION_OBJECT_BTN_LABEL}\n(only available when offline)");
            if (SceneManager.Instance.SceneStarted) {
                AddActionObjectButton.SetInteractivity(false, $"{ADD_ACTION_OBJECT_BTN_LABEL}\n(only available when offline)");
            } else if (ActionsManager.Instance.AbstractOnlyObjects) {
                AddActionObjectButton.SetInteractivity(false, $"{ADD_ACTION_OBJECT_BTN_LABEL}\n(no object type available)");
            } else {
                AddActionObjectButton.SetInteractivity(true);
            }
#if UNITY_ANDROID && AR_ON
            if (!CalibrationManager.Instance.Calibrated && !TrackingManager.Instance.IsDeviceTracking()) {
                ActionObjectAimingMenuButton.SetInteractivity(false, "AR is not calibrated");
                AddActionObjectButton.SetInteractivity(false, "AR is not calibrated");
            } else
#endif
            if (requestingObject || obj == null) {
                SelectedObjectText.text = "";
                ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(no object selected)");
                CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(no object selected)");
            } else if (obj.IsLocked && obj.LockOwner != LandingScreen.Instance.GetUsername()) {
                ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(the object is used by {obj.LockOwner})");
                CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(the object is used by {obj.LockOwner})");

            } else {
                if (obj is ActionObject actionObject) {
                    if (!SceneManager.Instance.SceneStarted) {
                        ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(only available when online)");
                    } else if (!actionObject.ActionObjectMetadata.HasPose) {
                        ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(not available for objects without pose)");
                        CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(not available for objects without pose)");
                    } else if (actionObject.IsRobot()) {
                        ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(not available for robots)");
                        CopyButton.SetInteractivity(true);
                    } else {
                        ActionObjectAimingMenuButton.SetInteractivity(true);
                        CopyButton.SetInteractivity(true);
                    }

                    CopyButton.SetInteractivity(!SceneManager.Instance.SceneStarted, $"{COPY_LABEL}\n(only available when offline)");
                } else {
                    ActionObjectAimingMenuButton.SetInteractivity(false, $"{AIMING_MENU_BTN_LABEL}\n(selected object is not action object)");
                    CopyButton.SetInteractivity(false, $"{COPY_LABEL}\n(selected object is not action object)");
                }
            }


            previousUpdateDone = true;
        } finally {
            previousUpdateDone = true;
        }
    }

    public override void DeactivateAllSubmenus() {
        if (CheckActionObjectAiming())
            return;

        base.DeactivateAllSubmenus();
        AddActionObjectButton.GetComponent<Image>().enabled = false;
        AddNewCollisionObjectButton.GetComponent<Image>().enabled = false;
        ActionObjectAimingMenuButton.GetComponent<Image>().enabled = false;

        ActionObjectPickerMenu.Instance.Hide();
        ActionObjectAimingMenu.Instance.Hide();
    }

    private async void CancelObjectAiming() {
        try {
            var response = await CommunicationManager.Instance.Client.ObjectAimingCancelAsync();
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to cancel object aiming", string.Join(",", response.Messages));
                return;
            }

            ActionObjectAimingMenu.Instance.AimingInProgress = false;
            ActionObjectAimingMenu.Instance.Highlight(true);
            DeactivateAllSubmenus();
            ConfirmationDialog.Close();
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to cancel object aiming", ex.Message);
        }


    }

    public void AddMeshClick() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !AddActionObjectButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (AddActionObjectButton.GetComponent<Image>().enabled) {
            AddActionObjectButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionObjectPickerMenu.Instance.Hide();
        } else {
            AddActionObjectButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            ActionObjectPickerMenu.Instance.Show(ActionObjectPickerMenu.Type.ActionObjects);
        }

    }

    public override void UpdateVisibility() {

        if (GameManager.Instance.GetGameState() == GameStateEnum.SceneEditor &&
            MainMenu.Instance.CurrentState() == SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }

    }

    public override void UpdateVisibility(bool visible, bool force = false) {
        base.UpdateVisibility(visible, force);
        if (GameManager.Instance.GetGameState() == GameStateEnum.SceneEditor)
            AREditorResources.Instance.StartStopSceneBtn.gameObject.SetActive(visible);
    }

    public async void SaveScene() {
        SaveButton.SetInteractivity(false, "Saving scene...");
        SaveSceneResponse saveSceneResponse = await GameManager.Instance.SaveScene();
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
        if (GameManager.Instance.GetGameState() != GameStateEnum.SceneEditor)
            return;
        if (CurrentSubmenuOpened != LeftMenuSelection.Home)
            return;

        SaveButton.SetInteractivity(false, "Loading...");
        CloseButton.SetInteractivity(false, "Loading...");
        //WebsocketManager.Instance.CloseScene(true, true, CloseSceneCallback);
        if (SceneManager.Instance.SceneStarted) {
            var response = await CommunicationManager.Instance.Client.StopSceneAsync(true);
            if (response.Messages != null) {
                SaveButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
            } else {
                SaveButton.SetInteractivity(response.Result);
            }
        } else {
            CloseButton.SetInteractivity(true);
        }

        if (!SceneManager.Instance.SceneChanged) {
            SaveButton.SetInteractivity(false, "There are no unsaved changes");
        } else {
            var response = await CommunicationManager.Instance.Client.SaveSceneAsync();
            if (response.Messages != null) {
                SaveButton.SetInteractivity(response.Result, response.Messages.FirstOrDefault());
            } else {
                SaveButton.SetInteractivity(response.Result);
            }
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

    private async Task<RequestResult> ValidateProjectName(string name) {
        try {
            await CommunicationManager.Instance.Client.AddNewProjectAsync(new NewProjectRequestArgs(
                       SceneManager.Instance.SceneMeta.Id,
                       name,
                       "",
                       InputDialogWithToggle.GetToggleValue()), true);
        } catch (Arcor2ConnectionException ex) {
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
            await CommunicationManager.Instance.Client.AddNewProjectAsync(new NewProjectRequestArgs(
            SceneManager.Instance.SceneMeta.Id,
            nameOfNewProject,
            "",
            InputDialogWithToggle.GetToggleValue()));
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to create new project", ex.Message);
            GameManager.Instance.HideLoadingScreen(true);
        }
        InputDialogWithToggle.Close();
    }


    public async void ShowCloseSceneDialog() {
        (bool success, _) = await GameManager.Instance.CloseScene(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            string message = "Are you sure you want to close current scene? ";
            if (SceneManager.Instance.SceneChanged) {
                message += "Unsaved changes will be lost";
                if (SceneManager.Instance.SceneStarted) {
                    message += " and system will go offline";
                }
                message += ".";
            } else if (SceneManager.Instance.SceneStarted) {
                message += "System will go offline.";
            }

            ConfirmationDialog.Open("Close scene",
                         message,
                         () => CloseScene(),
                         () => ConfirmationDialog.Close());
        }
    }


    public async void CloseScene() {
        if (SceneManager.Instance.SceneStarted)
            await CommunicationManager.Instance.Client.StopSceneAsync();
        (bool success, string message) = await GameManager.Instance.CloseScene(true);
        if (success) {

            ConfirmationDialog.Close();
            MainMenu.Instance.Close();
        }
    }


    public void ShowNewObjectTypeMenu() {
        if (!SelectorMenu.Instance.gameObject.activeSelf && !AddNewCollisionObjectButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(CurrentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }
        if (AddNewCollisionObjectButton.GetComponent<Image>().enabled) {
            AddNewCollisionObjectButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            ActionObjectPickerMenu.Instance.Hide();
        } else {
            ActionObjectPickerMenu.Instance.Show(ActionObjectPickerMenu.Type.CollisionObjects);
            AddNewCollisionObjectButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
        }
    }

    public void ActionObjectAimingMenuClick() {
        if (CheckActionObjectAiming())
            return;
        if (!SceneManager.Instance.IsRobotAndEESelected()) {
            OpenRobotSelector(ActionObjectAimingMenuClick);
            return;
        }
        if (selectedObject is ActionObject actionObject) {
            if (!SelectorMenu.Instance.gameObject.activeSelf && !ActionObjectAimingMenuButton.GetComponent<Image>().enabled) { //other menu/dialog opened
                SetActiveSubmenu(CurrentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
            }
            if (ActionObjectAimingMenuButton.GetComponent<Image>().enabled) {
                ActionObjectAimingMenuButton.GetComponent<Image>().enabled = false;
                SelectorMenu.Instance.gameObject.SetActive(true);
                if (RobotSteppingMenu.Instance.IsVisible) {
                    RobotSteppingMenu.Instance.Hide();
                }
                ActionObjectAimingMenu.Instance.Hide();

            } else {
                _ = ActionObjectAimingMenu.Instance.Show(actionObject, false);
                ActionObjectAimingMenuButton.GetComponent<Image>().enabled = true;
                SelectorMenu.Instance.gameObject.SetActive(false);
            }
        }
    }

    private bool CheckActionObjectAiming() {
        if (ActionObjectAimingMenu.Instance.AimingInProgress) {
            ConfirmationDialog.Open("Cancel object aiming?",
                "Action object aiming is running, do you want to cancel it?",
                CancelObjectAiming,
                null,
                "Cancel aiming",
                "Keep running",
                true);
            return true;
        } else
            return false;
    }

    public async override void CopyObjectClick() {
        if (selectedObject is ActionObject actionObject) {
            List<Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter> parameters = new();
            foreach (Parameter p in actionObject.ObjectParameters.Values) {
                parameters.Add(DataHelper.ActionParameterToParameter(p));
            }
            string newName = SceneManager.Instance.GetFreeAOName(actionObject.GetName());
            SceneManager.Instance.SelectCreatedActionObject = newName;
            SceneManager.Instance.OpenTransformMenuOnCreatedObject = true;
            await CommunicationManager.Instance.Client.AddActionObjectToSceneAsync(new AddObjectToSceneRequestArgs(newName,
                actionObject.ActionObjectMetadata.Type, new Pose(
                    orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(actionObject.transform.localRotation)),
                    position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(actionObject.transform.localPosition))), parameters));
        }
    }
}

