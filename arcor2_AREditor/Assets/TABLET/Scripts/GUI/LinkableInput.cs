using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.Parameter;

public class LinkableInput : MonoBehaviour, IParameter {
    public Button CreateLinkBtn, RemoveLinkBtn;
    public LabeledInput Input;
    private string originalType, type;
    public DropdownParameter Dropdown;
    private OnChangeParameterHandlerDelegate onChangeParameterHandler;



    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
        SetType("link");
    }

    public void RemoveLinkCb() {
        SetType(originalType);
    }

    public string GetName() {
        return Input.GetName();
    }

    public object GetValue() {
        if (type == "link")
            return EncodeLinkValue((string) Dropdown.GetValue());
        else 
            return Input.GetValue();
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
        return linkValue;
    }

    public void SetDarkMode(bool dark) {
        Input.SetDarkMode(dark);
        Dropdown.SetDarkMode(dark);
    }

    public void SetLabel(string label, string description) {
        Input.SetLabel(label, description);
        Dropdown.SetLabel(label, description);
    }

    public void SetValue(object value) {
        List<string> actions = new List<string>();
        foreach (Base.Action action in Base.ProjectManager.Instance.GetActionsWithReturnType(originalType)) {
            actions.Add(action.GetName());
        }
        Debug.LogError(actions.Count);
        Dropdown.PutData(actions, DecodeLinkValue(value.ToString()), (string v) => onChangeParameterHandler(Input.GetName(), v, type));
        if (type != "link")
            Input.SetValue(value);
    }

    public void SetOnValueChanged(OnChangeParameterHandlerDelegate onChangeParameterHandler) {
        //input.Input.Input.onValueChanged.AddListener((string newValue)
        //        => onChangeParameterHandler(actionParameterMetadata.Name, newValue, type));
        this.onChangeParameterHandler = onChangeParameterHandler;
    }

    public void InitDropdown(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        Dropdown.Init(layoutGroupToBeDisabled, canvasRoot);
    }

    public void SetType(string type) {
        this.type = type;
        if (type == "link") {
            RemoveLinkBtn.gameObject.SetActive(true);
            CreateLinkBtn.gameObject.SetActive(false);
            Dropdown.gameObject.SetActive(true);
            Input.gameObject.SetActive(false);
            Input.Input.onValueChanged.RemoveAllListeners();

        } else {
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
            Dropdown.gameObject.SetActive(false);
            Input.gameObject.SetActive(true);
            Input.Input.onValueChanged.RemoveAllListeners();
            Input.Input.onValueChanged.AddListener((string value) => onChangeParameterHandler(Input.GetName(), int.Parse(value), type));
        }
        Input.SetType(type);
    }

    public void SetMetadataType(string type) {
        originalType = type;
        
    }

    public string GetCurrentType() {
        return type;
    }
}
