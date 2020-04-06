using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ProjectTile : Tile
{
    public string ProjectId;

    public void InitTile(string userId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string projectId) {
        base.InitTile(userId, mainCallback, optionCallback, starVisible);
        ProjectId = projectId;
    }
}
