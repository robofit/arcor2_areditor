using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ProjectTile : Tile
{
    public string ProjectId;

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string projectId) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible);
        ProjectId = projectId;
    }
}
