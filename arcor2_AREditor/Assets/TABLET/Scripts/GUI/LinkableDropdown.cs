using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableDropdown : LinkableParameter {
    public DropdownParameter DropdownParameter;

    public override void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        Parameter = DropdownParameter;

        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);

        SetOnValueChanged(onChangeParameterHandler);
        SetValue(value);


    }


    public override void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        base.InitDropdown(layoutGroupToBeDisabled, canvasRoot);
        DropdownParameter.Init(layoutGroupToBeDisabled, canvasRoot, type);
    }

    public override void SetInteractable(bool interactable) {
        DropdownParameter.SetInteractable(interactable);
    }

    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        this.onChangeParameterHandler = onChangeParameterHandler;
    }

    public override void SetType(string type, bool linkable, bool switchBtnClicked) {
        base.SetType(type, linkable, switchBtnClicked);
        this.type = type;
    }

    protected override object GetDefaultValue() {
        return null;
    }
}
