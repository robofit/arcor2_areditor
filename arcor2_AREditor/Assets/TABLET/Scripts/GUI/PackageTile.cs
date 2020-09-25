using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PackageTile : Tile
{
    public string PackageId;

    [SerializeField]
    private TMPro.TMP_Text projectName, timestamp;

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, DateTime created, DateTime modified, string packageId,
        string projectName, string timestamp) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible, created, modified);
        PackageId = packageId;
        string filename = PlayerPrefsHelper.LoadString(packageId + "/image", "");
        if (!string.IsNullOrEmpty(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
        this.projectName.text = "Project: " + projectName;
        this.timestamp.text = "Created: " + timestamp;
    }



}
