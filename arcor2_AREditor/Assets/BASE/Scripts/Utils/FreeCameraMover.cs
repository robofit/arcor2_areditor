using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMover : MonoBehaviour
{
    [SerializeField]
    private float WalkingSpeed = 5f;
    [SerializeField]
    private float CameraRotationSpeed = 0.5f;

    private Vector3 mousePosition;

    // Start is called before the first frame update
    void Start() {
        mousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.Mouse1)) {
            mousePosition = Input.mousePosition - mousePosition;
            mousePosition = new Vector3(-mousePosition.y * CameraRotationSpeed, mousePosition.x * CameraRotationSpeed, 0);
            mousePosition = new Vector3(transform.eulerAngles.x + mousePosition.x, transform.eulerAngles.y + mousePosition.y, 0);
            transform.eulerAngles = mousePosition;
        }
        mousePosition = Input.mousePosition;

        Vector3 translation = GetDirection();
        transform.Translate(translation * Time.deltaTime * WalkingSpeed);        
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
