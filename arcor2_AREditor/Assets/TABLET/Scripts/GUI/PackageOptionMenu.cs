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
        confirmationDialog.Open("Remove package",
                         "Are you sure you want to remove package " + packageTile.GetLabel() + "?",
                         () => RemovePackage(),
                         () => inputDialog.Close());
    }

    public async void RemovePackage() {
        GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RemovePackage(packageTile.PackageId);
            WebsocketManager.Instance.LoadPackages(MainScreen.Instance.LoadPackagesCb);
            confirmationDialog.Close();
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove package", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }

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
                         validateInput: ValidateProjectName);
    }

    public async Task<RequestResult> ValidateProjectName(string newName) {
        try {
            await WebsocketManager.Instance.RenamePackage(packageTile.PackageId, newName, true);
            return (true, "");
        } catch (RequestFailedException e) {
            return (false, e.Message);
        }
    }

    public async void RenamePackage(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        try {
            await WebsocketManager.Instance.RenamePackage(packageTile.PackageId, newUserId, false);
            inputDialog.Close();
            packageTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename package", e.Message);
        } finally {
            Base.GameManager.Instance.HideLoadingScreen();
        }        
    }


}
