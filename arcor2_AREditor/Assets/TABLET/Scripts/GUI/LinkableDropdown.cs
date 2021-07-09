using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableDropdown : LinkableParameter
{
    public DropdownParameter DropdownParameter;

    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        Parameter = DropdownParameter;

        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        
        SetOnValueChanged(onChangeParameterHandler);
        SetValue(value);
        /*object v;

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
        }*/

    }

   
    public override void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        base.InitDropdown(layoutGroupToBeDisabled, canvasRoot);
        DropdownParameter.Init(layoutGroupToBeDisabled, canvasRoot, type);
    }

    public override void SetInteractable(bool interactable) {
        DropdownParameter.SetInteractable(interactable);
    }

    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //input.Input.Input.onValueChanged.AddListener((string newValue)
        //        => onChangeParameterHandler(actionParameterMetadata.Name, newValue, type));
        this.onChangeParameterHandler = onChangeParameterHandler;
    }

    public override void SetType(string type, bool linkable, bool switchBtnClicked) {
        base.SetType(type, linkable, switchBtnClicked);
        this.type = type;
        if (type == "link") {
            DropdownParameter.gameObject.SetActive(false);
            //DropdownParameter.Dropdown.onValueChanged.RemoveAllListeners();
        } else {
            DropdownParameter.gameObject.SetActive(true);
            //Input.Input.onValueChanged.RemoveAllListeners();
            //Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), int.Parse(value), type));
            //Input.SetType(type);
        }
    }
}
