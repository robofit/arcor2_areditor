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
            VRCamera.transform.Translate(new Vector3(CameraMoveJoystick.Horizontal, 0f, CameraMoveJoystick.Vertical) * Time.deltaTime * WalkingSpeed);
            //VRCamera.transform.position += VRCamera.transform.rotation * (ARCamera.transform.position - arCameraPosition);

            // Add joystick
            rotationY = VRCamera.transform.eulerAngles.y + CameraRotateJoystick.Horizontal * RotatingSpeed;
            rotationX = VRCamera.transform.eulerAngles.x - CameraRotateJoystick.Vertical * RotatingSpeed;
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
        VRCamera.transform.position = ARCamera.transform.position;
        VRCamera.transform.rotation = ARCamera.transform.rotation;
        arCameraRotation = ARCamera.transform.eulerAngles;
        arCameraPosition = ARCamera.transform.position;
        
        CameraMoveJoystick.gameObject.SetActive(true);
        CameraRotateJoystick.gameObject.SetActive(true);

        // Switch Camera.main by tag
        ARCamera.tag = "Untagged";
        VRCamera.tag = "MainCamera";

        ARCameraVis.SetActive(true);

        // Switch camera in Gizmo to VRCamera
        TFGizmo.myCamera = VRCamera;

        ARCamera.enabled = false;
        
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

        // Switch camera in Gizmo back to ARCamera (tablet)
        TFGizmo.myCamera = ARCamera;

        VRModeON = false;
    }
}
