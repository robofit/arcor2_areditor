using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Events;

public class RobotSteppingMenu : Singleton<RobotSteppingMenu> {
    public ButtonWithTooltip StepuUpButton, StepDownButton, SetEEfPerpendicular, HandTeachingModeButton;
    public Slider SpeedSlider;
    public GameObject StepButtons;
    public TranformWheelUnits Units, UnitsDegrees;
    public TwoStatesToggle RobotWorldBtn, RotateTranslateBtn, SafeButton;
    public Image HandBtnRedBackground;
    public ButtonWithTooltip BackBtn;

    public CanvasGroup CanvasGroup;

    private Gizmo gizmo;

    private bool safe = true, world = false, translate = true;

    private UnityAction closeCallback;
    private Gizmo.Axis selectedAxis;

    private void OnSelectedGizmoAxis(object sender, GizmoAxisEventArgs args) {
        SelectAxis(args.SelectedAxis);
    }

    private void SelectAxis(Gizmo.Axis axis, bool forceUpdate = false) {
        if (forceUpdate || selectedAxis != axis) {
            selectedAxis = axis;
            gizmo.HiglightAxis(axis);
            SetRotationAxis(axis);
        }
    }

    private void Start() {
        WebsocketManager.Instance.OnRobotMoveToPoseEvent += OnRobotMoveToPoseEvent;
        WebsocketManager.Instance.OnRobotMoveToJointsEvent += OnRobotMoveToJointsEvent;
        closeCallback = null;
    }  

