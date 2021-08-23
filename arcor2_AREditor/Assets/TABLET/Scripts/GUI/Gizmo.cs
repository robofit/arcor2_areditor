using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour {

    [SerializeField] private GameObject XAxisRotation;
    [SerializeField] private GameObject YAxisRotation;
    [SerializeField] private GameObject ZAxisRotation;

    public void SetRotationAxis(TransformMenu.Axis axis) {
        switch (axis) {
            case TransformMenu.Axis.X:
                XAxisRotation.SetActive(true);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(false);
                break;
            case TransformMenu.Axis.Y:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(true);
                ZAxisRotation.SetActive(false);
                break;
            case TransformMenu.Axis.Z:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(true);
                break;
            case TransformMenu.Axis.NONE:
                XAxisRotation.SetActive(false);
                YAxisRotation.SetActive(false);
                ZAxisRotation.SetActive(false);
                break;
        }
    }

}
