using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class SceneTile : Tile
{
    public string SceneId;
    public void ShowMessage(string msg) {
        Debug.LogError(msg);
    }

    public void InitTile(string sceneUserId, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, string sceneId) {
        base.InitTile(sceneUserId, mainCallback, optionCallback, starVisible);
        SceneId = sceneId;
    }



}
