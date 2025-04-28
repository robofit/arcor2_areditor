using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MainScreen : Singleton<MainScreen> {
    public TMP_Text[] ScenesBtns, ProjectsBtns, PackagesBtns;
    public GameObject SceneTilePrefab, TileNewPrefab, ProjectTilePrefab, PackageTilePrefab, ScenesDynamicContent, ProjectsDynamicContent, PackagesDynamicContent;
    public NewProjectDialog NewProjectDialog;
    public InputDialog InputDialog;
    public ButtonWithTooltip AddNewBtn, AscendingBtn, DescendingBtn;

    [SerializeField]
    private SceneOptionMenu sceneOptionMenu;
    public SceneOptionMenu SceneOptionMenu => sceneOptionMenu;

    public List<SceneTile> SceneTiles {
        get;
    } = new();

    public List<ProjectTile> ProjectTiles {
        get;
    } = new();

    public List<PackageTile> PackageTiles {
        get;
    } = new();

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

    private bool scenesLoaded, projectsLoaded, packagesLoaded, scenesUpdating, projectsUpdating, packagesUpdating;

    //filters
    private bool starredOnly = false;

    private string orderBy = "modified";

    private bool ascendingOrder = false;

    private void Awake() {
        scenesLoaded = projectsLoaded = scenesUpdating = projectsUpdating = packagesLoaded = packagesUpdating = false;
    }


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
        GameManager.Instance.OnOpenMainScreen += ShowSceneProjectManagerScreen;
        GameManager.Instance.OnOpenProjectEditor += HideSceneProjectManagerScreen;
        GameManager.Instance.OnOpenSceneEditor += HideSceneProjectManagerScreen;
        GameManager.Instance.OnDisconnectedFromServer += HideSceneProjectManagerScreen;
        GameManager.Instance.OnRunPackage += HideSceneProjectManagerScreen;
        GameManager.Instance.OnScenesListChanged += UpdateScenes;
        GameManager.Instance.OnProjectsListChanged += UpdateProjects;
        GameManager.Instance.OnPackagesListChanged += UpdatePackages;
        CommunicationManager.Instance.Client.ProjectRemoved += CommunicationManager.SafeEventHandler<BareProjectEventArgs>(OnProjectRemoved);
        CommunicationManager.Instance.Client.ProjectBaseUpdated += CommunicationManager.SafeEventHandler<BareProjectEventArgs>(OnProjectBaseUpdated);
        CommunicationManager.Instance.Client.SceneRemoved += CommunicationManager.SafeEventHandler<BareSceneEventArgs>(OnSceneRemoved);
        CommunicationManager.Instance.Client.SceneBaseUpdated += CommunicationManager.SafeEventHandler<BareSceneEventArgs>(OnSceneBaseUpdated);
    }


    private void OnSceneBaseUpdated(object sender, BareSceneEventArgs args) {
        foreach (SceneTile s in SceneTiles) {
            if (s.SceneId == args.Data.Id) {
                s.SetLabel(args.Data.Name);
                s.SetTimestamp(args.Data.Modified.ToString());
                break;
            }
        }
    }

    private void OnSceneRemoved(object sender, BareSceneEventArgs args) {
        int i = 0;
        foreach (SceneTile s in SceneTiles) {
            if (s.SceneId == args.Data.Id) {
                Destroy(s.gameObject);
                SceneTiles.RemoveAt(i);
                break;
            }
            i++;
        }
    }

    private void OnProjectBaseUpdated(object sender, BareProjectEventArgs args) {
        foreach (ProjectTile p in ProjectTiles) {
            if (p.ProjectId == args.Data.Id) {
                p.SetLabel(args.Data.Name);
                p.SetTimestamp(args.Data.Modified.ToString());
                break;
            }
        }
    }

    private void OnProjectRemoved(object sender, BareProjectEventArgs args) {
        int i = 0;
        foreach (ProjectTile p in ProjectTiles) {
            if (p.ProjectId == args.Data.Id) {
                Destroy(p.gameObject);
                ProjectTiles.RemoveAt(i);
                break;
            }
            i++;
        }
    }

    private async Task WaitUntilScenesLoaded() {
        await Task.Run(() => {
            Stopwatch sw = new();
            sw.Start();

            while (true) {
                if (sw.ElapsedMilliseconds > 5000)
                    throw new TimeoutException("Failed to load scenes");
                if (scenesLoaded) {
                    return true;
                } else {
                    Thread.Sleep(10);
                }
            }
        });
    }

    private async Task WaitUntilProjectsLoaded() {
        await Task.Run(() => {
            Stopwatch sw = new();
            sw.Start();

            while (true) {
                if (sw.ElapsedMilliseconds > 5000)
                    throw new TimeoutException("Failed to load projects");
                if (projectsLoaded) {
                    return true;
                } else {
                    Thread.Sleep(10);
                }
            }
        });

    }

    private async Task WaitUntilPackagesLoaded() {
        await Task.Run(() => {
            Stopwatch sw = new();
            sw.Start();

            while (true) {
                if (sw.ElapsedMilliseconds > 5000)
                    throw new TimeoutException("Failed to load packages");
                if (packagesLoaded) {
                    return true;
                } else {
                    Thread.Sleep(10);
                }
            }
        });

    }


    public async void SwitchToProjects() {
        GameManager.Instance.ShowLoadingScreen("Updating projects...", true);
        if (!scenesUpdating) {
            scenesUpdating = true;
            scenesLoaded = false;
            var scenesResponse = await CommunicationManager.Instance.Client.ListScenesAsync();
            LoadScenes(scenesResponse);
        }
        try {
            await WaitUntilScenesLoaded();
            if (!projectsUpdating) {
                projectsUpdating = true;
                projectsLoaded = false;
                var projectsResponse = await CommunicationManager.Instance.Client.ListProjectsAsync();
                LoadProjects(projectsResponse);
            }
            await WaitUntilProjectsLoaded();
            foreach (TMP_Text btn in ScenesBtns) {
                btn.color = new Color(0.687f, 0.687f, 0.687f);
            }
            foreach (TMP_Text btn in PackagesBtns) {
                btn.color = new Color(0.687f, 0.687f, 0.687f);
            }
            foreach (TMP_Text btn in ProjectsBtns) {
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
        } catch (TimeoutException ex) {
            Notifications.Instance.ShowNotification("Failed to switch to projects", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen(true);
        }

    }

    public async void SwitchToScenes() {
        GameManager.Instance.ShowLoadingScreen("Updating scenes..", true);
        if (!scenesUpdating) {
            scenesUpdating = true;
            scenesLoaded = false;
            var scenesResponse = await CommunicationManager.Instance.Client.ListScenesAsync();
            LoadScenes(scenesResponse);
        }
        try {
            await WaitUntilScenesLoaded();


            foreach (TMP_Text btn in ScenesBtns) {
                btn.color = new Color(0, 0, 0);
            }
            foreach (TMP_Text btn in PackagesBtns) {
                btn.color = new Color(0.687f, 0.687f, 0.687f);
            }
            foreach (TMP_Text btn in ProjectsBtns) {
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
        } catch (TimeoutException ex) {
            Notifications.Instance.ShowNotification("Failed to switch to scenes", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen(true);
        }
    }

    public async void SwitchToPackages() {
        GameManager.Instance.ShowLoadingScreen("Updating packages...", true);
        if (!scenesUpdating) {
            scenesUpdating = true;
            scenesLoaded = false;
            var responseScenes = await CommunicationManager.Instance.Client.ListScenesAsync();
            LoadScenes(responseScenes);
        }
        try {
            await WaitUntilScenesLoaded();
            if (!projectsUpdating) {
                projectsUpdating = true;
                projectsLoaded = false;
                var responseProjects = await CommunicationManager.Instance.Client.ListProjectsAsync();
                LoadProjects(responseProjects);
            }
            await WaitUntilProjectsLoaded();
            if (!packagesUpdating) {
                packagesUpdating = true;
                packagesLoaded = false;
                var responsePackages = await CommunicationManager.Instance.Client.ListPackagesAsync();
                LoadPackages(responsePackages);
            }
            await WaitUntilPackagesLoaded();
            foreach (TMP_Text btn in ScenesBtns) {
                btn.color = new Color(0.687f, 0.687f, 0.687f);
            }
            foreach (TMP_Text btn in PackagesBtns) {
                btn.color = new Color(0, 0, 0);
            }
            foreach (TMP_Text btn in ProjectsBtns) {
                btn.color = new Color(0.687f, 0.687f, 0.687f);
            }
            ProjectsList.gameObject.SetActive(false);
            ScenesList.gameObject.SetActive(false);
            PackageList.gameObject.SetActive(true);
            AddNewBtn.gameObject.SetActive(false);
            FilterLists();
            SortCurrentList();
        } catch (TimeoutException ex) {
            Notifications.Instance.ShowNotification("Failed to switch to packages", ex.Message);
        } finally {
            GameManager.Instance.HideLoadingScreen(true);
        }
    }

    public void LoadScenes(ListScenesResponse response) {
        if (response == null || !response.Result) {
            Notifications.Instance.ShowNotification("Failed to load scenes", "Please, try again later.");
            scenesUpdating = false;
            return;
        }
        GameManager.Instance.Scenes = response.Data;
        GameManager.Instance.Scenes.Sort(delegate (ListScenesResponseData x, ListScenesResponseData y) {
            return y.Modified.CompareTo(x.Modified);
        });
        scenesUpdating = false;
        scenesLoaded = true;
        GameManager.Instance.InvokeScenesListChanged();
    }

    public void LoadProjects(ListProjectsResponse response) {
        if (response == null) {
            Notifications.Instance.ShowNotification("Failed to load projects", "Please, try again later.");
        }
        GameManager.Instance.Projects = response.Data;
        GameManager.Instance.Projects.Sort(delegate (ListProjectsResponseData x, ListProjectsResponseData y) {
            return y.Modified.CompareTo(x.Modified);
        });
        projectsUpdating = false;
        projectsLoaded = true;
        GameManager.Instance.InvokeProjectsListChanged();
    }
    public void LoadPackages(ListPackagesResponse response) {
        if (response == null)
            Notifications.Instance.ShowNotification("Failed to load packages", "Please, try again later.");
        GameManager.Instance.Packages = response.Data;
        GameManager.Instance.Packages.Sort(delegate (PackageSummary x, PackageSummary y) {
            return y.PackageMeta.Built.CompareTo(x.PackageMeta.Built);
        });
        packagesUpdating = false;
        packagesLoaded = true;
        GameManager.Instance.InvokePackagesListChanged();
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
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        foreach (ListScenesResponseData scene in GameManager.Instance.Scenes) {
            SceneTile tile = Instantiate(SceneTilePrefab, ScenesDynamicContent.transform).GetComponent<SceneTile>();
            bool starred = PlayerPrefsHelper.LoadBool("scene/" + scene.Id + "/starred", false);
            if (scene.Problems == null) {
                tile.InitTile(scene.Name,
                              () => GameManager.Instance.OpenScene(scene.Id),
                              () => SceneOptionMenu.Open(tile),
                              starred,
                              scene.Created,
                              scene.Modified,
                              scene.Id);
            } else {
                tile.InitInvalidScene(scene.Name, starred, scene.Created, scene.Modified, scene.Id, scene.Problems.FirstOrDefault());
            }
            SceneTiles.Add(tile);
        }
        SortCurrentList();
        GameManager.Instance.HideLoadingScreen();
    }

    public async void NewScene(string name) {
        if (await GameManager.Instance.NewScene(name)) {
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
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        foreach (PackageSummary package in GameManager.Instance.Packages) {
            PackageTile tile = Instantiate(PackageTilePrefab, PackagesDynamicContent.transform).GetComponent<PackageTile>();
            bool starred = PlayerPrefsHelper.LoadBool("package/" + package.Id + "/starred", false);
            string projectName;
            if (package.ProjectMeta == null || package.ProjectMeta.Name == null)
                projectName = "unknown";
            else
                projectName = package.ProjectMeta.Name;
            tile.InitTile(package.PackageMeta.Name,
                          async () => await GameManager.Instance.RunPackage(package.Id),
                          () => PackageOptionMenu.Open(tile),
                          starred,
                          package.PackageMeta.Built,
                          package.PackageMeta.Executed,
                          package.Id,
                          projectName,
                          package.PackageMeta.Built.ToString());
            PackageTiles.Add(tile);
        }
        SortCurrentList();
        GameManager.Instance.HideLoadingScreen();
    }

    public void UpdateProjects(object sender, EventArgs eventArgs) {
        ProjectTiles.Clear();
        foreach (Transform t in ProjectsDynamicContent.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        foreach (ListProjectsResponseData project in GameManager.Instance.Projects) {
            ProjectTile tile = Instantiate(ProjectTilePrefab, ProjectsDynamicContent.transform).GetComponent<ProjectTile>();
            bool starred = PlayerPrefsHelper.LoadBool("project/" + project.Id + "/starred", false);
            if (project.Problems == null) {
                try {
                    string sceneName = GameManager.Instance.GetSceneName(project.SceneId);
                    tile.InitTile(project.Name,
                                  () => GameManager.Instance.OpenProject(project.Id),
                                  () => ProjectOptionMenu.Open(tile),
                                  starred,
                                  project.Created,
                                  project.Modified,
                                  project.Id,
                                  project.SceneId,
                                  sceneName);
                } catch (ItemNotFoundException ex) {
                    Debug.LogError(ex);
                    tile.InitInvalidProject(project.Id, project.Name, project.Created, project.Modified, starred, "Scene not found");
                }
            } else {
                string sceneName = "unknown";
                try {
                    sceneName = GameManager.Instance.GetSceneName(project.SceneId);
                } catch (ItemNotFoundException) { }
                tile.InitInvalidProject(project.Id, project.Name, project.Created, project.Modified, starred, project.Problems.FirstOrDefault(), sceneName);
            }
            ProjectTiles.Add(tile);
        }
        SortCurrentList();
        GameManager.Instance.HideLoadingScreen();
        // Button button = Instantiate(TileNewPrefab, ProjectsDynamicContent.transform).GetComponent<Button>();
        // TODO new scene
        // button.onClick.AddListener(() => NewProjectDialog.Open());
    }

    public void NotImplemented() {
        Notifications.Instance.ShowNotification("Not implemented", "Not implemented");
    }

    public void SaveLogs() {
        Notifications.Instance.SaveLogs();
    }

    public bool IsActive() {
        return CanvasGroup.alpha == 1 && CanvasGroup.blocksRaycasts == true;
    }
    public bool IsInactive() {
        return CanvasGroup.alpha == 0 && CanvasGroup.blocksRaycasts == false;
    }

    public SceneTile GetSceneTile(string sceneName) {
        foreach (SceneTile sceneTile in Instance.SceneTiles) {
            if (sceneTile.GetLabel() == sceneName) {
                return sceneTile;
            }
        }
        throw new ItemNotFoundException("Scene tile not found");
    }

    public ProjectTile GetProjectTile(string projectName) {
        foreach (ProjectTile projectTile in ProjectTiles) {
            if (projectTile.GetLabel() == projectName) {
                return projectTile;
            }
        }
        throw new ItemNotFoundException("Project tile not found");
    }

    public void DuplicateScene() {

    }


}
