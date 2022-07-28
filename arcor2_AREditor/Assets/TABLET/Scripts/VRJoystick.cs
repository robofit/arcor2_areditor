using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public bool VRJoystickEnabled {
        private set; get;
    }

    private void Awake() {
        VRJoystickEnabled = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        EnableVRJoysticks(true);
    }

    public void OnPointerUp(PointerEventData eventData) {
        EnableVRJoysticks(false);
    }

    public void EnableVRJoysticks(bool enable) {
        VRJoystickEnabled = enable;
        VRModeManager.Instance.EnableVRJoysticks(enable);
    }

}
