using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using UnityEngine.Events;
using UnityEngine.Analytics;

public class SwitchComponent : MonoBehaviour, IParameter
{
    public SwitchManager Switch;
    public TMPro.TMP_Text Label;

    public Button SwitchButton;

    private bool interactable;

    private UnityAction<bool> onChangeCallback;
    public ManualTooltip ManualTooltip;

    public bool Interactable {
        get => interactable;
        set {
            interactable = value;
            SwitchButton.interactable = value;
        }
    }

    public string GetName() {
        return Label.text;
    }

    public object GetValue() {
        return Switch.isOn;
    }

    public void SetLabel(string value) {
        Label.text = value;
    }

    public void SetLabel(string label, string description) {
        SetLabel(label);
        if (!string.IsNullOrEmpty(description)) {
            ManualTooltip.Description = description;
            ManualTooltip.DisplayAlternativeDescription = false;
        } else {
            ManualTooltip.DisableTooltip();
        }
    }

    public void SetValue(object value) {
        SetValue(value, true);
    }

    public void SetValue(object value, bool invokeEvent) {
        if (value == null)
            return;
        bool newValue = (bool) value;
        Switch.isOn = newValue;
        // switch gets updated upon onEnable event
        Switch.gameObject.SetActive(false);
        Switch.gameObject.SetActive(true);
        if (invokeEvent) {
            // manually invoke switch methods, because they wont be invoked just by itself
            if (Switch.isOn) {
                Switch.OnEvents.Invoke();
            } else {
                Switch.OffEvents.Invoke();
            }
        }        
    }

    public void SetDarkMode(bool dark) {
        if (dark) {
            Label.color = Color.black;
        } else {
            Label.color = Color.white;
        }
    }

    public void AddOnValueChangedListener(UnityAction<bool> callback) {
        Switch.OnEvents.RemoveAllListeners();
        Switch.OnEvents.AddListener(OnChange);
        Switch.OffEvents.RemoveAllListeners();
        Switch.OffEvents.AddListener(OnChange);
        onChangeCallback = callback;
    }

    private void OnChange() {
        onChangeCallback.Invoke(Switch.isOn);
    }

    public string GetCurrentType() {
        return "boolean";
    }

    public Transform GetTransform() {
        return transform;
    }

    public void SetInteractable(bool interactable) {
        Interactable = interactable;
        Label.color = interactable ? Color.white : Color.gray;
    }

    public bool IsOn() {
        return (bool) GetValue();
    }
}
