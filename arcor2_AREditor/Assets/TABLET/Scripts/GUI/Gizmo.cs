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

}
