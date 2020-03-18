using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : MonoBehaviour {
    [SerializeField]
    private SimpleSideMenu menu;
    [SerializeField]
    private TMPro.TMP_Text sceneName;

    public void Open(string sceneId) {
        menu.Open();
        sceneName.text = sceneId;
    }

    public void Close() {
        menu.Close();
    }

    

}
