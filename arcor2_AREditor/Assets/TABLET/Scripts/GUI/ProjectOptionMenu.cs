using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectOptionMenu : TileOptionMenu
{
    private ProjectTile projectTile;

    public void Open(ProjectTile tile) {
        projectTile = tile;
        Open(tile.GetLabel(), tile.GetStarred());
    }

    public override void SetStar(bool starred) {
        Base.GameManager.Instance.SaveBool("project/" + GetLabel() + "/starred", starred);
        projectTile.SetStar(starred);
        Close();
    }
}
