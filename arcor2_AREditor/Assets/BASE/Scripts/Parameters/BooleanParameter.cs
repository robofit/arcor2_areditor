using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BooleanParameter : MonoBehaviour, IParameter {
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private TMPro.TMP_Text label;

    public string GetName() {
        return label.text;
    }

    public object GetValue() {
        return toggle.isOn;
    }

    public void SetLabel(string label, string description) {
        this.label.text = label;
    }

    public void SetValue(object value) {
        toggle.isOn = (bool) value;
    }

    public void AddOnValueChangedListener(UnityAction<bool> callback) {
        toggle.onValueChanged.AddListener(callback);
    }

    public void SetDarkMode(bool dark) {
        throw new System.NotImplementedException();
    }

    public string GetCurrentType() {
        return "boolean";
    }
}
