using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;
using System.Linq;
using static Base.GameManager;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;
using TriLibCore.Extensions;
using RuntimeInspectorNamespace;

[RequireComponent(typeof(CanvasGroup))]
public class SelectorMenu : Singleton<SelectorMenu> {
    public GameObject SelectorItemPrefab;

    public CanvasGroup CanvasGroup;
    public GameObject ContentAim, ContentAlphabet, ContentNoPose, ContainerAlphabet, ContainerAim;
    private List<SelectorItem> selectorItemsAimMenu = new List<SelectorItem>();
    private List<SelectorItem> selectorItemsNoPoseMenu = new List<SelectorItem>();
    public event AREditorEventArgs.InteractiveObjectEventHandler OnObjectSelectedChangedEvent;

    public Dictionary<string, SelectorItem> SelectorItems = new Dictionary<string, SelectorItem>();


    public bool ManuallySelected {
        get;
        private set;
    }

    private long iteration = 0;

    private EditorStateEnum editorState = EditorStateEnum.Normal;

    private bool requestingObject = false;



    public ToggleIconButton RobotsToggle, ObjectsToggle, PointsToggle, ActionsToggle, IOToggle, OthersToggle;
    private SelectorItem lastSelectedItem = null;

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
        await ShowRobots(RobotsToggle.Toggled);
        ShowActionObjects(ObjectsToggle.Toggled);
        ShowActionPoints(PointsToggle.Toggled);
        ShowActions(ActionsToggle.Toggled);
        ShowIO(IOToggle.Toggled);
        ShowOthers(OthersToggle.Toggled);
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
            case EditorStateEnum.SelectingActionPointParent:
                DeselectObject(true);
                requestingObject = true;
                break;
        }
    }

    private void OnCloseProjectScene(object sender, System.EventArgs e) {
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
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            MenuManager.Instance.IsAnyMenuOpened) {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
        }
        
    }
    /*
    public void ForceUpdateMenus() {
        UpdateAlphabetMenu();
        if (ContentNoPose.activeSelf)
            UpdateNoPoseMenu();
    }*/

    private void OnProjectChanged(object sender, System.EventArgs e) {
        //UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        //UpdateAlphabetMenu();
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

    private void PrintItems() {
        Debug.Log(selectorItemsAimMenu.Count);
        foreach (SelectorItem item in selectorItemsAimMenu) {
            Debug.LogError(item.InteractiveObject.GetName() + ": " + item.Score + " (" + item.InteractiveObject.GetId() + ")");
        }
    }

    public void UpdateAimMenu(List<Tuple<float, InteractiveObject>> items) {
        
        if (ContainerAim.activeSelf) {
            int maxItems = 8;
            foreach (Tuple<float, InteractiveObject> item in items) {
                if (item.Item2.GetType() == typeof(ActionObjectNoPose))
                    continue;
                if (true) { //selectorItemsAimMenu.Count < 6 || item.Item1 <= selectorItemsAimMenu.Last().Score) {
                    if (!SelectorItems.ContainsKey(item.Item2.GetId())) {
                        continue;
                    }
                    SelectorItem selectorItem = GetSelectorItemInAimMenu(item.Item2);
                    if (selectorItem == null) {
                        if (selectorItemsAimMenu.Count > maxItems)
                            break;
                        selectorItem = SelectorItems[item.Item2.GetId()];
                        AddItemToAimingList(selectorItem);
                    } else {
                        if (IsRootItem(selectorItem) && selectorItem.transform.parent != ContentAim.transform) {
                            selectorItem.transform.SetParent(ContentAim.transform);
                        }
                    }
                    selectorItem.UpdateScore(item.Item1, iteration);
                    
                }

            }

            ++iteration;
            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            for (int i = selectorItemsAimMenu.Count - 1; i >= 0; --i) {
                //Debug.LogError(selectorItemsAimMenu[i].InteractiveObject.GetName() + ", visibleSubItem: " + selectorItemsAimMenu[i].AnyVisibleSubitem() + ", lastUpdate: " + (iteration - selectorItemsAimMenu[i].GetLastUpdate()).ToString() + ", selectorItemsAimMenu.Count: " + selectorItemsAimMenu.Count);

                if (selectorItemsAimMenu[i].IsSelected() && ManuallySelected) {
                    continue;
                }
                if (!selectorItemsAimMenu[i].AnyVisibleSubitem() &&
                    ((iteration - selectorItemsAimMenu[i].GetLastUpdate()) > 5 ||
                    selectorItemsAimMenu.Count > maxItems)) {
                    RemoveFromAimingList(selectorItemsAimMenu[i]);
                }
            }


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
                SelectedObjectChanged(null);
            }
        }


    }


    public void RemoveFromAimingList(SelectorItem selectorItem) {
        if (IsRootItem(selectorItem)) {
            selectorItem.HideAllSubitems(false);
            selectorItem.transform.SetParent(ContentAlphabet.transform);
        } else
            selectorItem.HideAllSubitems(true);
        selectorItemsAimMenu.Remove(selectorItem);
    }

    public void AddItemToAimingList(SelectorItem selectorItem) {
        if (selectorItemsAimMenu.Contains(selectorItem)) {
            selectorItem.SetCollapsedState(false);
            return;
        }
        if (IsRootItem(selectorItem)) {
            selectorItem.transform.SetParent(ContentAim.transform);            
        } else {
            selectorItem.gameObject.SetActive(selectorItem.InteractiveObject.Enabled);
            if (!selectorItem.gameObject.activeSelf)
                return;
            InteractiveObject parent = ((ISubItem) selectorItem.InteractiveObject).GetParentObject();
            if (SelectorItems.TryGetValue(parent.GetId(), out SelectorItem parentItem)) {
                parentItem.SetCollapsedState(false);
                AddItemToAimingList(parentItem);
            }           
            
        }
        selectorItemsAimMenu.Add(selectorItem);
    }

    private bool IsRootItem(SelectorItem selectorItem) {
        return (!(selectorItem.InteractiveObject is ISubItem)) || (selectorItem.InteractiveObject is ISubItem subItem && subItem.GetParentObject() == null);
    }

    private void RemoveItem(int index, List<SelectorItem> selectorItems) {
        if (selectorItems[index].IsSelected()) {
            ManuallySelected = false;
            selectorItems[index].SetSelected(false, true);
        }
        Destroy(selectorItems[index].gameObject);
        selectorItems.RemoveAt(index);
    }

    public void SetSelectedObject(InteractiveObject interactiveObject, bool manually = false) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem item)) {
            SetSelectedObject(item, manually);            
        }
    }

    public void SetSelectedObject(SelectorItem selectorItem, bool manually = false) {
        if (manually && requestingObject) {
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
            if (selectorItem != lastSelectedItem) {
                DeselectObject(manually);
                selectorItem.SetSelected(true, manually);
                SelectedObjectChanged(selectorItem);
            }            
            if (manually)
                ManuallySelected = true;
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
        if (lastSelectedItem != null)
            lastSelectedItem.SetSelected(false, manually);
        /*foreach (SelectorItem item in selectorItems.Values.ToList()) {
            item.SetSelected(false, manually);
        }*/
    }

    public void CreateSelectorItem(InteractiveObject interactiveObject) {
        if (string.IsNullOrEmpty(interactiveObject.GetId()))
            return;
        if (SelectorItems.ContainsKey(interactiveObject.GetId()))
            return;
        SelectorItem selectorItem = Instantiate(SelectorItemPrefab).GetComponentInChildren<SelectorItem>();
        selectorItem.SublistContent.SetActive(false);
        if (interactiveObject is ISubItem subItem) {
            if (SelectorItems.TryGetValue(subItem.GetParentObject().GetId(), out SelectorItem selectorItemParent)) {
                selectorItem.transform.SetParent(selectorItemParent.SublistContent.transform);
                selectorItemParent.Childs.Add(selectorItem);
            } else {
                throw new RequestFailedException("Trying to create subitem without parent item in list. This should not had happened, please report");
            }
        } else {
            selectorItem.transform.SetParent(ContentAlphabet.transform);
        }
        //selectorItem.gameObject.SetActive(false);
        selectorItem.SetText(interactiveObject.GetName());
        selectorItem.SetObject(interactiveObject, 0, iteration);
        SelectorItems.Add(interactiveObject.GetId(), selectorItem);
    }

    public void EnableItem(InteractiveObject interactiveObject, bool enable) {
        if (SelectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem selectorItem)) {
            selectorItem.gameObject.SetActive(enable);
        }
    }

    

    public void DestroySelectorItem(string id) {
        if (SelectorItems.TryGetValue(id, out SelectorItem selectorItem)) {
            TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsAimMenu);
            TryRemoveFromList(selectorItem.InteractiveObject, selectorItemsNoPoseMenu);
            if (selectorItem.IsSelected()) {
                DeselectObject(true);
            }
            if (selectorItem.InteractiveObject is ISubItem subItem) {
                InteractiveObject parentObject = subItem.GetParentObject();
                if (parentObject != null)
                    if (SelectorItems.TryGetValue(parentObject.GetId(), out SelectorItem parentSelectorItem)) {
                        parentSelectorItem.Childs.Remove(selectorItem);
                    }
            }
            Destroy(selectorItem.transform.gameObject);
            SelectorItems.Remove(id);
        }
    }

    public void DestroySelectorItem(InteractiveObject interactiveObject) {
        DestroySelectorItem(interactiveObject.GetId());
    }

    private void TryRemoveFromList(InteractiveObject io, List<SelectorItem> selectorItems) {
        for (int i = 0; i < selectorItems.Count; ++i) {
            if (string.Compare(selectorItems[i].InteractiveObject.GetId(), io.GetId()) == 0) {
                selectorItems.RemoveAt(i);
                return;
            }
        }
    }

    /*
    public void UpdateAlphabetMenu() {
        //ClearMenu(selectorItemsAlphabetMenu);
        List<string> idsToRemove = selectorItems.Keys.ToList();
        foreach (InteractiveObject io in GameManager.Instance.GetAllInteractiveObjects()) {
            if (!io.Enabled)
                continue;
            if (selectorItems.TryGetValue(io.GetId(), out SelectorItem item)) {
                item.transform.SetParent(ContentAlphabet.transform);
                item.transform.SetAsLastSibling();
                item.SetText(io.GetName());
                idsToRemove.Remove(io.GetId());
                if (item.InteractiveObject == null)
                    item.InteractiveObject = io;
            } else {
                SelectorItem newItem = CreateSelectorItem(io, ContentAlphabet.transform, 0);
                selectorItems.Add(io.GetId(), newItem);
            }            
        }
        foreach (string id in idsToRemove) {
            if (selectorItems.TryGetValue(id, out SelectorItem item)) {
                if (item.IsSelected()) {
                    item.SetSelected(false, true);
                    if (manuallySelected) {
                        manuallySelected = false;
                    }
                }                
                Destroy(item.gameObject);
                selectorItems.Remove(id);
                RemoveItemWithId(id, selectorItemsAimMenu);
            }
           
        }
        // force update of left menu icons
        SelectedObjectChanged(lastSelectedItem, true);
        
    }*/


    public void UpdateNoPoseMenu() {
        if (!ContentNoPose.activeSelf || !GameManager.Instance.Scene.activeSelf)
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
        ContentAim.SetActive(true);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(false);
        foreach (SelectorItem item in SelectorItems.Values) {
            if (!IsRootItem(item)) {
                item.gameObject.SetActive(false);
            }
        }
        if (ManuallySelected) {
            InteractiveObject selectedItem = GetSelectedObject();
            foreach (SelectorItem item in selectorItemsAimMenu) {
                if (item.InteractiveObject.GetId() == selectedItem.GetId()) {
                    
                    return;
                }
            }
            ManuallySelected = false;
            DeselectObject(true);
        }
        
        
    }

    public void SwitchToNoPose() {        
        ContentAim.SetActive(false);
        ContentNoPose.SetActive(true);
        ContainerAlphabet.SetActive(false);
        UpdateNoPoseMenu();
    }

    public void SwitchToAlphabet() {
        /*ContentAim.SetActive(false);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(true);        
        UpdateAlphabetMenu();*/
        foreach (SelectorItem item in SelectorItems.Values.OrderBy(item => item.InteractiveObject.GetName())) {
            if (IsRootItem(item)) {
                item.transform.SetParent(ContentAlphabet.transform);
            } else {
                item.gameObject.SetActive(item.InteractiveObject.Enabled);
            }
            item.SetCollapsedState(true);
        }
        selectorItemsAimMenu.Clear();
        selectorItemsNoPoseMenu.Clear();
        PointsToggle.Button.interactable = true;
        ContainerAim.SetActive(false);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(true);
    }

    public InteractiveObject GetSelectedObject() {
        /*
        foreach (SelectorItem item in selectorItems.Values.ToList()) {
            if (item.IsSelected())
                return item.InteractiveObject;
        }
        return null;*/
        return lastSelectedItem.InteractiveObject;
    }

    public async Task ShowRobots(bool show) {
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

    public void ShowIO(bool show) {
        ProjectManager.Instance.EnableAllActionInputs(show);
        ProjectManager.Instance.EnableAllActionOutputs(show);
    }

    public void ShowActions(bool show) {
        ProjectManager.Instance.EnableAllActions(show);
    }

    public void ShowOthers(bool show) {
        GameManager.Instance.EnableServiceInteractiveObjects(show);
    }


}
