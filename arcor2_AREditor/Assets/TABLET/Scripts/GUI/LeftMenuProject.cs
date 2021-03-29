using System;
using UnityEngine.UI;
using Base;
using System.Collections.Generic;
using IO.Swagger.Model;
using UnityEngine;
using System.Threading.Tasks;

public class LeftMenuProject : LeftMenu
{

    public ButtonWithTooltip SetActionPointParentButton, AddActionButton, AddActionButton2, RunButton, RunButton2,
        AddConnectionButton, AddConnectionButton2, BuildPackageButton;

    public GameObject ActionPicker;
    private InputDialog inputDialog;


    protected override void Update() {
        base.Update();

    }

    private void Start() {
        Base.ProjectManager.Instance.OnProjectSavedSatusChanged += OnProjectSavedStatusChanged;
        Base.GameManager.Instance.OnOpenProjectEditor += OnOpenProjectEditor;
    }

    protected override void OnEnable() {
        base.OnEnable();
        
    }

    protected override void OnDisable() {
        base.OnEnable();
    }

    protected override void UpdateBtns(InteractiveObject selectedObject) {
        base.UpdateBtns(selectedObject);
        if (requestingObject || selectedObject == null) {
            SetActionPointParentButton.SetInteractivity(false, "No action point is selected");
            AddActionButton.SetInteractivity(false, "No action point is selected");
            AddActionButton2.SetInteractivity(false, "No action point is selected");
            AddConnectionButton.SetInteractivity(false, "No input / output is selected");
            AddConnectionButton2.SetInteractivity(false, "No input / output is selected");
            RunButton.SetInteractivity(false, "No object is selected");
            RunButton2.SetInteractivity(false, "No object is selected");
        } else {
            SetActionPointParentButton.SetInteractivity(selectedObject is ActionPoint3D, "Selected object is not action point");
            AddActionButton.SetInteractivity(selectedObject is ActionPoint3D, "Selected object is not action point");
            AddActionButton2.SetInteractivity(selectedObject is ActionPoint3D, "Selected object is not action point");
            AddConnectionButton.SetInteractivity(selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput), "Selected object is not input or output of an action");
            AddConnectionButton2.SetInteractivity(selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput), "Selected object is not input or output of an action");
            string runBtnInteractivity = null;

            if (selectedObject.GetType() == typeof(Action3D)) {
                if (!SceneManager.Instance.SceneStarted)
                    runBtnInteractivity = "Scene offline";
                else if (!string.IsNullOrEmpty(GameManager.Instance.ExecutingAction)) {
                    runBtnInteractivity = "Some action is already excecuted";
                }
                RunButton.SetDescription("Execute action");
                RunButton2.SetDescription("Execute action");
            } else if (selectedObject.GetType() == typeof(StartAction)) {
                if (!ProjectManager.Instance.ProjectMeta.HasLogic) {
                    runBtnInteractivity = "Project without logic could not be started from editor";
                } else if (SaveButton.IsInteractive()) {
                    runBtnInteractivity = "Project has unsaved changes";
                }
                RunButton.SetDescription("Run project");
                RunButton2.SetDescription("Run project");
            } else {
                runBtnInteractivity = "Selected object is not action or START";
            }

