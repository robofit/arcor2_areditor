using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;

public class SceneOptionMenu : TileOptionMenu {
    
    private SceneTile sceneTile;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    protected override void Start() {
        base.Start();
        Debug.Assert(inputDialog != null);
        Debug.Assert(confirmationDialog != null);
    }

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


    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove scene",
                         "Are you sure you want to remove scene " + sceneTile.GetLabel() + "?",
                         () => RemoveScene(),
                         () => inputDialog.Close());
    }

    public async void RemoveScene() {
        bool result = await Base.GameManager.Instance.RemoveScene(sceneTile.SceneId);
        if (result) {
            confirmationDialog.Close();
            Close();
        }
    }

}
