using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectOptionMenu : TileOptionMenu
{
    private ProjectTile projectTile;

    public void Open(ProjectTile tile) {
        projectTile = tile;
        Open((Tile) tile);
    }

    public override void SetStar(bool starred) {
        PlayerPrefsHelper.SaveBool("project/" + GetLabel() + "/starred", starred);
        projectTile.SetStar(starred);
        Close();
    }

}
