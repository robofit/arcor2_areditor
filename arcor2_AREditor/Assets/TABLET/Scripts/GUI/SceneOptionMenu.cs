using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : TileOptionMenu {
    
    private SceneTile sceneTile;
    [SerializeField]
    private InputDialog inputDialog;

    public void Open(SceneTile sceneTile) {
        this.sceneTile = sceneTile;
        Open((Tile) sceneTile);
    }

    public override void SetStar(bool starred) {
        Base.GameManager.Instance.SaveBool("scene/" + GetLabel() + "/starred", starred);
        SetStar(sceneTile, starred);
        Close();
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename scene",
                         "Type new name",
                         "New name",
                         sceneTile.GetLabel(),
                         () => RenameScene(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameScene(string newUserId) {
        bool result = await Base.GameManager.Instance.RenameScene(sceneTile.SceneId, newUserId);
        if (result) {
            inputDialog.Close();
            sceneTile.SetLabel(newUserId);
            SetLabel(newUserId);
        }
    }

}
