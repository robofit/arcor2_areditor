using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PackageTile : Tile
{
    public string PackageId;
   

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string packageId) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible);
        PackageId = packageId;
        string filename = PlayerPrefsHelper.LoadString(packageId + "/image", "");
        if (!string.IsNullOrEmpty(filename)) {
            Sprite sprite = ImageHelper.LoadNewSprite(filename);
            TopImage.sprite = sprite;
        }
    }



}
