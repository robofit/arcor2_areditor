using UnityEngine;
using System.IO;
using System;
using Base;
using UnityEngine.UI;

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
        PlayerPrefsHelper.SaveBool("scene/" + sceneTile.SceneId + "/starred", starred);
        SetStar(sceneTile, starred);
        Close();
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename scene",
                         "",
                         "New name",
                         sceneTile.GetLabel(),
                         () => RenameScene(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameScene(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        bool result = await Base.GameManager.Instance.RenameScene(sceneTile.SceneId, newUserId);
        if (result) {
            inputDialog.Close();
            sceneTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        }
        Base.GameManager.Instance.HideLoadingScreen();
    }


    public async void ShowRemoveDialog() {
        int projects = (await Base.GameManager.Instance.GetProjectsWithScene(sceneTile.SceneId)).Count;
        if (projects == 1) {
            Base.Notifications.Instance.ShowNotification("Failed to remove scene", "There is one project associated with this scene. Remove it first.");
            return;
        } else if (projects > 1) {
            Base.Notifications.Instance.ShowNotification("Failed to remove scene", "There are " + projects + " projects associated with this scene. Remove them first.");
            return;
        }
        confirmationDialog.Open("Remove scene",
                         "Are you sure you want to remove scene " + sceneTile.GetLabel() + "?",
                         () => RemoveScene(),
                         () => inputDialog.Close());
    }

    public async void RemoveScene() {
        Base.GameManager.Instance.ShowLoadingScreen();
        bool result = await Base.GameManager.Instance.RemoveScene(sceneTile.SceneId);
        if (result) {
            confirmationDialog.Close();
            Close();
        }
        Base.GameManager.Instance.HideLoadingScreen();
    }

    public void ShowRelatedProjects() {
        MainScreen.Instance.ShowRelatedProjects(sceneTile.SceneId);
        Close();
    }


    public async void ChangeImage() {
        GameManager.Instance.ShowLoadingScreen();
        Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(sceneTile.SceneId + "/image", image.Item2);
            sceneTile.TopImage.sprite = image.Item1;
        }
        Close();
        GameManager.Instance.HideLoadingScreen();
        /*NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
           Notifications.Instance.ShowNotification("Image path: ", path);
            
        }, "Select a PNG image", "image/png");
        */

    }

   


}
