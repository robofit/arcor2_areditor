using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class FreeCameraMover : MonoBehaviour
{
    public bool ControlWithMouseOnly = true;
    public float CameraRotationSpeed = 4f;

    [ConditionalField("ControlWithMouseOnly", inverse:true)]
    public float WalkingSpeed = 5f;

    [ConditionalField("ControlWithMouseOnly")]
    public float zoomSpeed = 2f;

    [ConditionalField("ControlWithMouseOnly")]
    public float dragSpeed = 4f;

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start() {
        Vector3 defaultCameraRotation = GetComponent<DefaultCameraPosition>().InitialRotation;
        yaw = defaultCameraRotation.y;
        pitch = defaultCameraRotation.x;
    }

    // Update is called once per frame
    void Update() {
        if (Base.GameManager.Instance.SceneInteractable) {
            //rotate with right mouse
            if (Input.GetMouseButton(1)) {
                yaw += CameraRotationSpeed * Input.GetAxis("Mouse X");
                pitch -= CameraRotationSpeed * Input.GetAxis("Mouse Y");

                Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }

            if (ControlWithMouseOnly) {
                //drag camera around with Middle Mouse
                if (Input.GetMouseButton(2)) {
                    Camera.main.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
                }

                //Zoom in and out with Mouse Wheel
                Camera.main.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
            } else {
                Vector3 translation = GetDirection();
                Camera.main.transform.Translate(translation * Time.deltaTime * WalkingSpeed);
            }
        }
    }

    private Vector3 GetDirection() {
        if (Input.GetKey(KeyCode.W)) {
            return new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            return new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A)) {
            return new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            return new Vector3(1, 0, 0);
        }
        return Vector3.zero;
    }
}
