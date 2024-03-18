using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoVariant : MonoBehaviour
{
    public OutlineOnClick XYOutline;
    public OutlineOnClick XZOutline;
    public OutlineOnClick YZOutline;

    public GameObject XYPlaneMesh;
    public GameObject XZPlaneMesh;
    public GameObject YZPlaneMesh;

    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;

    public GameObject XCone;
    public GameObject YCone;
    public GameObject ZCone;

    public GameObject ClippingPlane;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HighlightXY() {
        XYOutline.Highlight();
        XZOutline.UnHighlight();
        YZOutline.UnHighlight();
    }

    public void HighlightXZ() {
        XZOutline.Highlight();
        XYOutline.UnHighlight();
        YZOutline.UnHighlight();
    }

    public void HighlightYZ() {
        YZOutline.Highlight();
        XYOutline.UnHighlight();
        XZOutline.UnHighlight();
    }

    public void UnhighlightAll() {
        XYOutline.UnHighlight();
        XZOutline.UnHighlight();
        YZOutline.UnHighlight();
        XCone.SetActive(true);
        YCone.SetActive(true);
        ZCone.SetActive(true);
    }

    public void HideXCone() {
        XCone.SetActive(false);
    }

    public void HideYCone() {
        YCone.SetActive(false);
    }

    public void HideZCone() {
        ZCone.SetActive(false);
    }

    public void SetXZClippingPlane() {
        ClippingPlane.transform.SetParent(XZPlaneMesh.transform);
        ClippingPlane.transform.position = Vector3.zero;
        ClippingPlane.transform.rotation = Quaternion.Euler(0f, -90f, -90f);
    }

    public void SetYZClippingPlane() {
        ClippingPlane.transform.SetParent(YZPlaneMesh.transform);
        ClippingPlane.transform.position = Vector3.zero;
        ClippingPlane.transform.rotation = Quaternion.Euler(0f, -180f, -180f);
    }

    public void SetXYClippingPlane() {
        ClippingPlane.transform.SetParent(XYPlaneMesh.transform);
        ClippingPlane.transform.position = Vector3.zero;
        ClippingPlane.transform.rotation = Quaternion.Euler(0f, -180f, -180f);
    }

    public void SetDir(bool dir) {
        ClippingPlane.GetComponent<ClippingPlane>().dir = dir;
    }

}
