using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class ActionObjectsSettingsMenu : MonoBehaviour, IMenu {
    public SwitchComponent Visiblity, Interactibility;
    public GameObject DynamicContent;

    private void Start() {
        Base.GameManager.Instance.OnLoadScene += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnLoadProject += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnSceneChanged += OnSceneChanged;
        Interactibility.SetValue(false);
    }

    public void UpdateMenu() {

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
        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (Base.ActionObject actionObject in Base.Scene.Instance.ActionObjects.Values) {
            GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, DynamicContent.transform);
            btnGO.transform.localScale = new Vector3(1, 1, 1);
            Button btn = btnGO.GetComponent<Button>();
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = actionObject.Data.Name;
            btn.onClick.AddListener(() => ShowActionObject(actionObject));
        }
    }

    private void ShowActionObject(Base.ActionObject actionObject) {
        MenuManager.Instance.ActionObjectSettingsMenu.Close();
        actionObject.ShowMenu();
        Base.Scene.Instance.SetSelectedObject(actionObject.gameObject);
        actionObject.SendMessage("Select");

    }


}
