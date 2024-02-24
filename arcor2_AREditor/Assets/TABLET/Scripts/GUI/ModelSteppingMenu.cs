using System;
using System.Collections.Generic;
using Base;
using IO.Swagger.Model;
using TMPro;
using UnityEngine;
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

    public GameObject SelectionText;
    public GameObject ButtonHintText;
    public Slider SensitivitySlider;
    public Button SelectButton;

    private float pointDistance = 0.5f;
    private float DragMultiplier = 0.3f;
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
    

    // Start is called before the first frame update
    private void Start() {
        SensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
    
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
            Vector3 difference = ray.GetPoint(pointDistance) - rayHitPosition;

            if (selection == Selection.ee) {
                pointInstance.transform.position = originalEEPosition + difference * DragMultiplier;

            } else if (selection == Selection.x) {
                Vector3 position = pointInstance.transform.position;
                position.x = originalEEPosition.x + difference.x * DragMultiplier;
                pointInstance.transform.position = position;
            } else if (selection == Selection.y) {
                Vector3 position = pointInstance.transform.position;
                position.z = originalEEPosition.z + difference.z * DragMultiplier;
                pointInstance.transform.position = position;
            } else if (selection == Selection.z) {
                Vector3 position = pointInstance.transform.position;
                position.y = originalEEPosition.y + difference.y * DragMultiplier;
                pointInstance.transform.position = position;
            }

            Vector3 position2 = new Vector3();
            Quaternion rotation = new Quaternion();
            robot.transform.GetPositionAndRotation(out position2, out rotation);
            float a = pointInstance.transform.position.x - position2.x;
            float b = pointInstance.transform.position.z - position2.z;

            double angle = -Math.Atan(a / b);
            var testjoint = robot.RobotModel.GetJoints()[0];
            robot.RobotModel.SetJointAngle(testjoint.Name, (float) angle);


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
        //return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        if (!isMoving) {
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            originalEEPosition = pointInstance.transform.position;
            rayHitPosition = ray.GetPoint(pointDistance);
            Vector3 PointPosition = pointInstance.transform.position;
            Vector3 RayOrigin = ray.origin;

            //pointDistance = Vector3.Distance(rayHitPosition, RayOrigin);
        }

        isMoving = !isMoving;

        if (isMoving) {
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";
        } else {
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
        }



    }
    public void OnSelectButtonHold() {
        Debug.Log("pressed down");

        return;

        if (selection != Selection.ee)
            return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        originalEEPosition = pointInstance.transform.position;
        Vector3 PointPosition = pointInstance.transform.position;
        Vector3 RayOrigin = ray.origin;

        pointDistance = Vector3.Distance(PointPosition, RayOrigin);

        isMoving = true;

        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";
    }
    public void OnSelectButtonRelease() {
        return;
        Debug.Log("relaesed");
        isMoving = false;
        ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
    }
    private void OnSelectedGizmoAxis(object sender, GizmoAxisEventArgs args) {
        SelectAxis(args.SelectedAxis);
    }
    private void SelectAxis(Gizmo.Axis axis, bool forceUpdate = false) {
        if (forceUpdate || selectedAxis != axis) {
            selectedAxis = axis;
            gizmo.HiglightAxis(axis);
            gizmo.SetRotationAxis(Gizmo.Axis.NONE);

        }

        SelectionText.GetComponent<TextMeshProUGUI>().text = "Axis: " + axis.ToString();
        UnselectEE();
        gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();

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
                Debug.Log("problem");
                break;
        }

    }

    private void SelectEE() {
        SelectionText.GetComponent<TextMeshProUGUI>().text = "End-Effector";
        draggablePoint.Highlight();
        gizmo.UnhighlightAllAxis();
        gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
        Select(Selection.ee);
    }
    private void UnselectEE() {
        draggablePoint.Unhighlight();
    }

    private void SelectPlane(Selection plane) {
        if (plane == Selection.XY) {
            gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXY();
        } else if (plane == Selection.XZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXZ();
        } else if (plane == Selection.YZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().HighlightYZ();
        }


        Select(plane);
        SelectionText.GetComponent<TextMeshProUGUI>().text = "Plane: " + plane.ToString();
    }

    private void Select(Selection value) {
        if (isMoving) {
            return;
        }

        selection = value;
    }

    private void UpdateSensitivity(float value) {
        DragMultiplier = value;
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

        Orientation orientation = new Orientation(y: (decimal) -1.0);

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
        isMoving = !isMoving;/*
        if (isMoving && PointInstance == null) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
 
            Vector3 position = /*TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(0.5f)))ray.GetPoint(0.5f);
            PointInstance = Instantiate(Point, position, Quaternion.identity);
        }*/
    }

    #endregion DEBUG BUTTONS

}
