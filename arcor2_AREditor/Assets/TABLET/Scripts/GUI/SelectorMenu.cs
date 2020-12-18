using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;

public class SelectorMenu : Singleton<SelectorMenu> {
    public GameObject SelectorItemPrefab;

    public GameObject ContentAim, ContentAlphabet, ContentNoPose;

    private void Start() {
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
        ProjectManager.Instance.OnProjectChanged += OnProjectChanged;
    }

    private void OnProjectChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        UpdateAlphabetMenu();
        UpdateNoPoseMenu();
    }

    public void UpdateAimMenu(List<InteractiveObject> items) {
        List<string> strings = new List<string>();

        int count = 0;
        foreach (InteractiveObject item in items) {



            strings.Add(item.GetName());
            if (count++ > 6)
                return;
        }
        UpdateAimMenu(strings);

    }

    public void UpdateAimMenu(List<string> items) {
        ClearMenu(ContentAim.transform);
        foreach (string item in items) {
            SelectorItem selectorItem = Instantiate(SelectorItemPrefab, ContentAim.transform).GetComponent<SelectorItem>();
            selectorItem.SetText(item);
        }

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
}
