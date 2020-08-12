using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Inherited class of OutlineOnClick for selecting and highlighting an interactable object in Scene (ActionObject, ActionPoint, Action).
/// Selected object is deselected upon clicking on some blind spot or upon selecting another selectable object.
/// </summary>
public class OutlineOnClickSelect : OutlineOnClick {

    private bool objSelected = false;
    private bool forceSelected = false;

    private void OnEnable() {
        GameManager.Instance.OnSceneInteractable += OnDeselect;
    }

    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnSceneInteractable -= OnDeselect;
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

    private void OnDeselect(object sender, EventArgs e) {
        if (objSelected && !forceSelected) {
            objSelected = false;
            SceneManager.Instance.SetSelectedObject(null);
            Deselect();
        }
        forceSelected = false;
    }

    protected override void Select(bool force = false) {
        forceSelected = force;
        objSelected = true;
        SceneManager.Instance.SetSelectedObject(gameObject);
        base.Select();
    }
}
