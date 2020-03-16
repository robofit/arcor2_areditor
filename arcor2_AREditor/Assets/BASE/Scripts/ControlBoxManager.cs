using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;

public class ControlBoxManager : Singleton<ControlBoxManager> {

    private TransformGizmo tfGizmo;

    private bool useGizmoMove = false;
    public bool UseGizmoMove {
        get => useGizmoMove;
        set {
            useGizmoMove = value;
            tfGizmo.transformType = TransformType.Move;
        }
    }

    private bool useGizmoRotate = false;
    public bool UseGizmoRotate {
        get => useGizmoRotate;
        set {
            useGizmoRotate = value;
            tfGizmo.transformType = TransformType.Rotate;
        }
    }

    private void Start() {
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
    }


    public void DisplayTrackables(bool active) {
        CalibrationManager.Instance.DisplayPlanes(active);
    }

    public void ShowActionObjectSettingsMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectSettingsMenu);
    }
}
