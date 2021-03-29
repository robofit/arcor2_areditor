using System;
using UnityEngine;
using UnityEngine.UI;
using Base;
using static Base.GameManager;
using System.Threading.Tasks;
using System.Collections;
using TMPro;

public class LeftMenuScene : LeftMenu
{

    //public GameObject MeshPicker;

    public ButtonWithTooltip CreateProjectBtn, AddNewObjectTypeButton;

    public InputDialogWithToggle InputDialogWithToggle;

    public Button AddMeshButton;

    private void Start() {
        Base.SceneManager.Instance.OnSceneSavedStatusChanged += OnSceneSavedStatusChanged;
    }
    protected override void Update() {
        base.Update();
        if (SceneManager.Instance.SceneMeta != null)
            EditorInfo.text = "Scene: \n" + SceneManager.Instance.SceneMeta.Name;
    }


    private void OnSceneSavedStatusChanged(object sender, EventArgs e) {
        _ = UpdateBuildAndSaveBtns();
    }

    protected async override Task UpdateBtns(InteractiveObject obj) {
        try {
        
        if (CanvasGroup.alpha == 0) {
            return;
        }

        await base.UpdateBtns(obj);

        if (requestingObject || obj == null) {

        } else {

        }
        previousUpdateDone = true;
        } finally {
            previousUpdateDone = true;
        }
    }

    protected override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();
        AddMeshButton.GetComponent<Image>().enabled = false;

        //MeshPicker.SetActive(false);
    }

    public void AddMeshClick() {
        if (AddMeshButton.GetComponent<Image>().enabled) {
            AddMeshButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            //MeshPicker.SetActive(false);
        } else {
            AddMeshButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            //MeshPicker.SetActive(true);
        }

    }

    public override void UpdateVisibility() {
        
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor &&
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
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
            _ = UpdateBuildAndSaveBtns();
        }
    }

    public override async Task UpdateBuildAndSaveBtns() {
        string messageForce;
        bool successForce;
        (successForce, messageForce) = await GameManager.Instance.CloseScene(true, true);
        if (!SceneManager.Instance.SceneChanged) {
            SaveButton.SetInteractivity(false, "There are no unsaved changes");
            CreateProjectBtn.SetInteractivity(true);
        } else {
            SaveButton.SetInteractivity(true);
            CreateProjectBtn.SetInteractivity(false, "There are unsaved changes");
        }
        CloseButton.SetInteractivity(successForce, messageForce); 
    }

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
                         "Are you sure you want to close current scene? Unsaved changes will be lost.",
                         () => CloseScene(),
                         () => ConfirmationDialog.Close());
        }
    }


    public async void CloseScene() {
        (bool success, string message) = await Base.GameManager.Instance.CloseScene(true);
        if (success) {

            ConfirmationDialog.Close();
            MenuManager.Instance.MainMenu.Close();
        }
    }


    public void ShowNewObjectTypeMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.NewObjectTypeMenu);
    }


}

