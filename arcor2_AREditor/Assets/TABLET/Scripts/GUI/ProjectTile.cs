using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ProjectTile : Tile
{
    public string ProjectId;
    public string SceneId;

    [SerializeField]
    private TMPro.TMP_Text sceneName, timestamp;

    public void InitTile(string userId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string projectId, string sceneId,
        string sceneName, string timestamp) {
        base.InitTile(userId, mainCallback, optionCallback, starVisible);
        ProjectId = projectId;
        SceneId = sceneId;
        string filename = PlayerPrefsHelper.LoadString(projectId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
        this.sceneName.text = "Scene: " + sceneName;
        this.timestamp.text = "Last modified: " + timestamp;
    }
}
