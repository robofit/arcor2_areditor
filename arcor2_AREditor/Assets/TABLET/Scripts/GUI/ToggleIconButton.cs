using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleIconButton : MonoBehaviour
{

    private void Awake() {
        Button = GetComponent<Button>();
    }
    public Image Icon;
    [HideInInspector]
    public Button Button;
    private bool toggled = true;
    

    public UnityEvent OnToggledOn, OnToggledOff;

    public void SwitchToggle() {
        Toggle(!toggled);
    }

    public void Toggle(bool toggle) {
        toggled = toggle;
        if (toggled) {
            Icon.color = Color.white;
            OnToggledOn.Invoke();
        } else {
            Icon.color = Color.grey;
            OnToggledOff.Invoke();
        }
    }

}
