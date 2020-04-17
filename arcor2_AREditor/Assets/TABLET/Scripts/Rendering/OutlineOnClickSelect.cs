using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using UnityEngine.EventSystems;

/// <summary>
/// Inherited class of OutlineOnClick for selecting and highlighting an interactable object in Scene (ActionObject, ActionPoint, Action).
/// Selected object is deselected upon clicking on some blind spot or upon selecting another selectable object.
/// </summary>
public class OutlineOnClickSelect : OutlineOnClick {

    private void OnEnable() {
        InputHandler.Instance.OnBlindClick += OnBlindClick;
    }

    private void OnDisable() {
        if (InputHandler.Instance != null) {
            InputHandler.Instance.OnBlindClick -= OnBlindClick;
        }
    }

    public override void OnClick(Click type) {
        // HANDLE MOUSE
        if (type == Click.MOUSE_RIGHT_BUTTON) {
            Select();
        }
        // HANDLE TOUCH
        else if (type == Click.TOUCH && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
            Select();
        }
    }

    
    private void OnBlindClick(object sender, EventClickArgs e) {
        if (e.ClickType == Click.MOUSE_LEFT_BUTTON || e.ClickType == Click.MOUSE_RIGHT_BUTTON || e.ClickType == Click.MOUSE_MIDDLE_BUTTON ||
            e.ClickType == Click.TOUCH) {
            if (GameManager.Instance.SceneInteractable) {
                Scene.Instance.SetSelectedObject(null);
                RemoveMaterial(ClickMaterial);
                foreach (Renderer renderer in Renderers) {
                    renderer.materials = materials[renderer].ToArray();
                }
            }
        }
    }

    protected override void Select() {
        Scene.Instance.SetSelectedObject(gameObject);
        base.Select();
    }
}