            RunButton.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), runBtnInteractivity);
            RunButton2.SetInteractivity(string.IsNullOrEmpty(runBtnInteractivity), runBtnInteractivity);
        }
    }

    protected override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();

        AddActionButton.GetComponent<Image>().enabled = false;
        AddActionButton2.GetComponent<Image>().enabled = false;
        //ActionPicker.SetActive(false);
    }

    private void OnOpenProjectEditor(object sender, EventArgs eventArgs) {
        if (ProjectManager.Instance.ProjectMeta.HasLogic) {
            RunButton.SetInteractivity(true);
            RunButton2.SetInteractivity(true);
        } else {
            RunButton.SetInteractivity(false, "Project without defined logic could not be run from editor");
            RunButton2.SetInteractivity(false, "Project without defined logic could not be run from editor");
        }
    }

    public async void SaveProject() {
        SaveButton.SetInteractivity(false, "Saving project...");
        IO.Swagger.Model.SaveProjectResponse saveProjectResponse = await Base.GameManager.Instance.SaveProject();
        if (!saveProjectResponse.Result) {
            saveProjectResponse.Messages.ForEach(Debug.LogError);
            Base.Notifications.Instance.ShowNotification("Failed to save project", (saveProjectResponse.Messages.Count > 0 ? ": " + saveProjectResponse.Messages[0] : ""));
            return;
        }
    }

    public async void BuildPackage(string name) {
        try {
            await Base.GameManager.Instance.BuildPackage(name);
            inputDialog.Close();
            Notifications.Instance.ShowToastMessage("Package was built sucessfully.");
        } catch (Base.RequestFailedException ex) {

        }

    }


    public async void RunProject() {
        GameManager.Instance.ShowLoadingScreen("Running project", true);
        try {
            await Base.WebsocketManager.Instance.TemporaryPackage();
            MenuManager.Instance.MainMenu.Close();
        } catch (RequestFailedException ex) {
            Base.Notifications.Instance.ShowNotification("Failed to run temporary package", "");
            Debug.LogError(ex);
            GameManager.Instance.HideLoadingScreen(true);
        }
    }

    public void ShowBuildPackageDialog() {
        inputDialog.Open("Build package",
                         "",
                         "Package name",
                         Base.ProjectManager.Instance.ProjectMeta.Name + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"),
                         () => BuildPackage(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }


    private void OnProjectSavedStatusChanged(object sender, EventArgs e) {
        _ = UpdateBuildAndSaveBtns();
    }
    

    public override async Task UpdateBuildAndSaveBtns() {
        bool successForce;
        string messageForce;
        
        if (!ProjectManager.Instance.ProjectChanged) {
            BuildPackageButton.SetInteractivity(true);
            SaveButton.SetInteractivity(false, "There are no unsaved changes");
            if (ProjectManager.Instance.ProjectMeta.HasLogic) {
                RunButton.SetInteractivity(true);
                RunButton2.SetInteractivity(true);
            }
        } else {
            BuildPackageButton.SetInteractivity(false, "There are unsaved changes on project");
            RunButton.SetInteractivity(false, "There are unsaved changes on project");
            RunButton2.SetInteractivity(false, "There are unsaved changes on project");
            SaveButton.SetInteractivity(true);
        }
        (successForce, messageForce) = await GameManager.Instance.CloseProject(true, true);
        CloseButton.SetInteractivity(successForce, messageForce);        
    }



    public void CopyObjectClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            ProjectManager.Instance.SelectAPNameWhenCreated = "copy_of_" + selectedObject.GetName();
            WebsocketManager.Instance.CopyActionPoint(selectedObject.GetId(), null);
        } else if (selectedObject.GetType() == typeof(Action3D)) {
            Action3D action = (Action3D) selectedObject;
            List<ActionParameter> parameters = new List<ActionParameter>();
            foreach (Base.Parameter p in action.Parameters.Values) {
                parameters.Add(new ActionParameter(p.ParameterMetadata.Name, p.ParameterMetadata.Type, p.Value));
            }
            WebsocketManager.Instance.AddAction(action.ActionPoint.GetId(), parameters, action.ActionProvider.GetProviderId() + "/" + action.Metadata.Name, action.GetName() + "_copy", action.GetFlows());
        }
    }

    public void AddConnectionClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if ((selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput))) {
            ((InputOutput) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
    }


    public void AddActionClick() {
        //was clicked the button in favorites or settings submenu?
        Button clickedButton = AddActionButton.Button;
        if (currentSubmenuOpened == LeftMenuSelection.Favorites) {
            clickedButton = AddActionButton2.Button;
        }

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(currentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            //ActionPicker.SetActive(false);
        } else {
            clickedButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            //ActionPicker.SetActive(true);
        }
    }



    public void AddActionPointClick() {
        ControlBoxManager.Instance.ShowCreateGlobalActionPointDialog();
    }

    public override void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor &&
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
    }

    public async void ShowCloseProjectDialog() {
        (bool success, _) = await Base.GameManager.Instance.CloseProject(false);
        if (!success) {
            GameManager.Instance.HideLoadingScreen();
            ConfirmationDialog.Open("Close project",
                         "Are you sure you want to close current project? Unsaved changes will be lost.",
                         () => CloseProject(),
                         () => ConfirmationDialog.Close());
        }

    }

    public async void CloseProject() {
        GameManager.Instance.ShowLoadingScreen("Closing project..");
        _ = await GameManager.Instance.CloseProject(true);
        ConfirmationDialog.Close();
        MenuManager.Instance.MainMenu.Close();
        GameManager.Instance.HideLoadingScreen();
    }

    public async void RunClicked() {
        try {
            InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
            if (selectedObject is null)
                return;
            if (selectedObject is Action3D) {
                ((Action3D) selectedObject).ActionBeingExecuted = true;
                await WebsocketManager.Instance.ExecuteAction(selectedObject.GetId(), false);
                ((Action3D) selectedObject).ActionBeingExecuted = false;
            } else if (selectedObject.GetType() == typeof(APOrientation)) {
                
                //await WebsocketManager.Instance.MoveToActionPointOrientation(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId(), 0.5m, selectedObject.GetId(), false);
            } else {
                RunProject();
            }
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
            return;
        }
        
    }
}
