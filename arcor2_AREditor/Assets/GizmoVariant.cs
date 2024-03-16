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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HighlightXY() {
        /*XYOutline.Highlight();
        XZOutline.UnHighlight();
        YZOutline.UnHighlight();*/
    }

    public void HighlightXZ() {
        /*XZOutline.Highlight();
        XYOutline.UnHighlight();
        YZOutline.UnHighlight();*/
    }

    public void HighlightYZ() {
        /*YZOutline.Highlight();
        XYOutline.UnHighlight();
        XZOutline.UnHighlight();*/
    }

    public void UnhighlightAll() {
        /*XYOutline.UnHighlight();
        XZOutline.UnHighlight();
        YZOutline.UnHighlight();*/
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

}
