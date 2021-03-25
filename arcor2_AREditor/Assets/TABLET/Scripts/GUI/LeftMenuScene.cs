using System;
using UnityEngine;
using UnityEngine.UI;
using Base;
using static Base.GameManager;

public class LeftMenuScene : LeftMenu
{

    //public GameObject MeshPicker;

    public Button AddMeshButton;
    protected override void Update() {
        base.Update();

    }

    protected override void UpdateBtns(InteractiveObject selectedObject) {
        base.UpdateBtns(selectedObject);
        if (requestingObject || selectedObject == null) {

        } else {

        }
    }

    protected override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();
        AddMeshButton.GetComponent<Image>().enabled = false;

        //MeshPicker.SetActive(false);
    }

    public void AddMeshClick() {
        if (AddMeshButton.GetComponent<Image>().enabled) {
            AddMeshButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            //MeshPicker.SetActive(false);
        } else {
            AddMeshButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            //MeshPicker.SetActive(true);
        }

    }

    public override void UpdateVisibility() {
        
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor &&
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
        
    }
}
