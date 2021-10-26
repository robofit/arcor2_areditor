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
        InitTile(sceneId);
        SetTimestamp(modified.ToString());
    }

    private void InitTile(string sceneId) {
        string filename = PlayerPrefsHelper.LoadString(sceneId + "/image", "");
        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
    }

    public void SetTimestamp(string timestamp) {
        this.timestamp.text = "Last modified: " + timestamp;
    }

    public void InitInvalidScene(string sceneName, bool starVisible, DateTime created, DateTime modified, string sceneId, string problem) {
        SetLabel($"{sceneName} (invalid)");
        Created = created;
        Modified = modified;
        SetTimestamp(modified.ToString());
        InitTile(sceneId);
        SetStar(starVisible);
        OptionButton.gameObject.SetActive(false);
        MainButton.interactable = false;
        TopImage.color = new Color(1, 1, 1, 0.4f);
        Outline.color = Color.gray;
        Tooltip.Description = problem;
        Tooltip.EnableTooltip();
    }

}
