using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using Newtonsoft.Json;
using System;

public class LabeledInput : MonoBehaviour, IParameter
{
    public string ParameterType;

    [SerializeField]
    private TMPro.TMP_Text Label;
    public TMPro.TMP_InputField Input;

    public ManualTooltip ManualTooltip;

    public void Init() {
        
    }

    private void Awake() {
        Debug.Assert(Label != null);
        Debug.Assert(Input != null);
        Debug.Assert(ManualTooltip != null);
        if (!string.IsNullOrEmpty(ParameterType)) {
            SetType(ParameterType);
        }
        SetLabel(Label.text, null);
    }

    private void Start() {
        
        
    }

    public void SetLabel(string label, string description) {

        Label.text = label;
        if (!string.IsNullOrEmpty(description)) {
            ManualTooltip.Description = description;
            ManualTooltip.DisplayAlternativeDescription = false;
        } else {
            ManualTooltip.DisableTooltip();
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
        Input.text = value?.ToString();
    }

    public void SetDarkMode(bool dark) {
        if (dark) {
            Input.textComponent.color = Color.black;
            Label.color = Color.black;
        } else {
            Input.textComponent.color = Color.white;
            Label.color = Color.white;
        }
        
    }

    public string GetCurrentType() {
        return ParameterType;
    }

    public Transform GetTransform() {
        return transform;
    }

    public void SetInteractable(bool interactable) {
        try {
            Input.interactable = interactable;
            Input.textComponent.color = interactable ? Color.white : Color.gray;
            Label.color = interactable ? Color.white : Color.gray;
        } catch (NullReferenceException ex) {
            Debug.LogError($"Null reference exception on labeled input: {GetName()}: {ex.Message}");
        }
        
    }
}
