using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(CanvasGroup))]
public class LeftMenu : Base.Singleton<LeftMenu> {

    private CanvasGroup CanvasGroup;

    public Button MoveBtn, MenuBtn, ConnectionsBtn, InteractBtn;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
    }

    private bool updateButtonsInteractivity = false;

    private bool requestingObject = false;

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        switch (args.Data) {
            case GameManager.EditorStateEnum.Normal:
                requestingObject = false;
                updateButtonsInteractivity = true;
                break;
            case GameManager.EditorStateEnum.InteractionDisabled:
                updateButtonsInteractivity = false;
                ConnectionsBtn.interactable = false;
                MoveBtn.interactable = false;
                MenuBtn.interactable = false;
                InteractBtn.interactable = false;
                break;
            case GameManager.EditorStateEnum.Closed:
                updateButtonsInteractivity = false;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingActionPointParent:
                requestingObject = true;
                break;
        }
    }

    private void Update() {
        UpdateVisibility();
        if (!updateButtonsInteractivity)
            return;
        InteractiveObject selectedObject = SelectorMenu.Instance.GetSelectedObject();
        if (requestingObject || selectedObject == null) {
            ConnectionsBtn.interactable = false;
            MoveBtn.interactable = false;
            MenuBtn.interactable = false;
            InteractBtn.interactable = false;
            
        } else {
            ConnectionsBtn.interactable = selectedObject.GetType() == typeof(PuckInput) ||
                 selectedObject.GetType() == typeof(PuckOutput);

            MoveBtn.interactable = selectedObject.Movable();
            MenuBtn.interactable = selectedObject.HasMenu();
            InteractBtn.interactable = selectedObject.GetType() == typeof(Recalibrate) ||
                selectedObject.GetType() == typeof(CreateAnchor);
        }
        
    }

    private void UpdateVisibility() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            MenuManager.Instance.CheckIsAnyMenuOpened()) {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
        } else {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
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
