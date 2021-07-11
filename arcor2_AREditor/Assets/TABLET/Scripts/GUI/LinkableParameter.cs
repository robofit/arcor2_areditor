using System;
using UnityEngine;
using Base;
using System.Collections.Generic;
using static Base.Parameter;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public abstract class LinkableParameter : MonoBehaviour, IParameter {
    public Button CreateLinkBtn, RemoveLinkBtn, PickProjectParameterBtn, DontPickProjectParameterBtn;
    public DropdownParameter ActionsDropdown; //also for project parameters
    public ParameterMetadata ParameterMetadata;
    protected OnChangeParameterHandlerDelegate onChangeParameterHandler;
    protected string type;
    protected object value;

    public const string ProjectParameterText = "constant";
    private const string NewProjectParameterText = "New project parameter";

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
        else if (type == ProjectParameterText) {
            return EncodeProjectParameterValue((string) ActionsDropdown.GetValue());
        } else
            return Parameter.GetValue();
    }

    private object EncodeProjectParameterValue(string v) {
        return ProjectManager.Instance.ProjectParameters.Find(p => p.Name == v).Id;
        //throw new NotImplementedException(); //TODO
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
        if (type != "link" && type != ProjectParameterText && value != null) {
            Parameter.SetValue(value);
        } else if (type == "link") {
            SetupDropdownForActions(value);
        } else if (type == ProjectParameterText) {
            SetupDropdownForProjectParameters(ParameterMetadata.Type, value);
        }



    }

    private void SetupDropdownForActions(object value) {
        List<string> actions = new List<string>();
        foreach (Base.Action action in Base.ProjectManager.Instance.GetActionsWithReturnType(ParameterMetadata.Type)) {
            actions.Add(action.GetName());
        }
        ActionsDropdown.PutData(actions, DecodeLinkValue(value?.ToString()), (string v) => onChangeParameterHandler(GetName(), v, type));
    }

    private void SetupDropdownForProjectParameters(string type, object value) {
        List<string> projectParameters = new List<string>();
        foreach (IO.Swagger.Model.ProjectParameter pp in ProjectManager.Instance.ProjectParameters.Where(c => c.Type == type)) {
            projectParameters.Add(pp.Name);
        }
        projectParameters.Add(NewProjectParameterText);
        ActionsDropdown.PutData(projectParameters, DecodeProjectParameterValue(value?.ToString()), OnProjectParameterPicked);
    }

    private void OnProjectParameterPicked(string name) {
        if (name == NewProjectParameterText) {
            bool hideActionParametersMenu = AREditorResources.Instance.ActionParametersMenu.IsVisible();
            if (hideActionParametersMenu)
                AREditorResources.Instance.ActionParametersMenu.SetVisibility(false);
            AREditorResources.Instance.EditProjectParameterDialog.Init(() => {
                if (hideActionParametersMenu)
                    AREditorResources.Instance.ActionParametersMenu.SetVisibility(true); //make menu visible again
                SetupDropdownForProjectParameters(ParameterMetadata.Type, null);
                if (ActionsDropdown.Dropdown.dropdownItems.Count >= 2) {
                    ActionsDropdown.Dropdown.selectedItemIndex = ActionsDropdown.Dropdown.dropdownItems.Count - 2;
                    ActionsDropdown.Dropdown.SetupDropdown();
                    ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke(); //select last added project parameter
                }
            },
            ofType: ParameterMetadata.Type);
            AREditorResources.Instance.EditProjectParameterDialog.Open();
        } else {
            onChangeParameterHandler?.Invoke(GetName(), GetValue(), type);
        }
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
            PickProjectParameterBtn.gameObject.SetActive(true);
            DontPickProjectParameterBtn.gameObject.SetActive(false);
            ActionsDropdown.gameObject.SetActive(true);
            SetupDropdownForActions(null);
            if (ActionsDropdown.Dropdown.dropdownItems.Count > 0) {
                ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke();
            }
        } else if (type == ProjectParameterText) {
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
            PickProjectParameterBtn.gameObject.SetActive(false);
            DontPickProjectParameterBtn.gameObject.SetActive(true);
            SetupDropdownForProjectParameters(ParameterMetadata.Type, null);
            if (switchBtnClicked && ActionsDropdown.Dropdown.dropdownItems.Count > 0) {
                ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke();
            }
            ActionsDropdown.gameObject.SetActive(true);
        } else {
            ActionsDropdown.gameObject.SetActive(false);
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
            PickProjectParameterBtn.gameObject.SetActive(true);
            DontPickProjectParameterBtn.gameObject.SetActive(false);
        }
        
    }

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType("link", true, true);
    }

    public void RemoveLinkCb() { //TODO use SetValueManually instead
        SetType(ParameterMetadata.Type, true, true);
    }

    public void PickProjectParameterCb() {
        SetType(ProjectParameterText, true, true);
    }

    public void SetValueManually() {
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

    private string DecodeProjectParameterValue(string value) {
        if (string.IsNullOrEmpty(value))
            return null;

        Regex r = new Regex(@"[a-z_0-9]+", RegexOptions.IgnoreCase);
        var matches = r.Matches(value);
        if (matches.Count == 0)
            return null;

        string projectParameterId = matches[0].Value;
        IO.Swagger.Model.ProjectParameter pp = ProjectManager.Instance.ProjectParameters.Find(p => p.Id == projectParameterId);
        return pp?.Name;
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
