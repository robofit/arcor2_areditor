using System;
using UnityEngine;
using Base;
using System.Collections.Generic;
using static Base.Parameter;
using UnityEngine.UI;

public class LinkableParameter : MonoBehaviour, IParameter {
    public Button CreateLinkBtn, RemoveLinkBtn;
    public DropdownParameter ActionsDropdown;
    public ParameterMetadata ParameterMetadata;
    protected OnChangeParameterHandlerDelegate onChangeParameterHandler;
    protected string type;

    public virtual string GetCurrentType() {
        return type;
    }

    public virtual string GetName() {
        return ActionsDropdown.GetName();
    }
    public virtual object GetValue() {
        return EncodeLinkValue((string) ActionsDropdown.GetValue());
    }
    public virtual void SetDarkMode(bool dark) {
        ActionsDropdown.SetDarkMode(dark);
    }
    public virtual void SetLabel(string label, string description) {
        ActionsDropdown.SetLabel(label, description);
    }
    public virtual void SetValue(object value) {
        List<string> actions = new List<string>();
        foreach (Base.Action action in Base.ProjectManager.Instance.GetActionsWithReturnType(ParameterMetadata.Type)) {
            actions.Add(action.GetName());
        }
        ActionsDropdown.PutData(actions, DecodeLinkValue(value?.ToString()), (string v) => onChangeParameterHandler(GetName(), v, type));
    }

    public void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        ActionsDropdown.Init(layoutGroupToBeDisabled, canvasRoot);
    }

    public virtual void SetType(string type) {
        this.type = type;
        if (type == "link") {
            RemoveLinkBtn.gameObject.SetActive(true);
            CreateLinkBtn.gameObject.SetActive(false);
            ActionsDropdown.gameObject.SetActive(true);
        } else {
            ActionsDropdown.gameObject.SetActive(false);
        }
        
    }

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType("link");
    }

    public void RemoveLinkCb() {
        SetType(ParameterMetadata.Type);
    }

    private string EncodeLinkValue(string dropdownValue) {
        try {
            Base.Action action = Base.ProjectManager.Instance.GetActionByName(dropdownValue);
            return action.GetId() + "/default/0";
        } catch (ItemNotFoundException ex) {
            return "";
        }

    }

    private string DecodeLinkValue(string linkValue) {
        if (string.IsNullOrEmpty(linkValue))
            return null;
        if (!linkValue.Contains("/"))
            return null;
        string actionId = linkValue.Substring(0, linkValue.IndexOf('/'));
        try {
            Base.Action action = ProjectManager.Instance.GetAction(actionId);
            return action.GetName();
        } catch (ItemNotFoundException) { }
        return null;
    }
}
