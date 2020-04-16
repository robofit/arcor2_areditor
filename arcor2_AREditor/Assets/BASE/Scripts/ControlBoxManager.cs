using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;

public class ControlBoxManager : Singleton<ControlBoxManager> {

    private TransformGizmo tfGizmo;
    [SerializeField]
    private InputDialog InputDialog;

    [SerializeField]
    private GameObject CreateGlobalActionPointBtn;

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
        TrackingManager.Instance.DisplayPlanesAndFeatures(active);
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
}
