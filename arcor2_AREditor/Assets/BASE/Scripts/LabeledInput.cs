using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using Newtonsoft.Json;

public class LabeledInput : MonoBehaviour, IParameter
{
    public string ParameterType;

    [SerializeField]
    private TMPro.TMP_Text Label;
    public TMPro.TMP_InputField Input;
    public TMPro.TMP_Text Text; 

    private TooltipContent tooltipContent;

    public void Init() {
        
    }

    private void Awake() {
        Debug.Assert(Label != null);
        Debug.Assert(Input != null);
        tooltipContent = Label.GetComponent<TooltipContent>();
        if (!string.IsNullOrEmpty(ParameterType)) {
            SetType(ParameterType);
        }
        SetLabel(Label.text, null);
    }

    private void Start() {
        
        
    }

    public void SetLabel(string label, string description) {

        Label.text = label;
        if (tooltipContent == null)
            return;
        if (!string.IsNullOrEmpty(description)) {
            tooltipContent.enabled = true;
            if (tooltipContent.tooltipRect == null) {
                tooltipContent.tooltipRect = Base.GameManager.Instance.Tooltip;
            }
            if (tooltipContent.descriptionText == null) {
                tooltipContent.descriptionText = Base.GameManager.Instance.Text;
            }
            tooltipContent.description = description;
        } else {
            tooltipContent.enabled = false;
        }
            
    }

    public void SetType(string contentType) {
        switch (contentType) {
            case "integer":
                Input.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
                break;
            case "double":
                Input.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
                break;
            default:
                Input.contentType = TMPro.TMP_InputField.ContentType.Standard;
                break;
        }
        ParameterType = contentType;
    }


    public object GetValue() {
        switch (ParameterType) {
            case "integer":
                return int.Parse(Input.text);
            case "double":
                return Base.Parameter.ParseDouble(Input.text);
            default:
                return Input.text;
        }
    }

    public string GetName() {
        return Label.text;
    }

    public void SetValue(object value) {
        Input.text = value.ToString();
    }

    public void SetDarkMode(bool dark) {
        if (dark) {
            Text.color = Color.black;
            Label.color = Color.black;
        } else {
            Text.color = Color.white;
            Label.color = Color.white;
        }
        
    }
}
