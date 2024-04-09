using System;
using Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class VRModeManager : Singleton<VRModeManager> {

    public Camera VRCamera;
    public Camera ARCamera;
    public GameObject ARCameraVis;
    public GameObject GridPlane;
    public Joystick CameraMoveJoystick;
    public Joystick CameraMoveUpJoystick;
    public Joystick CameraRotateJoystick;
    public float WalkingSpeed = 5f;
    public float RotatingSpeed = 4f;
    public float DeadZone = 0f;
    public bool LinearJoysticks = true;
    public Canvas VRJoystickCanvas;
    public Image RightMenuHideBackground;
    public Image LeftMenuHideBackground;

    private VRJoystick CameraMoveVR;
    private VRJoystick CameraMoveUpVR;
    private VRJoystick CameraRotateVR;

    private ARCameraBackground arCameraBG;
    public bool VRModeON { get; private set; } = false;

    private Vector3 arCameraPosition;
    private Vector3 arCameraRotation;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float minimumX = -90f;
    private float maximumX = 90f;

    private float gridInitPos;

    private void Start() {
#if UNITY_ANDROID
        VRModeON = false;
        arCameraBG = ARCamera.GetComponent<ARCameraBackground>();
        arCameraPosition = ARCamera.transform.position;
        arCameraRotation = ARCamera.transform.eulerAngles;

        CameraMoveVR = CameraMoveJoystick.GetComponent<VRJoystick>();
        CameraMoveUpVR = CameraMoveUpJoystick.GetComponent<VRJoystick>();
        CameraRotateVR = CameraRotateJoystick.GetComponent<VRJoystick>();

        CameraMoveJoystick.gameObject.SetActive(false);
        CameraMoveUpJoystick.gameObject.SetActive(false);
        CameraRotateJoystick.gameObject.SetActive(false);
        ARCameraVis.SetActive(false);
        VRCamera.enabled = false;
        GridPlane.SetActive(false);
        gridInitPos = GridPlane.transform.position.y;

#if AR_ON
        TrackingManager.Instance.NewLowestPlanePosition += AdjustGridPlane;
#endif

#endif
    }

    private void AdjustGridPlane(object sender, FloatEventArgs args) {
        if (args.Data < gridInitPos) {
            GridPlane.transform.position = new Vector3(GridPlane.transform.position.x, args.Data, GridPlane.transform.position.z);
        }
    }

    private void Update() {
#if UNITY_ANDROID
        if (VRModeON) {
            float moveHorizontal = 0, moveVertical = 0,
                rotateHorizontal = 0, rotateVertical = 0, moveUp = 0;
            // Move with joysticks only when no menu is opened
            if (GameManager.Instance.SceneInteractable) {
                //moveHorizontal = CameraMoveJoystick.Horizontal;
                //moveVertical = CameraMoveJoystick.Vertical;
                //rotateHorizontal = CameraRotateJoystick.Horizontal;
                //rotateVertical = CameraRotateJoystick.Vertical;
                //moveUp = CameraMoveUpJoystick.Vertical;
                moveHorizontal = Math.Abs(CameraMoveJoystick.Horizontal) > DeadZone ? CameraMoveJoystick.Horizontal : 0f;
                moveVertical = Math.Abs(CameraMoveJoystick.Vertical) > DeadZone ? CameraMoveJoystick.Vertical : 0f;
                rotateHorizontal = Math.Abs(CameraRotateJoystick.Horizontal) > DeadZone ? CameraRotateJoystick.Horizontal : 0f;
                rotateVertical = Math.Abs(CameraRotateJoystick.Vertical) > DeadZone ? CameraRotateJoystick.Vertical : 0f;
                moveUp = Math.Abs(CameraMoveUpJoystick.Vertical) > DeadZone ? CameraMoveUpJoystick.Vertical : 0f;

                if (!LinearJoysticks) {
                    moveHorizontal = Mathf.Pow(moveHorizontal, 2) * Mathf.Sign(moveHorizontal);
                    moveVertical = Mathf.Pow(moveVertical, 2) * Mathf.Sign(moveVertical);
                    rotateHorizontal = Mathf.Pow(rotateHorizontal, 2) * Mathf.Sign(rotateHorizontal);
                    rotateVertical = Mathf.Pow(rotateVertical, 2) * Mathf.Sign(rotateVertical);
                    moveUp = Mathf.Pow(moveUp, 2) * Mathf.Sign(moveUp);
                }
            }
            // Translate camera based on left joystick and movement of tablet
            VRCamera.transform.Translate(ARCamera.transform.InverseTransformDirection(ARCamera.transform.position - arCameraPosition), Space.Self);
            float posY = VRCamera.transform.localPosition.y;
            // Translate in horizontal plane
            VRCamera.transform.Translate(new Vector3(moveHorizontal, 0f, moveVertical) * Time.deltaTime * WalkingSpeed, Space.Self);
            // Negate Y axis traslation
            VRCamera.transform.localPosition = new Vector3(VRCamera.transform.localPosition.x, posY, VRCamera.transform.localPosition.z);
            // Translate the camera wrapper on Y axis to move up/down
            VRCamera.transform.parent.Translate(new Vector3(0f, moveUp, 0f) * Time.deltaTime * WalkingSpeed, Space.World);

            // Add joystick
            rotationY = VRCamera.transform.eulerAngles.y + rotateHorizontal * RotatingSpeed;
            rotationX = VRCamera.transform.eulerAngles.x - rotateVertical * RotatingSpeed;
            // Correct the rotation around X (when the rotation goes to negative values, euler angles becomes 360, we want it to stay negative)
            rotationX = (rotationX > 180) ? rotationX - 360 : rotationX;

            // Add rotation from AR camera (tablet)
            Vector3 rotation = new Vector3(rotationX, rotationY, VRCamera.transform.eulerAngles.z) + ARCamera.transform.eulerAngles - arCameraRotation;
            VRCamera.transform.eulerAngles = rotation;

            // Update new values of the AR camera
            arCameraRotation = ARCamera.transform.eulerAngles;
            arCameraPosition = ARCamera.transform.position;
        }
#endif
    }

    public void EnableVRMode() {
        float cameraFoV = ARCamera.fieldOfView;

        // Disable camera view. By disabling, FoV will change back to original value 60, keep the FoV all time the same as ARCameraBackground sets it.
        arCameraBG.enabled = false;
        ARCamera.fieldOfView = cameraFoV;
        VRCamera.fieldOfView = cameraFoV;
        VRCamera.enabled = true;

        // Init position/rotation variables
        VRCamera.transform.parent.position = ARCamera.transform.localPosition;
        VRCamera.transform.localPosition = Vector3.zero;
        VRCamera.transform.rotation = ARCamera.transform.rotation;
        arCameraRotation = ARCamera.transform.eulerAngles;
        arCameraPosition = ARCamera.transform.position;

        CameraMoveJoystick.gameObject.SetActive(true);
        CameraMoveUpJoystick.gameObject.SetActive(true);
        CameraRotateJoystick.gameObject.SetActive(true);

        GridPlane.SetActive(true);

        // Switch Camera.main by tag
        ARCamera.tag = "Untagged";
        VRCamera.tag = "MainCamera";

        ARCameraVis.SetActive(true);

        ARCamera.enabled = false;

        TrackingManager.Instance.ChangePlaneTransparency(false);

        VRModeON = true;

        SceneManager.Instance.SetVisibilityActionObjects(PlayerPrefsHelper.LoadFloat("AOVisibilityVR", 1f));
    }

    public void DisableVRMode() {
        // Enable camera view
        arCameraBG.enabled = true;
        ARCamera.enabled = true;

        VRCamera.enabled = false;
        CameraMoveJoystick.gameObject.SetActive(false);
        CameraMoveUpJoystick.gameObject.SetActive(false);
        CameraRotateJoystick.gameObject.SetActive(false);

        GridPlane.SetActive(false);

        // Switch Camera.main by tag
        ARCamera.tag = "MainCamera";
        VRCamera.tag = "Untagged";

        ARCameraVis.SetActive(false);

        TrackingManager.Instance.ChangePlaneTransparency(true);

        VRModeON = false;

        SceneManager.Instance.SetVisibilityActionObjects(PlayerPrefsHelper.LoadFloat("AOVisibilityAR", 0f));
    }

    public void EnableVRJoysticks(bool enable) {
        if (enable && (CameraMoveVR.VRJoystickEnabled || CameraMoveUpVR.VRJoystickEnabled || CameraRotateVR.VRJoystickEnabled)) {
            VRJoystickCanvas.sortingOrder = 1;
            LeftMenuHideBackground.enabled = true;
            RightMenuHideBackground.enabled = true;
        } else if (!enable && !(CameraMoveVR.VRJoystickEnabled || CameraMoveUpVR.VRJoystickEnabled || CameraRotateVR.VRJoystickEnabled)) {
            VRJoystickCanvas.sortingOrder = -1;
            LeftMenuHideBackground.enabled = false;
            RightMenuHideBackground.enabled = false;
        }
    }
}
