using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;
using UnityEngine.UI;

public class RobotSteppingMenu : Singleton<RobotSteppingMenu>
{
    public Button StepuUpButton, StepDownButton;
    public Slider SpeedSlider;
    public GameObject StepButtons;
    public CoordinatesBtnGroup Coordinates;
    public TranformWheelUnits Units, UnitsDegrees;
    public TwoStatesToggle RobotWorldBtn, RotateTranslateBtn, SafeButton;

    public CanvasGroup CanvasGroup;

    private GameObject gizmo;

    private bool safe = true, world = false, translate = true;

    private void OnEnable() {
        WebsocketManager.Instance.OnRobotMoveToPoseEvent += OnRobotMoveToPoseEvent;
    }

    private void OnDisable() {
        WebsocketManager.Instance.OnRobotMoveToPoseEvent -= OnRobotMoveToPoseEvent;
    }

    private void OnRobotMoveToPoseEvent(object sender, RobotMoveToPoseEventArgs args) {
        if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.End ||
            args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.Failed) {
            StepuUpButton.interactable = true;
            StepDownButton.interactable = true;
        }
    }

    private void Update() {
        if (CanvasGroup.alpha == 1) {
            
            if (gizmo != null && SceneManager.Instance.IsRobotAndEESelected()) {

                if (world) {
                    gizmo.transform.rotation = GameManager.Instance.Scene.transform.rotation;
                } else {
                    gizmo.transform.rotation = SceneManager.Instance.SelectedRobot.GetTransform().rotation;// * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation);
                }

                if (translate) {
                    Vector3 position;
                    if (world) {
                        position = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(SceneManager.Instance.SelectedEndEffector.transform.position));
                    } else {
                        //position = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedRobot.GetTransform().InverseTransformPoint(SceneManager.Instance.SelectedEndEffector.transform.position));
                        position = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedEndEffector.transform.localPosition);
                    }
                    Coordinates.X.SetValueMeters(position.x);
                    Coordinates.Y.SetValueMeters(position.y);
                    Coordinates.Z.SetValueMeters(position.z);

                } else {
                    Quaternion newrotation;
                    if (world)
                        newrotation = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedEndEffector.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation));
                    else
                        newrotation = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedEndEffector.transform.rotation * Quaternion.Inverse(SceneManager.Instance.SelectedRobot.GetTransform().rotation));
                    Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
                    Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
                    Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
                }
            }
        }
        
    }

    public async void SetPerpendicular() {
        try {
            
            await WebsocketManager.Instance.SetEefPerpendicularToWorld(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), (decimal) SpeedSlider.value, safe);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to set robot perpendicular", ex.Message);
        }
    }

    public void SwitchToSafe() {
        safe = true;
        SafeButton.SetDescription("Switch to unsafe movements");
    }

    public void SwithToUnsafe() {
        safe = false;
        SafeButton.SetDescription("Switch to safe movements");
    }

    public void SwitchToRobot() {
        world = false;
        SafeButton.SetDescription("Switch to world coordinate system");
    }

    public void SwitchToWorld() {
        world = true;
        SafeButton.SetDescription("Switch to robot coordinate system");
    }

    public void SwithToTranslate() {
        translate = true;
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        SafeButton.SetDescription("Switch to rotate");
    }

    public void SwitchToRotate() {
        translate = false;
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        SafeButton.SetDescription("Switch to translate");
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

        gizmo = Instantiate(GameManager.Instance.GizmoPrefab);
        gizmo.transform.SetParent(SceneManager.Instance.SelectedEndEffector.transform);
        gizmo.transform.localPosition = Vector3.zero;
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void RobotStepUp() {
        if (translate)
            RobotStep(GetPositionValue(1));
        else
            RobotStep(GetRotationValue(0.01745329252f)); // pi/180 - rotation in radians
    }


    public void RobotStepDown() {
        if (translate)
            RobotStep(GetPositionValue(-1));
        else
            RobotStep(GetRotationValue(-0.01745329252f)); // pi/180
    }

    public async void RobotStep(float step) {

        StepuUpButton.interactable = false;
        StepDownButton.interactable = false;
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
        try {
            await WebsocketManager.Instance.StepRobotEef(axis, SceneManager.Instance.SelectedEndEffector.GetName(), safe, SceneManager.Instance.SelectedRobot.GetId(), (decimal) SpeedSlider.value,
            (decimal) step, translate ? IO.Swagger.Model.StepRobotEefRequestArgs.WhatEnum.Position : IO.Swagger.Model.StepRobotEefRequestArgs.WhatEnum.Orientation,
            world ? IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.World : IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.Robot);
        } catch (RequestFailedException ex ) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
            StepuUpButton.interactable = true;
            StepDownButton.interactable = true;
        }
        

    }

    private float GetPositionValue(float v) {
        switch (Units.GetValue()) {
            case "dm":
                return v * 0.1f;
            case "cm":
                return v * 0.01f;
            case "mm":
                return v * 0.001f;
            case "0.1mm":
                return v * 0.0001f;
            default:
                return v;
        };
    }

    private int ComputePositionValue(float value) {
        switch (Units.GetValue()) {
            case "dm":
                return (int) (value * 10);
            case "cm":
                return (int) (value * 100);
            case "mm":
                return (int) (value * 1000);
            case "0.1mm":
                return (int) (value * 10000);
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
