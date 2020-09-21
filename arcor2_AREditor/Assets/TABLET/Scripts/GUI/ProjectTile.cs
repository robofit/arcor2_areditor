using System;
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

    public void InitTile(string name, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, DateTime created, DateTime modified, string projectId, string sceneId,
        string sceneName) {
        base.InitTile(name, mainCallback, optionCallback, starVisible, created, modified);
        ProjectId = projectId;
        SceneId = sceneId;
        string filename = PlayerPrefsHelper.LoadString(projectId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
        this.sceneName.text = "Scene: " + sceneName;
        SetTimestamp(modified.ToString());
    }

    public void SetTimestamp(string value) {
        this.timestamp.text = "Last modified: " + value;
    }

    
}
