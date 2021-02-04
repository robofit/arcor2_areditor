using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class LeftMenu : Base.Singleton<LeftMenu> {

    private CanvasGroup CanvasGroup;

    public Button MoveBtn, MenuBtn, ConnectionsBtn, InteractBtn;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            MenuManager.Instance.IsAnyMenuOpened) {
            CanvasGroup.interactable = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.alpha = 1;
        }
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject != null) {
            //Debug.LogError(selectedObject.GetType());
            ConnectionsBtn.interactable = selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput);

            MoveBtn.interactable = selectedObject.Movable();
            MenuBtn.interactable = selectedObject.HasMenu();
            InteractBtn.interactable = selectedObject.GetType() == typeof(Recalibrate) ||
                selectedObject.GetType() == typeof(CreateAnchor);
        } else {
            ConnectionsBtn.interactable = false;
            MoveBtn.interactable = false;
            MenuBtn.interactable = false;
            InteractBtn.interactable = false;
        }
    }

    public void MoveButtonCb() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.Movable()) {
            selectedObject.StartManipulation();
        }

    }

    public void MenuButtonCb() {     
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.HasMenu()) {
            selectedObject.OpenMenu();
        }
    }

    public void ConnectionButtonCb() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(PuckInput) ||
                selectedObject.GetType() == typeof(PuckOutput)) {
            ((InputOutput) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
        
    }

    public void InteractButtonCb() {
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (selectedObject is null)
            return;
        if (selectedObject.GetType() == typeof(Recalibrate)) {
            ((Recalibrate) selectedObject).OnClick(Clickable.Click.TOUCH);
        } else if (selectedObject.GetType() == typeof(CreateAnchor)) {
            ((CreateAnchor) selectedObject).OnClick(Clickable.Click.TOUCH);
        }
    }
}
