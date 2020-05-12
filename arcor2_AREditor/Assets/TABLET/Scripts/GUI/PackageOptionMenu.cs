using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class PackageOptionMenu : TileOptionMenu {

    private PackageTile packageTile;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ConfirmationDialog confirmationDialog;
    public override void SetStar(bool starred) {
        PlayerPrefsHelper.SaveBool("package/" + packageTile.PackageId + "/starred", starred);
        SetStar(packageTile, starred);
        Close();
    }

    public void Open(PackageTile packageTile) {
        this.packageTile = packageTile;
        Open((Tile) packageTile);
    }

    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove scene",
                         "Are you sure you want to remove scene " + packageTile.GetLabel() + "?",
                         () => RemovePackage(),
                         () => inputDialog.Close());
    }

    public async void RemovePackage() {
        GameManager.Instance.ShowLoadingScreen();
        if (await GameManager.Instance.RemovePackage(packageTile.PackageId)) {
            await GameManager.Instance.LoadPackages();
            confirmationDialog.Close();
            Close();
        }
        GameManager.Instance.HideLoadingScreen();
    }

}
