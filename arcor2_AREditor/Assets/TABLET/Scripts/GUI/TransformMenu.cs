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

    private Vector3 offsetPosition = new Vector3(), interPosition = new Vector3(), cameraOrig = new Vector3();
    private Quaternion offsetRotation = new Quaternion(), interRotation = Quaternion.identity;

    public CanvasGroup CanvasGroup;

    private bool handHolding = false;


    private GameObject gizmo;


    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        if (model == null)
            return;
        if (RobotTabletBtn.CurrentState == "robot") {
            if (SceneManager.Instance.IsRobotAndEESelected()) {
                model.transform.position = SceneManager.Instance.SelectedEndEffector.transform.position;
                Coordinates.X.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.x);
                Coordinates.X.SetDeltaMeters(model.transform.position.x - InteractiveObject.transform.position.x);
                Coordinates.Y.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.y);
                Coordinates.Y.SetDeltaMeters(model.transform.position.y - InteractiveObject.transform.position.y);
                Coordinates.Z.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.z);
                Coordinates.Z.SetDeltaMeters(model.transform.position.z - InteractiveObject.transform.position.z);
                UpdateTranslate(GetPositionValue(TransformWheel.GetValue()));
                return;
            }
        }
        if (RotateTranslateBtn.CurrentState == "rotate") {
            UpdateRotate(GetRotationValue(TransformWheel.GetValue()));
        } else {
            UpdateTranslate(GetPositionValue(TransformWheel.GetValue()));
        }
        
        if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
            Vector3 position = TransformConvertor.ROSToUnity(interPosition + offsetPosition);
            model.transform.localPosition = position;
            
        } else if (InteractiveObject.GetType().IsSubclassOf(typeof(ActionObject))) {
            Vector3 position = TransformConvertor.ROSToUnity(interPosition + offsetPosition);
            model.transform.localPosition = position;
            Quaternion rotation = TransformConvertor.ROSToUnity(interRotation * offsetRotation);
            model.transform.localRotation = rotation;
        }
        
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

    private async void UpdateTranslate(float wheelValue) {
        
        if (handHolding) {
            Vector3 cameraNow = TransformConvertor.UnityToROS(InteractiveObject.transform.InverseTransformPoint(Camera.main.transform.position));
            offsetPosition.x = GetRoundedValue(cameraNow.x - cameraOrig.x);
            offsetPosition.y = GetRoundedValue(cameraNow.y - cameraOrig.y);
            offsetPosition.z = GetRoundedValue(cameraNow.z - cameraOrig.z);
                    
        } else {

            switch (Coordinates.GetSelectedAxis()) {
                case "x":
                    offsetPosition.x = wheelValue;
                    break;
                case "y":
                    offsetPosition.y = wheelValue;
                    break;
                case "z":
                    offsetPosition.z = wheelValue;
                    break;
            }
        }

        Vector3 newPosition = TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition);
        Coordinates.X.SetValueMeters(newPosition.x);
        Coordinates.X.SetDeltaMeters(offsetPosition.x + interPosition.x);
        Coordinates.Y.SetValueMeters(newPosition.y);
        Coordinates.Y.SetDeltaMeters(offsetPosition.y + interPosition.y);
        Coordinates.Z.SetValueMeters(newPosition.z);
        Coordinates.Z.SetDeltaMeters(offsetPosition.z + interPosition.z);
    }

    private void UpdateRotate(float wheelValue) {
        if (handHolding) {
            
        } else {

            switch (Coordinates.GetSelectedAxis()) {
                case "x":
                    offsetRotation = Quaternion.Euler(wheelValue, offsetRotation.eulerAngles.y, offsetRotation.eulerAngles.z);
                    break;
                case "y":
                    offsetRotation = Quaternion.Euler(offsetRotation.eulerAngles.x, wheelValue, offsetRotation.eulerAngles.z);
                    break;
                case "z":
                    offsetRotation = Quaternion.Euler(offsetRotation.eulerAngles.x, offsetRotation.eulerAngles.y, wheelValue);
                    break;
            }

        }
        Quaternion delta = Quaternion.identity;
        if (InteractiveObject.GetType() == typeof(ActionObject3D)) {
            delta = TransformConvertor.UnityToROS(model.transform.localRotation);
        }
       
        Quaternion newrotation = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.rotation * Quaternion.Inverse(model.transform.rotation));
        Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
        Coordinates.X.SetDeltaDegrees(delta.eulerAngles.x);
        Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
        Coordinates.Y.SetDeltaDegrees(delta.eulerAngles.y);
        Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
        Coordinates.Z.SetDeltaDegrees(delta.eulerAngles.z);
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
        TransformWheel.Units = Units;
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        ResetPosition();
    }

    public void SwitchToRotate() {
        TransformWheel.Units = UnitsDegrees;
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        ResetPosition();
    }

    public void SwitchToTablet() {
        TransformWheel.gameObject.SetActive(true);
        ResetPosition();
        Wheel.gameObject.SetActive(true);
        StepButtons.gameObject.SetActive(false);
        RotateTranslateBtn.SetInteractivity(true);
    }

    public void SwitchToRobot() {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Robot not ready", "Scene offline");
            return;
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Notifications.Instance.ShowNotification("Robot not ready", "Robot or EE not selected");
            return;
        }
        //TransformWheel.gameObject.SetActive(false);
        Wheel.gameObject.SetActive(false);
        StepButtons.gameObject.SetActive(true);
        ResetPosition();
        if (RotateTranslateBtn.CurrentState == "rotate") {
            RotateTranslateBtn.SetState("translate");
            SwitchToTranslate();
        }
        RotateTranslateBtn.SetInteractivity(false);
    }

    public void HoldPressed() {
        if (RobotTabletBtn.CurrentState == "tablet") {
            cameraOrig = TransformConvertor.UnityToROS(InteractiveObject.transform.InverseTransformPoint(Camera.main.transform.position));
            StoreInterPosition();
            handHolding = true;
        } else {
            WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true);
        }
    }

    public void HoldReleased() {
        if (RobotTabletBtn.CurrentState == "tablet") {
            handHolding = false;
            StoreInterPosition();
        }
        else
            WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false);
    }

    public void StoreInterPosition() {
        
        interPosition += offsetPosition;
        interRotation *= offsetRotation;
        offsetPosition = Vector3.zero;
        offsetRotation = Quaternion.identity;
    }

    public void Show(InteractiveObject interactiveObject) {
        InteractiveObject = interactiveObject;
        RobotTabletBtn.SetState("tablet");
        RotateTranslateBtn.SetState("translate");
        RobotTabletBtn.SetInteractivity(SceneManager.Instance.SceneStarted);
        RotateTranslateBtn.SetInteractivity(InteractiveObject.GetType() != typeof(ActionPoint3D));
        offsetPosition = Vector3.zero;
        ResetTransformWheel();
        SwitchToTranslate();
        SwitchToTablet();
        
        if (interactiveObject.GetType() == typeof(ActionPoint3D)) {
            model = ((ActionPoint3D) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(false);
            model.transform.SetParent(interactiveObject.transform);
            model.transform.rotation = GameManager.Instance.Scene.transform.rotation;
        } else if (interactiveObject.GetType() == typeof(ActionObject3D)) {
            model = ((ActionObject3D) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(true);
            model.transform.SetParent(interactiveObject.transform);
            model.transform.localRotation = Quaternion.identity;
        } else if (interactiveObject.GetType() == typeof(RobotActionObject)) {
            model = ((RobotActionObject) interactiveObject).GetModelCopy();
            RotateTranslateBtn.SetInteractivity(true);
            model.transform.SetParent(interactiveObject.transform);
            model.transform.localRotation = Quaternion.identity;
        }
        Debug.LogError(model);
        if (model == null) {
            Hide();
            return;
        }
        if (gizmo == null)
            gizmo = Instantiate(GameManager.Instance.GizmoPrefab, model.transform);
        else
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
        StoreInterPosition();
        /*if (handHolding)
            cameraOrig = TransformConvertor.UnityToROS(InteractiveObject.transform.InverseTransformPoint(Camera.main.transform.position));*/
        switch (Coordinates.GetSelectedAxis()) {
            case "x":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.x));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.x));
                break;
            case "y":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.y));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.y));
                break;
            case "z":
                if (RotateTranslateBtn.CurrentState == "rotate")
                    TransformWheel.InitList(ComputeRotationValue(offsetPosition.z));
                else
                    TransformWheel.InitList(ComputePositionValue(offsetPosition.z));
                break;
        }
    }

    public async void SubmitPosition() {
        if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
            try {               
                if (RobotTabletBtn.CurrentState == "tablet")
                    await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition)));
                else {
                    await WebsocketManager.Instance.UpdateActionPointUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId());
                }
                ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        } else if (InteractiveObject.GetType().IsSubclassOf(typeof(ActionObject))) {
            try {
                if (RobotTabletBtn.CurrentState == "tablet")
                    await WebsocketManager.Instance.UpdateActionObjectPose(InteractiveObject.GetId(), new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition)),
                                                                                                                                orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(InteractiveObject.transform.localRotation * model.transform.localRotation))));
                else {
                    await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetId(), IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Top);
                }
                ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        }
    }

    public void ResetPosition(bool manually = false) {
        offsetPosition = Vector3.zero;
        interPosition = Vector3.zero;
        offsetRotation = Quaternion.identity;
        interRotation = Quaternion.identity;
        //if (InteractiveObject.GetType() == typeof(ActionPoint3D)) {
        //    origRotation = TransformConvertor.UnityToROS(((ActionPoint3D) InteractiveObject).GetRotation());
        //}
        ResetTransformWheel();
    }
    /*if (RobotTabletBtn.CurrentState == "robot" && endEffector != null) {
            IO.Swagger.Model.Position position = endEffector.Position;
            if (prevValue != wheelValue) {
                switch (Coordinates.GetSelectedAxis()) {
                    case "x":
                        position.X += (decimal) wheelValue;
                        break;
                    case "y":
                        position.Y += (decimal) wheelValue;
                        break;
                    case "z":
                        position.Z += (decimal) wheelValue;
                        break;
                }

                //Vector3 p = TransformConvertor.ROSToUnity(interPosition + offsetPosition);
                //model.transform.localPosition = ((ActionPoint3D) InteractiveObject).GetRotation() * p;
                try {
                    await WebsocketManager.Instance.MoveToPose(robotId, endEffector.EEId, (decimal) 0.3, position, endEffector.Orientation, false);

                    prevValue = wheelValue;
                } catch (RequestFailedException ex) {
                    prevValue = float.MinValue;
                }
            }          
            
            return;
        }*/

    
}
