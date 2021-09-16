using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RosSharp.Urdf;
using TrilleonAutomation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using System.Threading.Tasks;
using UnityEngine.XR.LegacyInputHelpers;

[RequireComponent(typeof(CanvasGroup))]
public class TransformMenu : Singleton<TransformMenu> {

    public enum State {
        Translate,
        Rotate,
        Scale
    }

    public InteractiveObject InteractiveObject;
    public TransformWheel TransformWheel;
    public GameObject Wheel, StepButtons;
    //public CoordinatesBtnGroup Coordinates;
    public TranformWheelUnits Units, UnitsDegrees;
    private GameObject model;
    public TwoStatesToggleNew RobotTabletBtn;
    public ButtonWithTooltip RedoBtn, UndoBtn;
    private float prevValue;
    private Gizmo.Axis selectedAxis;
    public State CurrentState;
    private Vector3 origPosition = new Vector3();
    private List<TransformPoseAndScale> history = new List<TransformPoseAndScale>();
    public Transform GizmoTransform;

    private Vector3 origScale = new Vector3();
    public ButtonWithTooltip RotateBtn, ScaleBtn, HandBtn;

    private int historyIndex;


    public CanvasGroup CanvasGroup;

    private bool handHolding = false;

    public ToggleGroupIconButtons BottomButtons;


