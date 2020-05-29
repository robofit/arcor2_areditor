using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async void ChangeImage() {
        GameManager.Instance.ShowLoadingScreen();
        System.Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(packageTile.PackageId + "/image", image.Item2);
            packageTile.TopImage.sprite = image.Item1;
        }
        Close();
        GameManager.Instance.HideLoadingScreen();
    }

    // TODO: add validation once the rename rename package RPC has dryRun parameter
    public void ShowRenameDialog() {
        inputDialog.Open("Rename package",
                         "",
                         "New name",
                         packageTile.GetLabel(),
                         () => RenamePackage(inputDialog.GetValue()),
                         () => inputDialog.Close(),
                         /*validateInput: ValidateProjectName*/ 
                         null);
    }

    public RequestResult ValidateProjectName(string newName) {
        Task<RequestResult> result = Task.Run(async () => await GameManager.Instance.RenamePackage(packageTile.PackageId, newName, true));
        return result.Result;
    }

    public async void RenamePackage(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        Base.RequestResult result = await Base.GameManager.Instance.RenamePackage(packageTile.PackageId, newUserId, false);
        if (result.Success) {
            inputDialog.Close();
            packageTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        }
        Base.GameManager.Instance.HideLoadingScreen();
    }


}
