using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectOptionMenu : TileOptionMenu
{
    private ProjectTile projectTile;
    [SerializeField]
    private InputDialog inputDialog;

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
        bool result = await Base.GameManager.Instance.RenameScene(projectTile.ProjectId, newUserId);
        if (result) {
            inputDialog.Close();
            projectTile.SetLabel(newUserId);
        }
    }


}
