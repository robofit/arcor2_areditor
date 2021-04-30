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

    private Button switchButton;

    private bool interactable;

    private UnityAction<bool> onChangeCallback;

    private void Start() {
        switchButton = Switch.gameObject.GetComponent<Button>();
    }

    public bool Interactable {
        get => interactable;
        set {
            interactable = value;
            switchButton.interactable = value;
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
    }

    public void SetValue(object value) {
        if (value == null)
            return;
        bool newValue = (bool) value;
        Switch.isOn = newValue;
        // switch gets updated upon onEnable event
        Switch.gameObject.SetActive(false);
        Switch.gameObject.SetActive(true);
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
}
