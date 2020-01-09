using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindParentToChild : MonoBehaviour
{

    public GameObject ChildToBind;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    // Start is called before the first frame update
    private void Start() {
        // Save original child transform
        originalLocalPosition = ChildToBind.transform.localPosition;
        originalLocalRotation = ChildToBind.transform.localRotation;
    }

    // Update is called once per frame
    private void Update() {
        // Update parent transform to match moved child
        transform.position = ChildToBind.transform.position;
        transform.rotation = ChildToBind.transform.rotation;

        // Set child transform back to original values
        ChildToBind.transform.localPosition = originalLocalPosition;
        ChildToBind.transform.localRotation = originalLocalRotation;
    }
}
