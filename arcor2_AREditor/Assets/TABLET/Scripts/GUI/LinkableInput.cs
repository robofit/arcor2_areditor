using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableInput : LinkableParameter {
   public LabeledInput Input;


    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        Parameter = Input;
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);

        SetOnValueChanged(onChangeParameterHandler);
        object v = value;
        
        switch (type) {
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
        }

        SetValue(v);
    }
    



    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //input.Input.Input.onValueChanged.AddListener((string newValue)
        //        => onChangeParameterHandler(actionParameterMetadata.Name, newValue, type));
        this.onChangeParameterHandler = onChangeParameterHandler;
    }    

    public override void SetType(string type, bool linkable) {
        base.SetType(type, linkable);
        
        if (type == "link") {
            Input.gameObject.SetActive(false);
            Input.Input.onValueChanged.RemoveAllListeners();
        }
        else {
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
