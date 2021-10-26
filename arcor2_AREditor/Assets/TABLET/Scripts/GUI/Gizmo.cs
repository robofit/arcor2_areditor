using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour {
    public enum Axis {
        X,
        Y,
        Z,
        NONE
    }

    [SerializeField] private GameObject XAxisRotation;
    [SerializeField] private GameObject YAxisRotation;
    [SerializeField] private GameObject ZAxisRotation;

    [SerializeField] private OutlineOnClick XAxisOutline;
    [SerializeField] private OutlineOnClick YAxisOutline;
    [SerializeField] private OutlineOnClick ZAxisOutline;

    [SerializeField] private TMPro.TMP_Text XAxisLabel;
    [SerializeField] private TMPro.TMP_Text YAxisLabel;
    [SerializeField] private TMPro.TMP_Text ZAxisLabel;

    public void SetRotationAxis(Axis axis) {
        switch (axis) {
            case Axis.X:
                XAxisRotation.SetActive(true);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(false);
                break;
            case Axis.Y:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(true);
                ZAxisRotation.SetActive(false);
                break;
            case Axis.Z:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(true);
                break;
            case Axis.NONE:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(false);
                break;
        }
    }

    public void HiglightAxis(Axis axis) {
        switch (axis) {
            case Axis.X:
                XAxisOutline.Highlight();
                YAxisOutline.UnHighlight();
                ZAxisOutline.UnHighlight();
                break;
            case Axis.Y:
                YAxisOutline.Highlight();
                XAxisOutline.UnHighlight();
                ZAxisOutline.UnHighlight();
                break;
            case Axis.Z:
                ZAxisOutline.Highlight();
                YAxisOutline.UnHighlight();
                XAxisOutline.UnHighlight();
                break;
        }
        
    }

    private string FormatValue(float value) {
        if (Mathf.Abs(value) < 0.000099f)
            return $"0cm";
        if (Mathf.Abs(value) < 0.00999f)
            return $"{value * 1000:0.##}mm";
        if (Mathf.Abs(value) < 0.9999f)
            return $"{value * 100:0.##}cm";
        return $"{value:0.###}m";
    }

    public void SetXDelta(float value) {        
        XAxisLabel.text = $"Δ{FormatValue(value)}";
    }

    public void SetYDelta(float value) {        
        YAxisLabel.text = $"Δ{FormatValue(value)}";
    }

    public void SetZDelta(float value) {        
        ZAxisLabel.text = $"Δ{FormatValue(value)}";
    }

    public void SetXDeltaRotation(float value) {        
        XAxisLabel.text = $"Δ{value:0.##}°";
    }

    public void SetYDeltaRotation(float value) {        
        YAxisLabel.text = $"Δ{value:0.##}°";
    }

    public void SetZDeltaRotation(float value) {        
        ZAxisLabel.text = $"Δ{value:0.##}°";
    }

}
