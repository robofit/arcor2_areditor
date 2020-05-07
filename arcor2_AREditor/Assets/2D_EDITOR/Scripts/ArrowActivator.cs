using System;
using Base;
using UnityEngine;

public class ArrowActivator : MonoBehaviour {

    private void Start() {
        Base.GameManager.Instance.OnRunPackage += ActivateArrow;
        Base.GameManager.Instance.OnResumePackage += ActivateArrow;
        Base.GameManager.Instance.OnStopPackage += DeactivateArrow;
    }

    private void ActivateArrow(object sender, ProjectMetaEventArgs args) {
        gameObject.SetActive(true);
    }

    private void DeactivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(false);
    }
}
