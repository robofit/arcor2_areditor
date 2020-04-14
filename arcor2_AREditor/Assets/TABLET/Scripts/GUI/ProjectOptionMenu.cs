using System.Collections;
using System.Collections.Generic;
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
        Base.GameManager.Instance.SaveBool("project/" + GetLabel() + "/starred", starred);
        projectTile.SetStar(starred);
        Close();
    }
    public void ShowRenameDialog() {
        inputDialog.Open("Rename project",
                         "Type new name",
                         "New name",
                         projectTile.GetLabel(),
                         () => RenameProject(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameProject(string newUserId) {
        bool result = await Base.GameManager.Instance.RenameProject(projectTile.ProjectId, newUserId);
        if (result) {
            inputDialog.Close();
            projectTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        }
    }

    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove project",
                         "Are you sure you want to remove project " + projectTile.GetLabel() + "?",
                         () => RemoveProject(),
                         () => inputDialog.Close());
    }

    public async void RemoveProject() {
        bool result = await Base.GameManager.Instance.RemoveProject(projectTile.ProjectId);
        if (result) {
            confirmationDialog.Close();
            Close();
        }
    }

    public void ShowRelatedScene() {
        MainScreen.Instance.ShowRelatedScene(projectTile.SceneId);
        Close();
    }


}
