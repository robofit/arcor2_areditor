using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class SelectorMenu : Singleton<SelectorMenu> {
    public GameObject SelectorItemPrefab;

    public CanvasGroup CanvasGroup;
    public GameObject ContentAim, ContentAlphabet, ContentNoPose, ContainerAlphabet;
    private List<SelectorItem> selectorItemsAimMenu = new List<SelectorItem>();
    private List<SelectorItem> selectorItemsNoPoseMenu = new List<SelectorItem>();

    private Dictionary<string, SelectorItem> selectorItems = new Dictionary<string, SelectorItem>();

    private bool manuallySelected;

    private long iteration = 0;


    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseProjectScene;
        GameManager.Instance.OnCloseScene += OnCloseProjectScene;
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
        GameManager.Instance.OnOpenSceneEditor += OnSceneChanged;
        ProjectManager.Instance.OnProjectChanged += OnProjectChanged;
        GameManager.Instance.OnOpenProjectEditor += OnProjectChanged;
    }



    private void OnCloseProjectScene(object sender, System.EventArgs e) {
        foreach (SelectorItem selectorItem in selectorItems.Values) {
            Destroy(selectorItem.gameObject);
        }
        selectorItemsAimMenu.Clear();
        selectorItems.Clear();
        selectorItemsNoPoseMenu.Clear();
        manuallySelected = false;
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

    public void ForceUpdateMenus() {
        UpdateAlphabetMenu();
        if (ContentNoPose.activeSelf)
            UpdateNoPoseMenu();
    }

    private void OnProjectChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private SelectorItem GetSelectorItem(InteractiveObject io) {
        foreach (SelectorItem item in selectorItemsAimMenu) {
            if (string.Compare(item.InteractiveObject.GetId(), io.GetId()) == 0) {
                //Debug.LogError("has");
                return item;
            }
        }
        return null;
    }

    private void PrintItems() {
        Debug.Log(selectorItemsAimMenu.Count);
        foreach (SelectorItem item in selectorItemsAimMenu) {
            Debug.LogError(item.InteractiveObject.GetName() + ": " + item.Score + " (" + item.InteractiveObject.GetId() + ")");
        }
    }

    public void UpdateAimMenu(Vector3? aimingPoint) {
        List<Tuple<float, InteractiveObject>> items = new List<Tuple<float, InteractiveObject>>();
        if (aimingPoint.HasValue) {
            foreach (SelectorItem item in selectorItems.Values) {
                float dist = item.InteractiveObject.GetDistance(aimingPoint.Value);
                items.Add(new Tuple<float, InteractiveObject>(dist, item.InteractiveObject));
            }
            items.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }
        if (ContentAim.activeSelf) {
            int count = 0;
            for (int i = selectorItemsAimMenu.Count - 1; i >= 0; --i) {
                if ((iteration - selectorItemsAimMenu[i].GetLastUpdate()) > 5) {
                    selectorItemsAimMenu[i].transform.SetParent(ContentAlphabet.transform);
                    selectorItemsAimMenu.RemoveAt(i);
                }
            }
            List<SelectorItem> newItems = new List<SelectorItem>();
            foreach (Tuple<float, InteractiveObject> item in items) {
                if (selectorItemsAimMenu.Count < 6 || item.Item1 <= selectorItemsAimMenu.Last().Score) {
                    SelectorItem selectorItem = GetSelectorItem(item.Item2);
                    if (selectorItem == null) {
                        selectorItem = selectorItems[item.Item2.GetId()];
                        selectorItemsAimMenu.Add(selectorItem);
                        selectorItem.transform.SetParent(ContentAim.transform);
                        newItems.Add(selectorItem);
                    } else {
                        if (selectorItem.transform.parent != ContentAim.transform)
                          selectorItem.transform.SetParent(ContentAim.transform);
                    }
                    selectorItem.UpdateScore(item.Item1, iteration);
                }
                if (count++ > 7)
                    break;
            }
            selectorItemsAimMenu.Sort(new SelectorItemComparer());
            while (selectorItemsAimMenu.Count > 6) {
                if (selectorItemsAimMenu.Last().IsSelected()) {
                    SelectorItem item = selectorItemsAimMenu.Last();
                    selectorItemsAimMenu.RemoveAt(selectorItemsAimMenu.Count - 1);
                    selectorItemsAimMenu.Insert(selectorItemsAimMenu.Count - 2, item);
                }
                    
                selectorItemsAimMenu.Last().transform.SetParent(ContentAlphabet.transform);
                selectorItemsAimMenu.RemoveAt(selectorItemsAimMenu.Count - 1);
            }
        }
        if (!manuallySelected) {
            if (ContentAim.activeSelf) {
                if (selectorItemsAimMenu.Count > 0)
                    SetSelectedObject(selectorItemsAimMenu.First(), false);
            } else if (ContentAlphabet.activeSelf && items.Count > 0) {
                SetSelectedObject(items.First().Item2, false);
            }
            if (items.Count == 0) {
                DeselectObject(false);
            }
        }

        ++iteration;
    }


    private void RemoveItem(int index, List<SelectorItem> selectorItems) {
        if (selectorItems[index].IsSelected()) {
            manuallySelected = false;
            selectorItems[index].SetSelected(false, true);
        }
        Destroy(selectorItems[index].gameObject);
        selectorItems.RemoveAt(index);
    }

    public void SetSelectedObject(InteractiveObject interactiveObject, bool manually = false) {
        if (selectorItems.TryGetValue(interactiveObject.GetId(), out SelectorItem item)) {
            SetSelectedObject(item, manually);            
        }
    }

    public void SetSelectedObject(SelectorItem selectorItem, bool manually = false) {
        if (manually) {
            if (selectorItem.IsSelected() && manuallySelected) {
                selectorItem.SetSelected(false, manually);
                manuallySelected = false;
                return;
            }
        }
        DeselectObject(manually);
        selectorItem.SetSelected(true, manually);
        if (manually)
            manuallySelected = true;
    }

    private void DeselectObject(bool manually = true) {
        foreach (SelectorItem item in selectorItems.Values.ToList()) {
            item.SetSelected(false, manually);
        }
    }

    public SelectorItem CreateSelectorItem(InteractiveObject interactiveObject, Transform parent, float score) {
        SelectorItem selectorItem = Instantiate(SelectorItemPrefab, parent).GetComponent<SelectorItem>();
        //selectorItem.gameObject.SetActive(false);
        selectorItem.SetText(interactiveObject.GetName());
        selectorItem.SetObject(interactiveObject, score, iteration);
        return selectorItem;
    }


    public void UpdateAlphabetMenu() {
        //ClearMenu(selectorItemsAlphabetMenu);
        List<string> idsToRemove = selectorItems.Keys.ToList();
        foreach (InteractiveObject io in GameManager.Instance.GetAllInteractiveObjects()) {
            if (selectorItems.TryGetValue(io.GetId(), out SelectorItem item)) {
                item.transform.SetParent(ContentAlphabet.transform);
                item.transform.SetAsLastSibling();
                idsToRemove.Remove(io.GetId());
            } else {
                SelectorItem newItem = CreateSelectorItem(io, ContentAlphabet.transform, 0);
                selectorItems.Add(io.GetId(), newItem);
            }            
        }
        foreach (string id in idsToRemove) {
            if (selectorItems.TryGetValue(id, out SelectorItem item)) {
                if (manuallySelected && item.IsSelected()) {
                    item.SetSelected(false, manuallySelected);
                    manuallySelected = false;
                }
                Destroy(item.gameObject);
                selectorItems.Remove(id);
            }
           
        }
    }

    public void UpdateNoPoseMenu() {
        if (!ContentNoPose.activeSelf || !GameManager.Instance.Scene.activeSelf)
            return;
        selectorItemsNoPoseMenu.Clear();
        foreach (ActionObject actionObject in SceneManager.Instance.GetAllActionObjectsWithoutPose()) {
            SelectorItem newItem = selectorItems[actionObject.GetId()];
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
        if (manuallySelected) {
            InteractiveObject selectedItem = GetSelectedObject();
            foreach (SelectorItem item in selectorItemsAimMenu) {
                if (item.InteractiveObject.GetId() == selectedItem.GetId()) {
                    
                    return;
                }
            }
            manuallySelected = false;
            DeselectObject(true);
        }
        
        
    }

    public void SwitchToNoPose() {
        
        ContentAim.SetActive(false);
        ContentNoPose.SetActive(true);
        ContainerAlphabet.SetActive(false);
        //selectorItems = selectorItemsNoPoseMenu;
        UpdateNoPoseMenu();
    }

    public void SwitchToAlphabet() {
        ContentAim.SetActive(false);
        ContentNoPose.SetActive(false);
        ContainerAlphabet.SetActive(true);
        
        UpdateAlphabetMenu();
        /*if (manuallySelected) {
            InteractiveObject selectedItem = GetSelectedObject();
            
            foreach (SelectorItem item in selectorItemsAlphabetMenu) {
                if (item.InteractiveObject.GetId() == selectedItem.GetId()) {
                    selectorItems = selectorItemsAimMenu;
                    selectorItems = selectorItemsAlphabetMenu;
                    SetSelectedObject(item, true);
                    return;
                }
            }
        }
        selectorItems = selectorItemsAlphabetMenu;*/
    }

    public InteractiveObject GetSelectedObject() {
        
        foreach (SelectorItem item in selectorItems.Values.ToList()) {
            if (item.IsSelected())
                return item.InteractiveObject;
        }
        return null;
    }
}
