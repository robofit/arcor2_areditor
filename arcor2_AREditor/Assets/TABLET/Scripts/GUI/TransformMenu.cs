using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RosSharp.Urdf;
using TrilleonAutomation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;

[RequireComponent(typeof(CanvasGroup))]
public class TransformMenu : Singleton<TransformMenu> {
    public InteractiveObject InteractiveObject;
    public TransformWheel TransformWheel;
    public GameObject Wheel, StepButtons;
    public CoordinatesBtnGroup Coordinates;
    public TranformWheelUnits Units, UnitsDegrees;
    private GameObject model;
    public TwoStatesToggle RobotTabletBtn, RotateTranslateBtn;
    private float prevValue;

    private Vector3 cameraPrev = new Vector3();
    

    public CanvasGroup CanvasGroup;

    private bool handHolding = false;


    private GameObject gizmo;


    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();

        Coordinates.X.Select();
    }

    private void Update() {
        if (model == null)
            return;
        if (RobotTabletBtn.CurrentState == "robot") {
            if (SceneManager.Instance.IsRobotAndEESelected()) {
                model.transform.position = SceneManager.Instance.SelectedEndEffector.transform.position;
                Coordinates.X.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.x);
                Coordinates.X.SetDeltaMeters(model.transform.localPosition.x);
                Coordinates.Y.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.y);
                Coordinates.Y.SetDeltaMeters(model.transform.localPosition.y);
                Coordinates.Z.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.z);
                Coordinates.Z.SetDeltaMeters(model.transform.localPosition.z);
                UpdateTranslate(GetPositionValue(TransformWheel.GetValue()));
                return;
            }
        }

        float newValue = 0;
        if (RotateTranslateBtn.CurrentState == "rotate") {
            newValue = GetRotationValue(TransformWheel.GetValue());
            if (prevValue != newValue)
                UpdateRotate(newValue - prevValue);

            Quaternion delta = TransformConvertor.UnityToROS(model.transform.localRotation);
            Quaternion newrotation = TransformConvertor.UnityToROS(model.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation));
            Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
            Coordinates.X.SetDeltaDegrees(delta.eulerAngles.x);
            Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
            Coordinates.Y.SetDeltaDegrees(delta.eulerAngles.y);
            Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
            Coordinates.Z.SetDeltaDegrees(delta.eulerAngles.z);
        } else {
            newValue = GetPositionValue(TransformWheel.GetValue());
            if (handHolding || prevValue != newValue)
                UpdateTranslate(newValue - prevValue);

            Coordinates.X.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).x);
            Coordinates.X.SetDeltaMeters(TransformConvertor.UnityToROS(model.transform.localPosition).x);
            Coordinates.Y.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).y);
            Coordinates.Y.SetDeltaMeters(TransformConvertor.UnityToROS(model.transform.localPosition).y);
            Coordinates.Z.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).z);
            Coordinates.Z.SetDeltaMeters(TransformConvertor.UnityToROS(model.transform.localPosition).z);
        }


        prevValue = newValue;

    }

    private float GetPositionValue(float v) {
        switch (Units.GetValue()) {
            case "dm":
                return v * 0.1f;
            case "5cm":
                return v * 0.2f;
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
            case "5cm":
                return (int) (value * 20);
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

    private int ComputeRotationValue(float value) {
        switch (UnitsDegrees.GetValue()) {
            case "45°":
                return (int) (value / 45);
            case "10°":
                return (int) (value / 10);
            case "°":
                return (int) value;
            case "'":
                return (int) (value * 60);
            default:
                return (int) value;
        };
    }

    private void UpdateTranslate(float wheelValue) {
        if (model == null)
            return;

        if (handHolding) {
            Vector3 cameraNow = Camera.main.transform.position;
            model.transform.position += new Vector3(cameraNow.x - cameraPrev.x, cameraNow.y - cameraPrev.y, cameraNow.z - cameraPrev.z);
            cameraPrev = cameraNow;
        } else {
            
            switch (Coordinates.GetSelectedAxis()) {
                case "x":
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right));
                    break;
                case "y":
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up));
                    break;
                case "z":
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward));
                    break;
            }
        }
        
    }

    private void UpdateRotate(float wheelValue) {
        if (handHolding) {
            
        } else {

            switch (Coordinates.GetSelectedAxis()) {
                case "x":
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right));
                    break;
                case "y":
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up));
                    break;
                case "z":
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward));
                    break;
            }

        }

        
    }

    public float GetRoundedValue(float value) {
        switch (Units.GetValue()) {
            case "cm":
                return Mathf.Floor(value * 100) / 100f;
            case "mm":
                return Mathf.Floor(value * 1000) / 1000;
            case "μm":
                return Mathf.Floor(value * 1000000) / 1000000;
            default:
                return Mathf.Floor(value);
        };
    }

    public void SwitchToTranslate() {
        ResetTransformWheel();
        TransformWheel.Units = Units;
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        RotateTranslateBtn.SetDescription("Swith to rotate");

        //ResetPosition();
    }

    public void SwitchToRotate() {
        ResetTransformWheel();
        TransformWheel.Units = UnitsDegrees;
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        //ResetPosition();
        RotateTranslateBtn.SetDescription("Swith to translate");
    }

    public void SwitchToTablet() {
        TransformWheel.gameObject.SetActive(true);
        ResetPosition();
        Wheel.gameObject.SetActive(true);
        if (InteractiveObject.GetType() != typeof(ActionPoint3D))
            RotateTranslateBtn.SetInteractivity(true);
        RobotTabletBtn.SetDescription("Switch to robot control");
    }

    public void SwitchToRobot() {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Robot not ready", "Scene offline");
            RobotTabletBtn.SetState("tablet");
            return;
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Notifications.Instance.ShowNotification("Robot not ready", "Robot or EE not selected");
            RobotTabletBtn.SetState("tablet");
            return;
        }
        TransformWheel.gameObject.SetActive(false);
        //Wheel.gameObject.SetActive(false);
        //StepButtons.gameObject.SetActive(true);
        ResetPosition();
        if (RotateTranslateBtn.CurrentState == "rotate") {
            RotateTranslateBtn.SetState("translate");
            SwitchToTranslate();
        }
        RotateTranslateBtn.SetInteractivity(false);
        RotateTranslateBtn.SetInteractivity(false, "Unable to rotate with robot");
        RobotTabletBtn.SetDescription("Switch to tablet control");
    }

    public void HoldPressed() {
        if (RobotTabletBtn.CurrentState == "tablet") {
            cameraPrev = Camera.main.transform.position;
            handHolding = true;
        } else {
            WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true);
        }
    }

    public void HoldReleased() {
        if (RobotTabletBtn.CurrentState == "tablet") {
            handHolding = false;
        }
        else
            WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false);
    }

    

    public void Show(InteractiveObject interactiveObject) {
        InteractiveObject = interactiveObject;
        RobotTabletBtn.SetState("tablet");
        RotateTranslateBtn.SetState("translate");
        /*RobotTabletBtn.SetInteractivity(SceneManager.Instance.SceneStarted);
        robotTabletBtnTooltip.SetInteractivity(SceneManager.Instance.SceneStarted, "Scene offline");
        RotateTranslateBtn.SetInteractivity(InteractiveObject.GetType() != typeof(ActionPoint3D));
        rotateTranslateBtnTooltip.SetInteractivity(InteractiveObject.GetType() != typeof(ActionPoint3D), "Action point could not be translated");
        */
        //offsetPosition = Vector3.zero;
        ResetTransformWheel();
        SwitchToTranslate();
        SwitchToTablet();
        
        if (interactiveObject.GetType() == typeof(ActionPoint3D)) {
            model = ((ActionPoint3D) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(false, "Action point could not be rotated");
            RobotTabletBtn.SetInteractivity(true);
            model.transform.SetParent(interactiveObject.transform);
            model.transform.rotation = GameManager.Instance.Scene.transform.rotation;
            model.transform.localPosition = Vector3.zero;
        } else if (interactiveObject.GetType() == typeof(ActionObject3D)) {
            model = ((ActionObject3D) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(true);
            RobotTabletBtn.SetInteractivity(true);
            model.transform.SetParent(interactiveObject.transform);
            model.transform.localRotation = Quaternion.identity;
            model.transform.localPosition = Vector3.zero;
        } else if (interactiveObject.GetType() == typeof(RobotActionObject)) {
            model = ((RobotActionObject) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(true);
            RobotTabletBtn.SetInteractivity(false, "Robot position could not be set using robot");
            model.transform.SetParent(interactiveObject.transform);
            model.transform.localRotation = Quaternion.identity;
            model.transform.localPosition = Vector3.zero;
        }

        if (model == null) {
            Hide();
            return;
        }
        if (gizmo != null)
            Destroy(gizmo);

        gizmo = Instantiate(GameManager.Instance.GizmoPrefab);
        gizmo.transform.SetParent(model.transform);
        gizmo.transform.localPosition = Vector3.zero;
        gizmo.transform.localRotation = Quaternion.identity;
        gizmo.SetActive(true);
        enabled = true;
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void Hide() {
        InteractiveObject = null;
        Destroy(model);
        model = null;
        enabled = false;
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);        
    }

    public void ResetTransformWheel() {
        prevValue = 0;
        TransformWheel.InitList(0);
    }

    public async void SubmitPosition() {
        if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
            try {               
                if (RobotTabletBtn.CurrentState == "tablet")
                    await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition)));
                else {
                    await WebsocketManager.Instance.UpdateActionPointUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName());
                }
                ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        } else if (InteractiveObject.GetType().IsSubclassOf(typeof(ActionObject))) {
            try {
                if (RobotTabletBtn.CurrentState == "tablet")
                    await WebsocketManager.Instance.UpdateActionObjectPose(InteractiveObject.GetId(), new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position) /*InteractiveObject.transform.localPosition + model.transform.localPosition*/)),
                                                                                                                                orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(model.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation)   /*InteractiveObject.transform.localRotation * model.transform.localRotation*/))));
                else {
                    await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Top);
                }
                ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        }
    }

    public void ResetPosition(bool manually = false) {
        if (model == null)
            return;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        ResetTransformWheel();
    }
        
}
