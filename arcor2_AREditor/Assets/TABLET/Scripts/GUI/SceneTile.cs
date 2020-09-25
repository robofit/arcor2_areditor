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

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, DateTime created, DateTime modified, string sceneId) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible, created, modified);
        SceneId = sceneId;
        string filename = PlayerPrefsHelper.LoadString(SceneId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
        SetTimestamp(modified.ToString());
    }

    public void SetTimestamp(string timestamp) {
        this.timestamp.text = "Last modified: " + timestamp;
    }

}
