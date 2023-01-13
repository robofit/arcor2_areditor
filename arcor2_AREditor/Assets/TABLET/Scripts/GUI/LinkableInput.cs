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
       
    }




    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        this.onChangeParameterHandler = onChangeParameterHandler;
    }

    public override void SetType(string type, bool linkable, bool switchBtnClicked) {
        base.SetType(type, linkable, switchBtnClicked);

        if (type == LINK) {
            //Input.gameObject.SetActive(false);
            //Input.Input.onValueChanged.
            //Input.Input.onValueChanged.RemoveAllListeners();
        } else if(type == PROJECT_PARAMETER) {
            //Input.gameObject.SetActive(false);
            //Input.Input.onValueChanged.RemoveAllListeners();
        } else {
            
            //Input.gameObject.SetActive(true);
            //Input.Input.onValueChanged.RemoveAllListeners();
            Input.SetType(type);
            switch (ParameterMetadata.Type) {
                case "integer":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<int>());
                    }
                    
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    else
                        Input.Input.onValueChanged.AddListener((string value) => OnChangeInt(value, type));
                    break;
                case "double":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<double>());
                    }
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    else
                        Input.Input.onValueChanged.AddListener((string value) => OnChangeDouble(value, type));
                    break;
                case "string":
                    if (string.IsNullOrEmpty(Input.Input.text)) {
                        Input.SetValue(ParameterMetadata.GetDefaultValue<string>());
                    }
                    if (switchBtnClicked)
                        Input.Input.onValueChanged.Invoke(Input.Input.text);
                    else
                        Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), value, type));
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

    protected override object GetDefaultValue() {
        return null;
    }
}
