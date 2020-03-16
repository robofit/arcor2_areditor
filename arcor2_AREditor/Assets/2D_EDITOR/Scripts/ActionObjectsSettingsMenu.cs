using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionObjectsSettingsMenu : MonoBehaviour, IMenu {
    public SwitchComponent Visiblity, Interactibility;

    private void Start() {
        Base.GameManager.Instance.OnLoadScene += OnSceneOrProjectLoaded;
        Base.GameManager.Instance.OnLoadProject += OnSceneOrProjectLoaded;
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


}
