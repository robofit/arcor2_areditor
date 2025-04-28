using System;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;

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
            var response = await CommunicationManager.Instance.Client.RenameSceneAsync(new RenameArgs(sceneTile.SceneId, newName), true);
            if (!response.Result) {
                return (false, response.Messages.FirstOrDefault());
            }
            return (true, "");
        } catch (Arcor2ConnectionException e) {
            return (false, e.Message);
        }
    }

    public async void RenameScene(string newUserId) {
        GameManager.Instance.ShowLoadingScreen();
        try {
            var response = await CommunicationManager.Instance.Client.RenameSceneAsync(new RenameArgs(sceneTile.SceneId, newUserId), false);
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to rename scene", string.Join(',', response.Messages));
                return;
            }
            inputDialog.Close();
            sceneTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to rename scene", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }
    }


    public async void ShowRemoveDialog() {
        int projects;
        try {
            var response = await CommunicationManager.Instance.Client.GetProjectsWithSceneAsync(new IdArgs(sceneTile.SceneId));
            if (!response.Result) {
                Debug.LogError(string.Join(',', response.Messages));
                return;
            }

            projects = response.Data.Count;
        } catch (Arcor2ConnectionException e) {
            Debug.LogError(e);
            return;
        }
        if (projects == 1) {
            Notifications.Instance.ShowNotification("Failed to remove scene", "There is one project associated with this scene. Remove it first.");
            return;
        } else if (projects > 1) {
            Notifications.Instance.ShowNotification("Failed to remove scene", "There are " + projects + " projects associated with this scene. Remove them first.");
            return;
        }
        ConfirmationDialog.Open("Remove scene",
                         "Are you sure you want to remove scene " + sceneTile.GetLabel() + "?",
                         () => RemoveScene(),
                         () => inputDialog.Close());
    }

    public async void RemoveScene() {
        GameManager.Instance.ShowLoadingScreen();
        try {
            var response =
                await CommunicationManager.Instance.Client.RemoveSceneAsync(new IdArgs(sceneTile.SceneId));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove scene", string.Join(',', response.Messages));
                return;
            }
            ConfirmationDialog.Close();
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to remove scene", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
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
    public async void DuplicateScene() {
        try {
            string name = SceneManager.Instance.GetFreeSceneName($"{sceneTile.GetLabel()}_copy");
            GameManager.Instance.ShowLoadingScreen($"Creating {name} project...");
            var response = await CommunicationManager.Instance.Client.DuplicateSceneAsync(new CopySceneRequestArgs(sceneTile.SceneId, name));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to duplicate scenne",
                    string.Join(',', response.Messages));
                return;
            }
            Close();
            MainScreen.Instance.SwitchToScenes();
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to duplicate scene", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }
    }



}
