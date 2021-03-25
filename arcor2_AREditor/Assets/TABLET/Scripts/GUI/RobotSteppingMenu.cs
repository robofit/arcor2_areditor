using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;

public class RobotSteppingMenu : Singleton<RobotSteppingMenu>
{
    public GameObject StepButtons;
    public CoordinatesBtnGroup Coordinates;
    public TranformWheelUnits Units, UnitsDegrees;
    public TwoStatesToggle RobotWorldBtn, RotateTranslateBtn, SafeButton;

    public CanvasGroup CanvasGroup;

    private GameObject gizmo;

    private bool safe = true, world = false;


    private void Update() {
        if (CanvasGroup.alpha == 1 && gizmo != null) {
            if (world) {
                gizmo.transform.rotation = GameManager.Instance.Scene.transform.rotation;
            } else {
                gizmo.transform.rotation = Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation) * SceneManager.Instance.SelectedRobot.GetTransform().rotation;
            }
        }
    }

    public async void SetPerpendicular() {
        try {
            await WebsocketManager.Instance.SetEefPerpendicularToWorld(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId(), 0.3m, safe);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to set robot perpendicular", ex.Message);
        }
    }

    public void SwitchToSafe() {
        safe = true;
    }

    public void SwithToUnsafe() {
        safe = false;
    }

    public void SwitchToRobot() {
        world = false;
    }

    public void SwitchToWorld() {
        world = true;
    }

    public async void HoldPressed() {
        try {
            await WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to enable hand teaching mode", ex.Message);
        }      
    }

    public async void HoldReleased() {
        try {
            await WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to disable hand teaching mode", ex.Message);
        }
    }

    public void Show() {
        if (gizmo != null)
            Destroy(gizmo);

        gizmo = Instantiate(GameManager.Instance.GizmoPrefab, SceneManager.Instance.SelectedEndEffector.transform);
        

        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void RobotStepUp() {
        RobotStep(GetPositionValue(1));
    }


    public void RobotStepDown() {
        RobotStep(GetPositionValue(-1));
    }

    public async void RobotStep(float step) {
        IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.X;
        switch (Coordinates.GetSelectedAxis()) {
            case "x":
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.X;
                break;
            case "y":
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.Y;
                break;
            case "z":
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.Z;
                break;
        }
        await WebsocketManager.Instance.StepRobotEef(axis, SceneManager.Instance.SelectedEndEffector.GetId(), safe, SceneManager.Instance.SelectedRobot.GetId(), 0.3m,
            (decimal) step, IO.Swagger.Model.StepRobotEefRequestArgs.WhatEnum.Position, world ? IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.World : IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.Robot);

    }

    private float GetPositionValue(float v) {
        switch (Units.GetValue()) {
            case "m":
                return v;
            case "cm":
                return v * 0.01f;
            case "mm":
                return v * 0.001f;
            case "μm":
                return v * 0.000001f;
            default:
                return v;
        };
    }

    private int ComputePositionValue(float value) {
        switch (Units.GetValue()) {
            case "cm":
                return (int) (value * 100);
            case "mm":
                return (int) (value * 1000);
            case "μm":
                return (int) (value * 1000000);
            default:
                return (int) value;
        };
    }

    private float GetRotationValue(float v) {
        switch (UnitsDegrees.GetValue()) {
            case "°":
                return v;
            case "'":
                return v / 60f;
            case "''":
                return v / 3600f;
            default:
                return v;
        };
    }

    private int ComputeRotationValue(float value) {
        switch (UnitsDegrees.GetValue()) {
            case "°":
                return (int) value;
            case "'":
                return (int) (value * 60);
            case "''":
                return (int) (value * 3600);
            default:
                return (int) value;
        };
    }

    internal void Hide() {
        if (gizmo != null)
            Destroy(gizmo);
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }
}
