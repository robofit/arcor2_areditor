using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        InitTile(projectId);
        this.sceneName.text = "Scene: " + sceneName;
        SetTimestamp(modified.ToString());
    }

    private void InitTile(string projectId) {
        string filename = PlayerPrefsHelper.LoadString(projectId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
    }

    public void InitInvalidProject(string projectId, string name, DateTime created, DateTime modified, bool starVisible, string sceneName = "unknown") {
        SetLabel($"{name} (invalid)");
        this.sceneName.text = "Scene: " + sceneName;
        Created = created;
        Modified = modified;
        SetTimestamp(modified.ToString());
        InitTile(projectId);
        SetStar(starVisible);
        OptionButton.gameObject.SetActive(false);
        MainButton.interactable = false;
        TopImage.color = new Color(1, 1, 1, 0.4f);
        Outline.color = Color.gray;
    }

    public void SetTimestamp(string value) {
        this.timestamp.text = "Last modified: " + value;
    }

    
}
