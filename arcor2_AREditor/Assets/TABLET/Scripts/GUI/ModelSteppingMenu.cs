using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using RuntimeInspectorNamespace;
using TMPro;
using TrilleonAutomation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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

    private bool isWaiting = false;
    
    private void Start() {
        SensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        UpDownSensitivitySlider.onValueChanged.AddListener(UpdateUpDownSensitivity);
    }

    private async void OnEnable() {
        robot = (RobotActionObject) SceneManager.Instance.GetRobot(SceneManager.Instance.SelectedRobot.GetId());
        robot.SetVisibility(1.0f);

        foreach (Renderer i in robot.robotRenderers) {
            Debug.Log("materials: ");
            foreach (Material j in i.materials) {
                Debug.Log("mat: " +  j.name);
            }
        }

        /*List<Material> mats = new List<Material> {
            ClippingMaterial
        };

        /*foreach (Renderer i in robot.robotRenderers) {
            List<Material> materials = i.materials.ToList();
            materials.Add(ClippingMaterial);
            i.materials = materials.ToArray();
        }*/

        EnableClippingMaterial();

        /*foreach (Renderer i in robot.robotRenderers) {
            Material[] materials = new Material[i.materials.Length + 1];

            for (int k = 0; k<materials.Length; k++) {
                materials[k] = i.materials[k];
            }

            materials[i.materials.Length] = ClippingMaterial;

            i.materials = materials;
        }*/

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
        //SelectAxis(Gizmo.Axis.X, true);

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

        Sight.Instance.SelectedGizmoAxis -= OnSelectedGizmoAxis;

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
                Vector3 lineEnd = robot.transform.position;
                lineEnd.y = targetPosition.y;
                draggablePoint.GetComponent<LineRenderer>().SetPosition(0, targetPosition);
                draggablePoint.GetComponent<LineRenderer>().SetPosition(1, lineEnd);
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

            if (!isWaiting) {
                isWaiting = true;
                MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position));
            }
                
            
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
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    //SelectEE();
                    Select(Selection.ee);
                    break;
                }
                else if (hit.collider.gameObject.CompareTag("xy_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    //SelectPlane(Selection.XY);
                    Select(Selection.XY);
                    return;
                } else if (hit.collider.gameObject.CompareTag("xz_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    //SelectPlane(Selection.XZ);
                    Select(Selection.XZ);
                    return;
                } else if (hit.collider.gameObject.CompareTag("yz_plane")) {
                    if (!isMoving) {
                        pointDistance = Vector3.Distance(hit.transform.position, ray.origin);
                    }
                    //SelectPlane(Selection.YZ);
                    Select(Selection.YZ);
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
            //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            originalEEPosition = pointInstance.transform.position;
            rayHitPosition = ray.GetPoint(pointDistance);

            OnMove();
            LeftMenu.SetActive(false);
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "";
        } else {
            ButtonHintText.GetComponent<TextMeshProUGUI>().text = "Hold to drag";
            Debug.Log("robot pose: " + robot.GetPose());
            //MoveHereModel(SceneManager.Instance.SceneOrigin.transform.parent.InverseTransformPoint(pointInstance.transform.position));
            gizmo.GetComponent<GizmoVariant>().UnhighlightAll();
            OnStopMove();
            LeftMenu.SetActive(true);

        }
    }

    private void OnMove() {
        Vector3 lineOrigin = draggablePoint.transform.position;
        Vector3 lineEnd = draggablePoint.transform.position;


        if (selection == Selection.ee) {
            Debug.Log("got to hidin");
            gizmo.gameObject.SetActive(false);
            DistanceControl.SetActive(true);

        } else if (selection == Selection.XY) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            //gizmo.gameObject.GetComponent<GizmoVariant>().SetDir(true);
            //XYPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } else if (selection == Selection.XZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            //XZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } else if (selection == Selection.YZ) {
            gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
            //YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue = 2000;

        } else if (selection == Selection.x) {
            //gizmo.gameObject.SetActive(false);
            gizmo.GetComponent<GizmoVariant>().HideXCone();
        } else if (selection == Selection.z) {
            //gizmo.gameObject.SetActive(false);
            gizmo.GetComponent<GizmoVariant>().HideZCone();
        } else if (selection == Selection.y) {
            //gizmo.gameObject.SetActive(false);
            gizmo.GetComponent<GizmoVariant>().HideYCone();
        }

    }
    private void OnStopMove() {
        gizmo.gameObject.SetActive(true);
        /*XYPlaneMesh.transform.localScale = OrigPlaneScale;
        XZPlaneMesh.transform.localScale = OrigPlaneScale;
        YZPlaneMesh.transform.localScale = OrigPlaneScale;*/
        DistanceControl.SetActive(false);

        //DisableClippingMaterial();
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
    /*private void SelectAxis(Gizmo.Axis axis, bool forceUpdate = false) {
        if (forceUpdate) {
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

    }*/

    /*private void SelectEE() {
        Select(Selection.ee);
    }*/
    /*private void SelectPlane(Selection plane) {
        Select(plane);
    }*/
    private void Select(Selection value) {
        if (isMoving) {
            return;
        }

        draggablePoint.GetComponent<LineRenderer>().enabled = false;
        gizmo.gameObject.GetComponent<GizmoVariant>().UnhighlightAll();
        gizmo.UnhighlightAllAxis();
        draggablePoint.Unhighlight();

        selection = value;

        if (value == Selection.x || value == Selection.y || value == Selection.z) {
            SelectionText.GetComponent<TextMeshProUGUI>().text = value.ToString() + " Axis";
            if (value == Selection.x) {
                gizmo.HiglightAxis(Gizmo.Axis.X);
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            } else if (value == Selection.y) {
                gizmo.HiglightAxis(Gizmo.Axis.Y);
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(0f, 90f, 90f);
            } else if (value == Selection.z) {
                gizmo.HiglightAxis(Gizmo.Axis.Z);
                gizmo.gameObject.GetComponent<GizmoVariant>().ClippingPlane.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

        } else if (value == Selection.XY || value == Selection.XZ || value == Selection.YZ) {
            SelectionText.GetComponent<TextMeshProUGUI>().text = value.ToString() + " Plane";
            if (value == Selection.XY) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXY();
                //gizmo.gameObject.GetComponent<GizmoVariant>().SetXYClippingPlane();
            } else if (value == Selection.XZ) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightXZ();
                //gizmo.gameObject.GetComponent<GizmoVariant>().SetXZClippingPlane();
            } else if (value == Selection.YZ) {
                gizmo.gameObject.GetComponent<GizmoVariant>().HighlightYZ();
                //gizmo.gameObject.GetComponent<GizmoVariant>().SetYZClippingPlane();
            }
        } else if (value == Selection.ee) {
            draggablePoint.GetComponent<LineRenderer>().enabled = true;
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

            Debug.Log("joint 1: " + startJoints[0].Value);
            Debug.Log("joint 2: " + startJoints[1].Value);
            Debug.Log("joint 3: " + startJoints[2].Value);
            Debug.Log("joint 4: " + startJoints[3].Value);
            Debug.Log("joint 5: " + startJoints[4].Value);

            modelJoints = await WebsocketManager.Instance.InverseKinematics(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), true, pose, startJoints);
            //modelJoints = DobotInverseKinematics(startJoints, pose);
            //modelJoints = DobotInverseKinematicsAbsolute(startJoints, pose);
            //await PrepareRobotModel(SceneManager.Instance.SelectedRobot.GetId(), false);
            if (!avoid_collision) {
                Notifications.Instance.ShowNotification("The model is in a collision with other object!", "");
            }
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            isWaiting = false;
            return;
        } catch (RequestFailedException ex) {
            Debug.Log("am i here");
            if (avoid_collision) //if this is first call, try it again without avoiding collisions
                MoveHereModel(position, false);
            else
                Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
            isWaiting = false;
            return;
        }

        foreach (IO.Swagger.Model.Joint joint in modelJoints) {
            SceneManager.Instance.SelectedRobot.SetJointValue(joint.Name, (float) joint.Value);
        }

        isWaiting = false;

        /*Debug.Log("result joint 1: " + modelJoints[0].Value);
        Debug.Log("result joint 2: " + modelJoints[1].Value);
        Debug.Log("result joint 3: " + modelJoints[2].Value);
        Debug.Log("result joint 4: " + modelJoints[3].Value);
        Debug.Log("result joint 5: " + modelJoints[4].Value);*/
    }

    private void UpdateSensitivity(float value) {
        DragMultiplier = value;
    }

    private void UpdateUpDownSensitivity(float value) {
        UpDownMultiplier = value;
    }

    private void DisableClippingMaterial() {
        foreach (Renderer i in robot.robotRenderers) {
            i.materials[^1] = null;
        }
    }

    private void EnableClippingMaterial() {
        foreach (Renderer i in robot.robotRenderers) {
            var materials = i.materials.ToList();
            materials.Add(ClippingMaterial);
            i.materials = materials.ToArray();
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

        /*YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue += 100;
        Debug.Log("Renderqueue: " + YZPlaneMesh.gameObject.GetComponent<MeshRenderer>().material.renderQueue);*/

        //gizmo.GetComponent<GizmoVariant>().GetChildren
        
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
