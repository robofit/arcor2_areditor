using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : TileOptionMenu {
    
    private SceneTile sceneTile;

    public void Open(SceneTile sceneTile) {
        this.sceneTile = sceneTile;
        Open((Tile) sceneTile);
    }

    public override void SetStar(bool starred) {
        Base.GameManager.Instance.SaveBool("scene/" + GetLabel() + "/starred", starred);
        SetStar(sceneTile, starred);
    }

    

}
