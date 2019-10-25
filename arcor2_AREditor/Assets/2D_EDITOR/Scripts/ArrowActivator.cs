using System;
using UnityEngine;

public class ArrowActivator : MonoBehaviour {

    private void OnEnable() {
        Base.GameManager.Instance.OnRunProject += ActivateArrow;
        Base.GameManager.Instance.OnResumeProject += ActivateArrow;
        Base.GameManager.Instance.OnStopProject += DeactivateArrow;
    }

    private void OnDisable() {
        Base.GameManager.Instance.OnRunProject -= ActivateArrow;
        Base.GameManager.Instance.OnResumeProject -= ActivateArrow;
        Base.GameManager.Instance.OnStopProject -= DeactivateArrow;
    }

    private void ActivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(true);
    }

    private void DeactivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(false);
    }
}
