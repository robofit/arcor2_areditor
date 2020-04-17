using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using Base;
using UnityEngine.EventSystems;

public class ActionObjectsSettingsMenu : MonoBehaviour, IMenu {
    public SwitchComponent Visiblity, Interactibility;
    public GameObject ActionObjectsList, ActionPointsList;
    [SerializeField]
    private GameObject ActionPointsScrollable, ActionObjectsScrollable;
    [SerializeField]
    private Slider APSizeSlider;

    private void Start() {
        Debug.Assert(ActionPointsScrollable != null);
        Debug.Assert(ActionObjectsScrollable != null);
        Debug.Assert(APSizeSlider != null);
        Base.GameManager.Instance.OnLoadScene += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnLoadProject += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnSceneChanged += OnSceneChanged;
        Base.GameManager.Instance.OnActionPointsChanged += OnActionPointsChanged;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        Interactibility.SetValue(false);
    }

    private void GameStateChanged(object sender, GameStateEventArgs args) {
        ActionPointsScrollable.SetActive(args.Data == GameManager.GameStateEnum.ProjectEditor);
    }

    public void UpdateMenu() {
        APSizeSlider.value = Scene.Instance.APSize;
    }

    public void ShowActionObjects() {
        Base.Scene.Instance.ShowActionObjects();
    }

    public void HideActionObjects() {
         Base.Scene.Instance.HideActionObjects();
    }

    public void InteractivityOn() {
        Base.Scene.Instance.SetActionObjectsInteractivity(true);
    }

    public void InteractivityOff() {
         Base.Scene.Instance.SetActionObjectsInteractivity(false);
    }

    public void OnSceneOrProjectLoaded(object sender, EventArgs eventArgs) {
        Visiblity.SetValue(Base.Scene.Instance.ActionObjectsVisible);        
        Interactibility.SetValue(Base.Scene.Instance.ActionObjectsInteractive);
    }

    public void OnSceneChanged(object sender, EventArgs eventArgs) {
        foreach (Transform t in ActionObjectsList.transform) {
            Destroy(t.gameObject);
        }
        foreach (Base.ActionObject actionObject in Base.Scene.Instance.ActionObjects.Values) {
            GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, ActionObjectsList.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = actionObject.Data.Name;
            btn.onClick.AddListener(() => ShowActionObject(actionObject));

            // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding AO when hovering over button
            OutlineOnClick AOoutline = actionObject.GetComponent<OutlineOnClick>();
            EventTrigger eventTrigger = btnGO.AddComponent<EventTrigger>();
            // Create OnPointerEnter entry
            EventTrigger.Entry OnPointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            OnPointerEnter.callback.AddListener((eventData) => AOoutline.Highlight());
            eventTrigger.triggers.Add(OnPointerEnter);

            // Create OnPointerExit entry
            EventTrigger.Entry OnPointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            OnPointerExit.callback.AddListener((eventData) => AOoutline.UnHighlight());
            eventTrigger.triggers.Add(OnPointerExit);
        }
    }

    public void OnActionPointsChanged(object sender, EventArgs eventArgs) {
        foreach (Transform t in ActionPointsList.transform) {
            Destroy(t.gameObject);
        }
        foreach (Base.ActionPoint actionPoint in Base.Scene.Instance.GetAllGlobalActionPoints()) {
            GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, ActionPointsList.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = actionPoint.Data.Name;
            btn.onClick.AddListener(() => ShowActionPoint(actionPoint));

            // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding AP when hovering over button
            OutlineOnClick APoutline = actionPoint.GetComponent<OutlineOnClick>();
            EventTrigger eventTrigger = btnGO.AddComponent<EventTrigger>();
            // Create OnPointerEnter entry
            EventTrigger.Entry OnPointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            OnPointerEnter.callback.AddListener((eventData) => APoutline.Highlight());
            eventTrigger.triggers.Add(OnPointerEnter);

            // Create OnPointerExit entry
            EventTrigger.Entry OnPointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            OnPointerExit.callback.AddListener((eventData) => APoutline.UnHighlight());
            eventTrigger.triggers.Add(OnPointerExit);
        }
    }

    private void ShowActionPoint(ActionPoint actionPoint) {
        MenuManager.Instance.ActionObjectSettingsMenu.Close();
        actionPoint.ShowMenu();
        Base.Scene.Instance.SetSelectedObject(actionPoint.gameObject);
        actionPoint.SendMessage("Select");
    }

    private void ShowActionObject(Base.ActionObject actionObject) {
        MenuManager.Instance.ActionObjectSettingsMenu.Close();
        actionObject.ShowMenu();
        Base.Scene.Instance.SetSelectedObject(actionObject.gameObject);
        actionObject.SendMessage("Select");

    }

    public void OnAPSizeChange(float value) {
        Scene.Instance.SetAPSize(value);
    }


}
