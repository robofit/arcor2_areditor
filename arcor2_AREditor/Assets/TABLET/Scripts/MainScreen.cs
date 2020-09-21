using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Playables;
using Base;
using System.Linq;

public class MainScreen : Base.Singleton<MainScreen>
{
    public TMPro.TMP_Text[] ScenesBtns, ProjectsBtns, PackagesBtns;
    public GameObject SceneTilePrefab, TileNewPrefab, ProjectTilePrefab, PackageTilePrefab, ScenesDynamicContent, ProjectsDynamicContent, PackagesDynamicContent;
    public NewProjectDialog NewProjectDialog;
    public InputDialog InputDialog;
    public ButtonWithTooltip AddNewBtn, AscendingBtn, DescendingBtn;

    [SerializeField]
    private SceneOptionMenu sceneOptionMenu;
    public SceneOptionMenu SceneOptionMenu => sceneOptionMenu;

    public List<SceneTile> SceneTiles => sceneTiles;
    public List<ProjectTile> ProjectTiles => projectTiles;
    public List<PackageTile> PackageTiles => packageTiles;

    public CanvasGroup ProjectsList => projectsList;
    public CanvasGroup ScenesList => scenesList;
    public CanvasGroup PackageList => packageList;

    [SerializeField]
    private ProjectOptionMenu projectOptionMenu;
    public ProjectOptionMenu ProjectOptionMenu => projectOptionMenu;

    [SerializeField]
    private PackageOptionMenu PackageOptionMenu;

    [SerializeField]
    private CanvasGroup projectsList, scenesList, packageList;

    [SerializeField]
    private CanvasGroup CanvasGroup;

    [SerializeField]
    private GameObject ButtonsPortrait, ButtonsLandscape;

    private List<SceneTile> sceneTiles = new List<SceneTile>();
    private List<ProjectTile> projectTiles = new List<ProjectTile>();
    private List<PackageTile> packageTiles = new List<PackageTile>();

    //filters
    private bool starredOnly = false;

    private string orderBy = "modified";

    private bool ascendingOrder = false;


    private void ShowSceneProjectManagerScreen(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;

        SortCurrentList();
    }

    private void HideSceneProjectManagerScreen(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }

    private void Update() {
        if (Input.deviceOrientation == DeviceOrientation.Portrait) {
            ButtonsPortrait.SetActive(true);
            ButtonsLandscape.SetActive(false);
        } else {
            ButtonsPortrait.SetActive(false);
            ButtonsLandscape.SetActive(true);
        }
    }

    public void SetOrderBy(string orderBy) {
        this.orderBy = orderBy;
        SortCurrentList();
    }

    public void SetAscending(bool ascending) {
        ascendingOrder = ascending;
        AscendingBtn.gameObject.SetActive(ascending);
        DescendingBtn.gameObject.SetActive(!ascending);
        SortCurrentList();
    }

    private void SortCurrentList() {
        List<Tile> tiles = null;
        if (ScenesList.gameObject.activeSelf) {
            tiles = SceneTiles.Select<SceneTile, Tile>(x => x).ToList();   
        } else if (ProjectsList.gameObject.activeSelf) {
            tiles = ProjectTiles.Select<ProjectTile, Tile>(x => x).ToList();
        } else if (PackageList.gameObject.activeSelf) {
            tiles = PackageTiles.Select<PackageTile, Tile>(x => x).ToList();
        }
        if (tiles == null)
            return;
        switch (orderBy) {
            case "name":
                if (ascendingOrder)
                    tiles.Sort((x, y) => x.GetLabel().CompareTo(y.GetLabel()));
                else
                    tiles.Sort((x, y) => y.GetLabel().CompareTo(x.GetLabel()));
                break;
            case "modified":
                if (ascendingOrder)
                    tiles.Sort((x, y) => x.Modified.CompareTo(y.Modified));
                else
                    tiles.Sort((x, y) => y.Modified.CompareTo(x.Modified));
                break;
            case "created":
                if (ascendingOrder)
                    tiles.Sort((x, y) => x.Created.CompareTo(y.Created));
                else
                    tiles.Sort((x, y) => y.Created.CompareTo(x.Created));
                break;
        }
        for (int i = 0; i < tiles.Count; ++i) {
            tiles[i].transform.SetSiblingIndex(i);
        }
    }

    private void Start() {
        Base.GameManager.Instance.OnOpenMainScreen += ShowSceneProjectManagerScreen;
        Base.GameManager.Instance.OnOpenProjectEditor += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnOpenSceneEditor += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnDisconnectedFromServer += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnRunPackage += HideSceneProjectManagerScreen;
        Base.GameManager.Instance.OnSceneListChanged += UpdateScenes;
        Base.GameManager.Instance.OnProjectsListChanged += UpdateProjects;
        Base.GameManager.Instance.OnPackagesListChanged += UpdatePackages;
        WebsocketManager.Instance.OnProjectRemoved += OnProjectRemoved;
        WebsocketManager.Instance.OnProjectBaseUpdated += OnProjectBaseUpdated;
        WebsocketManager.Instance.OnSceneRemoved += OnSceneRemoved;
        WebsocketManager.Instance.OnSceneBaseUpdated += OnSceneBaseUpdated;
        SwitchToScenes();
    }


