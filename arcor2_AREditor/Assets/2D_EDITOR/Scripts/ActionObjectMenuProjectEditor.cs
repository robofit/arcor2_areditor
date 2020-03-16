using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Base;

public class ActionObjectMenuProjectEditor : MonoBehaviour, IMenu {
    public Base.ActionObject CurrentObject;
    [SerializeField]
    private TMPro.TMP_Text objectName;
    public Slider VisibilitySlider;

    
    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.Scene.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>(), null);
         
    }

    public void UpdateMenu() {
        objectName.text = CurrentObject.Data.Id;
        VisibilitySlider.value = CurrentObject.GetVisibility()*100;
    }

    public void OnVisibilityChange(float value) {
        CurrentObject.SetVisibility(value/100f); 
    }

    public void ShowNextAO() {
        ActionObject nextAO = Scene.Instance.GetNextActionObject(CurrentObject.Data.Uuid);
        ShowActionObject(nextAO);
    }

    public void ShowPreviousAO() {
        ActionObject previousAO = Scene.Instance.GetNextActionObject(CurrentObject.Data.Uuid);
        ShowActionObject(previousAO);
    }

    private static void ShowActionObject(ActionObject actionObject) {
        actionObject.ShowMenu();
        Scene.Instance.SetSelectedObject(actionObject.gameObject);
        actionObject.SendMessage("Select");
    }
}
