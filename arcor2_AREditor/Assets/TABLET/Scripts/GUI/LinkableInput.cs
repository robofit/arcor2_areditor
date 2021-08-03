using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableInput : LinkableParameter {
    public LabeledInput Input;

    private object defaultValue;

    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        Parameter = Input;
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);

        SetOnValueChanged(onChangeParameterHandler);
        SetValue(value);
        //object v = value;
        /*switch (type) {
            case "string":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<string>();
                break;
            case "integer":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<int>();
                break;
            case "double":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<double>();
                break;
            case "link":
                if (value == null)
                    v = null;
                break;
            default:
                v = null;
                break;
        }*/
    }




    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //input.Input.Input.onValueChanged.AddListener((string newValue)
        //        => onChangeParameterHandler(actionParameterMetadata.Name, newValue, type));
        this.onChangeParameterHandler = onChangeParameterHandler;
    }

    public override void SetType(string type, bool linkable, bool switchBtnClicked) {
        base.SetType(type, linkable, switchBtnClicked);

        if (type == "link") {
            //Input.gameObject.SetActive(false);
            Input.Input.onValueChanged.RemoveAllListeners();
        } else if(type == ProjectParameterText) {
            //Input.gameObject.SetActive(false);
            Input.Input.onValueChanged.RemoveAllListeners();
        } else {
            
            //Input.gameObject.SetActive(true);
            Input.Input.onValueChanged.RemoveAllListeners();
            Input.SetType(type);
            switch (ParameterMetadata.Type) {
                case "integer":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<int>());
                    }
                    
                    Input.Input.onValueChanged.AddListener((string value) => OnChangeInt(value, type));
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    break;
                case "double":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<double>());
                    }
                    Input.Input.onValueChanged.AddListener((string value) => OnChangeDouble(value, type));
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    break;
                case "string":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<string>());
                    }
                    Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), value, type));
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    break;
            }
            
        }
    }

    private void OnChangeDouble(string value, string type) {
        double dValue;
        try {
            dValue = double.Parse(value, CultureInfo.InvariantCulture);
            onChangeParameterHandler(Input.GetName(), dValue, type);
        } catch (FormatException) {
            return;
        }
    }

    private void OnChangeInt(string value, string type) {
        int iValue;
        try {
            iValue = int.Parse(value);
            onChangeParameterHandler(Input.GetName(), iValue, type);
        } catch (FormatException) {
            return;
        }
    }

    public override void SetInteractable(bool interactable) {
        Input.SetInteractable(interactable);
    }
}
