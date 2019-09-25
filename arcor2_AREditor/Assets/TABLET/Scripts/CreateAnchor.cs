using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CreateAnchor : MonoBehaviour
{
    [SerializeField]
    private GameObject scenePrefab;

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
        worldAnchor = arRefPointManager.AddReferencePoint(new Pose(transform.position, transform.rotation));
        //arPlaneManager.GetPlane()
        //worldAnchor = arRefPointManager.AttachReferencePoint()
        GameObject scene = Instantiate(scenePrefab);
        scene.transform.parent = worldAnchor.transform;
        scene.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        scene.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
    }
}
