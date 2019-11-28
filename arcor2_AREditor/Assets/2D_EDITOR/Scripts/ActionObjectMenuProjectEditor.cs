using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;

public class ActionObjectMenuProjectEditor : MonoBehaviour {
    public GameObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab;

    
    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.GameManager.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>());

    }

    /*
    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown dropdownEE = endEffectorList.GetComponent<Dropdown>();
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }*/


}
