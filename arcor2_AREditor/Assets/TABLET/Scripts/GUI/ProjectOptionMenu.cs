using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class ProjectOptionMenu : TileOptionMenu
{
    private ProjectTile projectTile;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    protected override void Start() {
        base.Start();
        Debug.Assert(inputDialog != null);
        Debug.Assert(confirmationDialog != null);
    }

    public void Open(ProjectTile tile) {
        projectTile = tile;
        Open((Tile) tile);
    }

    public override void SetStar(bool starred) {
        PlayerPrefsHelper.SaveBool("project/" + projectTile.ProjectId + "/starred", starred);
        projectTile.SetStar(starred);
        Close();
    }
    public void ShowRenameDialog() {
        inputDialog.Open("Rename project",
                         "",
                         "New name",
                         projectTile.GetLabel(),
                         () => RenameProject(inputDialog.GetValue()),
                         () => inputDialog.Close(),
                         validateInput: ValidateProjectName);
    }

    public RequestResult ValidateProjectName(string newName) {
        Task<RequestResult> result = Task.Run(async () => await GameManager.Instance.RenameProject(projectTile.ProjectId, newName, true));
       
        return result.Result;        
    }

    public async void RenameProject(string newUserId) {
        Base.GameManager.Instance.ShowLoadingScreen();
        Base.RequestResult result = await Base.GameManager.Instance.RenameProject(projectTile.ProjectId, newUserId, false);
        if (result.Success) {
            inputDialog.Close();
            projectTile.SetLabel(newUserId);
            SetLabel(newUserId);
            Close();
        }
        Base.GameManager.Instance.HideLoadingScreen();
    }

    public void ShowRemoveDialog() {
        confirmationDialog.Open("Remove project",
                         "Are you sure you want to remove project " + projectTile.GetLabel() + "?",
                         () => RemoveProject(),
                         () => inputDialog.Close());
    }

    public async void RemoveProject() {
        Base.GameManager.Instance.ShowLoadingScreen();
        bool result = await Base.GameManager.Instance.RemoveProject(projectTile.ProjectId);
        if (result) {
            confirmationDialog.Close();
            Close();
        }
        Base.GameManager.Instance.HideLoadingScreen();
    }

    public void ShowRelatedScene() {
        MainScreen.Instance.ShowRelatedScene(projectTile.SceneId);
        Close();
    }


    public async void ChangeImage() {
        Base.GameManager.Instance.ShowLoadingScreen();
        System.Tuple<Sprite, string> image = await ImageHelper.LoadSpriteAndSaveToDb();
        if (image != null) {
            PlayerPrefsHelper.SaveString(projectTile.ProjectId + "/image", image.Item2);
            projectTile.TopImage.sprite = image.Item1;
        }
        Close();
        Base.GameManager.Instance.HideLoadingScreen();
    }


}
