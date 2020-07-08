using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCameraPosition : MonoBehaviour {
    
    public Vector3 InitialPosition = new Vector3(0, 0, 0);
    public Vector3 InitialRotation = new Vector3(0, 0, 0);

    private void Start() {
        Camera.main.transform.position = InitialPosition;
        Camera.main.transform.eulerAngles = InitialRotation;
    }

}
