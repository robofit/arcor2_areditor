using UnityEngine;
using System.IO;
using System;
using Base;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SceneOptionMenu : TileOptionMenu {

    private SceneTile sceneTile;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ConfirmationDialog confirmationDialog;
    public ConfirmationDialog ConfirmationDialog => confirmationDialog;
    

    protected override void Start() {
        base.Start();
        Debug.Assert(inputDialog != null);
        Debug.Assert(ConfirmationDialog != null);
    }

    public void Open(SceneTile sceneTile) {
        this.sceneTile = sceneTile;
        Open((Tile) sceneTile);
    }

    public override void SetStar(bool starred) {
        PlayerPrefsHelper.SaveBool("scene/" + sceneTile.SceneId + "/starred", starred);
        SetStar(sceneTile, starred);
        Close();
    }

    public async void ShowRenameDialog() {
        if (!await WriteLockProjectOrScene(sceneTile.SceneId))
            return;
        inputDialog.Open("Rename scene",
                         "",
                         "New name",
                         sceneTile.GetLabel(),
                         () => RenameScene(inputDialog.GetValue()),
                         () => CloseRenameDialog(),
                         validateInput: ValidateSceneNameAsync);
    }

    private async void CloseRenameDialog() {
        inputDialog.Close();
    }

    public async Task<RequestResult> ValidateSceneNameAsync(string newName) {
        try {
            await WebsocketManager.Instance.RenameScene(sceneTile.SceneId, newName, true);
            return (true, "");
        } catch (RequestFailedException e) {
            return (false, e.Message);
        }
    }

    public async void RenameScene(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RenameScene(sceneTile.SceneId, newUserId, false);
            inputDialog.Close();
            sceneTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename scene", e.Message);
        } finally {
            Base.GameManager.Instance.HideLoadingScreen();
        }        
    }


    public async void ShowRemoveDialog() {
        int projects;
        try {
            projects = (await WebsocketManager.Instance.GetProjectsWithScene(sceneTile.SceneId)).Count;
        } catch (RequestFailedException e) {
            Debug.LogError(e);
            return;
        }
        if (projects == 1) {
            Base.Notifications.Instance.ShowNotification("Failed to remove scene", "There is one project associated with this scene. Remove it first.");
            return;
        } else if (projects > 1) {
            Base.Notifications.Instance.ShowNotification("Failed to remove scene", "There are " + projects + " projects associated with this scene. Remove them first.");
            return;
        }
        ConfirmationDialog.Open("Remove scene",
                         "Are you sure you want to remove scene " + sceneTile.GetLabel() + "?",
                         () => RemoveScene(),
                         () => inputDialog.Close());
    }

    public async void RemoveScene() {
        Base.GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RemoveScene(sceneTile.SceneId);
            ConfirmationDialog.Close();
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove scene", e.Message);
        } finally {
            Base.GameManager.Instance.HideLoadingScreen();
        }
    }

    public void ShowRelatedProjects() {
        MainScreen.Instance.ShowRelatedProjects(sceneTile.SceneId);
        Close();
    }


    public async void ChangeImage() {
        GameManager.Instance.ShowLoadingScreen();
        Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(sceneTile.SceneId + "/image", image.Item2);
            sceneTile.TopImage.sprite = image.Item1;
        }
        Close();
        GameManager.Instance.HideLoadingScreen();

    }

    public void NewProject() {
        MainScreen.Instance.NewProjectDialog.Open(sceneTile.GetLabel());
    }

   


}
