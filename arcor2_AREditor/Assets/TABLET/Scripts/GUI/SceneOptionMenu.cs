using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : TileOptionMenu {
    
    private SceneTile sceneTile;

    public void Open(SceneTile sceneTile) {
        this.sceneTile = sceneTile;
        Open(sceneTile.GetLabel(), sceneTile.GetStarred());
    }

    public override void SetStar(bool starred) {
        Base.GameManager.Instance.SaveBool("scene/" + GetLabel() + "/starred", starred);
        sceneTile.SetStar(starred);
        Close();
    }

    

}
