using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.IO;

public class SceneTile : Tile
{
    public string SceneId;
    [SerializeField]
    private TMPro.TMP_Text timestamp;

    public void ShowMessage(string msg) {
        Debug.LogError(msg);
    }

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string sceneId, string timestamp) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible);
        SceneId = sceneId;
        string filename = PlayerPrefsHelper.LoadString(SceneId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
        SetTimestamp(timestamp);
    }

    public void SetTimestamp(string timestamp) {
        this.timestamp.text = "Last modified: " + timestamp;
    }

}
