using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;
using Action = Base.Action;

public abstract class LinkableParameter : MonoBehaviour, IParameter {
    public NStateToggle ParameterTypeToggle;
    public DropdownParameter ActionsDropdown; //also for project parameters
    public ParameterMetadata ParameterMetadata;
    protected OnChangeParameterHandlerDelegate onChangeParameterHandler;
    protected string type;
    protected object manualValue;
    protected int dropdownIndexSelected;

    private const string NewProjectParameterText = "New project parameter";

    public const string LINK = "link";
    public const string PROJECT_PARAMETER = "project_parameter";
    public const string VALUE = "value";

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
        if (type == LINK)
            return EncodeLinkValue((string) ActionsDropdown.GetValue());
        else if (type == PROJECT_PARAMETER) {
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
    public virtual void SetValue(object newValue) {
        if (type != LINK && type != PROJECT_PARAMETER && newValue != null) {
            manualValue = newValue;
            Parameter.SetValue(newValue);
        } else if (type == LINK) {
            SetupDropdownForActions(newValue);
        } else if (type == PROJECT_PARAMETER) {
            SetupDropdownForProjectParameters(ParameterMetadata.Type, newValue);
        }



    }

    private void SetupDropdownForActions(object newValue) {
        List<string> actions = new();
        foreach (Action action in ProjectManager.Instance.GetActionsWithReturnType(ParameterMetadata.Type)) {
            actions.Add(action.GetName());
        }

        ActionsDropdown.PutData(actions, DecodeLinkValue(newValue?.ToString()), (string v) => onChangeParameterHandler(GetName(), v, type));
    }

    private void SetupDropdownForProjectParameters(string type, object newValue) {
        if (ProjectParametersHelper.TypeSupported(type)) {
            List<string> projectParameters = new();
            List<string> labels = new();
            string selectedLabel = null;
            foreach (ProjectParameter pp in ProjectManager.Instance.ProjectParameters.Where(c => c.Type == type).OrderBy(p => p.Name)) {
                projectParameters.Add(pp.Name);
                labels.Add($"{pp.Name}: {ProjectParameterHelper.GetValue(pp)}");
                if (newValue != null && pp.Id == JsonConvert.DeserializeObject<string>(newValue.ToString())) {
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
            bool hideActionParametersMenu = AREditorResources.Instance.ActionParametersMenu.IsVisible;
            if (hideActionParametersMenu)
                AREditorResources.Instance.ActionParametersMenu.SetVisibility(false);

            _ = AREditorResources.Instance.EditProjectParameterDialog.Init((string newProjectParameterName) => {
                if (hideActionParametersMenu)
                    AREditorResources.Instance.ActionParametersMenu.SetVisibility(true); //make menu visible again
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
        } else if (type == LINK) {
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState(LINK, false);
            ActionsDropdown.gameObject.SetActive(true);
            Parameter.GetTransform().gameObject.SetActive(false);
            SetupDropdownForActions(null);
        } else if (type == PROJECT_PARAMETER) {
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState(PROJECT_PARAMETER, false);
            Parameter.GetTransform().gameObject.SetActive(false);
            SetupDropdownForProjectParameters(ParameterMetadata.Type, null);
            ActionsDropdown.gameObject.SetActive(true);
        } else {

            Parameter.GetTransform().gameObject.SetActive(true);
            ActionsDropdown.gameObject.SetActive(false);
            ParameterTypeToggle.gameObject.SetActive(true);
            ParameterTypeToggle.SetState(VALUE, false);
        }

    }

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType(LINK, true, true);
        if (ActionsDropdown.GetValue() != null)
            onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
    }

    public void PickProjectParameterCb() {
        manualValue = GetValue();
        SetType(PROJECT_PARAMETER, true, true);
        if (ActionsDropdown.Dropdown.isActiveAndEnabled && ActionsDropdown.GetValue().ToString() != NewProjectParameterText)
            onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
    }

    public void SetValueManually() {
        SetType(ParameterMetadata.Type, true, true);
        if (manualValue != null)
            SetValue(manualValue);
        else {
            SetValue(GetDefaultValue());
        }
        onChangeParameterHandler.Invoke(Parameter.GetName(), GetValue(), GetCurrentType());
    }

    protected string EncodeLinkValue(string dropdownValue) {
        try {
            Action action = ProjectManager.Instance.GetActionByName(dropdownValue);
            return action.GetId() + "/default/0";
        } catch (ItemNotFoundException ex) {
            return "";
        }

    }

    protected abstract object GetDefaultValue();

    protected string DecodeLinkValue(string linkValue) {
        if (string.IsNullOrEmpty(linkValue))
            return null;
        if (!linkValue.Contains("/"))
            return null;
        string actionId = linkValue.Substring(0, linkValue.IndexOf('/'));
        try {
            Action action = ProjectManager.Instance.GetAction(actionId);
            return action.GetName();
        } catch (ItemNotFoundException) { }
        return null;
    }

    private string DecodeProjectParameterValue(string newValue) {
        if (string.IsNullOrEmpty(newValue))
            return null;

        Regex r = new(@"[a-z_0-9]+", RegexOptions.IgnoreCase);
        MatchCollection matches = r.Matches(newValue);
        if (matches.Count == 0)
            return null;

        string projectParameterId = matches[0].Value;
        ProjectParameter pp = ProjectManager.Instance.ProjectParameters.Find(p => p.Id == projectParameterId);
        return pp?.Name;
    }
    public virtual void Init(ParameterMetadata parameterMetadata, string type, object newValue, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, OnChangeParameterHandlerDelegate onChangeParameterHandler, bool linkable = true) {
        InitDropdown(layoutGroupToBeDisabled, canvasRoot);
        ParameterMetadata = parameterMetadata;
        SetType(type, linkable, false);
        this.onChangeParameterHandler = onChangeParameterHandler;
    }



    public Transform GetTransform() {
        return transform;
    }

    public abstract void SetInteractable(bool interactable);
}
