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
        SetValue(value);
    }
    public override void SetType(string type, bool linkable) {
        base.SetType(type, linkable);
        this.type = type;
        if (type == "link") {
            SwitchComponent.gameObject.SetActive(false);
        } else {
            SwitchComponent.gameObject.SetActive(true);
        }
    }


}
