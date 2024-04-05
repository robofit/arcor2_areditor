using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ModelSteppingMenu : RightMenu<ModelSteppingMenu> {
    private RobotActionObject robot;
    private RobotEE endEffector;

    private Gizmo gizmo;
    public GameObject GizmoPrefab;

    private GameObject pointInstance;
    private DraggablePoint draggablePoint;
    public GameObject PointPrefab;

    public GameObject DistanceControl;
    public GameObject LeftMenu;
    public GameObject SelectionText;
    public GameObject ButtonHintText;
    public GameObject ExitDialog;
    public GameObject ConfirmPoseDialog;
    public GameObject CoordsToggle;
    public GameObject LockButton;
    public GameObject ImpossiblePoseNotification;
    public UnityEngine.UI.Slider SensitivitySlider;
    public UnityEngine.UI.Slider UpDownSensitivitySlider;
    public UnityEngine.UI.Button SelectButton;
    public GameObject XYPlaneMesh;
    public GameObject XZPlaneMesh;
    public GameObject YZPlaneMesh;
    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;
    public Material ClippingMaterial;

    private Vector3 OrigPlaneScale;
    private Vector3 ActivePlaneScale;
    private Vector3 HiddenPlaneScale = Vector3.zero;

    private Vector3 OrigAxisScale;
    private Vector3 ActiveAxisScale;
    private Vector3 HiddenAxisScale;

    private float pointDistance = 0.5f;
    private float DragMultiplier = 0.3f * 0.03f;
    private float UpDownMultiplier = 0.02f;
    private Vector3 fallbackEEPosition;
    private Vector3 originalPointPosition;
    private Vector3 rayHitPosition;

    private Vector3 pointPosition;

    private Vector3 forwardBackwardAdjust = Vector3.zero;

    private GameObject dummy;

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

    private Selection selection = Selection.none;

    private bool isMoving = false;
    private bool moveForwardHeld = false;
    private bool moveBackwardHeld = false;

    private bool isWaiting = false;
    private bool cameraCoord = false;
    
    private void Start() {
        SensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        UpDownSensitivitySlider.onValueChanged.AddListener(UpdateUpDownSensitivity);
        LockButton.GetComponent<LockButton>().OnUnlockedEvent += OnUnlockUserSystem;
        LockButton.GetComponent<LockButton>().OnLockedEvent += OnLockUserSystem;
    }

    public async Task TurnOn() {
        robot = (RobotActionObject) SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
        robot.SetVisibility(1.0f);
        robot.GetComponent<OutlineOnClick>().UnHighlight();
        robot.GetComponent<OutlineOnClick>().Enabled = false;
        await robot.DisableVisualisationOfEE();

        dummy = new GameObject();

        WebsocketManager.Instance.OnRobotEefUpdated -= SceneManager.Instance.RobotEefUpdated;
        WebsocketManager.Instance.OnRobotJointsUpdated -= SceneManager.Instance.RobotJointsUpdated;

        List<string> EEIDs = await robot.GetEndEffectorIds();
        string EEID = EEIDs[0];
        List<string> armsIDs = await robot.GetArmsIds();
        string armID = armsIDs[0];

        endEffector = await robot.GetEE(EEID, armID);

        fallbackEEPosition = endEffector.transform.position;

        if (endEffector.transform.position == pointPosition) {
            ConfirmPoseDialog.SetActive(false);
        } else {
            ConfirmPoseDialog.SetActive(true);
        }

        if (pointPosition != Vector3.zero) {
            pointInstance = Instantiate(PointPrefab, pointPosition, Quaternion.identity);
            MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position));
            
        } else {
            pointInstance = Instantiate(PointPrefab, endEffector.transform.position, Quaternion.identity);
        }
        

        draggablePoint = pointInstance.GetComponent<DraggablePoint>();

        gizmo = Instantiate(GizmoPrefab).GetComponent<Gizmo>();
        gizmo.transform.SetParent(pointInstance.transform);
        gizmo.transform.localPosition = Vector3.zero;
        Sight.Instance.SelectedGizmoAxis += OnSelectedGizmoAxis;

        XYPlaneMesh = gizmo.GetComponent<GizmoVariant>().XYPlaneMesh;
        XZPlaneMesh = gizmo.GetComponent<GizmoVariant>().XZPlaneMesh;
        YZPlaneMesh = gizmo.GetComponent<GizmoVariant>().YZPlaneMesh;

        XAxis = gizmo.GetComponent<GizmoVariant>().XAxis;
        YAxis = gizmo.GetComponent<GizmoVariant>().YAxis;
        ZAxis = gizmo.GetComponent<GizmoVariant>().ZAxis;

        OrigPlaneScale = XYPlaneMesh.transform.localScale;
        ActivePlaneScale = OrigPlaneScale;
        //ActivePlaneScale.z = OrigPlaneScale.z;
        //ActivePlaneScale.x = 3.0f;
        //ActivePlaneScale.y = 3.0f;

        OrigAxisScale = XAxis.transform.localScale;
        ActiveAxisScale = XAxis.transform.localScale;
        ActiveAxisScale.z = 10f;

        HiddenAxisScale = XAxis.transform.localScale;
        HiddenAxisScale.z = 0.0f;

        //EnableClippingMaterial();
    }

    public async Task TurnOff(bool reset = false) {
        CoordsToggle.GetComponent<TwoStatesToggleNew>().SwitchToLeft();
        LockButton.SetActive(false);

        pointPosition = pointInstance.transform.position;

        SceneManager.Instance?.GetActionObject(SceneManager.Instance.SelectedRobot.GetId()).SetVisibility(0.0f);

        Sight.Instance.SelectedGizmoAxis -= OnSelectedGizmoAxis;

        robot.GetComponent<OutlineOnClick>().Enabled = true;

        //endEffector.transform.position = pointInstance.transform.position;
        await robot.EnableVisualisationOfEE();

        /*
        if (reset) {
            MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(fallbackEEPosition));
        }
        else {
            Position position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position)));
            Orientation orientation = new Orientation(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

            await WebsocketManager.Instance.MoveToPose(
                robotId: SceneManager.Instance.SelectedRobot.GetId(),
                endEffectorId: endEffector.EEId,
                speed: (decimal) 0.5,
                position: position,
                orientation: orientation);

        }
        */

        Destroy(gizmo);
        Destroy(pointInstance);
        Destroy(dummy);

        WebsocketManager.Instance.OnRobotEefUpdated += SceneManager.Instance.RobotEefUpdated;
        WebsocketManager.Instance.OnRobotJointsUpdated += SceneManager.Instance.RobotJointsUpdated;

        //LeftMenu.GetComponent<LeftMenu>().ModelSteppingMenuClosed();
        
    }

    private void Update() {
        if (cameraCoord && !isMoving) {
            Vector3 targetPosition = new Vector3(
                Camera.main.transform.position.x,
                pointInstance.transform.position.y,
                Camera.main.transform.position.z);
            pointInstance.transform.LookAt(targetPosition);
        }

        //gizmo flipping
        Vector3 localCamPos = Camera.main.transform.position;
        localCamPos = pointInstance.transform.InverseTransformPoint(localCamPos);

        if (!isMoving && !cameraCoord) {
            if (localCamPos.z < 0) {
                gizmo.GetComponent<GizmoVariant>().FlipX(true);
            } else {
                gizmo.GetComponent<GizmoVariant>().FlipX(false);
            }

            if (localCamPos.x < 0) {
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

            dummy.transform.position = rayPoint;
            rayPoint = dummy.transform.position;


            Vector3 difference = rayPoint - rayHitPosition;

            difference = pointInstance.transform.InverseTransformVector(difference);

            //Debug: draw difference vector
            //draggablePoint.GetComponent<LineRenderer>().SetPosition(0, rayPoint);
            //draggablePoint.GetComponent<LineRenderer>().SetPosition(1, rayHitPosition);

            dummy.transform.position = originalPointPosition;
            dummy.transform.rotation = pointInstance.transform.rotation;

            if (selection == Selection.ee) {
                dummy.transform.Translate(difference * DragMultiplier);

            } else if (selection == Selection.x) {
                dummy.transform.Translate(difference.x * DragMultiplier, 0f, 0f);

            } else if (selection == Selection.y) {
                dummy.transform.Translate(0f, 0f, difference.z * DragMultiplier);

            } else if (selection == Selection.z) {
                dummy.transform.Translate(0f, difference.y * DragMultiplier, 0f);

            } else if (selection == Selection.XY) {
                dummy.transform.Translate(difference.x * DragMultiplier, difference.y * DragMultiplier, 0f);

            } else if (selection == Selection.XZ) {
                dummy.transform.Translate(difference.x * DragMultiplier, 0f, difference.z * DragMultiplier);

            } else if (selection == Selection.YZ) {
                dummy.transform.Translate(0f, difference.y * DragMultiplier, difference.z * DragMultiplier);

            }

            pointInstance.transform.position = dummy.transform.position;

            if (!isWaiting) {
                isWaiting = true;
                MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position));
            }   
            
        } 
            
    }
    private void FixedUpdate() {
        //Axis selection
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
        if (hits.Length > 0) {
            foreach (RaycastHit hit in hits) {
                if (hit.collider.gameObject.name == "DraggablePoint(Clone)") {
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
            rayHitPosition = ray.GetPoint(pointDistance);

            OnMove();
        } else {
            OnStopMove();
        }
    }

    private void OnMove() {
        ConfirmPoseDialog.gameObject.SetActive(true);
        forwardBackwardAdjust = Vector3.zero;
        LeftMenu.SetActive(false);
        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";

        StopAllCoroutines();
        StartCoroutines(selection);

        if (selection == Selection.ee) {
            //gizmo.gameObject.SetActive(false);
            draggablePoint.GetComponent<LineRenderer>().enabled = true;
            DistanceControl.SetActive(true);

        } else if (selection == Selection.XY) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            XYPlaneMesh.GetComponent<MeshRenderer>().material.renderQueue = 2000;
            EnableClippingMaterial();

        } else if (selection == Selection.XZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            XZPlaneMesh.GetComponent<MeshRenderer>().material.renderQueue = 2000;
            EnableClippingMaterial();

        } else if (selection == Selection.YZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            YZPlaneMesh.GetComponent<MeshRenderer>().material.renderQueue = 2000;
            EnableClippingMaterial();

        } 

        gizmo.gameObject.GetComponent<GizmoVariant>().HideXCone();
        gizmo.gameObject.GetComponent<GizmoVariant>().HideYCone();
        gizmo.gameObject.GetComponent<GizmoVariant>().HideZCone();
    }
    private void OnStopMove() {
        LeftMenu.SetActive(true);
        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
        gizmo.GetComponent<GizmoVariant>().UnhighlightAll();

        StopAllCoroutines();
        ShowAllAxisAndPlanes();

        //draggablePoint.GetComponent<LineRenderer>().enabled = false;
        XYPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 4700;
        YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 4700;
        XZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 4700;
        DistanceControl.SetActive(false);

        DisableClippingMaterial();
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
            SelectionText.GetComponent<TextMeshProUGUI>().text = value.ToString() + " Axis";
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

    private async void MoveHereModel(Vector3 position, bool avoid_collision = true) {
        List<IO.Swagger.Model.Joint> modelJoints; //joints to move the model to

        Orientation orientation = new Orientation(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        try {
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(orientation: orientation, position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position)));
            List<IO.Swagger.Model.Joint> startJoints = SceneManager.Instance.SelectedRobot.GetJoints();

            modelJoints = await WebsocketManager.Instance.InverseKinematics(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), true, pose, startJoints);
            if (!avoid_collision) {
                //Notifications.Instance.ShowNotification("The model is in a collision with other object!", "");
            }
        } catch (ItemNotFoundException ex) {
            ImpossiblePoseNotification.SetActive(true);
            //Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            isWaiting = false;
            return;
        } catch (RequestFailedException ex) {
            
            if (avoid_collision) //if this is first call, try it again without avoiding collisions
                MoveHereModel(position, false);
            else
                //Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            isWaiting = false;
            ImpossiblePoseNotification.SetActive(true);
            return;
        }

        ImpossiblePoseNotification.SetActive(false);

        foreach (IO.Swagger.Model.Joint joint in modelJoints) {
            SceneManager.Instance.SelectedRobot.SetJointValue(joint.Name, (float) joint.Value);
        }

        isWaiting = false;
    }

    private void OnSelectedGizmoAxis(object sender, GizmoAxisEventArgs args) {
        switch (args.SelectedAxis.ToString()) {
            case "X":
                Select(Selection.x);
                break;
            case "Y":
                Select(Selection.y);
                break;
            case "Z":
                Select(Selection.z);
                break;
            default:
                throw new NotImplementedException();
        }
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
                    StartCoroutine(AxisScale(SelectionToGameObject(i), ActivePlaneScale));
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
        UpDownMultiplier = value;
    }

    #endregion SENSITIVITY SLIDERS

    #region CLIPPING MATERIAL
    private void DisableClippingMaterial() {
        foreach (Renderer i in robot.robotRenderers) {
            if (i.materials.Length == 3) {
                i.materials[1].shader = Shader.Find("Standard");
                gizmo.GetComponent<GizmoVariant>().RemoveMaterial(i.materials[1]);
            } else {
                i.material.shader = Shader.Find("Standard");
                gizmo.GetComponent<GizmoVariant>().RemoveMaterial(i.material);
            }
        }
    }

    private void EnableClippingMaterial() {
        foreach (Renderer i in robot.robotRenderers) {
            if (i.materials.Length == 3) {
                i.materials[1].shader = Shader.Find("ClippingColorChange");
                gizmo.GetComponent<GizmoVariant>().AddMaterial(i.materials[1]);
            } else {
                i.material.shader = Shader.Find("ClippingColorChange");
                gizmo.GetComponent<GizmoVariant>().AddMaterial(i.material);
            }

        }
    }

    #endregion CLIPPING MATERIAL

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
    }

    public void OnLockUserSystem() {
        cameraCoord = false;
    }

    public void OnUnlockUserSystem() {
        cameraCoord = true;
    }

    #endregion COORD SYSTEM BUTTONS

    #region EXIT DIALOG
    public void ExitDialogShow() {
        ExitDialog.SetActive(true);

    }
    public void OnCancelButtonClick() {
        ExitDialog.SetActive(false);
    }

    public void OnYesButtonClick() {
        ExitDialog.SetActive(false);
        TurnOff();
    }

    public void OnNoButtonClick() {
        ExitDialog.SetActive(false);
        TurnOff(reset: true);
    }

    #endregion EXIT DIALOG

    #region CONFIRM DIALOG
    public async void OnConfirmButtonClick() {
        Position position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position)));
        Orientation orientation = new Orientation(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        await WebsocketManager.Instance.MoveToPose(
            robotId: SceneManager.Instance.SelectedRobot.GetId(),
            endEffectorId: endEffector.EEId,
            speed: (decimal) 0.5,
            position: position,
            orientation: orientation);

        endEffector.transform.position = pointInstance.transform.position;

        ConfirmPoseDialog.SetActive(false);
    }

    public void OnRevertButtonClick() {
        pointInstance.transform.position = endEffector.transform.position;
        MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position));

        ConfirmPoseDialog.SetActive(false);
    }

    #endregion CONFIRM DIALOG

    #region DEBUG BUTTONS
    public void OnFirstButtonClick() {
        cameraCoord = !cameraCoord;

    }

    public void OnSecondButtonClick() {
        pointInstance.transform.rotation = Quaternion.Euler(0, 45f, 0);
    }

    public async void OnThirdButtonClick() {
        pointInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    #endregion DEBUG BUTTONS

    #region IK
    private List<IO.Swagger.Model.Joint> DobotInverseKinematics(List<IO.Swagger.Model.Joint> startJoints, IO.Swagger.Model.Pose pose) {
        double link_2_length = 0.135;
        double link_3_length = 0.147;
        double link_4_length = 0.06;
        double end_effector_length = 0.06;


        Quaternion quat = new Quaternion((float)pose.Orientation.X, (float)pose.Orientation.Y, (float)pose.Orientation.Z, (float)pose.Orientation.W);
        Vector3 eul = Quaternion.ToEulerAngles(quat.normalized);
        double yaw = eul.z;

        double x = (double)pose.Position.X;
        double y = (double)pose.Position.Y;
        double z = (double)pose.Position.Z + end_effector_length;

        double r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        double rho_sq = Math.Pow(r - link_4_length, 2) + Math.Pow(y, 2);
        
        double rho = Math.Sqrt(rho_sq);

        double l2_sq = Math.Pow(link_2_length, 2);
        double l3_sq = Math.Pow(link_3_length, 2);

        double alpha = 0;
        double gamma = 0;

        try {
            alpha = Math.Acos(l2_sq + rho_sq - l3_sq) / (2.0 * link_2_length * rho);
            gamma = Math.Acos((l2_sq + l3_sq - rho_sq) / (2.0 * link_2_length * link_3_length));
        }
        catch (Exception e){
            Debug.Log("aint it, " + e);
        }

        double beta = Math.Atan2(z, r - link_4_length);

        double baseAngle = Math.Atan2(y, x);
        double rearAngle = Math.PI / 2 - beta - alpha;
        double frontAngle = Math.PI / 2 - gamma;

        Debug.Log("result 1: " + baseAngle);
        Debug.Log("result 2: " + rearAngle);
        Debug.Log("result 3: " + frontAngle);
        Debug.Log("result 4: " + (-rearAngle - frontAngle));
        Debug.Log("result 5: " + (yaw - baseAngle));

        List<IO.Swagger.Model.Joint> newJoints = new List<IO.Swagger.Model.Joint>() {
            new IO.Swagger.Model.Joint("magician_joint_1", (decimal)baseAngle),
            new IO.Swagger.Model.Joint("magician_joint_2", (decimal)rearAngle),
            new IO.Swagger.Model.Joint("magician_joint_3", (decimal)frontAngle),
            new IO.Swagger.Model.Joint("magician_joint_4", (decimal)(-rearAngle - frontAngle)),
            new IO.Swagger.Model.Joint("magician_joint_5", (decimal)(yaw - baseAngle))
        };




        return newJoints;
    }

    private List<IO.Swagger.Model.Joint> DobotInverseKinematicsAbsolute(List<IO.Swagger.Model.Joint> startJoints, IO.Swagger.Model.Pose pose) {
        return DobotInverseKinematics(startJoints, MakePoseRel(robot.GetPose(), pose));
    }

    private IO.Swagger.Model.Pose MakePoseRel(IO.Swagger.Model.Pose parent, IO.Swagger.Model.Pose child) {
        IO.Swagger.Model.Position position = new IO.Swagger.Model.Position();
        position.X = child.Position.X - parent.Position.X;
        position.Y = child.Position.Y - parent.Position.Y;
        position.Z = child.Position.Z - parent.Position.Z;

        Quaternion parentQuat = new Quaternion(x: (float)parent.Orientation.X, y: (float)parent.Orientation.Y, z: (float)parent.Orientation.Z, w: (float)parent.Orientation.W).normalized;
        Quaternion childQuat = new Quaternion(x: (float) child.Orientation.X, y: (float) child.Orientation.Y, z: (float) child.Orientation.Z, w: (float) child.Orientation.W).normalized;

        Quaternion resultQuat = Quaternion.Inverse(parentQuat) * childQuat;

        Orientation resultOrientation = new Orientation(x: (decimal)resultQuat.x, y: (decimal)resultQuat.y, z: (decimal)resultQuat.z, w: (decimal)resultQuat.w);

        return new IO.Swagger.Model.Pose(position, resultOrientation);

        


    }

    private IO.Swagger.Model.Position RotatePosition(IO.Swagger.Model.Position pose) {
        return null;
    }

    #endregion


}
