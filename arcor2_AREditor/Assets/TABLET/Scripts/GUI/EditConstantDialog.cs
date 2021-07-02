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

public class EditConstantDialog : Dialog
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

    private ProjectParameter constant;
    private bool isNewConstant, booleanValue;
    private ProjectConstantTypes selectedType;
    public ButtonWithTooltip CloseBtn, ConfirmButton;
    private System.Action onCloseCallback;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="constant"></param>
    public async Task<bool> Init(System.Action onCloseCallback, ProjectParameter constant = null) {
        this.constant = constant;
        isNewConstant = constant == null;
        this.onCloseCallback = onCloseCallback;

        foreach (string type in Enum.GetNames(typeof(ProjectConstantTypes))) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = type,
                OnItemSelection = new UnityEvent()
            };
            item.OnItemSelection.AddListener(() => OnTypeSelected(type));
            dropdown.Dropdown.dropdownItems.Add(item);
        }


        if (isNewConstant) {
            Title.text = "Add new constant";
            removeButton.SetActive(false);
            nameInput.SetValue("");
            valueInput.SetValue("");
            OnTypeSelected(ProjectConstantTypes.integer);
            dropdown.Dropdown.selectedItemIndex = (int) selectedType;
            dropdown.Dropdown.SetupDropdown();
            dropdown.Dropdown.GetComponent<Button>().interactable = true;

        } else { //editing constant
            try {
                await WebsocketManager.Instance.WriteLock(constant.Id, false);
                Title.text = "Edit constant";
                removeButton.SetActive(true);
                nameInput.SetValue(constant.Name);
                OnTypeSelected(constant.Type);
                dropdown.Dropdown.selectedItemIndex = (int) selectedType;
                dropdown.Dropdown.SetupDropdown();
                dropdown.Dropdown.GetComponent<Button>().interactable = false;

                object value = ProjectConstantPicker.GetValue(constant.Value, selectedType);
                if (selectedType == ProjectConstantTypes.boolean) {
                    trueToggle.isOn = (bool) value;
                } else {
                    valueInput.SetValue(value);
                }

            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to lock " + constant.Name, e.Message);
                this.constant = null;
                return false;
            }
        }
        return true;
    }

    private void SetValueInputType() {
        valueInput.SetValue(null);
        booleanBlock.gameObject.SetActive(selectedType == ProjectConstantTypes.boolean);
        valueInput.gameObject.SetActive(selectedType != ProjectConstantTypes.boolean);

        switch (selectedType) {
            case ProjectConstantTypes.integer:
                valueInput.SetType("integer");
                break;
            case ProjectConstantTypes.@string:
                valueInput.SetType("string"); //default
                break;
            case ProjectConstantTypes.boolean:
                break;
            case ProjectConstantTypes.@double:
                valueInput.SetType("double");
                break;
        }
    }

    public override void Open() {
        base.Open();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility(false, true);
    }

    private void OnTypeSelected(string type) {
        ProjectConstantTypes typeEnum = ProjectConstantPicker.ConvertStringConstantToEnum(type);
        OnTypeSelected(typeEnum);
    }

    private void OnTypeSelected(ProjectConstantTypes type) {
        selectedType = type;
        SetValueInputType();
    }

    


        //ProjectConstantTypes returnType = ProjectConstantTypes.integer;
        //switch (returnType) {
        //    case Enum.
        //        break;
        //    case ProjectConstantTypes.@string:
        //        break;
        //    case ProjectConstantTypes.boolean:
        //        break;
        //    case ProjectConstantTypes.@double:
        //        break;
        //        //case ProjectConstantTypes.integer.ToString("g"):
        //        //    returnType = ProjectConstantTypes.integer;
        //        //    break;
        //        //case ProjectConstantTypes.boolean.ToString():
        //        //    returnType = ProjectConstantTypes.integer;
        //        //    break;
        //        //case ProjectConstantTypes.integer.ToString("g"):
        //        //    returnType = ProjectConstantTypes.integer;
        //        //    break;
        //        //case ProjectConstantTypes.integer.ToString("g"):
        //        //    returnType = ProjectConstantTypes.integer;
        //        //    break;
        //}
        //return returnType;
    //}

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
            if(selectedType == ProjectConstantTypes.boolean) {
            value = JsonConvert.SerializeObject(trueToggle.isOn);
        } else {
        value = JsonConvert.SerializeObject(valueInput.GetValue());
        }
        //string value = Base.Parameter.Encode(valueInput.GetValue() as string, selectedType.ToString("g"));
        try {
            if (isNewConstant) {
                //Notifications.Instance.ShowNotification(ParameterTypesEnum.relative_pose.ToString("G"), ParameterTypesEnum.@string.ToString("g"));
                
                await WebsocketManager.Instance.AddConstant(name, selectedType.ToString("g"), value); //name, "{" + JsonConvert.SerializeObject(selectedType) + "}", value);
            } else {
                await WebsocketManager.Instance.UpdateConstant(constant.Id, name, value);
            }
            //after updating, constant is unlocked automatically by server
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to " + (isNewConstant ? "add " : "update ") + "constant", e.Message);
        }
    }

    public override async void Close() {
        base.Close();
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility();
        dropdown.Dropdown.dropdownItems.Clear();
        constant = null;
        onCloseCallback?.Invoke();

    }

    public async void Cancel() {
        if (constant == null || isNewConstant) {
            Close();
            return;
        }

        try {
            await WebsocketManager.Instance.WriteUnlock(constant.Id);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to unlock " + constant.Name, e.Message);
        }
        Close();
    }

    public async void Remove() {
        try {
            await WebsocketManager.Instance.RemoveConstant(constant.Id);
            Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove constant", e.Message);
        }
    }
}

public enum ProjectConstantTypes {
    integer, @string, boolean, @double
}
