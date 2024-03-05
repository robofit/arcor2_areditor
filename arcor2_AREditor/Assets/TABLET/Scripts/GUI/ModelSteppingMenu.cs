using System;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ModelSteppingMenu : RightMenu<ModelSteppingMenu> {
    private RobotActionObject robot;
    private RobotEE endEffector;

    private Gizmo gizmo;
    private Gizmo.Axis selectedAxis;
    public GameObject GizmoPrefab;

    private GameObject pointInstance;
    private DraggablePoint draggablePoint;
    public GameObject PointPrefab;

    public GameObject DistanceControl;
    public GameObject LeftMenu;
    public GameObject SelectionText;
    public GameObject ButtonHintText;
    public Slider SensitivitySlider;
    public Slider UpDownSensitivitySlider;
    public Button SelectButton;
    public GameObject XYPlaneMesh;
    public GameObject XZPlaneMesh;
    public GameObject YZPlaneMesh;
    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;

    private Vector3 OrigPlaneScale;
    private Vector3 ActivePlaneScale;

    private Vector3 OrigAxisScale;
    private Vector3 ActiveAxisScale;

    private float pointDistance = 0.5f;
    private float DragMultiplier = 0.3f;
    private float UpDownMultiplier = 0.3f;
    private Vector3 originalEEPosition;
    private Vector3 rayHitPosition;

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
    

    // Start is called before the first frame update
    private void Start() {
        SensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        UpDownSensitivitySlider.onValueChanged.AddListener(UpdateUpDownSensitivity);
    
    }

    private async void OnEnable() {
        robot = (RobotActionObject) SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
        robot.SetVisibility(1.0f);
        //robot.HideOutline();

        WebsocketManager.Instance.OnRobotEefUpdated -= SceneManager.Instance.RobotEefUpdated;
        WebsocketManager.Instance.OnRobotJointsUpdated -= SceneManager.Instance.RobotJointsUpdated;

        List<string> EEIDs = await robot.GetEndEffectorIds();
        string EEID = EEIDs[0];
        List<string> armsIDs = await robot.GetArmsIds();
        string armID = armsIDs[0];

        endEffector = await robot.GetEE(EEID, armID);

        Vector3 originalPos = endEffector.transform.position;

        
        pointInstance = Instantiate(PointPrefab, originalPos, Quaternion.identity);

        draggablePoint = pointInstance.GetComponent<DraggablePoint>();

        gizmo = Instantiate(GizmoPrefab).GetComponent<Gizmo>();
        gizmo.transform.SetParent(pointInstance.transform);
        gizmo.transform.localPosition = Vector3.zero;
        Sight.Instance.SelectedGizmoAxis += OnSelectedGizmoAxis;
        SelectAxis(Gizmo.Axis.X, true);

        XYPlaneMesh = gizmo.GetComponent<GizmoVariant>().XYPlaneMesh;
        XZPlaneMesh = gizmo.GetComponent<GizmoVariant>().XZPlaneMesh;
        YZPlaneMesh = gizmo.GetComponent<GizmoVariant>().YZPlaneMesh;

        XAxis = gizmo.GetComponent<GizmoVariant>().XAxis;
        YAxis = gizmo.GetComponent <GizmoVariant>().YAxis;
        ZAxis = gizmo.GetComponent<GizmoVariant>().ZAxis;

        OrigPlaneScale = XYPlaneMesh.transform.localScale;
        ActivePlaneScale.z = OrigPlaneScale.z;
        ActivePlaneScale.x = 3.0f;
        ActivePlaneScale.y = 3.0f;

        OrigAxisScale = XAxis.transform.localScale;
        ActiveAxisScale = XAxis.transform.localScale;
        ActiveAxisScale.z = 10f;
    }
    private void OnDisable() {
        SceneManager.Instance.GetActionObject(SceneManager.Instance.SelectedRobot.GetId()).SetVisibility(0.0f);
        //robot.ShowOutline();

        Destroy(gizmo);
        Destroy(pointInstance);
    }

    private void Update() {
        if (isMoving && pointInstance != null) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

            if (moveForwardHeld) {
                Debug.Log("move forward is held");
                pointDistance += 0.05f * UpDownMultiplier;
            }
            if (moveBackwardHeld) {
                Debug.Log("move backward is held");
                pointDistance += -0.05f * UpDownMultiplier;
            }

            Vector3 difference = ray.GetPoint(pointDistance) - rayHitPosition;



            Vector3 targetPosition = pointInstance.transform.position;

            if (selection == Selection.ee) {
                targetPosition = originalEEPosition + difference * DragMultiplier;
            } else if (selection == Selection.x) {
                targetPosition.x = originalEEPosition.x + difference.x * DragMultiplier;
                XAxis.transform.localScale = Vector3.Lerp(XAxis.transform.localScale, ActiveAxisScale, 0.25f);
            } else if (selection == Selection.y) {
                targetPosition.z = originalEEPosition.z + difference.z * DragMultiplier;
                YAxis.transform.localScale = Vector3.Lerp(YAxis.transform.localScale, ActiveAxisScale, 0.25f);
            } else if (selection == Selection.z) {
                targetPosition.y = originalEEPosition.y + difference.y * DragMultiplier;
                ZAxis.transform.localScale = Vector3.Lerp(ZAxis.transform.localScale, ActiveAxisScale, 0.25f);
            } else if (selection == Selection.XY) {
                targetPosition.y = originalEEPosition.y + difference.y * DragMultiplier;
                targetPosition.x = originalEEPosition.x + difference.x * DragMultiplier;

                XYPlaneMesh.transform.localScale = Vector3.Lerp(XYPlaneMesh.transform.localScale, ActivePlaneScale, 0.25f);
            } else if (selection == Selection.XZ) {
                targetPosition.x = originalEEPosition.x + difference.x * DragMultiplier;
                targetPosition.z = originalEEPosition.z + difference.z * DragMultiplier;

                XZPlaneMesh.transform.localScale = Vector3.Lerp(XZPlaneMesh.transform.localScale, ActivePlaneScale, 0.25f);
            } else if (selection == Selection.YZ) {
                targetPosition.z = originalEEPosition.z + difference.z * DragMultiplier;
                targetPosition.y = originalEEPosition.y + difference.y * DragMultiplier;

                YZPlaneMesh.transform.localScale = Vector3.Lerp(YZPlaneMesh.transform.localScale, ActivePlaneScale, 0.25f);
            }



            pointInstance.transform.position = targetPosition;

            MoveHereModel(targetPosition);

            //temporary
            //just to see the model move a bit
            /*
            robot.transform.GetPositionAndRotation(out Vector3 position2, out Quaternion rotation);
            float a = pointInstance.transform.position.x - position2.x;
            float b = pointInstance.transform.position.z - position2.z;

            double angle = -Math.Atan(a / b);
            IO.Swagger.Model.Joint testjoint = robot.RobotModel.GetJoints()[0];
            robot.RobotModel.SetJointAngle(testjoint.Name, (float) angle);
            */

        } else {
            XAxis.transform.localScale = Vector3.Lerp(XAxis.transform.localScale, OrigAxisScale, 0.25f);
            YAxis.transform.localScale = Vector3.Lerp(YAxis.transform.localScale, OrigAxisScale, 0.25f);
            ZAxis.transform.localScale = Vector3.Lerp(ZAxis.transform.localScale, OrigAxisScale, 0.25f);
            XYPlaneMesh.transform.localScale = Vector3.Lerp(XYPlaneMesh.transform.localScale, OrigPlaneScale, 0.25f);
            XZPlaneMesh.transform.localScale = Vector3.Lerp(XZPlaneMesh.transform.localScale, OrigPlaneScale, 0.25f);
            YZPlaneMesh.transform.localScale = Vector3.Lerp(YZPlaneMesh.transform.localScale, OrigPlaneScale, 0.25f);
        }
    }
    private void FixedUpdate() {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
        if (hits.Length > 0) {
            foreach (RaycastHit hit in hits) {
                if (hit.collider.gameObject.name == "DraggablePoint(Clone)") {
                    if (!isMoving) {
                        //rayHitPosition = hit.transform.position;
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    SelectEE();
                    break;
                }
                else if (hit.collider.gameObject.CompareTag("xy_plane")) {
                    if (!isMoving) {
                        //rayHitPosition = hit.transform.position;
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    SelectPlane(Selection.XY);
                    return;
                } else if (hit.collider.gameObject.CompareTag("xz_plane")) {
                    if (!isMoving) {
                        //rayHitPosition = hit.transform.position;
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    SelectPlane(Selection.XZ);
                    return;
                } else if (hit.collider.gameObject.CompareTag("yz_plane")) {
                    if (!isMoving) {
                        //rayHitPosition = hit.transform.position;
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    SelectPlane(Selection.YZ);
                    return;
                }
            }
        }

    }

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
    public void ToggleMove(bool forceStop = false) {
        isMoving = !isMoving;

        if (forceStop) {
            isMoving = false;
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        if (isMoving) {
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            originalEEPosition = pointInstance.transform.position;
            rayHitPosition = ray.GetPoint(pointDistance);

        }

        if (isMoving) {
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";
        } else {
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
        }

        if (isMoving) {
            HideGizmoOnMove();
            LeftMenu.SetActive(false);
        } else {
            ShowGizmo();
            LeftMenu.SetActive(true);
        }

    }

    private void HideGizmoOnMove() {
        Vector3 lineOrigin = draggablePoint.transform.position;
        Vector3 lineEnd = draggablePoint.transform.position;


        if (selection == Selection.ee) {
            Debug.Log("got to hidin");
            gizmo.gameObject.SetActive(false);
            DistanceControl.SetActive(true);

        } else if (selection == Selection.XY) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            
            //XYPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } else if (selection == Selection.XZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            
            //XZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } else if (selection == Selection.YZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            
            //YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } /*else if (selection == Selection.x) {
            gizmo.gameObject.SetActive(false);
            lineEnd.x += 5.0f;
            lineOrigin.x -= 5.0f;
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(0, lineOrigin);
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(1, lineEnd);
        } else if (selection == Selection.z) {
            gizmo.gameObject.SetActive(false);
            lineEnd.y += 5.0f;
            lineOrigin.y -= 5.0f;
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(0, lineOrigin);
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(1, lineEnd);
        } else if (selection == Selection.y) {
            gizmo.gameObject.SetActive(false);
            lineEnd.z += 5.0f;
            lineOrigin.z -= 5.0f;
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(0, lineOrigin);
            draggablePoint.gameObject.GetComponent<LineRenderer>().SetPosition(1, lineEnd);
        }*/

    }
    private void ShowGizmo() {
        gizmo.gameObject.SetActive(true);
        /*XYPlaneMesh.transform.localScale = OrigPlaneScale;
        XZPlaneMesh.transform.localScale = OrigPlaneScale;
        YZPlaneMesh.transform.localScale = OrigPlaneScale;*/
        DistanceControl.SetActive(false);
    }

    private void OnSelectedGizmoAxis(object sender, GizmoAxisEventArgs args) {
        SelectAxis(args.SelectedAxis);
    }
    private void SelectAxis(Gizmo.Axis axis, bool forceUpdate = false) {
        if (forceUpdate || selectedAxis != axis) {
            selectedAxis = axis;
            gizmo.SetRotationAxis(Gizmo.Axis.NONE);

        }

        switch (axis.ToString()) {
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

    private void SelectEE() {
        Select(Selection.ee);
    }

    private void SelectPlane(Selection plane) {
        Select(plane);
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

    private void UpdateSensitivity(float value) {
        DragMultiplier = value;
    }

    private void UpdateUpDownSensitivity(float value) {
        UpDownMultiplier = value;
    }

    private async void MoveHereModel(Vector3 position, bool avoid_collision = true) {
        List<IO.Swagger.Model.Joint> modelJoints; //joints to move the model to

        Orientation orientation = new Orientation(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        try {
            IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose(orientation: orientation, position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position)));
            List<IO.Swagger.Model.Joint> startJoints = SceneManager.Instance.SelectedRobot.GetJoints();

            modelJoints = await WebsocketManager.Instance.InverseKinematics(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), true, pose, startJoints);
            //await PrepareRobotModel(SceneManager.Instance.SelectedRobot.GetId(), false);
            if (!avoid_collision) {
                Notifications.Instance.ShowNotification("The model is in a collision with other object!", "");
            }
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            return;
        } catch (RequestFailedException ex) {
            Debug.Log("am i here");
            if (avoid_collision) //if this is first call, try it again without avoiding collisions
                MoveHereModel(position, false);
            else
                Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            return;
        }

        foreach (IO.Swagger.Model.Joint joint in modelJoints) {
            SceneManager.Instance.SelectedRobot.SetJointValue(joint.Name, (float) joint.Value);
        }
    }

    #region DEBUG BUTTONS
    public async void OnFirstButtonClick() {
        Debug.Log("vis: " + SceneManager.Instance.GetActionObject(SceneManager.Instance.SelectedRobot.GetId()).GetVisibility());

        OrientationJointsDetailMenu orientationJointsDetailMenu = this.gameObject.AddComponent<OrientationJointsDetailMenu>();

        List<string> ArmsIDs = await robot.GetArmsIds();
        string ArmsID = ArmsIDs[0];

        List<string> EEIDs = await robot.GetEndEffectorIds();
        string EEID = EEIDs[0];

        ProjectRobotJoints projectRobotJoints = new ProjectRobotJoints("what", robot.GetId(), robot.RobotModel.GetJoints(), true, ArmsID, EEID);

        orientationJointsDetailMenu.joints = projectRobotJoints;

        Orientation orientation = new Orientation(w: (decimal) 0.0, x: (decimal) 0.0, y: (decimal) 1.0, z: (decimal) 0.0);

        orientationJointsDetailMenu.orientation = new NamedOrientation("why", orientation);

        Vector3 newPos = new Vector3();

        newPos = endEffector.transform.position;

        /*newPos.x += 0.06f;
        newPos.y += 0.02f;
        newPos.z -= 0.04f;*/

        Debug.Log("newPos: " + newPos);
        orientationJointsDetailMenu.MoveHereModel(newPos, true);

        //Robot.RobotModel.SetZeroJointAngles();
    }

    public async void OnSecondButtonClick() {
        Debug.Log("got here?");
        List<string> EEIDs = await robot.GetEndEffectorIds();
        string EEID = EEIDs[0];
        List<string> ArmsIDs = await robot.GetArmsIds();
        string ArmsID = ArmsIDs[0];

        Vector3 newPosition = new Vector3();

        newPosition = endEffector.transform.position;

        Debug.Log("old: " + endEffector.transform.position);

        newPosition.x += 0.05f;

        endEffector.UpdatePosition(newPosition, Quaternion.AngleAxis(180, new Vector3(1, 0, 0)));

        IO.Swagger.Model.Pose pose = new IO.Swagger.Model.Pose();
        pose.Position = new IO.Swagger.Model.Position((decimal) newPosition.x, (decimal) newPosition.y, (decimal) newPosition.z);

        Debug.Log("new: " + endEffector.transform.position);


        //List<IO.Swagger.Model.Joint> joints = await WebsocketManager.Instance.InverseKinematics(Robot.GetId(), EEID, true, pose, Robot.RobotModel.GetJoints());

        foreach (var joint in robot.RobotModel.GetJoints()) {
            print("key: " + joint.Name + "value: " + joint.Value);
        }

        var testjoint = robot.RobotModel.GetJoints()[0];
        Vector3 position = new Vector3();
        Quaternion rotation = new Quaternion();
        robot.transform.GetPositionAndRotation(out position, out rotation);

        float a = pointInstance.transform.position.x - position.x;
        float b = pointInstance.transform.position.z - position.z;

        double angle = -Math.Atan(a / b);

        robot.RobotModel.SetJointAngle(testjoint.Name, (float) angle);





        /*foreach (var joint in joints) {
            print("name" + joint.Name + "value" + joint.Value);
        }

        foreach (var joint in joints) {
            Robot.RobotModel.SetJointAngle(joint.Name, (float)joint.Value);
        }*/


    }

    public void OnThirdButtonClick() {
        //isMoving = !isMoving;
        /*
        if (isMoving && PointInstance == null) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
 
            Vector3 position = /*TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(0.5f)))ray.GetPoint(0.5f);
            PointInstance = Instantiate(Point, position, Quaternion.identity);
        }*/

        YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue += 100;
        Debug.Log("Renderqueue: " + YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue);
        
    }

    #endregion DEBUG BUTTONS

}
