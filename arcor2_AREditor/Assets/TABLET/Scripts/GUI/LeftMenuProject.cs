using System;
using UnityEngine.UI;
using Base;
using System.Collections.Generic;
using IO.Swagger.Model;
using UnityEngine;

public class LeftMenuProject : LeftMenu
{

    public Button SetActionPointParentButton, AddActionButton, AddActionButton2, RunButton, RunButton2,
        AddConnectionButton, AddConnectionButton2;

    public GameObject ActionPicker;
    protected override void Update() {
        base.Update();

    }

    protected override void UpdateBtns(InteractiveObject selectedObject) {
        base.UpdateBtns(selectedObject);
        if (requestingObject || selectedObject == null) {
            SetActionPointParentButton.interactable = false;
            AddActionButton.interactable = false;
            AddActionButton2.interactable = false;
            AddConnectionButton.interactable = false;
            AddConnectionButton2.interactable = false;
            RunButton.interactable = false;
            RunButton2.interactable = false;
        } else {
            SetActionPointParentButton.interactable = selectedObject is ActionPoint3D;
            AddActionButton.interactable = selectedObject is ActionPoint3D;
            AddConnectionButton.interactable = selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput);
            AddConnectionButton2.interactable = AddConnectionButton.interactable;
            RunButton.interactable = selectedObject.GetType() == typeof(Action3D) || selectedObject.GetType() == typeof(ActionPoint3D);
            RunButton2.interactable = RunButton.interactable;
        }
    }

    protected override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();

        AddActionButton.GetComponent<Image>().enabled = false;
        AddActionButton2.GetComponent<Image>().enabled = false;
        //ActionPicker.SetActive(false);
    }

    public void CopyObjectClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(ActionPoint3D)) {
            ProjectManager.Instance.SelectAPNameWhenCreated = "copy_of_" + selectedObject.GetName();
            WebsocketManager.Instance.CopyActionPoint(selectedObject.GetId(), null);
        } else if (selectedObject.GetType() == typeof(Action3D)) {
            Action3D action = (Action3D) selectedObject;
            List<ActionParameter> parameters = new List<ActionParameter>();
            foreach (Base.Parameter p in action.Parameters.Values) {
                parameters.Add(new ActionParameter(p.ParameterMetadata.Name, p.ParameterMetadata.Type, p.Value));
            }
            WebsocketManager.Instance.AddAction(action.ActionPoint.GetId(), parameters, action.ActionProvider.GetProviderId() + "/" + action.Metadata.Name, action.GetName() + "_copy", action.GetFlows());
        }
    }

    public void AddConnectionClick() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if ((selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput))) {
            ((InputOutput) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
    }


    public void AddActionClick() {
        //was clicked the button in favorites or settings submenu?
        Button clickedButton = AddActionButton;
        if (currentSubmenuOpened == LeftMenuSelection.Favorites) {
            clickedButton = AddActionButton2;
        }

        if (!SelectorMenu.Instance.gameObject.activeSelf && !clickedButton.GetComponent<Image>().enabled) { //other menu/dialog opened
            SetActiveSubmenu(currentSubmenuOpened); //close all other opened menus/dialogs and takes care of red background of buttons
        }

        if (clickedButton.GetComponent<Image>().enabled) {
            clickedButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            //ActionPicker.SetActive(false);
        } else {
            clickedButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            //ActionPicker.SetActive(true);
        }
    }



    public void AddActionPointClick() {
        ControlBoxManager.Instance.ShowCreateGlobalActionPointDialog();
    }

    public override void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor &&
            MenuManager.Instance.MainMenu.CurrentState == DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu.State.Closed) {
            UpdateVisibility(true);
        } else {
            UpdateVisibility(false);
        }
    }
}
