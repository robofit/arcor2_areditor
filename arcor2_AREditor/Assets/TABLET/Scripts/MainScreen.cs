using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MainScreen : Base.Singleton<MainScreen>
{
    public TMPro.TMP_Text ScenesBtn, ProjectsBtn;
    public GameObject SceneTilePrefab, TileNewPrefab, ProjectTilePrefab, ScenesDynamicContent, ProjectsDynamicContent;
    public NewSceneDialog NewSceneDialog;
    public NewProjectDialog NewProjectDialog;

    [SerializeField]
    private SceneOptionMenu SceneOptionMenu;

    [SerializeField]
    private ProjectOptionMenu ProjectOptionMenu;

    [SerializeField]
    private CanvasGroup projectsList, scenesList;

    [SerializeField]
    private CanvasGroup CanvasGroup;

    private List<SceneTile> sceneTiles = new List<SceneTile>();
    private List<ProjectTile> projectTiles = new List<ProjectTile>();

    //filters
    private bool starredOnly = false;

    private void ShowSceneProjectManagerScreen(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }

    private void HideSceneProjectManagerScreen(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }

    private void Start() {
        Base.GameManager.Instance.OnOpenMainScreen += ShowSceneProjectManagerScreen;
        Base.GameManager.Instance.OnOpenProjectEditor += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnOpenSceneEditor += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnDisconnectedFromServer += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;
        Base.GameManager.Instance.OnProjectsListChanged += UpdateProjects;
        SwitchToScenes();
    }

    public void SwitchToProjects() {
        ScenesBtn.color = new Color(0.687f, 0.687f, 0.687f);
        ProjectsBtn.color = new Color(0, 0, 0);
        projectsList.alpha = 1;
        projectsList.blocksRaycasts = true;
        scenesList.alpha = 0;
        scenesList.blocksRaycasts = false;
    }

    public void SwitchToScenes() {
        ScenesBtn.color = new Color(0, 0, 0);
        ProjectsBtn.color = new Color(0.687f, 0.687f, 0.687f);
        projectsList.alpha = 0;
        projectsList.blocksRaycasts = false;
        scenesList.alpha = 1;
        scenesList.blocksRaycasts = true;
    }

    public void FilterLists() {
        foreach (SceneTile tile in sceneTiles) {
            FilterTile(tile);
        }
        foreach (ProjectTile tile in projectTiles) {
            FilterTile(tile);
        }
    }

    public void FilterTile(Tile tile) {
        if (starredOnly && !tile.GetStarred())
            tile.gameObject.SetActive(false);
        else
            tile.gameObject.SetActive(true);
    }

    public void EnableRecent(bool enable) {
        if (enable) {
            starredOnly = false;
            FilterLists();
        }            
    }

    public void EnableStarred(bool enable) {
        if (enable) {
            starredOnly = true;
            FilterLists();
        }
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        sceneTiles.Clear();
        foreach (Transform t in ScenesDynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.IdDesc scene in Base.GameManager.Instance.Scenes) {
            SceneTile tile = Instantiate(SceneTilePrefab, ScenesDynamicContent.transform).GetComponent<SceneTile>();
            bool starred = Base.GameManager.Instance.LoadBool("scene/" + scene.Id + "/starred", false);
            tile.InitTile(scene.Id,
                          () => Base.GameManager.Instance.OpenScene(scene.Id),
                          () => SceneOptionMenu.Open(tile),
                          starred);
            sceneTiles.Add(tile);
        }
        Button button = Instantiate(TileNewPrefab, ScenesDynamicContent.transform).GetComponent<Button>();
        // TODO new scene
        button.onClick.AddListener(() => NewSceneDialog.WindowManager.OpenWindow());
    }

    public void UpdateProjects(object sender, EventArgs eventArgs) {
        projectTiles.Clear();
        foreach (Transform t in ProjectsDynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.ListProjectsResponseData project in Base.GameManager.Instance.Projects) {
            ProjectTile tile = Instantiate(ProjectTilePrefab, ProjectsDynamicContent.transform).GetComponent<ProjectTile>();
            bool starred = Base.GameManager.Instance.LoadBool("project/" + project.Id + "/starred", false);
            tile.InitTile(project.Id,
                          () => Base.GameManager.Instance.OpenProject(project.Id),
                          () => ProjectOptionMenu.Open(tile),
                          starred);
            projectTiles.Add(tile);
        }
        Button button = Instantiate(TileNewPrefab, ProjectsDynamicContent.transform).GetComponent<Button>();
        // TODO new scene
        button.onClick.AddListener(() => NewProjectDialog.WindowManager.OpenWindow());
    }

    public void NotImplemented() {
        Base.Notifications.Instance.ShowNotification("Not implemented", "Not implemented");
    }
}
