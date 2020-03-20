using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using Newtonsoft.Json;

public class LabeledInput : MonoBehaviour, IActionParameter
{
    public string ParameterType;

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

    public void SetType(string contentType) {
        switch (ParameterType) {
            case "integer":
                Input.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
                break;
            case "double":
                Input.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
                break;
            default:
                Input.contentType = TMPro.TMP_InputField.ContentType.Alphanumeric;
                break;
        }
        ParameterType = contentType;
    }



    public object GetValue() {
        switch (ParameterType) {
            case "integer":
                return int.Parse(Input.text);
            case "double":
                return Base.Action.ParseDouble(Input.text);
            case "relative_pose":
                return JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(Input.text);
            default:
                return Input.text;
        }
    }

    public string GetName() {
        return Label.text;
    }

    public void SetValue(object value) {
        Input.text = (string) value;
    }
}
