using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableInput : LinkableParameter {
   public LabeledInput Input;


    public void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //Parameter.GetValue<int?>();

        InitDropdown(layoutGroupToBeDisabled, canvasRoot);
        ParameterMetadata = parameterMetadata;
        SetType(type);
        SetOnValueChanged(onChangeParameterHandler);
        object v;
        
        switch (type) {
            case "string":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<string>();
                else
                    v = Parameter.GetValue<string>((string) value);
                break;
            case "integer":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<int>();
                else
                    v = Parameter.GetValue<int>((string) value);
                break;
            case "double":
                if (value == null)
                    v = parameterMetadata.GetDefaultValue<double>();
                else
                    v = Parameter.GetValue<double>((string) value);
                break;
            case "link":
                if (value == null)
                    v = null;
                else
                    v = (string) value;
                break;
            default:
                v = null;
                break;
        }

        SetValue(v);
    }
    


    public override object GetValue() {
        if (type == "link")
            return base.GetValue();
        else 
            return Input.GetValue();
    }

   

    public override void SetDarkMode(bool dark) {
        base.SetDarkMode(dark);
        Input.SetDarkMode(dark);
    }

    public override void SetLabel(string label, string description) {
        base.SetLabel(label, description);
        Input.SetLabel(label, description);
    }

    public override void SetValue(object value) {
        base.SetValue(value);
        if (type != "link")
            Input.SetValue(value);
    }

    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //input.Input.Input.onValueChanged.AddListener((string newValue)
        //        => onChangeParameterHandler(actionParameterMetadata.Name, newValue, type));
        this.onChangeParameterHandler = onChangeParameterHandler;
    }    

    public override void SetType(string type) {
        base.SetType(type);
        
        if (type == "link") {
            Input.gameObject.SetActive(false);
            Input.Input.onValueChanged.RemoveAllListeners();
        }
        else {
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
            ActionsDropdown.gameObject.SetActive(false);
            Input.gameObject.SetActive(true);
            Input.Input.onValueChanged.RemoveAllListeners();
            switch (ParameterMetadata.Type) {
                case "integer":
                    Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), int.Parse(value), type));

                    break;
                case "double":
                    Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), double.Parse(value), type));
                    break;
                case "string":
                    Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), value, type));
                    break;
            }
            Input.SetType(type);
        }
    }

   
}