    private void OnSceneBaseUpdated(object sender, BareSceneEventArgs args) {
        foreach (SceneTile s in sceneTiles) {
            if (s.SceneId == args.Scene.Id) {
                s.SetLabel(args.Scene.Name);
                s.SetTimestamp(args.Scene.Modified.ToString());
                break;
            }
        }
    }

    private void OnSceneRemoved(object sender, StringEventArgs args) {
        int i = 0;
        foreach (SceneTile s in sceneTiles) {
            if (s.SceneId == args.Data) {
                Destroy(s.gameObject);
                sceneTiles.RemoveAt(i);
                break;
            }
            i++;
        }
    }

    private void OnProjectBaseUpdated(object sender, BareProjectEventArgs args) {
        foreach (ProjectTile p in projectTiles) {
            if (p.ProjectId == args.Project.Id) {
                p.SetLabel(args.Project.Name);
                p.SetTimestamp(args.Project.Modified.ToString());
                break;
            }
        }
    }

    private void OnProjectRemoved(object sender, StringEventArgs args) {
        int i = 0;
        foreach (ProjectTile p in projectTiles) {
            if (p.ProjectId == args.Data) {
                Destroy(p.gameObject);
                projectTiles.RemoveAt(i);
                break;
            }
            i++;
        }
    }

