using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class LabeledInput : MonoBehaviour, IActionParameter
{
    public TMPro.TMP_Text Label;
    public TMPro.TMP_InputField Input;

    public void SetLabel(string label) {
        Label.text = label;        
    }

    public void SetType(TMPro.TMP_InputField.ContentType contentType) {
        Input.contentType = contentType;
    }


    public void SetValue(string value) {
        Input.text = value;
    }

    public object GetValue() {
        return Input.text;
    }


}
