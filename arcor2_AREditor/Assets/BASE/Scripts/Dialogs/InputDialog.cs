using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class InputDialog : UniversalDialog {

    [SerializeField]
    private TMPro.TMP_InputField input;
    [SerializeField]
    private TMPro.TMP_Text placeholder;

    private Func<string, Task<Base.RequestResult>> validateFunc;

    public string GetValue() {
        return input.text;
    }

    public void SetInputHint(string hint) {
        placeholder.text = hint;
    }

    public void SetInputValue(string value) {
        input.text = value;        
        Validate(value);
    }

    public void Open(string title, string description, string inputHint, string inputValue, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel", Func<string, Task<Base.RequestResult>> validateInput = null) {
        SetInputHint(inputHint);
        validateFunc = null;
        Open(title, description, confirmationCallback, cancelCallback, confirmLabel, cancelLabel);
        if (validateInput != null) {
            input.onValueChanged.AddListener((value) => Validate(value));
            validateFunc = validateInput;
        }

        SetInputValue(inputValue);

    }

    public async void Validate(string value) {
        if (validateFunc == null) {
            okBtn.SetInteractivity(true, "");
            return;
        }
            
        Base.RequestResult result = await validateFunc.Invoke(value);
        if (result.Success) {
            okBtn.SetInteractivity(true, "");
        } else {
            okBtn.SetInteractivity(false, result.Message);
        }
            
    }
}
