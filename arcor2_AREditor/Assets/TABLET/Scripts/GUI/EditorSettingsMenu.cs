using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using Base;
using UnityEngine.EventSystems;

public class EditorSettingsMenu : MonoBehaviour, IMenu {
    public GameObject ActionObjectsList, ActionPointsList;
    
    [SerializeField]
    private LabeledInput markerOffsetX, markerOffsetY, markerOffsetZ, recalibrationTime;

    private void Start() {

        Base.SceneManager.Instance.OnLoadScene += OnSceneOrProjectLoaded;
        Base.ProjectManager.Instance.OnLoadProject += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnSceneChanged += OnSceneChanged;
        Base.ProjectManager.Instance.OnActionPointAddedToScene += OnActionPointAdded;
        Base.WebsocketManager.Instance.OnActionPointRemoved += OnActionPointRemoved;
        Base.WebsocketManager.Instance.OnActionPointBaseUpdated += OnActionPointBaseUpdated;
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
        //Interactibility.SetValue(false);


    }

    private void OnActionPointRemoved(object sender, StringEventArgs args) {
        try {
            ActionButton btn = GetActionPointBtn(args.Data);
            Destroy(btn.gameObject);
        } catch (ItemNotFoundException) {

        }
    }

    private void OnActionPointBaseUpdated(object sender, BareActionPointEventArgs args) {
        try {
            ActionButton btn = GetActionPointBtn(args.ActionPoint.Id);
            btn.SetLabel(args.ActionPoint.Name);
        } catch (ItemNotFoundException) {
            Debug.LogError("Action point button " + args.ActionPoint.Name + " does not exists");
        }
    }


    private void OnActionPointAdded(object sender, ActionPointEventArgs args) {
        AddActionPointButton(args.ActionPoint);
    }

    private void GameStateChanged(object sender, GameStateEventArgs args) {
        //ActionPointsScrollable.SetActive(args.Data == GameManager.GameStateEnum.ProjectEditor);
        if (args.Data == GameManager.GameStateEnum.MainScreen || args.Data == GameManager.GameStateEnum.Disconnected ||
            args.Data == GameManager.GameStateEnum.PackageRunning)
            ClearMenu();
    }

    private ActionButton GetActionPointBtn(string id) {
        foreach (Transform t in ActionPointsList.transform) {
            ActionButton apBtn = t.GetComponent<ActionButton>();
            if (apBtn != null && apBtn.ObjectId == id)
                return apBtn;
        }
        throw new ItemNotFoundException("Button with id " + id + " does not exists");
    }

    public void UpdateMenu() {
        
        Vector3 offset = TransformConvertor.UnityToROS(PlayerPrefsHelper.LoadVector3("/marker_offset", Vector3.zero));
        markerOffsetX.SetValue(offset.x);
        markerOffsetY.SetValue(offset.y);
        markerOffsetZ.SetValue(offset.z);
        recalibrationTime.SetValue(PlayerPrefsHelper.LoadString("/autoCalib/recalibrationTime", "120"));
    }

    

    public void OnSceneOrProjectLoaded(object sender, EventArgs eventArgs) {
    }

    public void OnSceneChanged(object sender, EventArgs eventArgs) {
        foreach (Transform t in ActionObjectsList.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
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

    public void ClearMenu() {
        foreach (Transform t in ActionPointsList.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
    }

    public void UpdateMarkerOffset() {

#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        Vector3 offset = TransformConvertor.ROSToUnity(new Vector3((float) (double) markerOffsetX.GetValue(),
                                      (float) (double) markerOffsetY.GetValue(),
                                      (float) (double) markerOffsetZ.GetValue()));
        PlayerPrefsHelper.SaveVector3("/marker_offset", offset);
        CalibrationManager.Instance.UpdateMarkerOffset(offset);

    
#endif
    }
    private void AddActionPointButton(Base.ActionPoint actionPoint) {
        ActionButton btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, ActionPointsList.transform).GetComponent<ActionButton>();
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.SetLabel(actionPoint.Data.Name);
        btn.Button.onClick.AddListener(() => ShowActionPoint(actionPoint));
        btn.ObjectId = actionPoint.Data.Id;
        // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding AP when hovering over button
        OutlineOnClick APoutline = actionPoint.GetComponent<OutlineOnClick>();
        EventTrigger eventTrigger = btn.gameObject.AddComponent<EventTrigger>();
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

    private void ShowActionPoint(ActionPoint actionPoint) {
        MenuManager.Instance.EditorSettingsMenu.Close();
        actionPoint.OpenMenu();
        Base.SceneManager.Instance.SetSelectedObject(actionPoint.gameObject);
        // Select(force = true) to force selection and not losing AP highlight upon EditorSettingsMenu closing 
        actionPoint.SendMessage("Select", true);
    }

    private void ShowActionObject(Base.ActionObject actionObject) {
        MenuManager.Instance.EditorSettingsMenu.Close();
        actionObject.OpenMenu();
        Base.SceneManager.Instance.SetSelectedObject(actionObject.gameObject);
        actionObject.SendMessage("Select", true);
    }

    
}