    public void SwitchToProjects() {
        foreach (TMPro.TMP_Text btn in ScenesBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        foreach (TMPro.TMP_Text btn in PackagesBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        foreach (TMPro.TMP_Text btn in ProjectsBtns) {
            btn.color = new Color(0, 0, 0);
        }
        ProjectsList.gameObject.SetActive(true);
        ScenesList.gameObject.SetActive(false);
        PackageList.gameObject.SetActive(false);
        AddNewBtn.gameObject.SetActive(true);
        AddNewBtn.SetDescription("Add project");
        FilterProjectsBySceneId(null);
        FilterLists();
        SortCurrentList();
    }

    public void SwitchToScenes() {
        foreach (TMPro.TMP_Text btn in ScenesBtns) {
            btn.color = new Color(0, 0, 0);
        }
        foreach (TMPro.TMP_Text btn in PackagesBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        foreach (TMPro.TMP_Text btn in ProjectsBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        
        ProjectsList.gameObject.SetActive(false);
        PackageList.gameObject.SetActive(false);
        ScenesList.gameObject.SetActive(true);
        AddNewBtn.gameObject.SetActive(true);
        AddNewBtn.SetDescription("Add scene");
        FilterScenesById(null);
        FilterLists();
        SortCurrentList();
    }

    public void SwitchToPackages() {
        foreach (TMPro.TMP_Text btn in ScenesBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        foreach (TMPro.TMP_Text btn in PackagesBtns) {
            btn.color = new Color(0, 0, 0);
        }
        foreach (TMPro.TMP_Text btn in ProjectsBtns) {
            btn.color = new Color(0.687f, 0.687f, 0.687f);
        }
        ProjectsList.gameObject.SetActive(false);
        ScenesList.gameObject.SetActive(false);
        PackageList.gameObject.SetActive(true);
        AddNewBtn.gameObject.SetActive(false);
        FilterLists();
        SortCurrentList();
    }

    public void HighlightTile(string tileId) {
        foreach (SceneTile s in SceneTiles) {
            if (s.SceneId == tileId) {
                s.Highlight();
                return;
            }            
        }
        foreach (ProjectTile p in ProjectTiles) {
            if (p.ProjectId == tileId) {
                p.Highlight();
                return;
            }            
        }
        foreach (PackageTile p in PackageTiles) {
            if (p.PackageId == tileId) {
                p.Highlight();
                return;
            }            
        }
    }

    public void FilterLists() {
        foreach (SceneTile tile in SceneTiles) {
            FilterTile(tile);
        }
        foreach (ProjectTile tile in ProjectTiles) {
            FilterTile(tile);
        }
        foreach (PackageTile tile in PackageTiles) {
            FilterTile(tile);
        }
    }

    public void FilterTile(Tile tile) {
        if (starredOnly && !tile.GetStarred())
            tile.gameObject.SetActive(false);
        else
            tile.gameObject.SetActive(true);
    }

    public void FilterProjectsBySceneId(string sceneId) {
        foreach (ProjectTile tile in ProjectTiles) {
            if (sceneId == null) {
                tile.gameObject.SetActive(true);
                return;
            }               

            if (tile.SceneId != sceneId) {
                tile.gameObject.SetActive(false);
            }
        }
    }

    public void FilterScenesById(string sceneId) {
        foreach (SceneTile tile in SceneTiles) {
            if (sceneId == null) {
                tile.gameObject.SetActive(true);
                return;
            }

            if (tile.SceneId != sceneId) {
                tile.gameObject.SetActive(false);
            }
        }
    }

    public void ShowRelatedProjects(string sceneId) {
        SwitchToProjects();
        FilterProjectsBySceneId(sceneId);
    }

     public void ShowRelatedScene(string sceneId) {
        SwitchToScenes();
        FilterScenesById(sceneId);
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

    public void AddNew() {
        if (ScenesList.gameObject.activeSelf) {
            ShowNewSceneDialog();
        } else if (ProjectsList.gameObject.activeSelf) {
            NewProjectDialog.Open();
        }
    }

    public void UpdateScenes(object sender, EventArgs eventArgs) {
        SceneTiles.Clear();
        foreach (Transform t in ScenesDynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.ListScenesResponseData scene in Base.GameManager.Instance.Scenes) {
            SceneTile tile = Instantiate(SceneTilePrefab, ScenesDynamicContent.transform).GetComponent<SceneTile>();
            bool starred = PlayerPrefsHelper.LoadBool("scene/" + scene.Id + "/starred", false);
            tile.InitTile(scene.Name,
                          () => Base.GameManager.Instance.OpenScene(scene.Id),
                          () => SceneOptionMenu.Open(tile),
                          starred,
                          scene.Modified,
                          scene.Modified,
                          scene.Id);
            SceneTiles.Add(tile);
        }
        SortCurrentList();
        //Button button = Instantiate(TileNewPrefab, ScenesDynamicContent.transform).GetComponent<Button>();
        // TODO new scene
        //button.onClick.AddListener(ShowNewSceneDialog);
    }

    public async void NewScene(string name) {
        if (await Base.GameManager.Instance.NewScene(name)) {
            InputDialog.Close();
        }
    }

    public void ShowNewSceneDialog() {
        InputDialog.Open("Create new scene",
                         null,
                         "Name",
                         "",
                         () => NewScene(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public void UpdatePackages(object sender, EventArgs eventArgs) {
        PackageTiles.Clear();
        foreach (Transform t in PackagesDynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.PackageSummary package in Base.GameManager.Instance.Packages) {
            PackageTile tile = Instantiate(PackageTilePrefab, PackagesDynamicContent.transform).GetComponent<PackageTile>();
            bool starred = PlayerPrefsHelper.LoadBool("package/" + package.Id + "/starred", false);
            string projectName;
            try {
                projectName = GameManager.Instance.GetProjectName(package.ProjectId);
            } catch (ItemNotFoundException _) {
                projectName = "unknown";
            }            
            tile.InitTile(package.PackageMeta.Name,
                          async () => await Base.GameManager.Instance.RunPackage(package.Id),
                          () => PackageOptionMenu.Open(tile),
                          starred,
                          package.Modified,
                          package.Modified,
                          package.Id,
                          projectName,
                          package.PackageMeta.Built.ToString());
            PackageTiles.Add(tile);
        }
        SortCurrentList();
    }

    public void UpdateProjects(object sender, EventArgs eventArgs) {
        ProjectTiles.Clear();
        foreach (Transform t in ProjectsDynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.ListProjectsResponseData project in Base.GameManager.Instance.Projects) {
            ProjectTile tile = Instantiate(ProjectTilePrefab, ProjectsDynamicContent.transform).GetComponent<ProjectTile>();
            bool starred = PlayerPrefsHelper.LoadBool("project/" + project.Id + "/starred", false);
            try {
                string sceneName = GameManager.Instance.GetSceneName(project.SceneId);
                tile.InitTile(project.Name,
                              () => GameManager.Instance.OpenProject(project.Id),
                              () => ProjectOptionMenu.Open(tile),
                              starred,
                              project.Modified,
                              project.Modified,
                              project.Id,
                              project.SceneId,
                              sceneName); 
                ProjectTiles.Add(tile);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.SaveLogs("Failed to load scene name.");
            }
        }
        SortCurrentList();
        // Button button = Instantiate(TileNewPrefab, ProjectsDynamicContent.transform).GetComponent<Button>();
        // TODO new scene
        // button.onClick.AddListener(() => NewProjectDialog.Open());
    }

    public void NotImplemented() {
        Base.Notifications.Instance.ShowNotification("Not implemented", "Not implemented");
    }

    public void SaveLogs() {
        Base.Notifications.Instance.SaveLogs();
    }

    public bool IsActive() {
        return CanvasGroup.alpha == 1 && CanvasGroup.blocksRaycasts == true;
    }
    public bool IsInactive() {
        return CanvasGroup.alpha == 0 && CanvasGroup.blocksRaycasts == false;
    }

    public SceneTile GetSceneTile(string sceneName) {
        foreach (SceneTile sceneTile in MainScreen.Instance.SceneTiles) {
            if (sceneTile.GetLabel() == sceneName) {
                return sceneTile;
            }
        }
        throw new ItemNotFoundException("Scene tile not found");
    }

    public ProjectTile GetProjectTile(string projectName) {
        foreach (ProjectTile projectTile in projectTiles) {
            if (projectTile.GetLabel() == projectName) {
                return projectTile;
            }
        }
        throw new ItemNotFoundException("Project tile not found");
    }
}
