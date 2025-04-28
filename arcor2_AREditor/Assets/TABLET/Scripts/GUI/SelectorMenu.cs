using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Base;
using UnityEngine;
using static Base.GameManager;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(CanvasGroup))]
public class SelectorMenu : Singleton<SelectorMenu> {
    private const int MaxItems = 6;
    public GameObject SelectorItemPrefab;

    public CanvasGroup CanvasGroup;
    public GameObject ContentAim, ContentAlphabet, ContentNoPose, ContentBlocklisted, ContainerAlphabet, ContainerAim, ContainerNoPose, ContainerBlocklisted;
    private List<SelectorItem> selectorItemsAimMenu = new();
    private List<SelectorItem> selectorItemsNoPoseMenu = new();
    public event EventHandler<InteractiveObjectEventArgs> OnObjectSelectedChangedEvent;
    public ToggleGroupIconButtons BottomButtons;

    public Dictionary<string, SelectorItem> SelectorItems = new();


    public bool ManuallySelected {
        get;
        private set;
    }

    private long iteration = 0;

    private EditorStateEnum editorState = EditorStateEnum.Normal;

    private bool requestingObject = false;



    public ToggleIconButton RobotsToggle, ObjectsToggle, PointsToggle, ActionsToggle, IOToggle, OthersToggle;
    public SelectorItem lastSelectedItem = null;

    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseProjectScene;
        GameManager.Instance.OnCloseScene += OnCloseProjectScene;
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
        GameManager.Instance.OnOpenSceneEditor += OnSceneChanged;
        ProjectManager.Instance.OnProjectChanged += OnProjectChanged;
        GameManager.Instance.OnOpenProjectEditor += OnProjectChanged;
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        ProjectManager.Instance.OnLoadProject += OnLoadProjectScene;
        SceneManager.Instance.OnLoadScene += OnLoadProjectScene;
        GameManager.Instance.OnRunPackage += OnLoadProjectScene;
    }

    private void OnLoadProjectScene(object sender, EventArgs e) {
        _ = UpdateFilters();
    }

    public async Task UpdateFilters() {
        await ShowRobotsAsync(RobotsToggle.Toggled);
        ShowActionObjects(ObjectsToggle.Toggled);
        ShowActionPoints(PointsToggle.Toggled);
        ShowActions(ActionsToggle.Toggled);
    }

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        editorState = args.Data;
        switch (editorState) {
            case EditorStateEnum.Normal:
            case EditorStateEnum.Closed:
            case EditorStateEnum.InteractionDisabled:
                DeselectObject(true);
                requestingObject = false;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingEndEffector:
            case EditorStateEnum.SelectingAPOrientation:
            case EditorStateEnum.SelectingActionPointParent:
                DeselectObject(true);
                requestingObject = true;
                break;
        }
    }

    private void OnCloseProjectScene(object sender, EventArgs e) {
        foreach (SelectorItem selectorItem in SelectorItems.Values) {
            Destroy(selectorItem.gameObject);
        }
        selectorItemsAimMenu.Clear();
        SelectorItems.Clear();
        selectorItemsNoPoseMenu.Clear();
        ManuallySelected = false;
    }

    private class SelectorItemComparer : IComparer<SelectorItem> {
        public int Compare(SelectorItem x, SelectorItem y) {
            return x.Score.CompareTo(y.Score);
        }
    }


    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
    }


    private void Update() {
        if (GameManager.Instance.GetGameState() == GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameStateEnum.Disconnected) {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
        }
        
    }

    private void OnProjectChanged(object sender, EventArgs e) {
        UpdateNoPoseMenu();
    }

    private void OnSceneChanged(object sender, EventArgs e) {
        UpdateNoPoseMenu();
    }

    private void SelectedObjectChanged(SelectorItem selectorItem, bool force = false) {
        if (force || selectorItem != lastSelectedItem) {
            OnObjectSelectedChangedEvent.Invoke(this, new InteractiveObjectEventArgs(selectorItem == null ? null : selectorItem.InteractiveObject));
            lastSelectedItem = selectorItem;
        }
    }

    private SelectorItem GetSelectorItemInAimMenu(InteractiveObject io) {
        foreach (SelectorItem item in selectorItemsAimMenu) {
            if (string.Compare(item.InteractiveObject.GetId(), io.GetId()) == 0) {
                return item;
            }
        }
        return null;
    }

    private void RemoveItemWithId(string id, List<SelectorItem> selectorItems) {
        for (int i = selectorItems.Count - 1; i >= 0; --i) {
            if (selectorItems[i].InteractiveObject.GetId() == id) {
                selectorItems.RemoveAt(i);
            }
        }
    }


    public void UpdateAimMenu(List<Tuple<float, InteractiveObject>> items) {
        if (ContainerAim.activeSelf) {

            // add all items to selector items aim menu list so they can be sorted and their score updated
            foreach (Tuple<float, InteractiveObject> item in items) {                
                if (item.Item2.GetType() == typeof(ActionObjectNoPose))
                    continue;
                if (!item.Item2.Enabled)
                    continue;
                if (!SelectorItems.ContainsKey(item.Item2.GetId())) {
                    continue;
                }

                SelectorItem selectorItem = selectorItemsAimMenu.Find(x => x.InteractiveObject.GetId() == item.Item2.GetId());
                if (selectorItem == null) {
                    selectorItem = item.Item2.SelectorItem;                    
                    selectorItemsAimMenu.Add(selectorItem);
                }                 
                selectorItem.UpdateScore(item.Item1, iteration);
            }            
            ++iteration;
            
            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            HashSet<string> newItems = new();
            int index = 0;
            bool selectedAdded = false;

            // find MaxItems to be inserted to the list
            // for each sub item find all ancestors and add them to list
            foreach (SelectorItem item in selectorItemsAimMenu) {
                if (iteration - selectorItemsAimMenu[index].GetLastUpdate() <= 5) {
                    newItems.Add(selectorItemsAimMenu[index].InteractiveObject.GetId());
                    newItems.UnionWith(GetAncestors(selectorItemsAimMenu[index].InteractiveObject));
                    if (ManuallySelected && selectorItemsAimMenu[index].IsSelected()) {
                        selectedAdded = true;                        
                    }
                }
                if (newItems.Count >= MaxItems) {
                    break;
                }
                ++index;
            }
            // add manually selected item if it is not there yet
            if (ManuallySelected) {
                if (!selectedAdded)
                    newItems.Add(GetSelectedObject().GetId());
                SelectorItem parent = GetSelectedObject().SelectorItem.ParentItem;
                while (parent != null) {
                    newItems.Add(parent.InteractiveObject.GetId());
                    parent = parent.ParentItem;
                }                
            }
            // remove all old items from list
            foreach (SelectorItem item in selectorItemsAimMenu) {
                if (!newItems.Contains(item.InteractiveObject.GetId()))
                    RemoveFromAimingList(item);
            }
            selectorItemsAimMenu.Clear();

            // add all new items to the list
            foreach (string io in newItems) {
                if (SelectorItems.TryGetValue(io, out SelectorItem selectorItem))
                    AddItemToAimingList(selectorItem);
            }

            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            
        }
        if (!ManuallySelected) {
            bool selected = false;
            if (items.Count > 0) {
                if (ContainerAim.activeSelf) {
                    if (selectorItemsAimMenu.Count > 0) {
                        SetSelectedObject(selectorItemsAimMenu.First(), false);
                        selected = true;
                    }
                } else if (ContentAlphabet.activeSelf && items.Count > 0) {

                    foreach (Tuple<float, InteractiveObject> item in items) {
                        if (item.Item2.Enabled) {
                            SetSelectedObject(item.Item2, false);
                            selected = true;
                            break;
                        }
                    }

                }
            }
            if (items.Count == 0 || !selected) {
                DeselectObject(false);
                SelectedObjectChanged(null);
            }
        }
    }

    private HashSet<string> GetAncestors(InteractiveObject interactiveObject) {
        HashSet<string> ancestors = new();
        if (interactiveObject is ISubItem subItem && subItem.GetParentObject() != null) {
            ancestors.Add(subItem.GetParentObject().GetId());
            ancestors.UnionWith(GetAncestors(subItem.GetParentObject()));
        }
        return ancestors;
    }
    
    public void RemoveFromAimingList(SelectorItem selectorItem) {
        if (selectorItem.InteractiveObject.Blocklisted)
            return;
        if (IsRootItem(selectorItem)) {
            selectorItem.transform.SetParent(ContentAlphabet.transform);
        } else {            
            selectorItem.gameObject.SetActive(false);
        }
    }

    public void AddItemToAimingList(SelectorItem selectorItem) {
        if (selectorItem.InteractiveObject.Blocklisted)
            return;
        selectorItem.SetCollapsedState(false);
        
        if (IsRootItem(selectorItem)) {
            if (selectorItem.transform.parent != ContentAim.transform)
                selectorItem.transform.SetParent(ContentAim.transform);
        } else {
            if (!selectorItem.gameObject.activeSelf)
                selectorItem.gameObject.SetActive(true);
            
        }
        selectorItemsAimMenu.Add(selectorItem);
    }

    private bool IsRootItem(SelectorItem selectorItem) {
        return !(selectorItem.InteractiveObject is ISubItem) || (selectorItem.InteractiveObject is ISubItem subItem && subItem.GetParentObject() == null);
    }

    private void RemoveItem(int index, List<SelectorItem> selectorItems) {
        if (selectorItems[index].IsSelected()) {
            ManuallySelected = false;
            selectorItems[index].SetSelected(false, true);
        }
        Destroy(selectorItems[index].gameObject);
        selectorItems.RemoveAt(index);
    }

    public void SetSelectedObject(InteractiveObject interactiveObject, bool manually = false, bool enableRequestingObject = true) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem item)) {
            SetSelectedObject(item, manually, enableRequestingObject);            
        }
    }

    public void SetSelectedObject(SelectorItem selectorItem, bool manually = false, bool enableRequestingObject = true) {
        if (manually && requestingObject && enableRequestingObject) {
            GameManager.Instance.ObjectSelected(selectorItem.InteractiveObject);
        } else {
            if (manually) {
                if (selectorItem.IsSelected() && ManuallySelected) {
                    selectorItem.SetSelected(false, manually);
                    ManuallySelected = false;
                    SelectedObjectChanged(null);
                    return;
                }
            }
            if (!selectorItem.IsSelected()) {
                DeselectObject(manually);                
            }
            if (manually) {
                ManuallySelected = true;
            }
            selectorItem.SetSelected(true, manually);

            SelectedObjectChanged(selectorItem, ManuallySelected);
        }        
    }

    public void UpdateSelectorItem(InteractiveObject interactiveObject) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem selectorItem)) {
            selectorItem.Label.text = interactiveObject.GetName();
        }
    }

    public void DeselectObject(bool manually = true) {
        if (manually)
            ManuallySelected = false;
        if (lastSelectedItem != null) {
            lastSelectedItem.SetSelected(false, manually);
            SelectedObjectChanged(null);
        }
    }

    public SelectorItem CreateSelectorItem(InteractiveObject interactiveObject) {
        Debug.Assert(!string.IsNullOrEmpty(interactiveObject.GetId()));
        Debug.Assert(!SelectorItems.ContainsKey(interactiveObject.GetId()));
        
        SelectorItem selectorItem = Instantiate(SelectorItemPrefab).GetComponentInChildren<SelectorItem>();
        selectorItem.SublistContent.SetActive(false);
        if (interactiveObject is ISubItem subItem) {
            InteractiveObject parent = subItem.GetParentObject();
            if (parent != null && parent.SelectorItem != null) {
                selectorItem.transform.SetParent(parent.SelectorItem.SublistContent.transform);
                parent.SelectorItem.AddChild(selectorItem);
            } else {
                throw new Arcor2ConnectionException("Trying to create subitem without parent item in list. This should not had happened, please report");
            }
        } else {
            selectorItem.transform.SetParent(ContentAlphabet.transform);
        }
        selectorItem.CollapsableButton.interactable = false;
        selectorItem.SetText(interactiveObject.GetName());
        selectorItem.SetObject(interactiveObject, 0, iteration);
        SelectorItems.Add(interactiveObject.GetId(), selectorItem);
        if (ContainerAlphabet.activeSelf && ((GameManager.Instance.GetGameState() == GameStateEnum.SceneEditor && SceneManager.Instance.Valid) ||
                                           (GameManager.Instance.GetGameState() == GameStateEnum.ProjectEditor && ProjectManager.Instance.Valid))) // only sort when project / scene is fully loaded
            SwitchToAlphabet(false);

        selectorItem.transform.localScale = Vector3.one;
        selectorItem.CollapsableButton.gameObject.SetActive(!ContainerAim.activeSelf);
        return selectorItem;
    }

    public void EnableItem(InteractiveObject interactiveObject, bool enable) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem selectorItem)) {
            selectorItem.gameObject.SetActive(enable);
        }
    }

    

    public void DestroySelectorItem(string id) {
        if (SelectorItems.TryGetValue(id, out SelectorItem selectorItem)) {
            DestroySelectorItem(selectorItem);
        }
    }

    public void DestroySelectorItem(InteractiveObject interactiveObject) {
        DestroySelectorItem(interactiveObject.GetId());
    }

    public void DestroySelectorItem(SelectorItem selectorItem) {
        TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsAimMenu);
        TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsNoPoseMenu);
        if (selectorItem.IsSelected()) {
            DeselectObject(true);
        }
        if (selectorItem.InteractiveObject is ISubItem subItem) {
            InteractiveObject parentObject = subItem.GetParentObject();
            if (parentObject != null)
                if (SelectorItems.TryGetValue(parentObject.GetId(), out SelectorItem parentSelectorItem)) {
                    parentSelectorItem.RemoveChild(selectorItem);
                }
        }
        if (selectorItem != null && selectorItem.gameObject != null)
            Destroy(selectorItem.gameObject);
        SelectorItems.Remove(selectorItem.InteractiveObject.GetId());
    }

    public void Clear() {
        foreach (SelectorItem item in SelectorItems.Values) {
            DestroySelectorItem(item.InteractiveObject.GetId());
        }
        SelectorItems.Clear();
        selectorItemsAimMenu.Clear();
        selectorItemsNoPoseMenu.Clear();
        foreach (Transform t in ContentAim.transform) {
            if (t != ContentAim.transform)
                Destroy(t.gameObject);
        }
        foreach (Transform t in ContentAlphabet.transform) {
            if (t != ContentAim.transform)
                Destroy(t.gameObject);
        }
        foreach (Transform t in ContentNoPose.transform) {
            if (t != ContentAim.transform)
                Destroy(t.gameObject);
        }
    }

    private void TryRemoveFromList(InteractiveObject io, List<SelectorItem> selectorItems) {
        for (int i = 0; i < selectorItems.Count; ++i) {
            if (string.Compare(selectorItems[i].InteractiveObject.GetId(), io.GetId()) == 0) {
                selectorItems.RemoveAt(i);
                return;
            }
        }
    }

   public void UpdateNoPoseMenu() {
        if (!ContainerNoPose.activeSelf || !GameManager.Instance.Scene.activeSelf)
            return;
        selectorItemsNoPoseMenu.Clear();
        foreach (ActionObject actionObject in SceneManager.Instance.GetAllActionObjectsWithoutPose()) {
            if (!actionObject.Enabled)
                continue;
            SelectorItem newItem = SelectorItems[actionObject.GetId()];
            selectorItemsNoPoseMenu.Add(newItem);
            newItem.transform.SetParent(ContentNoPose.transform);
        }
    }

    private void ClearMenu(List<SelectorItem> selectorItems) {
        foreach (SelectorItem item in selectorItems) {
            Destroy(item.gameObject);
        }
        selectorItems.Clear();
    }

    public void SwitchToAim() {
        ContainerAim.SetActive(true);
        ContainerNoPose.SetActive(false);
        ContainerAlphabet.SetActive(false);
        ContainerBlocklisted.SetActive(false);
        foreach (SelectorItem item in SelectorItems.Values) {
            if (!IsRootItem(item)) {
                item.gameObject.SetActive(false);
            }
            item.CollapsableButton.gameObject.SetActive(false);
        }
    }

    public void SwitchToNoPose() {
        ContainerAim.SetActive(false);
        ContainerNoPose.SetActive(true);
        ContainerAlphabet.SetActive(false);
        ContainerBlocklisted.SetActive(false);
        UpdateNoPoseMenu();
    }

    public void SwitchToBlocklisted() {
        ContainerAim.SetActive(false);
        ContainerNoPose.SetActive(false);
        ContainerAlphabet.SetActive(false);
        ContainerBlocklisted.SetActive(true);
        UpdateNoPoseMenu();
    }

    public void SwitchToAlphabet() {
        SwitchToAlphabet(true);
    }

    public void SwitchToAlphabet(bool updateCollapsedState) {
        foreach (SelectorItem item in SelectorItems.Values.OrderBy(item => item.InteractiveObject.GetName())) {
            if (item.InteractiveObject.Blocklisted)
                continue;
            if (IsRootItem(item)) {
                if (item.transform.parent != ContentAlphabet.transform) {
                    item.transform.SetParent(ContentAlphabet.transform);
                }
                item.CollapsableButton.gameObject.SetActive(item.Collapsable);
                item.transform.SetAsLastSibling();
            } else {
                item.gameObject.SetActive(item.InteractiveObject.Enabled);
                item.transform.SetAsLastSibling();
            }
            if (updateCollapsedState)
                item.SetCollapsedState(true);
        }
        selectorItemsAimMenu.Clear();
        selectorItemsNoPoseMenu.Clear();
        ContainerAim.SetActive(false);
        ContainerNoPose.SetActive(false);
        ContainerAlphabet.SetActive(true);
        ContainerBlocklisted.SetActive(false);
    }

    public void PutOnBlocklist(SelectorItem selectorItem) {
        selectorItem.transform.SetParent(ContentBlocklisted.transform);
    }

    public void RemoveFromBlacklist(SelectorItem selectorItem) {
        selectorItem.transform.SetParent(ContentAlphabet.transform);
    }

    public InteractiveObject GetSelectedObject() {
        return lastSelectedItem?.InteractiveObject;
    }

    public async void ShowRobots(bool show) {
        if (SceneManager.Instance.SceneStarted)
            await ProjectManager.Instance.EnableAllRobotsEE(show);
        SceneManager.Instance.EnableAllRobots(show);
    }

    public async Task ShowRobotsAsync(bool show) {
        if (SceneManager.Instance.SceneStarted)
            await ProjectManager.Instance.EnableAllRobotsEE(show);
        SceneManager.Instance.EnableAllRobots(show);
    }

    public void ShowActionObjects(bool show) {
        SceneManager.Instance.EnableAllActionObjects(show, false);
    }

    public void ShowActionPoints(bool show) {
        ProjectManager.Instance.EnableAllActionPoints(show);
        ProjectManager.Instance.EnableAllOrientations(show);
    }

    
    public void ShowActions(bool show) {
        ProjectManager.Instance.EnableAllActions(show);
    }

    public void ShowOthers(bool show) {
        GameManager.Instance.EnableServiceInteractiveObjects(show);
    }


}
