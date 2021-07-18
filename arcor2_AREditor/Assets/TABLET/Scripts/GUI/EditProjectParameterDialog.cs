using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;

public class EditProjectParameterDialog : Dialog
{
    public TMPro.TMP_Text Title;

    [SerializeField]
    private LabeledInput nameInput, valueInput;
    [SerializeField]
    private DropdownParameter dropdown;
    [SerializeField]
    private Toggle trueToggle;
    [SerializeField]
    private GameObject booleanBlock, removeButton;

    private ProjectParameter projectParameter;
    private bool isNewConstant, booleanValue;
    private ProjectParameterTypes selectedType;
    public ButtonWithTooltip CloseBtn, ConfirmButton;
    private System.Action onCloseCallback;
    private System.Action onCancelCallback;
    private bool cancelCallbackInvoked; //flag: only cancel callback should be invoked if canceled

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectParameter"></param>
    public async Task<bool> Init(System.Action onCloseCallback, System.Action onCancelCallback, ProjectParameter projectParameter = null, string ofType = null) {
        this.projectParameter = projectParameter;
        isNewConstant = projectParameter == null;
        this.onCloseCallback = onCloseCallback;
        this.onCancelCallback = onCancelCallback;
        cancelCallbackInvoked = false;

        dropdown.Dropdown.dropdownItems.Clear();
        foreach (string type in Enum.GetNames(typeof(ProjectParameterTypes))) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = type,
                OnItemSelection = new UnityEvent()
            };
            item.OnItemSelection.AddListener(() => OnTypeSelected(type));
            dropdown.Dropdown.dropdownItems.Add(item);
        }


        if (isNewConstant) {
            Title.text = "New project parameter";
            removeButton.SetActive(false);
            nameInput.SetValue("");
            valueInput.SetValue("");
            OnTypeSelected(ofType == null ? ProjectParameterTypes.integer : ProjectParametersHelper.ConvertStringParameterTypeToEnum(ofType));
            dropdown.Dropdown.selectedItemIndex = (int) selectedType;
            dropdown.Dropdown.SetupDropdown();
            dropdown.Dropdown.GetComponent<Button>().interactable = ofType == null;

        } else { //editing constant
            try {
                await WebsocketManager.Instance.WriteLock(projectParameter.Id, false);
                Title.text = "Edit project paramater";
                removeButton.SetActive(true);
                nameInput.SetValue(projectParameter.Name);
                OnTypeSelected(projectParameter.Type);
                dropdown.Dropdown.selectedItemIndex = (int) selectedType;
                dropdown.Dropdown.SetupDropdown();
                dropdown.Dropdown.GetComponent<Button>().interactable = false;

                object value = ProjectParametersHelper.GetValue(projectParameter.Value, selectedType);
                if (selectedType == ProjectParameterTypes.boolean) {
                    trueToggle.isOn = (bool) value;
                } else {
                    valueInput.SetValue(value);
                }

            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to lock " + projectParameter.Name, e.Message);
                this.projectParameter = null;
                return false;
            }
        }
        return true;
    }

    private void SetValueInputType() {
        valueInput.SetValue(null);
        booleanBlock.gameObject.SetActive(selectedType == ProjectParameterTypes.boolean);
        valueInput.gameObject.SetActive(selectedType != ProjectParameterTypes.boolean);

        switch (selectedType) {
            case ProjectParameterTypes.integer:
                valueInput.SetType("integer");
                break;
            case ProjectParameterTypes.@string:
                valueInput.SetType("string"); //default
                break;
            case ProjectParameterTypes.boolean:
                break;
            case ProjectParameterTypes.@double:
                valueInput.SetType("double");
                break;
        }
    }

    public override void Open() {
        base.Open();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility(false, true);
    }

    private void OnTypeSelected(string type) {
        ProjectParameterTypes typeEnum = ProjectParametersHelper.ConvertStringParameterTypeToEnum(type);
        OnTypeSelected(typeEnum);
    }

    private void OnTypeSelected(ProjectParameterTypes type) {
        selectedType = type;
        SetValueInputType();
    }


    public void ValidateInput() {
        //TODO
        //if (isNewConstant) {
        //    ConfirmButton.SetInteractivity(true);
        //    return;
        //}

        //bool valid = ((string) nameInput.GetValue()) != selectedObject.GetName();

        //ConfirmButton.SetInteractivity(valid, "Name has not been changed");
    }

    public override async void Confirm() {
        string name = nameInput.GetValue() as string;
        string value;
            if(selectedType == ProjectParameterTypes.boolean) {
            value = JsonConvert.SerializeObject(trueToggle.isOn);
        } else {
        value = JsonConvert.SerializeObject(valueInput.GetValue());
        }
        //string value = Base.Parameter.Encode(valueInput.GetValue() as string, selectedType.ToString("g"));
        try {
            if (isNewConstant) {
                //Notifications.Instance.ShowNotification(ParameterTypesEnum.relative_pose.ToString("G"), ParameterTypesEnum.@string.ToString("g"));
                
                await WebsocketManager.Instance.AddProjectParameter(name, selectedType.ToString("g"), value); //name, "{" + JsonConvert.SerializeObject(selectedType) + "}", value);
            } else {
                await WebsocketManager.Instance.UpdateProjectParameter(projectParameter.Id, name, value);
            }
            //after updating, constant is unlocked automatically by server
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to " + (isNewConstant ? "add " : "update ") + "project parameter", e.Message);
        }
    }

    public override async void Close() {
        base.Close();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility();
        dropdown.Dropdown.dropdownItems.Clear();
        projectParameter = null;
        if(!cancelCallbackInvoked)
            onCloseCallback?.Invoke();

    }

    public async void Cancel() {
        if (projectParameter == null || isNewConstant) {
            cancelCallbackInvoked = true;
            Close();
            onCancelCallback?.Invoke();
            return;
        }

        try {
            await WebsocketManager.Instance.WriteUnlock(projectParameter.Id);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to unlock " + projectParameter.Name, e.Message);
        }
        Close();
        onCancelCallback?.Invoke();
    }

    public async void Remove() {
        try {
            await WebsocketManager.Instance.RemoveProjectParameter(projectParameter.Id);
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove project parameter", e.Message);
        }
    }
}

public static class ProjectParametersHelper {
    public static ProjectParameterTypes ConvertStringParameterTypeToEnum(string type) {
        return (ProjectParameterTypes) Enum.Parse(typeof(ProjectParameterTypes), type);
    }

    public static object GetValue(string value, ProjectParameterTypes type) {
        object toReturn = null;
        switch (type) {
            case ProjectParameterTypes.integer:
                toReturn = JsonConvert.DeserializeObject<int>(value);
                break;
            case ProjectParameterTypes.@string:
                toReturn = JsonConvert.DeserializeObject<string>(value);
                break;
            case ProjectParameterTypes.boolean:
                toReturn = JsonConvert.DeserializeObject<bool>(value);
                break;
            case ProjectParameterTypes.@double:
                toReturn = JsonConvert.DeserializeObject<double>(value);
                break;
        }
        return toReturn;
    }
}

public enum ProjectParameterTypes {
    integer, @string, boolean, @double
}


