using System;
using UnityEngine;

public class ArrowActivator : MonoBehaviour {

    private void Start() {
        Base.GameManager.Instance.OnRunPackage += ActivateArrow;
        Base.GameManager.Instance.OnResumeProject += ActivateArrow;
        Base.GameManager.Instance.OnStopProject += DeactivateArrow;
    }
    
    private void ActivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(true);
    }

    private void DeactivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(false);
    }
}
