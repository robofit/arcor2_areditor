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

}
