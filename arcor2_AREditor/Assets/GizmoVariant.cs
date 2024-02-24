using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoVariant : MonoBehaviour
{
    [SerializeField] private OutlineOnClick XYOutline;
    [SerializeField] private OutlineOnClick XZOutline;
    [SerializeField] private OutlineOnClick YZOutline;

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
    }

}
