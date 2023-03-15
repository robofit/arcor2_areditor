using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableBoolParameter : LinkableParameter
{
    public SwitchComponent SwitchComponent;

    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        Parameter = SwitchComponent;
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        SwitchComponent.AddOnValueChangedListener((bool v) => onChangeParameterHandler(parameterMetadata.Name, v, parameterMetadata.Type));
        SetValue(value);
    }

    public override void SetInteractable(bool interactable) {
        SwitchComponent.Interactable = interactable;
    }

    public override void SetType(string type, bool linkable, bool switchBtnClicked) {
        base.SetType(type, linkable, switchBtnClicked);
        this.type = type;
    }

    protected override object GetDefaultValue() {
        return false;
    }
}
