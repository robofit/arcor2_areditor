using System;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;

public class ProjectOptionMenu : TileOptionMenu
{
    private ProjectTile projectTile;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    protected override void Start() {
        base.Start();
        Debug.Assert(inputDialog != null);
        Debug.Assert(confirmationDialog != null);
    }

    public void Open(ProjectTile tile) {
        projectTile = tile;
        Open((Tile) tile);
    }

    public override void SetStar(bool starred) {
        PlayerPrefsHelper.SaveBool("project/" + projectTile.ProjectId + "/starred", starred);
        projectTile.SetStar(starred);
        Close();
    }
    public async void ShowRenameDialog() {
        if (!await WriteLockProjectOrScene(projectTile.ProjectId))
            return;
        inputDialog.Open("Rename project",
                         "",
                         "New name",
                         projectTile.GetLabel(),
                         () => RenameProject(inputDialog.GetValue()),
                         () => inputDialog.Close(),
                         validateInput: ValidateProjectNameAsync);
    }

    public async Task<RequestResult> ValidateProjectNameAsync(string newName) {
        try {
            var response =
                await CommunicationManager.Instance.Client.RenameProjectAsync(new RenameProjectRequestArgs(projectTile.ProjectId, newName), true);
            if (!response.Result) {
                return (false, response.Messages.FirstOrDefault());
            }
            return (true, "");
        } catch (Arcor2ConnectionException e) {
            return (false, e.Message);
        }
    }

    public async void RenameProject(string newUserId) {
        GameManager.Instance.ShowLoadingScreen();
        try {
            var response =
                await CommunicationManager.Instance.Client.RenameProjectAsync(new RenameProjectRequestArgs(projectTile.ProjectId, newUserId));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to rename project", string.Join(",", response.Messages));
                return;
            }
            inputDialog.Close();
            projectTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to rename project", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }        
    }

    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove project",
                         "Are you sure you want to remove project " + projectTile.GetLabel() + "?",
                         () => RemoveProject(),
                         () => inputDialog.Close());
    }

    public async void RemoveProject() {
        GameManager.Instance.ShowLoadingScreen();
        try {
            var response =
                await CommunicationManager.Instance.Client.RemoveProjectAsync(new IdArgs(projectTile.ProjectId));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove project", string.Join(",", response.Messages));
                return;
            }
            confirmationDialog.Close();
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to remove project", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }        
    }

    public void ShowRelatedScene() {
        MainScreen.Instance.ShowRelatedScene(projectTile.SceneId);
        Close();
    }


    public async void ChangeImage() {
        GameManager.Instance.ShowLoadingScreen();
        Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(projectTile.ProjectId + "/image", image.Item2);
            projectTile.TopImage.sprite = image.Item1;
        }
        Close();
        GameManager.Instance.HideLoadingScreen();
    }


    public async void DuplicateProject() {
        try {
            string name = ProjectManager.Instance.GetFreeProjectName($"{projectTile.GetLabel()}_copy");
            GameManager.Instance.ShowLoadingScreen($"Creating {name} project...");
            var response = await CommunicationManager.Instance.Client.DuplicateProjectAsync(new CopyProjectRequestArgs(projectTile.ProjectId, name), false);
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to duplicate project",
                                    string.Join(',', response.Messages));
                return;
            }
            Close();
            MainScreen.Instance.SwitchToProjects();
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to duplicate project", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }
    }


}
