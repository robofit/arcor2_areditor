using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowActivator : MonoBehaviour
{

    private void OnEnable() {
        GameManager.Instance.OnRunProject += ActivateArrow;
        GameManager.Instance.OnResumeProject += ActivateArrow;
        GameManager.Instance.OnStopProject += DeactivateArrow;
    }

    private void OnDisable() {
        GameManager.Instance.OnRunProject -= ActivateArrow;
        GameManager.Instance.OnResumeProject -= ActivateArrow;
        GameManager.Instance.OnStopProject -= DeactivateArrow;
    }

    private void ActivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(true);
    }

    private void DeactivateArrow(object sender, EventArgs eventArgs) {
        gameObject.SetActive(false);
    }
}
