using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CreateAnchor : MonoBehaviour
{
    private ARReferencePointManager arRefPointManager;
    private ARPlaneManager arPlaneManager;
    private ARReferencePoint worldAnchor;

    // Start is called before the first frame update
    private void Start() {
        arRefPointManager = GameObject.FindWithTag("AR_MANAGERS").GetComponent<ARReferencePointManager>();
        arPlaneManager = GameObject.FindWithTag("AR_MANAGERS").GetComponent<ARPlaneManager>();
    }

    public void OnClick() {
        Debug.Log("ADDING ANCHOR");
#if UNITY_EDITOR
        Base.Scene.Instance.transform.parent = transform;
#else
        worldAnchor = arRefPointManager.AddReferencePoint(new Pose(transform.position, transform.rotation));
        Base.Scene.Instance.transform.parent = worldAnchor.transform;
#endif
        Base.Scene.Instance.transform.localPosition = new Vector3(0f, 0f, 0f);
        Base.Scene.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        //Base.Scene.Instance.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }
}