    private void OnRobotMoveToJointsEvent(object sender, RobotMoveToJointsEventArgs args) {
        if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToJointsData.MoveEventTypeEnum.End ||
            args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToJointsData.MoveEventTypeEnum.Failed) {
            SetInteractivityOfRobotBtns(true);
        } else if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToJointsData.MoveEventTypeEnum.Start) {
            SetInteractivityOfRobotBtns(false, "Robot is already moving");
        }
    }

    private void OnRobotMoveToPoseEvent(object sender, RobotMoveToPoseEventArgs args) {
        if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.End ||
            args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.Failed) {
            SetInteractivityOfRobotBtns(true);
        } else if (args.Event.Data.MoveEventType == IO.Swagger.Model.RobotMoveToPoseData.MoveEventTypeEnum.Start) {
            SetInteractivityOfRobotBtns(false, "Robot is already moving");
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
                    //Coordinates.X.SetValueMeters(position.x);
                    //Coordinates.Y.SetValueMeters(position.y);
                    //Coordinates.Z.SetValueMeters(position.z);

                } else {
                    Quaternion newrotation;
                    if (world)
                        newrotation = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedEndEffector.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation));
                    else
                        newrotation = TransformConvertor.UnityToROS(SceneManager.Instance.SelectedEndEffector.transform.rotation * Quaternion.Inverse(SceneManager.Instance.SelectedRobot.GetTransform().rotation));
                    //Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
                    //Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
                    //Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
                }
            }
        }
    }

    private void SetInteractivityOfRobotBtns(bool interactive, string alternativeDescription = "") {
        SetEEfPerpendicular.SetInteractivity(interactive, alternativeDescription);
        StepuUpButton.SetInteractivity(interactive, alternativeDescription);
        StepDownButton.SetInteractivity(interactive, alternativeDescription);
    }

    public async void SetPerpendicular() {
        try {
            SetInteractivityOfRobotBtns(false, "Robot is already moving");
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;

            await WebsocketManager.Instance.SetEefPerpendicularToWorld(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), GetSpeedSliderValue(), safe, armId);
        } catch (RequestFailedException ex) {
            SetInteractivityOfRobotBtns(true);
            Notifications.Instance.ShowNotification("Failed to set robot perpendicular", ex.Message);
        }
    }

    private decimal GetSpeedSliderValue() {
        double sliderMin = SpeedSlider.minValue;
        double sliderMax = SpeedSlider.maxValue;
        double halfSliderMax = sliderMax / 2;
        double halfLogValue = 0.1;
        double value = SpeedSlider.value;
        if (value > halfSliderMax)
            return (decimal) ((value - halfSliderMax) * (1 - halfLogValue) / (sliderMax - halfSliderMax) + halfLogValue); // maps interval <0.5;1> to <0.1;1> (https://stackoverflow.com/questions/14353485/how-do-i-map-numbers-in-c-sharp-like-with-map-in-arduino)
        else
            return (decimal) ((value) * (halfLogValue) / (halfSliderMax - 0.001) + 0.001); // maps interval <0.5;1> to <0.1;1> (https://stackoverflow.com/questions/14353485/how-do-i-map-numbers-in-c-sharp-like-with-map-in-arduino)
        /*double minp = sliderMin;
        double maxp = halfSliderMax;

        double minv = Math.Log(0.001);
        double maxv = Math.Log(halfLogValue);

        // calculate adjustment factor
        double scale = (maxv - minv) / (maxp - minp);

        return (decimal) Math.Exp(minv + scale * (SpeedSlider.value - minp));*/

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
        RobotWorldBtn.SetDescription("Switch to world coordinate system");
    }

    public void SwitchToWorld() {
        world = true;
        RobotWorldBtn.SetDescription("Switch to robot coordinate system");
    }

    public void SwithToTranslate() {
        translate = true;
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        RotateTranslateBtn.SetDescription("Switch to rotate");
        SetRotationAxis(Gizmo.Axis.NONE);
    }

    public void SwitchToRotate() {
        translate = false;
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        RotateTranslateBtn.SetDescription("Switch to translate");
        SetRotationAxis(selectedAxis);
    }

    public async void HoldPressed() {
        if (!HandTeachingModeButton.IsInteractive())
            return;
        HandBtnRedBackground.enabled = true;
        try {
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            await WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true, armId);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to enable hand teaching mode", ex.Message);
        }
    }

    public async void HoldReleased() {
        if (!HandTeachingModeButton.IsInteractive())
            return;
        HandBtnRedBackground.enabled = false;
        try {
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            await WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false, armId);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to disable hand teaching mode", ex.Message);
        }
    }

    public void Show(bool showBackBtn = false, string backBtnDescritpion = null, UnityAction closeCallback = null) {
        if (gizmo != null)
            Destroy(gizmo.gameObject);
        this.closeCallback = closeCallback;
        gizmo = Instantiate(GameManager.Instance.GizmoPrefab).GetComponent<Gizmo>();
        gizmo.transform.SetParent(SceneManager.Instance.SelectedEndEffector.transform);
        gizmo.transform.localPosition = Vector3.zero;
        Sight.Instance.SelectedGizmoAxis += OnSelectedGizmoAxis;
        SelectAxis(Gizmo.Axis.X, true);
        BackBtn.gameObject.SetActive(showBackBtn);
        if (!string.IsNullOrEmpty(backBtnDescritpion)) {
            BackBtn.SetDescription(backBtnDescritpion);
        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);

        switch (RotateTranslateBtn.CurrentState) {
            case "translate":
                SetRotationAxis(Gizmo.Axis.NONE);
                break;
            case "rotate":
                SetRotationAxis(selectedAxis);
                break;
        }

        SetHandTeachingButtonInteractivity();
    }

    private void SetHandTeachingButtonInteractivity() {
        ActionObject ao = SceneManager.Instance.GetActionObject(SceneManager.Instance.SelectedRobot.GetId());
        bool success = ActionsManager.Instance.RobotsMeta.TryGetValue(ao.ActionObjectMetadata.Type, out IO.Swagger.Model.RobotMeta robotMeta);
        if(success)
            HandTeachingModeButton.SetInteractivity(robotMeta.Features.HandTeaching, "Robot does not support hand teaching mode");
        else
            HandTeachingModeButton.SetInteractivity(true); //actually this should never happen
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
        SetInteractivityOfRobotBtns(false, "Robot is already moving");
        IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.X;
        switch (selectedAxis) {
            case Gizmo.Axis.X:
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.X;
                break;
            case Gizmo.Axis.Y:
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.Y;
                break;
            case Gizmo.Axis.Z:
                axis = IO.Swagger.Model.StepRobotEefRequestArgs.AxisEnum.Z;
                break;
        }
        try {

            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            await WebsocketManager.Instance.StepRobotEef(axis, SceneManager.Instance.SelectedEndEffector.GetName(), safe, SceneManager.Instance.SelectedRobot.GetId(), GetSpeedSliderValue(),
            (decimal) step, translate ? IO.Swagger.Model.StepRobotEefRequestArgs.WhatEnum.Position : IO.Swagger.Model.StepRobotEefRequestArgs.WhatEnum.Orientation,
            world ? IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.World : IO.Swagger.Model.StepRobotEefRequestArgs.ModeEnum.Robot, armId);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
            SetInteractivityOfRobotBtns(true);
        }


    }

    private float GetPositionValue(float v) {
        switch (Units.GetValue()) {
            case "dm":
                return v * 0.1f;
            case "5cm":
                return v * 0.05f;
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


    private float GetRotationValue(float v) {
        switch (UnitsDegrees.GetValue()) {
            case "45°":
                return v * 45;
            case "10°":
                return v * 10;
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


    public void Hide(bool unlock = true) {
        if (unlock && SceneManager.Instance.IsRobotAndEESelected()) {
            SceneManager.Instance.SelectedRobot.WriteUnlock();
        }
        Sight.Instance.SelectedGizmoAxis -= OnSelectedGizmoAxis;
        if (gizmo != null)
            Destroy(gizmo.gameObject);
        closeCallback?.Invoke();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);        
    }

    public void SetRotationAxis(Gizmo.Axis axis) {
        if (RotateTranslateBtn.CurrentState == "translate") {
            gizmo?.SetRotationAxis(Gizmo.Axis.NONE);
        } else {
            gizmo?.SetRotationAxis(axis);
        }
    }

}

