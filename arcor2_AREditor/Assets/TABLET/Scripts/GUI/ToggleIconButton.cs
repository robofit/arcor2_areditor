using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleIconButton : MonoBehaviour
{

    private void Awake() {
        Button = GetComponent<Button>();
        if (PersistentState) {
            Toggle(PlayerPrefsHelper.LoadBool("buttons/" + PersistentTag, true), false);
        }
    }
    public Image Icon;
    [HideInInspector]
    public Button Button;
    private bool toggled = true;
    [Tooltip("Defines if state of this button should be persistent")]
    public bool PersistentState = false;
    [Tooltip("If PersistentState is true, this has to be unique.")]
    public string PersistentTag = "";
    

    public UnityEvent OnToggledOn, OnToggledOff;

    public bool Toggled => toggled;

    public void SwitchToggle() {
        Toggle(!Toggled);
    }

    public void Toggle(bool toggle, bool invoke = true) {
        PlayerPrefsHelper.SaveBool("buttons/" + PersistentTag, toggle);
        this.toggled = toggle;
        if (Toggled) {
            Icon.color = Color.white;
            if (invoke)
                OnToggledOn.Invoke();
        } else {
            Icon.color = Color.grey;
            if (invoke)
                OnToggledOff.Invoke();
        }
    }



}
