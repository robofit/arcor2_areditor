using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCameraPosition : MonoBehaviour {
    
    public Vector3 InitialPosition = new Vector3(0f, 1.5f, -2f);
    public Vector3 InitialRotation = new Vector3(45f, 0f, 0f);

    private void Start() {
        Camera.main.transform.position = InitialPosition;
        Camera.main.transform.eulerAngles = InitialRotation;
    }

}
