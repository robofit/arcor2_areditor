using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.UI;

public class ControlBoxManager : Singleton<ControlBoxManager> {

    private TransformGizmo tfGizmo;
    [SerializeField]
    private InputDialog InputDialog;
    [SerializeField]
    private GameObject CreateGlobalActionPointBtn;

    public Toggle MoveToggle;
    public Toggle RotateToggle;
    public Toggle TrackablesToggle;
    public Toggle ConnectionsToggle;

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
        Debug.Assert(CreateGlobalActionPointBtn != null);
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
        MoveToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_gizmo_move", false);
        RotateToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_gizmo_rotate", false);
#if UNITY_ANDROID && !UNITY_EDITOR
        TrackablesToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_display_trackables", false);
#endif
        ConnectionsToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_display_connections", true);
        Base.GameManager.Instance.OnGameStateChanged += GameStateChanged;
    }

    private void GameStateChanged(object sender, GameStateEventArgs args) {
        if (args.Data == GameManager.GameStateEnum.ProjectEditor) {
            CreateGlobalActionPointBtn.SetActive(true);
        } else {
            CreateGlobalActionPointBtn.SetActive(false);
        }
    }

    public void DisplayTrackables(bool active) {
#if UNITY_ANDROID && !UNITY_EDITOR
        TrackingManager.Instance.DisplayPlanesAndFeatures(active);
#endif
    }


    public void ShowActionObjectSettingsMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectSettingsMenu);
    }

    public void DisplayConnections(bool active) {
        ConnectionManagerArcoro.Instance.DisplayConnections(active);
    }

    public void ShowCreateGlobalActionPointDialog() {
        InputDialog.Open("Create action point",
                         "Type action point name",
                         "Name",
                         Scene.Instance.GetFreeAPName("global"),
                         () => CreateGlobalActionPoint(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public async void CreateGlobalActionPoint(string name) {
        bool result = await GameManager.Instance.AddActionPoint(name, "", new IO.Swagger.Model.Position());
        if (result)
            InputDialog.Close();
    }

    private void OnDestroy() {
        PlayerPrefsHelper.SaveBool("control_box_gizmo_move", MoveToggle.isOn);
        PlayerPrefsHelper.SaveBool("control_box_gizmo_rotate", RotateToggle.isOn);
#if UNITY_ANDROID && !UNITY_EDITOR
        PlayerPrefsHelper.SaveBool("control_box_display_trackables", TrackablesToggle.isOn);
#endif
        PlayerPrefsHelper.SaveBool("control_box_display_connections", ConnectionsToggle.isOn);
    }
}
