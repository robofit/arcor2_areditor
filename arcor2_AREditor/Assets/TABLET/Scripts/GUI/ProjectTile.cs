using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ProjectTile : Tile
{
    public string ProjectId;
    public string SceneId;

    public void InitTile(string userId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string projectId, string sceneId) {
        base.InitTile(userId, mainCallback, optionCallback, starVisible);
        ProjectId = projectId;
        SceneId = sceneId;
    }
}
