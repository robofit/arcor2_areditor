using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputDialog : UniversalDialog {

    [SerializeField]
    private TMPro.TMP_InputField input;
    [SerializeField]
    private TMPro.TMP_Text placeholder;

    private Func<string, Base.RequestResult> validateFunc;

    public string GetValue() {
        return input.text;
    }

    public void SetInputHint(string hint) {
        placeholder.text = hint;
    }

    public void SetInputValue(string value) {
        input.text = value;
    }

    public void Open(string title, string description, string inputHint, string inputValue, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel", Func<string, Base.RequestResult> validateInput = null) {
        SetInputHint(inputHint);
        SetInputValue(inputValue);
        Open(title, description, confirmationCallback, cancelCallback, confirmLabel, cancelLabel);
        if (validateInput != null) {
            input.onValueChanged.AddListener((value) => Validate(value));
            validateFunc = validateInput;
        }
    }

    public void Validate(string value) {
        Base.RequestResult result = validateFunc.Invoke(value);
        if (result.Success) {
            okBtn.SetInteractivity(true, "");
        } else {
            okBtn.SetInteractivity(false, result.Message);
        }
            
    }
}
