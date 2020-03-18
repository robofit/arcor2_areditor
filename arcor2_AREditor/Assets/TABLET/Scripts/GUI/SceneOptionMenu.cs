using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : MonoBehaviour {
    [SerializeField]
    private SimpleSideMenu menu;
    [SerializeField]
    private TMPro.TMP_Text sceneName;
    [SerializeField]
    private GameObject AddStarBtn, RemoveStarBtn;
    private SceneTile sceneTile;

    public void Open(SceneTile sceneTile) {
        this.sceneTile = sceneTile;
        menu.Open();
        sceneName.text = sceneTile.GetLabel();
        AddStarBtn.SetActive(!sceneTile.GetStarred());
        RemoveStarBtn.SetActive(sceneTile.GetStarred());
    }

    public void Close() {
        menu.Close();
    }

    public void SetStar(bool starred) {
        Base.GameManager.Instance.SaveBool("scene/" + GetSceneName() + "/starred", starred);
        sceneTile.SetStar(starred);
        Close();
    }

    public string GetSceneName() {
        return sceneName.text;
    }

}
