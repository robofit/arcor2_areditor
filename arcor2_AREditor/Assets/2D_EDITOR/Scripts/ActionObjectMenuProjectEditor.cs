using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;

public class ActionObjectMenuProjectEditor : MonoBehaviour, IMenu {
    public Base.ActionObject CurrentObject;
    [SerializeField]
    private TMPro.TMP_Text objectName;

    
    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.Scene.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>(), null);

    }

    public void UpdateMenu() {
        objectName.text = CurrentObject.Data.Id;
    }

    /*
    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown dropdownEE = endEffectorList.GetComponent<Dropdown>();
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }*/


}
