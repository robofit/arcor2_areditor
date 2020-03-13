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


}
