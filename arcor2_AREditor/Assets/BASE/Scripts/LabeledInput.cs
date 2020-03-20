using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Michsky.UI.ModernUIPack;

public class LabeledInput : MonoBehaviour, IActionParameter
{
    public TMPro.TMP_Text Label;
    public TMPro.TMP_InputField Input;

    public void Init() {
        
    }

    public void SetLabel(string label, string description) {
        Label.text = label;
        if (Label.GetComponent<TooltipContent>().tooltipRect == null) {
            Label.GetComponent<TooltipContent>().tooltipRect = Base.GameManager.Instance.Tooltip;
        }
        if (Label.GetComponent<TooltipContent>().descriptionText == null) {
            Label.GetComponent<TooltipContent>().descriptionText = Base.GameManager.Instance.Text;
        }
        Label.GetComponent<TooltipContent>().description = description;
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
