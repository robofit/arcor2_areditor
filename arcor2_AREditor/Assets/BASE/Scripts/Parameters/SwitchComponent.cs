using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
public class SwitchComponent : MonoBehaviour, IActionParameter
{
    public SwitchManager Switch;
    public TMPro.TMP_Text Label;

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
        bool newValue = (bool) value;
        Switch.isOn = newValue;
        // switch gets updated upon onEnable event
        Switch.enabled = false;
        Switch.enabled = true;
    }
}