    private Gizmo gizmo;
    //private bool IsPositionChanged => model != null && (model.transform.localPosition != Vector3.zero || model.transform.localRotation != Quaternion.identity);
    private bool IsPositionChanged => model != null && ((model.transform.position - InteractiveObject.transform.position).magnitude > 0.001 || Quaternion.Angle(model.transform.rotation, InteractiveObject.transform.rotation) > 0.001);
    private bool IsSizeChanged => model != null && InteractiveObject != null && InteractiveObject is CollisionObject collisionObject && (model.transform.localScale != collisionObject.Model.transform.localScale);

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
        selectedAxis = Gizmo.Axis.X;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
        TransformWheel.List.MovementDone += TransformWheelMovementDone;
        TransformWheel.List.MovementStart += TransformWheelMovementStart;
    }

    private void TransformWheelMovementStart(object sender, EventArgs e) {
        RedoBtn.SetInteractivity(false);
    }

    private void TransformWheelMovementDone(object sender, EventArgs e) {
        SubmitPosition();
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (CanvasGroup.alpha < 1)
            return;
        if (args.Event.State == IO.Swagger.Model.SceneStateData.StateEnum.Started ||
            args.Event.State == IO.Swagger.Model.SceneStateData.StateEnum.Stopped)
            UpdateSceneStateRelatedStuff();
    }

    private void OnSelectedGizmoAxis(object sender, GizmoAxisEventArgs args) {
        SelectAxis(args.SelectedAxis);
    }

    private void SelectAxis(Gizmo.Axis axis, bool forceUpdate = false) {
        if (forceUpdate || (selectedAxis != axis && !handHolding && TransformWheel.List.Velocity.magnitude < 0.01f && !TransformWheel.List.Dragging)) {
            selectedAxis = axis;
            gizmo.HiglightAxis(axis);
            if (CurrentState == State.Rotate)
                SetRotationAxis(axis);
            ResetTransformWheel();
        }
    }

    private void Update() {
        //ResetButton.SetInteractivity(isPositionChanged || isSizeChanged);
        if (model == null)
            return;
        if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Left) {
            if (SceneManager.Instance.IsRobotAndEESelected()) {
                model.transform.position = SceneManager.Instance.SelectedEndEffector.transform.position;
                //Coordinates.X.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.x);
                gizmo.SetXDelta(model.transform.localPosition.x);
                //Coordinates.Y.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.y);
                gizmo.SetYDelta(model.transform.localPosition.y);
                //Coordinates.Z.SetValueMeters(SceneManager.Instance.SelectedEndEffector.transform.position.z);
                gizmo.SetZDelta(model.transform.localPosition.z);
                UpdateTranslate(GetPositionValue(TransformWheel.GetValue()));
                return;
            }
        }

        float newValue = 0;
        if (CurrentState == State.Rotate) {
            newValue = GetRotationValue(TransformWheel.GetValue());
            if (handHolding || prevValue != newValue)
                UpdateRotate(newValue - prevValue);

            Quaternion delta = TransformConvertor.UnityToROS(model.transform.localRotation);
            Quaternion newrotation = TransformConvertor.UnityToROS(model.transform.rotation * Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation));
            /*Coordinates.X.SetValueDegrees(newrotation.eulerAngles.x);
            Coordinates.X.SetDeltaDegrees(delta.eulerAngles.x);
            Coordinates.Y.SetValueDegrees(newrotation.eulerAngles.y);
            Coordinates.Y.SetDeltaDegrees(delta.eulerAngles.y);
            Coordinates.Z.SetValueDegrees(newrotation.eulerAngles.z);
            Coordinates.Z.SetDeltaDegrees(delta.eulerAngles.z);*/
            gizmo.SetXDeltaRotation(delta.eulerAngles.x);
            gizmo.SetYDeltaRotation(delta.eulerAngles.y);
            gizmo.SetZDeltaRotation(delta.eulerAngles.z);
        } else if (CurrentState == State.Translate) {
            newValue = GetPositionValue(TransformWheel.GetValue());
            if (handHolding || prevValue != newValue)
                UpdateTranslate(newValue - prevValue);

            //Coordinates.X.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).x);
  
            gizmo.SetXDelta(TransformConvertor.UnityToROS(model.transform.localPosition).x);
            //Coordinates.Y.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).y);
            gizmo.SetYDelta(TransformConvertor.UnityToROS(model.transform.localPosition).y);
            //Coordinates.Z.SetValueMeters(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position)).z);
            gizmo.SetZDelta(TransformConvertor.UnityToROS(model.transform.localPosition).z);
        } else {
            if (InteractiveObject is CollisionObject collisionObject) {
                newValue = GetPositionValue(TransformWheel.GetValue());
                if (newValue != prevValue)
                    UpdateScale(newValue - prevValue);
                Vector3 delta = TransformConvertor.UnityToROSScale(model.transform.localScale - origScale);
                gizmo.SetXDelta(delta.x);
                gizmo.SetYDelta(delta.y);
                gizmo.SetZDelta(delta.z);
            }
        }


        prevValue = newValue;

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
            model.transform.position = Camera.main.transform.TransformPoint(origPosition);
        } else {
            
            switch (selectedAxis) {
                case Gizmo.Axis.X:
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right));
                    break;
                case Gizmo.Axis.Y:
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up));
                    break;
                case Gizmo.Axis.Z:
                    model.transform.Translate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward));
                    break;
            }
        }
    }

    private void UpdateRotate(float wheelValue) {
        if (handHolding) {
            model.transform.position = Camera.main.transform.TransformPoint(origPosition);
        } else {
            switch (selectedAxis) {
                case Gizmo.Axis.X:
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.right));
                    break;
                case Gizmo.Axis.Y:
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.up));
                    break;
                case Gizmo.Axis.Z:
                    model.transform.Rotate(TransformConvertor.ROSToUnity(wheelValue * Vector3.forward));
                    break;
            }
        }
    }

    private void UpdateScale(float wheelValue) {
        if (InteractiveObject is CollisionObject collisionObject) {
            if (collisionObject.ActionObjectMetadata.ObjectModel.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Box) {
                switch (selectedAxis) {
                    case Gizmo.Axis.X:
                        model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * Vector3.right);
                        break;
                    case Gizmo.Axis.Y:
                        model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * Vector3.up);
                        break;
                    case Gizmo.Axis.Z:
                        model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * Vector3.forward);
                        break;
                }
            } else if (collisionObject.ActionObjectMetadata.ObjectModel.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Sphere) {
                model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * Vector3.one);
            } else if (collisionObject.ActionObjectMetadata.ObjectModel.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Cylinder) {
                switch (selectedAxis) {
                    case Gizmo.Axis.X:
                    case Gizmo.Axis.Y:
                        model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * new Vector3(1, 1, 0));
                        break;
                    case Gizmo.Axis.Z:
                        model.transform.localScale += TransformConvertor.ROSToUnityScale(wheelValue * Vector3.forward);
                        break;
                }
            }
        }
        
        NormalizeGizmoScale();
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
        ResetTransformWheel();
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        RobotTabletBtn.SetInteractivity(SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotAndEESelected());
        SetRotationAxis(Gizmo.Axis.NONE);
        CurrentState = State.Translate;
        HandBtn.SetInteractivity(true);
        BottomButtons.SelectButton(BottomButtons.Buttons[0], false);
    }

    public void SwitchToRotate() {
        TransformWheel.Units = UnitsDegrees;
        ResetTransformWheel();
        Units.gameObject.SetActive(false);
        UnitsDegrees.gameObject.SetActive(true);
        RobotTabletBtn.SetInteractivity(false);
        SetRotationAxis(selectedAxis);
        HandBtn.SetInteractivity(false);
        CurrentState = State.Rotate;
    }

    public void SwitchToScale() {
        TransformWheel.Units = Units;
        ResetTransformWheel();
        Units.gameObject.SetActive(true);
        UnitsDegrees.gameObject.SetActive(false);
        RobotTabletBtn.SetInteractivity(false);
        SetRotationAxis(Gizmo.Axis.NONE);

        HandBtn.SetInteractivity(false, "Not available for scaling");
        CurrentState = State.Scale;
    }

    public void SwitchToTablet() {
        TransformWheel.gameObject.SetActive(true);
        //ResetPosition();
        Wheel.gameObject.SetActive(true);
        if (InteractiveObject.GetType() != typeof(ActionPoint3D))
            RotateBtn.SetInteractivity(true);
    }

    public void SwitchToRobot() {
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Robot not ready", "Scene offline");
            RobotTabletBtn.SwitchToRight();
            return;
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            Notifications.Instance.ShowNotification("Robot not ready", "Robot or EE not selected");
            RobotTabletBtn.SwitchToRight();
            return;
        }
        TransformWheel.gameObject.SetActive(false);
        //ResetPosition();
        if (CurrentState == State.Rotate) {
            SwitchToTranslate();
        }
        RotateBtn.SetInteractivity(false, "Unable to rotate with robot");
    }

    public void HoldPressed() {
        RedoBtn.SetInteractivity(false);
        if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Right) {
            origPosition = Camera.main.transform.InverseTransformPoint(model.transform.position);
            handHolding = true;
        } else {
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            _ = WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: true, armId);
        }
        Debug.LogError("Hold pressed");
    }

    public void HoldReleased() {
        if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Right) {
            handHolding = false;
        } else {
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            _ = WebsocketManager.Instance.HandTeachingMode(robotId: SceneManager.Instance.SelectedRobot.GetId(), enable: false, armId);
        }
        Debug.LogError("Hold released");
        SubmitPosition(true);
    }


    public async Task<bool> Show(InteractiveObject interactiveObject) {
        InteractiveObject = interactiveObject;
        if (! await interactiveObject.WriteLock(true))
            return false;
        if (interactiveObject is CollisionObject co) {
            if (!await co.WriteLockObjectType())
                return false;
        } 
        RobotTabletBtn.SwitchToRight();
        ResetTransformWheel();
        SwitchToTranslate();
        history.Clear();
        historyIndex = -1;
        GizmoTransform.transform.position = interactiveObject.transform.position;
        GizmoTransform.transform.rotation = interactiveObject.transform.rotation;
        if (interactiveObject is CollisionObject collisionObject)
            origScale = collisionObject.Model.transform.localScale;
        if (interactiveObject is ActionPoint3D actionPoint) {
            model = actionPoint.GetModelCopy();
            RotateBtn.SetInteractivity(false, "Action point could not be rotated");
            ScaleBtn.SetInteractivity(false, "Action point size could not be changed");
            RobotTabletBtn.SetInteractivity(true);
            model.transform.SetParent(GizmoTransform);
            model.transform.rotation = Quaternion.identity;
            model.transform.position = interactiveObject.transform.position;

            Target target = model.AddComponent<Target>();
            target.SetTarget(Color.yellow, false, true, false);
            target.enabled = true;

            actionPoint.EnableOffscreenIndicator(false);
            actionPoint.EnableVisual(false);

        } else if (interactiveObject is ActionObject3D actionObject) {
            model = actionObject.GetModelCopy();
            RotateBtn.SetInteractivity(true);
            ScaleBtn.SetInteractivity(interactiveObject is CollisionObject, "Only collision objects size could be changed");
            RobotTabletBtn.SetInteractivity(true);
            model.transform.SetParent(GizmoTransform);
            model.transform.rotation = interactiveObject.transform.rotation;
            model.transform.position = interactiveObject.transform.position;

            Target target = model.AddComponent<Target>();
            target.SetTarget(Color.yellow, false, true, false);
            target.enabled = true;

            actionObject.EnableOffscreenIndicator(false);
            actionObject.EnableVisual(false);

        } else if (interactiveObject is RobotActionObject robot) {
            model = robot.GetModelCopy();
            RotateBtn.SetInteractivity(true);
            ScaleBtn.SetInteractivity(false, "Robot size could not be changed");
            RobotTabletBtn.SetInteractivity(false, "Robot position could not be set using robot");
            model.transform.SetParent(GizmoTransform);
            model.transform.rotation = interactiveObject.transform.rotation;
            model.transform.position = interactiveObject.transform.position;

            Target target = model.AddComponent<Target>();
            target.SetTarget(Color.yellow, false, true, false);
            target.enabled = true;

            robot.EnableOffscreenIndicator(false);
            robot.EnableVisual(false);
        } else if (interactiveObject is StartEndAction action) {
            model = action.GetModelCopy();
            RotateBtn.SetInteractivity(false);
            ScaleBtn.SetInteractivity(false, "Robot size could not be changed");
            RobotTabletBtn.SetInteractivity(false, "START / STOP position could not be set using robot");
            model.transform.SetParent(GizmoTransform);
            model.transform.rotation = interactiveObject.transform.rotation;
            model.transform.position = interactiveObject.transform.position;

            Target target = model.AddComponent<Target>();
            target.SetTarget(Color.yellow, false, true, false);
            target.enabled = true;

            action.EnableOffscreenIndicator(false);
            action.EnableVisual(false);
        }
        
        if (model == null) {
            Hide();
            return false;
        }
        UpdateSceneStateRelatedStuff();
        gizmo = Instantiate(GameManager.Instance.GizmoPrefab).GetComponent<Gizmo>();
        gizmo.transform.SetParent(model.transform);
        // 0.1 is default scale for our gizmo
        NormalizeGizmoScale();
        gizmo.transform.localPosition = Vector3.zero;
        gizmo.transform.localRotation = Quaternion.identity;
        gizmo.gameObject.SetActive(true);
        SaveHistory();
        Sight.Instance.SelectedGizmoAxis += OnSelectedGizmoAxis;
        SelectAxis(Gizmo.Axis.X, true);
        enabled = true;
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        UndoBtn.SetInteractivity(false);
        RedoBtn.SetInteractivity(false);
        switch (CurrentState) {
            case State.Translate:
            case State.Scale:
                SetRotationAxis(Gizmo.Axis.NONE);
                break;
            case State.Rotate:
                SetRotationAxis(selectedAxis);
                break;
        }

        RobotInfoMenu.Instance.Show();
        return true;
    }

    private void NormalizeGizmoScale() {
        gizmo.transform.localScale = new Vector3(0.1f / model.transform.localScale.x, 0.1f / model.transform.localScale.y, 0.1f / model.transform.localScale.z);
    }

    private void UpdateSceneStateRelatedStuff() {
        RobotTabletBtn.SetInteractivity(SceneManager.Instance.SceneStarted, "Robot could not be used offline");
        if (!SceneManager.Instance.SceneStarted) {
            if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Left) {
                SwitchToTablet();
            }
        }
    }


    public async void Hide(bool unlock = true) {
        if (!IsVisible())
            return;
        SubmitPosition(false);
        Sight.Instance.SelectedGizmoAxis -= OnSelectedGizmoAxis;
        if (unlock) {
            await InteractiveObject.WriteUnlock();
            if (InteractiveObject is CollisionObject co) {
                await co.WriteLockObjectType();
            }
        }

        InteractiveObject.EnableOffscreenIndicator(true);
        InteractiveObject.EnableVisual(true);


        if (gizmo != null) {
            Destroy(gizmo.gameObject);
            gizmo = null;
        }

        InteractiveObject = null;
        Destroy(model);
        model = null;
        enabled = false;


        EditorHelper.EnableCanvasGroup(CanvasGroup, false);

        RobotInfoMenu.Instance.Hide();
    }

    public void ResetTransformWheel() {
        prevValue = 0;
        TransformWheel.InitList(0);
    }

    public void SetRotationAxis(Gizmo.Axis axis) {
        switch (CurrentState) {
            case State.Translate:
            case State.Scale:
            gizmo?.SetRotationAxis(Gizmo.Axis.NONE);
                break;
            case State.Rotate:
            gizmo?.SetRotationAxis(axis);
                break;
        }
    }

    public void Undo() {
        if (history.Count == 0 || historyIndex == 0)
            return;
        if (historyIndex < 0) {
            if (IsPositionChanged) {
                TransformWheel.List.Stop();
                SubmitPosition(true);
            }
            historyIndex = history.Count - 1;
        }
        
        historyIndex--;
        SetModelToHistoryPosition(historyIndex);

        SubmitPosition(false);
        RedoBtn.SetInteractivity(true);
    }

    private void SetModelToHistoryPosition(int index) {
        model.transform.localPosition = history[index].Position;
        if (!(InteractiveObject is ActionPoint))
            model.transform.localRotation = history[index].Rotation;
        if (InteractiveObject is CollisionObject)
            model.transform.localScale = history[index].Scale;
        BottomButtons.SelectButton(history[historyIndex].BottomMenuIndex, true);
        NormalizeGizmoScale();
    }

    public void Redo() {
        Debug.Assert(historyIndex >= 0 && historyIndex < history.Count - 1);
        historyIndex++;
        RedoBtn.SetInteractivity(historyIndex < history.Count - 1);
        SetModelToHistoryPosition(historyIndex);
        SubmitPosition(false);

    }

    private void SaveHistory() {
        Debug.Assert(history.Count == 0 || historyIndex < history.Count - 1);
        if (historyIndex >= 0) {
            history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
            historyIndex = -1;
        }
        history.Add(new TransformPoseAndScale(
            model.transform.localPosition,
            model.transform.localRotation,
            model.transform.localScale,
            BottomButtons.GetSelectedIndex()));
        historyIndex = -1;
        RedoBtn.SetInteractivity(false);
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0 && InteractiveObject != null;
    }

    public async void SubmitPosition(bool saveHistory = true) {
        if (!IsVisible())
            return;
        if (saveHistory) {
            SaveHistory();
        }
        UndoBtn.SetInteractivity(history.Count > 1 && historyIndex != 0);
        if (InteractiveObject is ActionPoint3D actionPoint) {
            try {
                if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Right) {
                    //await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.localPosition + model.transform.localPosition)));
                    await WebsocketManager.Instance.UpdateActionPointPosition(InteractiveObject.GetId(), DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(InteractiveObject.transform.parent.InverseTransformPoint(model.transform.position))));
                } else {
                    IRobot robot = SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
                    string armId = null;
                    if (robot.MultiArm())
                        armId = SceneManager.Instance.SelectedArmId;
                    await WebsocketManager.Instance.UpdateActionPointUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), armId);
                }
                //ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action point position", e.Message);
            }
        } else if (InteractiveObject is ActionObject actionObject) {
            if (IsSizeChanged) {
                try {
                    IO.Swagger.Model.ObjectModel objectModel = actionObject.ActionObjectMetadata.ObjectModel;
                    Vector3 transformedScale = TransformConvertor.UnityToROSScale(model.transform.localScale);

                    switch (objectModel.Type) {
                        case IO.Swagger.Model.ObjectModel.TypeEnum.Box:
                            objectModel.Box.SizeX = (decimal) transformedScale.x;
                            objectModel.Box.SizeY = (decimal) transformedScale.y;
                            objectModel.Box.SizeZ = (decimal) transformedScale.z;
                            break;
                        case IO.Swagger.Model.ObjectModel.TypeEnum.Cylinder:
                            objectModel.Cylinder.Radius = (decimal) transformedScale.x;
                            objectModel.Cylinder.Height = (decimal) transformedScale.z;
                            break;
                        case IO.Swagger.Model.ObjectModel.TypeEnum.Sphere:
                            objectModel.Sphere.Radius = (decimal) transformedScale.x;
                            break;
                    }
                    await WebsocketManager.Instance.UpdateObjectModel(actionObject.ActionObjectMetadata.Type, objectModel);
                } catch (RequestFailedException e) {
                    Notifications.Instance.ShowNotification("Failed to update size of collision object", e.Message);
                }
            }
            
            try {
                if (RobotTabletBtn.CurrentState == TwoStatesToggleNew.States.Right)
                    await WebsocketManager.Instance.UpdateActionObjectPose(InteractiveObject.GetId(), new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(model.transform.position) /*InteractiveObject.transform.localPosition + model.transform.localPosition*/)),
                                                                                                                                orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(Quaternion.Inverse(GameManager.Instance.Scene.transform.rotation) * model.transform.rotation   /*InteractiveObject.transform.localRotation * model.transform.localRotation*/))));
                else {
                    IRobot robot = SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
                    string armId = null;
                    if (robot.MultiArm())
                        armId = SceneManager.Instance.SelectedArmId;
                    await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(InteractiveObject.GetId(), SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs.PivotEnum.Top, armId);
                }
                //ResetPosition();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action object position", e.Message);
            }
            
        } else if (InteractiveObject is StartEndAction startEndAction) {
            startEndAction.transform.position = model.transform.position;
            startEndAction.SavePosition();
        }

        
    }

    public void ResetPosition(bool manually = false) {
        if (model == null)
            return;
        if (InteractiveObject is CollisionObject collisionObject) {
            model.transform.localScale = collisionObject.Model.transform.localScale;
            NormalizeGizmoScale();
        }
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        ResetTransformWheel();
    }
}

public class TransformPoseAndScale {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public int BottomMenuIndex; //TODO

    public TransformPoseAndScale(Vector3 position, Quaternion rotation, Vector3 scale, int bottomMenuIndex) {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        BottomMenuIndex = bottomMenuIndex;
    }
}
