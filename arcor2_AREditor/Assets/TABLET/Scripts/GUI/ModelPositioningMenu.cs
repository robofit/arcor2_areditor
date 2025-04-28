/*
 * ModelPositioningMenu
 * Author: Timotej Halenár
 * Login: xhalen00
 * Bachelor's Thesis
 * VUT FIT 2024
 *
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Joint = Arcor2.ClientSdk.Communication.OpenApi.Models.Joint;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class ModelPositioningMenu : RightMenu<ModelPositioningMenu> {
    private RobotActionObject robot;
    private RobotEE endEffector;

    private Gizmo gizmo;
    public GameObject GizmoPrefab;
    public GameObject SceneOrigin;

    private GameObject pointInstance;
    private DraggablePoint draggablePoint;
    public GameObject PointPrefab;

    public GameObject DistanceControl;
    public GameObject LeftMenu;
    public GameObject LeftMenuScene;
    public GameObject LeftMenuProject;
    public GameObject OnlineButton;
    public GameObject SelectionText;
    public GameObject ButtonHintText;
    public GameObject ExitDialog;
    public GameObject ConfirmPoseDialog;
    public GameObject ConfirmButtonDisable;
    public GameObject RevertButtonDisable;
    public GameObject CoordsToggle;
    public GameObject CoordsToggleDisable;
    public GameObject LockButton;
    public GameObject ImpossiblePoseNotification;
    public Slider SensitivitySlider;
    public Slider UpDownSensitivitySlider;
    public Button SelectButton;
    public GameObject XYPlaneMesh;
    public GameObject XZPlaneMesh;
    public GameObject YZPlaneMesh;
    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;
    public Material ClippingMaterial;

    private Vector3 OrigPlaneScale;
    private Vector3 HiddenPlaneScale = Vector3.zero;

    private Vector3 OrigAxisScale;
    private Vector3 ActiveAxisScale;
    private Vector3 HiddenAxisScale;

    private float pointDistance = 0.5f;
    private float DragMultiplier = 1.0f * 0.03f;
    private float ForwardBackwardMultiplier = 0.003f;
    private Vector3 fallbackEEPosition;
    private Vector3 originalPointPosition;
    private Vector3 originalRayPoint;

    private Vector3 pointPosition;

    private Dictionary<string, Vector3> pointPositions = new();

    private Vector3 lastValidPosition;

    private Vector3 forwardBackwardAdjust = Vector3.zero;

    private GameObject dummy;
    private GameObject lastValidTransform;

    private bool transformSet = false;

    private enum Selection {
        none,
        x,
        y,
        z,
        ee,
        XY,
        XZ,
        YZ
    }

    private enum ShaderEnabled {
        standard,
        clipping,
        invalid
    }

    private ShaderEnabled shaderEnabled = ShaderEnabled.standard;

    private Selection selection = Selection.none;

    private bool isMoving = false;
    private bool moveForwardHeld = false;
    private bool moveBackwardHeld = false;

    private bool isWaiting = false;
    private bool cameraCoord = false;
    private EventHandler<RobotEndEffectorUpdatedEventArgs> onRobotEndEffectorUpdated;
    private EventHandler<RobotJointsUpdatedEventArgs> onRobotJointsUpdated;

    private void Start() {
        SensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        UpDownSensitivitySlider.onValueChanged.AddListener(UpdateUpDownSensitivity);
        LockButton.GetComponent<LockButton>().OnUnlockedEvent += OnUnlockUserSystem;
        LockButton.GetComponent<LockButton>().OnLockedEvent += OnLockUserSystem;
    }

    public async Task TurnOn() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor) {
            LeftMenu = LeftMenuScene;
        } else if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor) {
            LeftMenu = LeftMenuProject;
        }
        robot = (RobotActionObject) SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
        robot.SetVisibility(1.0f);
        robot.GetComponent<OutlineOnClick>().UnHighlight();
        robot.GetComponent<OutlineOnClick>().Enabled = false;
        await robot.DisableVisualisationOfEE();

        dummy = new GameObject();

        CommunicationManager.Instance.Client.RobotEndEffectorUpdated -= onRobotEndEffectorUpdated;
        CommunicationManager.Instance.Client.RobotJointsUpdated -= onRobotJointsUpdated;

        List<string> EEIDs = await robot.GetEndEffectorIds();
        string EEID = EEIDs[0];
        List<string> armsIDs = await robot.GetArmsIds();
        string armID = armsIDs[0];

        endEffector = await robot.GetEE(EEID, armID);

        fallbackEEPosition = endEffector.transform.position;

        if (pointPositions.TryGetValue(robot.GetId(), out Vector3 outValue)) {
            pointPosition = outValue;
        } else {
            pointPosition = Vector3.zero;
        }

        if (endEffector.transform.position == pointPosition || pointPosition == Vector3.zero) {
            ConfirmPoseDialog.SetActive(false);
        } else {
            ConfirmPoseDialog.SetActive(true);
        }

        lastValidTransform = new GameObject();

        if (pointPosition != Vector3.zero) {
            pointInstance = Instantiate(PointPrefab, pointPosition, Quaternion.identity, SceneOrigin.transform);
            MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position), lastValidTransform.transform);

        } else {
            pointInstance = Instantiate(PointPrefab, endEffector.transform.position, Quaternion.identity, SceneOrigin.transform);
        }

        
        lastValidTransform.transform.position = pointInstance.transform.position;
        transformSet = true;

        draggablePoint = pointInstance.GetComponent<DraggablePoint>();

        gizmo = Instantiate(GizmoPrefab).GetComponent<Gizmo>();
        gizmo.transform.rotation = Quaternion.identity;
        gizmo.transform.SetParent(pointInstance.transform);
        gizmo.transform.localPosition = Vector3.zero;

        XYPlaneMesh = gizmo.GetComponent<GizmoVariant>().XYPlaneMesh;
        XZPlaneMesh = gizmo.GetComponent<GizmoVariant>().XZPlaneMesh;
        YZPlaneMesh = gizmo.GetComponent<GizmoVariant>().YZPlaneMesh;

        XAxis = gizmo.GetComponent<GizmoVariant>().XAxis;
        YAxis = gizmo.GetComponent<GizmoVariant>().YAxis;
        ZAxis = gizmo.GetComponent<GizmoVariant>().ZAxis;

        OrigPlaneScale = XYPlaneMesh.transform.localScale;

        OrigAxisScale = XAxis.transform.localScale;
        ActiveAxisScale = XAxis.transform.localScale;
        ActiveAxisScale.z = 10f;

        HiddenAxisScale = XAxis.transform.localScale;
        HiddenAxisScale.z = 0.0f;

        ImpossiblePoseNotify(false, true);
    }

    

    public async Task TurnOff(bool reset = false) {
        transformSet = false;

        CoordsToggle.GetComponent<TwoStatesToggleNew>().SwitchToLeft();
        LockButton.SetActive(false);

        pointPositions[robot.GetId()] = lastValidTransform.transform.position;

        SceneManager.Instance?.GetActionObject(SceneManager.Instance.SelectedRobot.GetId()).SetVisibility(0.0f);

        robot.GetComponent<OutlineOnClick>().Enabled = true;

        await robot.EnableVisualisationOfEE();

        Destroy(gizmo);
        Destroy(pointInstance);
        Destroy(dummy);
        Destroy(lastValidTransform);

        CommunicationManager.Instance.Client.RobotEndEffectorUpdated += onRobotEndEffectorUpdated;
        CommunicationManager.Instance.Client.RobotJointsUpdated += onRobotJointsUpdated;
        gameObject.SetActive(false);
    }

    private void Update() {
        //user-space gizmo rotation
        //https://discussions.unity.com/t/lookat-to-only-rotate-on-y-axis-how/10895/3
        if (cameraCoord && !isMoving) {
            Vector3 targetPosition = new(
                Camera.main.transform.position.x,
                pointInstance.transform.position.y,
                Camera.main.transform.position.z);
            pointInstance.transform.LookAt(targetPosition);
        }

        //gizmo flipping
        Vector3 localCamPos = Camera.main.transform.position;
        localCamPos = pointInstance.transform.InverseTransformPoint(localCamPos);

        if (!isMoving && !cameraCoord) {
            if (localCamPos.x > 0) {
                gizmo.GetComponent<GizmoVariant>().FlipX(true);
            } else {
                gizmo.GetComponent<GizmoVariant>().FlipX(false);
            }

            if (localCamPos.z < 0) {
                gizmo.GetComponent<GizmoVariant>().FlipZ(true);
            } else {
                gizmo.GetComponent<GizmoVariant>().FlipZ(false);
            }

            if (localCamPos.y < 0) {
                gizmo.GetComponent<GizmoVariant>().FlipY(true);
            } else {
                gizmo.GetComponent<GizmoVariant>().FlipY(false);
            }
        }

        //clipping plane flipping
        if (selection == Selection.XY) {
            if (localCamPos.z > 0) {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(0f, 90f + pointInstance.transform.rotation.eulerAngles.y, 90f);
            } else {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(0f, 90f + pointInstance.transform.rotation.eulerAngles.y, -90f);
            }
        }
        if (selection == Selection.YZ) {
            if (localCamPos.x > 0) {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(90f, 90f + pointInstance.transform.rotation.eulerAngles.y, 0f);
            } else {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(-90f, 90f + pointInstance.transform.rotation.eulerAngles.y, 0f);
            }
        }
        if (selection == Selection.XZ) {
            if (localCamPos.y > 0) {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            } else {
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            }
        }

        //movement
        if (isMoving && pointInstance != null && dummy != null) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

            Vector3 rayPoint = ray.GetPoint(pointDistance);

            //Forward/Backward adjust
            if (moveForwardHeld) {
                Vector3 dir = pointInstance.transform.position - Camera.main.transform.position;
                dir = Vector3.Normalize(dir);
                forwardBackwardAdjust.x += dir.x * ForwardBackwardMultiplier;
                forwardBackwardAdjust.z += dir.z * ForwardBackwardMultiplier;
            }
            if (moveBackwardHeld) {
                Vector3 dir = Camera.main.transform.position - pointInstance.transform.position;
                dir = Vector3.Normalize(dir);
                forwardBackwardAdjust.x += dir.x * ForwardBackwardMultiplier;
                forwardBackwardAdjust.z += dir.z * ForwardBackwardMultiplier;
            }
            rayPoint.x += forwardBackwardAdjust.x;
            rayPoint.z += forwardBackwardAdjust.z;

            dummy.transform.position = rayPoint;

            Vector3 difference = rayPoint - originalRayPoint;

            difference = pointInstance.transform.InverseTransformVector(difference);

            //Debug: draw difference vector
            //draggablePoint.GetComponent<LineRenderer>().SetPosition(0, rayPoint);
            //draggablePoint.GetComponent<LineRenderer>().SetPosition(1, originalRayPoint);

            dummy.transform.position = originalPointPosition;
            dummy.transform.rotation = pointInstance.transform.rotation;

            if (selection == Selection.ee) {
                dummy.transform.Translate(difference * DragMultiplier);

            } else if (selection == Selection.x) {
                dummy.transform.Translate(0f, 0f, difference.z * DragMultiplier);

            } else if (selection == Selection.y) {
                dummy.transform.Translate(difference.x * DragMultiplier, 0f, 0f);

            } else if (selection == Selection.z) {
                dummy.transform.Translate(0f, difference.y * DragMultiplier, 0f);

            } else if (selection == Selection.XY) {
                dummy.transform.Translate(0f, difference.y * DragMultiplier, difference.z * DragMultiplier);

            } else if (selection == Selection.XZ) {
                dummy.transform.Translate(difference.x * DragMultiplier, 0f, difference.z * DragMultiplier);

            } else if (selection == Selection.YZ) {
                dummy.transform.Translate(difference.x * DragMultiplier, difference.y * DragMultiplier, 0f);
            }

            pointInstance.transform.position = dummy.transform.position;

            if (!isWaiting) {
                isWaiting = true;
                MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position), dummy.transform);
            }

        } 
            
    }
    private void FixedUpdate() {
        //Axis selection
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
        if (hits.Length > 0) {
            foreach (RaycastHit hit in hits) {
                if (hit.collider.gameObject.CompareTag("draggable_point")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.ee);
                    break;
                }

                else if (hit.collider.gameObject.CompareTag("xy_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.XY);
                    return;
                }

                else if (hit.collider.gameObject.CompareTag("xz_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.XZ);
                    return;
                }

                else if (hit.collider.gameObject.CompareTag("yz_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.YZ);
                    return;
                }

                else if (hit.collider.gameObject.CompareTag("gizmo_x")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.x);
                    return;
                }

                else if (hit.collider.gameObject.CompareTag("gizmo_y")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.y);
                    return;
                }

                else if (hit.collider.gameObject.CompareTag("gizmo_z")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }

                    Select(Selection.z);
                    return;
                }
            }
        }

    }


    public void ToggleMove(bool forceStop = false) {
        isMoving = !isMoving;

        if (forceStop) {
            isMoving = false;
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        if (isMoving) {
            originalPointPosition = pointInstance.transform.position;
            originalRayPoint = ray.GetPoint(pointDistance);

            OnMove();
        } else {
            OnStopMove();
        }
    }

    private void OnMove() {
        CoordsToggleDisable.SetActive(true);
        ConfirmPoseDialog.SetActive(true);
        ConfirmButtonDisable.SetActive(true);
        RevertButtonDisable.SetActive(true);
        forwardBackwardAdjust = Vector3.zero;
        LeftMenu.SetActive(false);
        OnlineButton.SetActive(false);
        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";

        StopAllCoroutines();
        StartCoroutines(selection);

        DistanceControl.SetActive(true);

        gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
        gizmo.gameObject.GetComponent<GizmoVariant>().HideXCone();
        gizmo.gameObject.GetComponent<GizmoVariant>().HideYCone();
        gizmo.gameObject.GetComponent<GizmoVariant>().HideZCone();
    }
    private void OnStopMove() {
        OnlineButton.SetActive(true);
        CoordsToggleDisable.SetActive(false);

        if (ImpossiblePoseNotification.active == true) {
            ImpossiblePoseNotify(false, true);

            pointInstance.transform.position = lastValidTransform.transform.position;
            MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position), lastValidTransform.transform);

        } else {
            lastValidTransform.transform.position = pointInstance.transform.position;
        }

        //Makes me angry that this has to be here, but the notification won't disappear otherwise

        //ImpossiblePoseNotify should take care of this, but it just disables the red material and
        //leaves the notification active for some reason
        ImpossiblePoseNotification.SetActive(false);

        ConfirmButtonDisable.SetActive(false);
        RevertButtonDisable.SetActive(false);

        LeftMenu.SetActive(true);
        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
        gizmo.GetComponent<GizmoVariant>().UnhighlightAll();

        StopAllCoroutines();
        ShowAllAxisAndPlanes();

        DistanceControl.SetActive(false);

        EnableStandardShader();
    }

    private void Select(Selection value) {
        if (isMoving) {
            return;
        }

        gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
        gizmo.UnhighlightAllAxis();
        draggablePoint.Unhighlight();

        selection = value;

        if (value == Selection.x || value == Selection.y || value == Selection.z) {
            SelectionText.GetComponent<TextMeshProUGUI>().text = value.ToString().ToUpper() + " Axis";
            if (value == Selection.x) {
                gizmo.HiglightAxis(Gizmo.Axis.X);
            } else if (value == Selection.y) {
                gizmo.HiglightAxis(Gizmo.Axis.Y);
            } else if (value == Selection.z) {
                gizmo.HiglightAxis(Gizmo.Axis.Z);
            }

        } else if (value == Selection.XY || value == Selection.XZ || value == Selection.YZ) {
            SelectionText.GetComponent<TextMeshProUGUI>().text = value.ToString() + " Plane";
            if (value == Selection.XY) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXY();
            } else if (value == Selection.XZ) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXZ();
            } else if (value == Selection.YZ) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightYZ();
            }
        } else if (value == Selection.ee) {
            SelectionText.GetComponent<TextMeshProUGUI>().text = "End-Effector";
            draggablePoint.Highlight();
        }
    }

    private async void MoveHereModel(Vector3 position, Transform dummyTransform, bool avoid_collision = true) {
        List<Joint> modelJoints; //joints to move the model to

        Orientation orientation = new(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        try {
            Pose pose = new(orientation: orientation, position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position)));
            List<Joint> startJoints = SceneManager.Instance.SelectedRobot.GetJoints();

            var response = await CommunicationManager.Instance.Client.InverseKinematicsAsync(new InverseKinematicsRequestArgs(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), pose, startJoints, true));
            if (!response.Result) {
                isWaiting = false;
                ImpossiblePoseNotify(true, false);
                return;
            }

            modelJoints = response.Data;
        } catch (ItemNotFoundException ex) {
            ImpossiblePoseNotify(true, false);
            isWaiting = false;
            return;

        } catch (Arcor2ConnectionException ex) {
            isWaiting = false;
            ImpossiblePoseNotify(true, false);
            return;
        }
        
        foreach (Joint joint in modelJoints) {
            SceneManager.Instance.SelectedRobot.SetJointValue(joint.Name, (float) joint.Value);
        }

        isWaiting = false;

        ImpossiblePoseNotify(false, false);
    }

    private GameObject SelectionToGameObject(Selection selection) {
        switch (selection) {
            case Selection.x:
                return XAxis;
            case Selection.y:
                return YAxis;
            case Selection.z:
                return ZAxis;
            case Selection.XY:
                return XYPlaneMesh;
            case Selection.XZ:
                return XZPlaneMesh;
            case Selection.YZ:
                return YZPlaneMesh;
            default:
                throw new NotImplementedException();

        }
    }

    private void ImpossiblePoseNotify(bool isImpossible, bool force) {
        if (!(isMoving || force)) {
            return;
        }

        if (isImpossible) {
            EnableInvalidShader();
            ImpossiblePoseNotification.SetActive(true);
        } else {
            EnableStandardShader();
            ImpossiblePoseNotification.SetActive(false);
        }


    }

    #region SCALING COROUTINES
    private void StartCoroutines(Selection selected) {
        if (selected == Selection.ee) {
            HideAllAxisAndPlanes();
            return;
        }

        if (selected == Selection.XZ) {
            StartCoroutine(AxisScale(XAxis, ActiveAxisScale));
            StartCoroutine(AxisScale(ZAxis, HiddenAxisScale));
            StartCoroutine(AxisScale(YAxis, ActiveAxisScale));

            StartCoroutine(AxisScale(XYPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(XZPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(YZPlaneMesh, HiddenPlaneScale));
            return;
        }

        if (selected == Selection.YZ) {
            StartCoroutine(AxisScale(XAxis, HiddenAxisScale));
            StartCoroutine(AxisScale(ZAxis, ActiveAxisScale));
            StartCoroutine(AxisScale(YAxis, ActiveAxisScale));

            StartCoroutine(AxisScale(XYPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(XZPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(YZPlaneMesh, HiddenPlaneScale));
            return;
        }

        if (selected == Selection.XY) {
            StartCoroutine(AxisScale(XAxis, ActiveAxisScale));
            StartCoroutine(AxisScale(ZAxis, ActiveAxisScale));
            StartCoroutine(AxisScale(YAxis, HiddenAxisScale));

            StartCoroutine(AxisScale(XYPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(XZPlaneMesh, HiddenPlaneScale));
            StartCoroutine(AxisScale(YZPlaneMesh, HiddenPlaneScale));
            return;
        }

        foreach (Selection i in Enum.GetValues(typeof(Selection))) {
            if (i == Selection.x || i == Selection.y || i == Selection.z) {
                if (i == selected) {
                    StartCoroutine(AxisScale(SelectionToGameObject(i), ActiveAxisScale));
                    Debug.Log("Selected: " + i.ToString());
                } else {
                    StartCoroutine(AxisScale(SelectionToGameObject(i), HiddenAxisScale));
                }
            } else if (i == Selection.XY || i == Selection.XZ || i == Selection.YZ) {
                if (i == selected) {
                    //StartCoroutine(AxisScale(SelectionToGameObject(i), ActivePlaneScale));
                    Debug.Log("Selected: " + i.ToString());
                } else {
                    StartCoroutine(AxisScale(SelectionToGameObject(i), HiddenPlaneScale));
                }
            }
        }
    }

    private void HideAllAxisAndPlanes() {
        StartCoroutine(AxisScale(XAxis, HiddenAxisScale));
        StartCoroutine(AxisScale(YAxis, HiddenAxisScale));
        StartCoroutine(AxisScale(ZAxis, HiddenAxisScale));
        StartCoroutine(AxisScale(XYPlaneMesh, HiddenPlaneScale));
        StartCoroutine(AxisScale(XZPlaneMesh, HiddenPlaneScale));
        StartCoroutine(AxisScale(YZPlaneMesh, HiddenPlaneScale));
    }

    private void ShowAllAxisAndPlanes() {
        StartCoroutine(AxisScale(XAxis, OrigAxisScale));
        StartCoroutine(AxisScale(YAxis, OrigAxisScale));
        StartCoroutine(AxisScale(ZAxis, OrigAxisScale));
        StartCoroutine(AxisScale(XYPlaneMesh, OrigPlaneScale));
        StartCoroutine(AxisScale(XZPlaneMesh, OrigPlaneScale));
        StartCoroutine(AxisScale(YZPlaneMesh, OrigPlaneScale));
    }

    private IEnumerator AxisScale(GameObject axis, Vector3 scale) {
        while (Vector3.Distance(axis.transform.localScale, scale) > 0.01f) {
            axis.transform.localScale = Vector3.Lerp(axis.transform.localScale, scale, 0.25f);
            yield return null;
        }
        axis.transform.localScale = scale;
    }

    #endregion SCALING COROUTINES

    #region SENSITIVITY SLIDERS
    private void UpdateSensitivity(float value) {
        DragMultiplier = value * 0.03f;
    }

    private void UpdateUpDownSensitivity(float value) {
        ForwardBackwardMultiplier = value;
    }

    #endregion SENSITIVITY SLIDERS

    #region MATERIALS
    private void EnableStandardShader() {
        shaderEnabled = ShaderEnabled.standard;
        robot.EnableStandardShader();
    }

    private void EnableInvalidShader() {
        shaderEnabled = ShaderEnabled.invalid;
        robot.EnableInvalidShader();
    }

    #endregion MATERIALS

    #region MAIN BUTTONS
    public void OnSelectButtonClick() {
#if AR_ON
            return;
#endif

        ToggleMove();

    }
    public void OnSelectButtonHold() {
#if !AR_ON
        return;
#endif

        ToggleMove();
    }
    public void OnSelectButtonRelease() {
#if !AR_ON
        return;
#endif

        ToggleMove(true);
    }
    public void OnUpButtonHold() {
        moveForwardHeld = true;
    }
    public void OnUpButtonRelease() {
        moveForwardHeld = false;
    }
    public void OnDownButtonHold() {
        moveBackwardHeld = true;
    }
    public void OnDownButtonRelease() {
        moveBackwardHeld = false;
    }

    #endregion MAIN BUTTONS

    #region COORD SYSTEM BUTTONS
    public void OnWorldSystemButtonClick() {
        cameraCoord = false;
        pointInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void OnUserSystemButtonClick() {
        cameraCoord = true;
        gizmo.GetComponent<GizmoVariant>().FlipX(false);
        gizmo.GetComponent<GizmoVariant>().FlipY(false);
        gizmo.GetComponent<GizmoVariant>().FlipZ(false);

        LockButton.GetComponent<LockButton>().ChangeToUnlocked();
    }

    public void OnLockUserSystem() {
        cameraCoord = false;
    }

    public void OnUnlockUserSystem() {
        cameraCoord = true;
    }

    #endregion COORD SYSTEM BUTTONS

    #region CONFIRM DIALOG
    public async void OnConfirmButtonClick() {
        Position position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position)));
        Orientation orientation = new(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        await CommunicationManager.Instance.Client.MoveToPoseAsync(new MoveToPoseRequestArgs(
            SceneManager.Instance.SelectedRobot.GetId(),
            endEffector.EEId,
            (decimal) 0.5,
            position,
            orientation));

        endEffector.transform.position = pointInstance.transform.position;

        ConfirmPoseDialog.SetActive(false);
    }

    public void OnRevertButtonClick() {
        pointInstance.transform.position = endEffector.transform.position;
        lastValidTransform.transform.position = endEffector.transform.position;
        MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position), lastValidTransform.transform);
        

        ConfirmPoseDialog.SetActive(false);
    }

    #endregion CONFIRM DIALOG

    #region DEBUG BUTTONS
    public void OnFirstButtonClick() {
        
        EnableInvalidShader();

    }

    public void OnSecondButtonClick() {
        pointInstance.transform.position = lastValidTransform.transform.position;
    }

    public async void OnThirdButtonClick() {
    }

    #endregion DEBUG BUTTONS

}
