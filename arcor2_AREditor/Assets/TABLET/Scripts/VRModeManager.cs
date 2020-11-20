using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VRModeManager : Singleton<VRModeManager> {

    public Camera VRCamera;
    public Camera ARCamera;
    public TransformGizmo TFGizmo;
    public GameObject ARCameraVis;
    public GameObject GridPlane;
    public Joystick CameraMoveJoystick;
    public Joystick CameraMoveUpJoystick;
    public Joystick CameraRotateJoystick;
    public float WalkingSpeed = 5f;
    public float RotatingSpeed = 4f;

    private ARCameraBackground arCameraBG;
    private bool VRModeON = false;

    private Vector3 arCameraPosition;
    private Vector3 arCameraRotation;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float minimumX = -90f;
    private float maximumX = 90f;

    private float gridInitPos;

    private void Start() {
        arCameraBG = ARCamera.GetComponent<ARCameraBackground>();
        arCameraPosition = ARCamera.transform.position;
        arCameraRotation = ARCamera.transform.eulerAngles;

        CameraMoveJoystick.gameObject.SetActive(false);
        CameraMoveUpJoystick.gameObject.SetActive(false);
        CameraRotateJoystick.gameObject.SetActive(false);
        ARCameraVis.SetActive(false);
        VRCamera.enabled = false;
        GridPlane.SetActive(false);
        gridInitPos = GridPlane.transform.position.y;

        TrackingManager.Instance.NewLowestPlanePosition += AdjustGridPlane;
    }

    private void AdjustGridPlane(object sender, FloatEventArgs args) {
        if (args.Data < gridInitPos) {
            GridPlane.transform.position = new Vector3(GridPlane.transform.position.x, args.Data, GridPlane.transform.position.z);
        }
    }

    private void Update() {
        if (VRModeON) {
            float moveHorizontal = 0, moveVertical = 0,
                rotateHorizontal = 0, rotateVertical = 0, moveUp = 0;
            // Move with joysticks only when no menu is opened
            if (GameManager.Instance.SceneInteractable) {
                moveHorizontal = CameraMoveJoystick.Horizontal;
                moveVertical = CameraMoveJoystick.Vertical;
                rotateHorizontal = CameraRotateJoystick.Horizontal;
                rotateVertical = CameraRotateJoystick.Vertical;
                moveUp = CameraMoveUpJoystick.Vertical;
            }
            // Translate camera based on left joystick and movement of tablet
            VRCamera.transform.Translate(ARCamera.transform.InverseTransformDirection(ARCamera.transform.position - arCameraPosition), Space.Self);
            // Translate in horizontal plane
            VRCamera.transform.Translate(new Vector3(moveHorizontal, 0f, moveVertical) * Time.deltaTime * WalkingSpeed, Space.Self);
            // Lock the Y axis to move only on plane
            VRCamera.transform.localPosition = new Vector3(VRCamera.transform.localPosition.x, 0, VRCamera.transform.localPosition.z);
            // Translate the camera wrapper on Y axis to move up/down
            VRCamera.transform.parent.Translate(new Vector3(0f, moveUp, 0f) * Time.deltaTime * WalkingSpeed, Space.World);
            
            // Add joystick
            rotationY = VRCamera.transform.eulerAngles.y + rotateHorizontal * RotatingSpeed;
            rotationX = VRCamera.transform.eulerAngles.x - rotateVertical * RotatingSpeed;
            // Correct the rotation around X (when the rotation goes to negative values, euler angles becomes 360, we want it to stay negative)
            rotationX = (rotationX > 180) ? rotationX - 360 : rotationX;

            // Add rotation from AR camera (tablet)
            Vector3 rotation = new Vector3(rotationX, rotationY, VRCamera.transform.eulerAngles.z) + ARCamera.transform.eulerAngles - arCameraRotation;

            // Clamp rotation around X, to make sure it won't surpass the interval of <-90, 90>, because it would cause flickering, when camera is looking down/up
            VRCamera.transform.eulerAngles = new Vector3(Mathf.Clamp(rotation.x, minimumX, maximumX), rotation.y, rotation.z);

            // Actualize new values of the AR camera
            arCameraRotation = ARCamera.transform.eulerAngles;
            arCameraPosition = ARCamera.transform.position;

        }
    }

    public void EnableVRMode() {
        // Disable camera view
        arCameraBG.enabled = false;

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

        // Switch camera in Gizmo to VRCamera
        TFGizmo.myCamera = VRCamera;

        ARCamera.enabled = false;

        TrackingManager.Instance.ChangePlaneTransparency(false);

        VRModeON = true;
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

        // Switch camera in Gizmo back to ARCamera (tablet)
        TFGizmo.myCamera = ARCamera;

        TrackingManager.Instance.ChangePlaneTransparency(true);

        VRModeON = false;
    }
}
