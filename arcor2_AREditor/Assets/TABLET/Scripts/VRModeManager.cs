using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VRModeManager : Singleton<VRModeManager> {

    public Camera VRCamera;
    public Camera ARCamera;
    public GameObject VRCameraBase;
    public TransformGizmo TFGizmo;
    public GameObject ARCameraVis;
    public Joystick CameraMoveJoystick;
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

    private void Start() {
        arCameraBG = ARCamera.GetComponent<ARCameraBackground>();
        arCameraPosition = ARCamera.transform.position;
        arCameraRotation = ARCamera.transform.eulerAngles;

        CameraMoveJoystick.gameObject.SetActive(false);
        CameraRotateJoystick.gameObject.SetActive(false);
        ARCameraVis.SetActive(false);
        VRCamera.enabled = false;
    }

    private void Update() {
        if (VRModeON) {
            float moveHorizontal = 0, moveVertical = 0,
                rotateHorizontal = 0, rotateVertical = 0;
            // Move with joysticks only when no menu is opened
            if (GameManager.Instance.SceneInteractable) {
                moveHorizontal = CameraMoveJoystick.Horizontal;
                moveVertical = CameraMoveJoystick.Vertical;
                rotateHorizontal = CameraRotateJoystick.Horizontal;
                rotateVertical = CameraRotateJoystick.Vertical;
            }
            // Translate camera based on left joystick and movement of tablet
            VRCamera.transform.Translate(ARCamera.transform.InverseTransformDirection(ARCamera.transform.position - arCameraPosition), Space.Self);
            VRCamera.transform.Translate(new Vector3(moveHorizontal, 0f, moveVertical) * Time.deltaTime * WalkingSpeed);
            
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
        VRCamera.transform.position = ARCamera.transform.localPosition;
        VRCamera.transform.rotation = ARCamera.transform.rotation;
        arCameraRotation = ARCamera.transform.eulerAngles;
        arCameraPosition = ARCamera.transform.position;
        
        CameraMoveJoystick.gameObject.SetActive(true);
        CameraRotateJoystick.gameObject.SetActive(true);

        // Switch Camera.main by tag
        ARCamera.tag = "Untagged";
        VRCamera.tag = "MainCamera";

        ARCameraVis.SetActive(true);

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
        CameraRotateJoystick.gameObject.SetActive(false);

        // Switch Camera.main by tag
        ARCamera.tag = "MainCamera";
        VRCamera.tag = "Untagged";

        ARCameraVis.SetActive(false);

        TrackingManager.Instance.ChangePlaneTransparency(true);

        VRModeON = false;
    }
}
