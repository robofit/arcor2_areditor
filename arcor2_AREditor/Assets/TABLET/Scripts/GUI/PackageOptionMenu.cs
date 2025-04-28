using System;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
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
            var response = await CommunicationManager.Instance.Client.RemovePackageAsync(new IdArgs(packageTile.PackageId));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove package", string.Join(',', response.Messages));
                return;
            }
            CommunicationManager.Instance.Client.ListPackagesAsync().ContinueWith(task => MainScreen.Instance.LoadPackages(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
            confirmationDialog.Close();
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to remove package", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }

    }

    public async void ChangeImage() {
        GameManager.Instance.ShowLoadingScreen();
        Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
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
            var response = await CommunicationManager.Instance.Client.RenamePackageAsync(new RenamePackageRequestArgs(packageTile.PackageId, newName));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to rename package", string.Join(',', response.Messages));
                return (false, response.Messages.FirstOrDefault());
            }
            return (true, "");
        } catch (Arcor2ConnectionException e) {
            return (false, e.Message);
        }
    }

    public async void RenamePackage(string newUserId) {
        GameManager.Instance.ShowLoadingScreen();
        try {
            var response = await CommunicationManager.Instance.Client.RenamePackageAsync(new RenamePackageRequestArgs(packageTile.PackageId, newUserId));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to rename package", string.Join(',', response.Messages));
                return;
            }
            inputDialog.Close();
            packageTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to rename package", e.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen();
        }
    }


}
