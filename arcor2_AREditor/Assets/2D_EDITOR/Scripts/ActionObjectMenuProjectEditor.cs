using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Base;

public class ActionObjectMenuProjectEditor : MonoBehaviour {
    public ActionObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab;

    
    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.Scene.Instance.SpawnActionPoint(CurrentObject, null);

    }

    /*
    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown dropdownEE = endEffectorList.GetComponent<Dropdown>();
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }*/


}
