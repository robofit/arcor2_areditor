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
    public GameObject ContentAim, ContentAlphabet, ContentNoPose;

    private bool manuallySelected;

    private long iteration = 0;


    private void Start() {
        GameManager.Instance.OnCloseProject += OnCloseProjectScene;
        GameManager.Instance.OnCloseScene += OnCloseProjectScene;
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
        ProjectManager.Instance.OnProjectChanged += OnProjectChanged;
    }

    private void OnCloseProjectScene(object sender, System.EventArgs e) {
        Debug.LogError("on close");
        for (int i = 0; i < selectorItems.Count; ++i) {
            Destroy(selectorItems[i].gameObject);
        }
        selectorItems.Clear();
        manuallySelected = false;
    }

    private class SelectorItemComparer : IComparer<SelectorItem> {
        public int Compare(SelectorItem x, SelectorItem y) {
            return x.Score.CompareTo(y.Score);
        }
    }

    private List<SelectorItem> selectorItems = new List<SelectorItem>();

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

    private void OnProjectChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    private SelectorItem GetSelectorItem(string id) {
        foreach (SelectorItem item in selectorItems) {
            if (string.Equals(item.InteractiveObject.GetId(), id)) {
                return item;
            }
        }
        return null;
    }

    private void PrintItems() {
        Debug.Log(selectorItems.Count);
        foreach (SelectorItem item in selectorItems) {
            Debug.LogError(item.InteractiveObject.GetName() + ": " + item.Score + " (" + item.InteractiveObject.GetId() + ")");
        }
    }

    public void UpdateAimMenu(List<Tuple<float, InteractiveObject>> items) {
        
        int count = 0;
        for (int i = selectorItems.Count - 1; i >= 0; --i) {
            if ((iteration - selectorItems[i].GetLastUpdate()) > 5) {
                RemoveItem(i);
            }
        }
        List<SelectorItem> newItems = new List<SelectorItem>();
        foreach (Tuple<float, InteractiveObject> item in items) {
            if (selectorItems.Count < 6 || item.Item1 <= selectorItems.Last().Score) {
                SelectorItem selectorItem = GetSelectorItem(item.Item2.GetId());
                 if (selectorItem == null) {
                    SelectorItem newItem = CreateSelectorItem(item.Item2, ContentAim.transform, item.Item1);
                    selectorItems.Add(newItem);
                    newItems.Add(newItem);
                } else {
                    selectorItem.UpdateScore(item.Item1, iteration);
                }

            } 
            if (count++ > 7)
                break;
        }
        selectorItems.Sort(new SelectorItemComparer());
        while (selectorItems.Count > 6) {
            RemoveItem(selectorItems.Count - 1);
        }
        foreach (SelectorItem item in newItems) {
            item.gameObject.SetActive(true);
        }
        if (!manuallySelected && selectorItems.Count > 0) {
            SetSelectedObject(selectorItems.First(), false);
        }

        ++iteration;
    }

    private void RemoveItem(int index) {
        if (selectorItems[index].IsSelected()) {
            manuallySelected = false;
            selectorItems[index].SetSelected(false, true);
        }
        Destroy(selectorItems[index].gameObject);
        selectorItems.RemoveAt(index);
    }

    public void SetSelectedObject(SelectorItem selectorItem, bool manually = false) {
        if (manually) {
            if (selectorItem.IsSelected() && manuallySelected) {
                selectorItem.SetSelected(false, manually);
                manuallySelected = false;
                return;
            }
        }        
        foreach (SelectorItem item in selectorItems) {
            item.SetSelected(false, manually);
        }
        selectorItem.SetSelected(true, manually);
        if (manually)
            manuallySelected = true;
    }

    public SelectorItem CreateSelectorItem(InteractiveObject interactiveObject, Transform parent, float score) {
        SelectorItem selectorItem = Instantiate(SelectorItemPrefab, parent).GetComponent<SelectorItem>();
        selectorItem.gameObject.SetActive(false);
        selectorItem.SetText(interactiveObject.GetName());
        selectorItem.SetObject(interactiveObject, score, iteration);
        return selectorItem;
    }


    public void UpdateAlphabetMenu() {
        if (!ContentAlphabet.activeSelf)
            return;
        ClearMenu(ContentAlphabet.transform);
        foreach (InteractiveObject io in GameManager.Instance.GetAllInteractiveObjects()) {
            SelectorItem selectorItem = Instantiate(SelectorItemPrefab, ContentAlphabet.transform).GetComponent<SelectorItem>();
            selectorItem.SetText(io.GetName());
        }
    }

    public void UpdateNoPoseMenu() {
        if (!ContentNoPose.activeSelf)
            return;
        ClearMenu(ContentNoPose.transform);
        foreach (ActionObject actionObject in SceneManager.Instance.GetAllActionObjectsWithoutPose()) {
            SelectorItem selectorItem = Instantiate(SelectorItemPrefab, ContentNoPose.transform).GetComponent<SelectorItem>();
            selectorItem.SetText(actionObject.GetName());
        }
    }

    private void ClearMenu(Transform menu) {
        foreach (Transform t in menu) {
            if (!t.CompareTag("Persistent")) {
                Destroy(t.gameObject);
            }
        }
    }

    public void SwitchToAim() {
        ContentAim.SetActive(true);
        ContentNoPose.SetActive(false);
        ContentAlphabet.SetActive(false);
    }

    public void SwitchToNoPose() {
        
        ContentAim.SetActive(false);
        ContentNoPose.SetActive(true);
        ContentAlphabet.SetActive(false);
        UpdateNoPoseMenu();
    }

    public void SwitchToAlphabet() {
        ContentAim.SetActive(false);
        ContentNoPose.SetActive(false);
        ContentAlphabet.SetActive(true);
        UpdateAlphabetMenu();
    }

    public InteractiveObject GetSelectedObject() {
        foreach (SelectorItem item in selectorItems) {
            if (item.IsSelected())
                return item.InteractiveObject;
        }
        return null;
    }
}
