using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;

public class EditProjectParameterDialog : Dialog {
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
    private System.Action<string> onCloseCallback;
    private System.Action onCancelCallback;
    private bool cancelCallbackInvoked; //flag: only cancel callback should be invoked if canceled

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectParameter"></param>
    public async Task<bool> Init(System.Action<string> onCloseCallback, System.Action onCancelCallback, ProjectParameter projectParameter = null, string ofType = null) {
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
                Title.text = "Edit project parameter";
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
        ValidateInput();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility(false, true);
    }

    private void OnTypeSelected(string type) {
        ProjectParameterTypes typeEnum = ProjectParametersHelper.ConvertStringParameterTypeToEnum(type);
        OnTypeSelected(typeEnum);
    }

    private void OnTypeSelected(ProjectParameterTypes type) {
        selectedType = type;
        SetValueInputType();
        ValidateInput();
    }


    public async void ValidateInput() {
        bool valid = true;

        if (string.IsNullOrEmpty((string) nameInput.GetValue())) {
            ConfirmButton.SetInteractivity(false, "Name cannot be empty");
            valid = false;
        } else if (string.IsNullOrEmpty(valueInput.Input.text) && selectedType != ProjectParameterTypes.boolean) {
            ConfirmButton.SetInteractivity(false, "Value cannot be empty");
            valid = false;
        }

        if (!isNewConstant) {
            if (((string) nameInput.GetValue()) == projectParameter.Name && valueInput.Input.text == projectParameter.Value) { //known bug: always false when parameter's type is double or boolean
                ConfirmButton.SetInteractivity(false, "Project parameter unchanged");
                valid = false;
            }
        }

        if (!valid)
            return;

        try {
            await Confirm(true);
            ConfirmButton.SetInteractivity(true);
        } catch (RequestFailedException e) {
            ConfirmButton.SetInteractivity(false, e.Message);
        }
    }

    public async override void Confirm() {
        await Confirm(false);
    }

    public async Task Confirm(bool dryRun) {
        string name = nameInput.GetValue() as string;
        string value;
        if (selectedType == ProjectParameterTypes.boolean) {
            value = JsonConvert.SerializeObject(trueToggle.isOn);
        } else {
            value = JsonConvert.SerializeObject(valueInput.GetValue());
        }
        try {
            if (isNewConstant) {
                await WebsocketManager.Instance.AddProjectParameter(name, selectedType.ToString("g"), value, dryRun);
            } else {
                await WebsocketManager.Instance.UpdateProjectParameter(projectParameter.Id, name, value, dryRun);
            }
            //after updating, constant is unlocked automatically by server
            if (!dryRun)
                Close();
        } catch (RequestFailedException e) {
            if (dryRun)
                throw e;
            else
                Notifications.Instance.ShowNotification("Failed to " + (isNewConstant ? "add " : "update ") + "project parameter", e.Message);
        }
    }

    public override async void Close() {
        base.Close();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility();
        dropdown.Dropdown.dropdownItems.Clear();
        projectParameter = null;
        if (!cancelCallbackInvoked)
            onCloseCallback?.Invoke((string) nameInput.GetValue());
    }

    public async void Cancel() {
        cancelCallbackInvoked = true;
        if (projectParameter == null || isNewConstant) {
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

    public static bool TypeSupported(string type) {
        return type == "string" || type == "boolean" || type == "integer" || type == "double";
    }
}

public enum ProjectParameterTypes {
    integer, @string, boolean, @double
}


