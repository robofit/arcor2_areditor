using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            await WebsocketManager.Instance.RenameProject(projectTile.ProjectId, newName, true);
            return (true, "");
        } catch (RequestFailedException e) {
            return (false, e.Message);
        }
    }

    public async void RenameProject(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RenameProject(projectTile.ProjectId, newUserId, false);
            inputDialog.Close();
            projectTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename project", e.Message);
        } finally {
            Base.GameManager.Instance.HideLoadingScreen();
        }        
    }

    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove project",
                         "Are you sure you want to remove project " + projectTile.GetLabel() + "?",
                         () => RemoveProject(),
                         () => inputDialog.Close());
    }

    public async void RemoveProject() {
        Base.GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RemoveProject(projectTile.ProjectId);
            confirmationDialog.Close();
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove project", e.Message);
        } finally {
            Base.GameManager.Instance.HideLoadingScreen();
        }        
    }

    public void ShowRelatedScene() {
        MainScreen.Instance.ShowRelatedScene(projectTile.SceneId);
        Close();
    }


    public async void ChangeImage() {
        Base.GameManager.Instance.ShowLoadingScreen();
        System.Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(projectTile.ProjectId + "/image", image.Item2);
            projectTile.TopImage.sprite = image.Item1;
        }
        Close();
        Base.GameManager.Instance.HideLoadingScreen();
    }


    public async void DuplicateProject() {
        try {
            string name = ProjectManager.Instance.GetFreeProjectName($"{projectTile.GetLabel()}_copy");
            GameManager.Instance.ShowLoadingScreen($"Creating {name} project...");
            await WebsocketManager.Instance.DuplicateProject(projectTile.ProjectId, name, false);
            Close();
            MainScreen.Instance.SwitchToProjects();
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to duplicate project", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }
    }


}
