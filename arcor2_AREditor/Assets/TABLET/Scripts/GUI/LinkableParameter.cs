using System;
using UnityEngine;
using Base;
using System.Collections.Generic;
using static Base.Parameter;
using UnityEngine.UI;

public abstract class LinkableParameter : MonoBehaviour, IParameter {
    public Button CreateLinkBtn, RemoveLinkBtn;
    public DropdownParameter ActionsDropdown;
    public ParameterMetadata ParameterMetadata;
    protected OnChangeParameterHandlerDelegate onChangeParameterHandler;
    protected string type;

    //!! has to be set in either start or init !! 
    public IParameter Parameter;

    public virtual string GetCurrentType() {
        return type;
    }

    public virtual string GetName() {
        return ActionsDropdown.GetName();
    }
    public virtual object GetValue() {
        if (type == "link")
            return EncodeLinkValue((string) ActionsDropdown.GetValue());
        else
            return Parameter.GetValue();
    }
    public virtual void SetDarkMode(bool dark) {
        ActionsDropdown.SetDarkMode(dark);
        Parameter.SetDarkMode(dark);
    }
    public virtual void SetLabel(string label, string description) {
        ActionsDropdown.SetLabel(label, description);
        Parameter.SetLabel(label, description);
    }
    public virtual void SetValue(object value) {
        List<string> actions = new List<string>();
        foreach (Base.Action action in Base.ProjectManager.Instance.GetActionsWithReturnType(ParameterMetadata.Type)) {
            actions.Add(action.GetName());
        }
        ActionsDropdown.PutData(actions, DecodeLinkValue(value?.ToString()), (string v) => onChangeParameterHandler(GetName(), v, type));

        if (type != "link" && value != null)
            Parameter.SetValue(value);
    }

    public virtual void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        ActionsDropdown.Init(layoutGroupToBeDisabled, canvasRoot, type);
    }

    public virtual void SetType(string type, bool linkable, bool switchBtnClicked) {
        this.type = type;
        if (!linkable) {
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(false);
            ActionsDropdown.gameObject.SetActive(false);
        } else if (type == "link") {
            RemoveLinkBtn.gameObject.SetActive(true);
            CreateLinkBtn.gameObject.SetActive(false);
            ActionsDropdown.gameObject.SetActive(true);
            if (ActionsDropdown.Dropdown.dropdownItems.Count > 0) {
                ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke();
            }
        } else {
            ActionsDropdown.gameObject.SetActive(false);
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
        }
        
    }

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType("link", true, true);
    }

    public void RemoveLinkCb() {
        SetType(ParameterMetadata.Type, true, true);
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
    public virtual void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        InitDropdown(layoutGroupToBeDisabled, canvasRoot);
        ParameterMetadata = parameterMetadata;
        SetType(type, linkable, false);
    }

    

    public Transform GetTransform() {
        return transform;
    }

    public abstract void SetInteractable(bool interactable);
}
