using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

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

    public bool Flipped = false;

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

    public void AddMaterial(Material material) {
        ClippingPlane.GetComponent<ClippingPlane>().Materials.Add(material);
    }

    public void RemoveMaterial(Material material) {
        ClippingPlane.GetComponent<ClippingPlane>().Materials.Remove(material);
    }

    public void FlipX(bool flip) {
        Vector3 scale = gameObject.transform.localScale;
        if (flip) {
            scale.x = -Math.Abs(scale.x);
        } else {
            scale.x = Math.Abs(scale.x);
        }
        gameObject.transform.localScale = scale;
    }

    public void FlipY(bool flip) {
        Vector3 scale = gameObject.transform.localScale;
        if (flip) {
            scale.y = -Math.Abs(scale.y);
        } else {
            scale.y = Math.Abs(scale.y);
        }
        gameObject.transform.localScale = scale;
    }

    public void FlipZ(bool flip) {
        Vector3 scale = gameObject.transform.localScale;
        if (flip) {
            scale.z = -Math.Abs(scale.z);
        } else {
            scale.z = Math.Abs(scale.z);
        }
        gameObject.transform.localScale = scale;
    }

    public void RotateTo(float angle) {
        gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

}
