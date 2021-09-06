using System;
using UnityEngine;
using Base;
using System.Collections.Generic;
using static Base.Parameter;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public abstract class LinkableParameter : MonoBehaviour, IParameter {
    public NStateToggle ParameterTypeToggle;
    public DropdownParameter ActionsDropdown; //also for project parameters
    public ParameterMetadata ParameterMetadata;
    protected OnChangeParameterHandlerDelegate onChangeParameterHandler;
    protected string type;
    protected object value;
    protected int dropdownIndexSelected;
    
    public const string ProjectParameterText = "project_parameter";
    private const string NewProjectParameterText = "New project parameter";

    /// <summary>
    ///  !! has to be set in either start or init !!
    /// </summary>     
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
            return EncodeProjectParameterValue(((string) ActionsDropdown.GetValue()).Split(':')[0]);
        } else
            return Parameter.GetValue();
    }

    private object EncodeProjectParameterValue(string v) {
        return ProjectManager.Instance.ProjectParameters.Find(p => p.Name == v).Id;
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
        if (ProjectParametersHelper.TypeSupported(type)) {
            List<string> projectParameters = new List<string>();
            List<string> labels = new List<string>();
            string selectedLabel = null;
            foreach (IO.Swagger.Model.ProjectParameter pp in ProjectManager.Instance.ProjectParameters.Where(c => c.Type == type).OrderBy(p => p.Name)) {
                projectParameters.Add(pp.Name);
                labels.Add($"{pp.Name}: {ProjectParameterHelper.GetValue(pp)}");
                if (value != null && pp.Id == JsonConvert.DeserializeObject<string>(value.ToString())) {
                    selectedLabel = labels.Last();
                }                
            }
            projectParameters.Add(NewProjectParameterText);
            labels.Add(NewProjectParameterText);
            ActionsDropdown.PutData(projectParameters, selectedLabel, OnProjectParameterPicked, labels);
            dropdownIndexSelected = ActionsDropdown.Dropdown.selectedItemIndex;
        } else {
            ActionsDropdown.PutData(new List<string>(), "", OnProjectParameterPicked);
        }

    }


    private void OnProjectParameterPicked(string projectParameterName) {
        if (projectParameterName == NewProjectParameterText) {
            bool hideActionParametersMenu = AREditorResources.Instance.ActionParametersMenu.IsVisible();
            bool hideAddNewActionDialog = AREditorResources.Instance.AddNewActionDialog.IsVisible;
            if (hideActionParametersMenu)
                AREditorResources.Instance.ActionParametersMenu.SetVisibility(false);
            else if (hideAddNewActionDialog) {
                AREditorResources.Instance.AddNewActionDialog.Close();
                AREditorResources.Instance.ActionPickerMenu.SetVisibility(false);
            }

            _ = AREditorResources.Instance.EditProjectParameterDialog.Init((string newProjectParameterName) => {
                if (hideActionParametersMenu)
                    AREditorResources.Instance.ActionParametersMenu.SetVisibility(true); //make menu visible again
                else if (hideAddNewActionDialog)
                    AREditorResources.Instance.AddNewActionDialog.Open();
                SetupDropdownForProjectParameters(ParameterMetadata.Type, null);
                if (!string.IsNullOrEmpty(newProjectParameterName)) {
                    ActionsDropdown.Dropdown.selectedItemIndex = ActionsDropdown.Dropdown.dropdownItems.FindIndex(i => i.itemName.Split(':')[0] == newProjectParameterName);
                    ActionsDropdown.Dropdown.SetupDropdown();
                    ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke(); //select newly added project parameter
                }
            },
            () => {
                if (hideActionParametersMenu)
                    AREditorResources.Instance.ActionParametersMenu.SetVisibility(true); //make menu visible again
                else if (hideAddNewActionDialog)
                    AREditorResources.Instance.AddNewActionDialog.Open();
                ActionsDropdown.Dropdown.selectedItemIndex = dropdownIndexSelected;
                ActionsDropdown.Dropdown.SetupDropdown();
            },
            ofType: ParameterMetadata.Type);
            AREditorResources.Instance.EditProjectParameterDialog.Open();

        } else {
            onChangeParameterHandler?.Invoke(GetName(), projectParameterName, type);
            dropdownIndexSelected = ActionsDropdown.Dropdown.selectedItemIndex;
        }
    }

    public virtual void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        ActionsDropdown.Init(layoutGroupToBeDisabled, canvasRoot, type);
    }

    public virtual void SetType(string type, bool linkable, bool switchBtnClicked) {
        this.type = type;
        if (!linkable) {
            ParameterTypeToggle.gameObject.SetActive(false);
            ActionsDropdown.gameObject.SetActive(false);
        } else if (type == "link") {
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState("link", false);
            ActionsDropdown.gameObject.SetActive(true);
            Parameter.GetTransform().gameObject.SetActive(false);
            SetupDropdownForActions(null);
            /*if (ActionsDropdown.Dropdown.dropdownItems.Count > 0) {
                ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke();
            }*/
        } else if (type == ProjectParameterText) {
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState("constant", false);
            Parameter.GetTransform().gameObject.SetActive(false);
            SetupDropdownForProjectParameters(ParameterMetadata.Type, null);
            /*if (switchBtnClicked && ActionsDropdown.Dropdown.dropdownItems.Count > 0) {
                ActionsDropdown.Dropdown.dropdownItems[ActionsDropdown.Dropdown.selectedItemIndex].OnItemSelection.Invoke();
            }*/
            ActionsDropdown.gameObject.SetActive(true);
        } else {
            Parameter.GetTransform().gameObject.SetActive(true);
            ActionsDropdown.gameObject.SetActive(false);
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState("value", false);
        }
        
    }

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType("link", true, true);
        if (ActionsDropdown.GetValue() != null)
            onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
    }

    public void PickProjectParameterCb() {
        SetType(ProjectParameterText, true, true);
        if (ActionsDropdown.Dropdown.isActiveAndEnabled && ActionsDropdown.GetValue().ToString() != NewProjectParameterText)
            onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
    }

    public void SetValueManually() {
        SetType(ParameterMetadata.Type, true, true);
        onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
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
