using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using IO.Swagger.Model;

public class DropdownParameter : MonoBehaviour, IParameter {

    public TMPro.TMP_Text Label, NoOption;
    public CustomDropdown Dropdown;
    public GameObject LoadingObject;
    public bool Loading;
    public VerticalLayoutGroup LayoutGroupToBeDisabled;
    public string Type;

    private TooltipContent tooltipContent;
    public GameObject Trigger, CanvasRoot;

    private void Awake() {
        tooltipContent = Label.GetComponent<TooltipContent>();
    }

    private void Start() {
        if (CanvasRoot != null)
            Dropdown.listParent = CanvasRoot.transform;
        enabled = false;
    }

    public void SetLoading(bool loading) {
        this.Loading = loading;
        if (Dropdown == null || Dropdown.gameObject == null)
            return;
        if (loading) {
            Dropdown.gameObject.SetActive(false);
            LoadingObject.SetActive(true);
        } else {
            Dropdown.gameObject.SetActive(true);
            LoadingObject.SetActive(false);
        }
    }
    
    public virtual object GetValue() {
        if (Dropdown.dropdownItems.Count > 0) {
            return Dropdown.selectedText.text;
        } else {
            return null;
        }
    }

    public void SetLabel(string label, string description) {
        Label.text = label;
        if (tooltipContent == null)
            return;
        if (!string.IsNullOrEmpty(description)) {
            tooltipContent.enabled = true;
            if (tooltipContent.tooltipRect == null) {
                tooltipContent.tooltipRect = Base.GameManager.Instance.Tooltip;
            }
            if (tooltipContent.descriptionText == null) {
                tooltipContent.descriptionText = Base.GameManager.Instance.Text;
            }
            tooltipContent.description = description;
        } else {
            tooltipContent.enabled = false;
        }
    }

    public void Init(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, string type, bool enableIcons = false) {
        Type = type;
        Dropdown.listParent = canvasRoot.transform;
        CanvasRoot = canvasRoot;
        Dropdown.enableIcon = enableIcons;
        Dropdown.selectedImage.gameObject.SetActive(enableIcons);
        
        LayoutGroupToBeDisabled = layoutGroupToBeDisabled;
       
        this.Loading = false;
    }

    public void OnClick() {
        gameObject.GetComponent<VerticalLayoutGroup>().enabled = false;
        LayoutGroupToBeDisabled.enabled = false;
        enabled = true;
    }

    public void DisableLayoutSelf() {
        gameObject.GetComponent<VerticalLayoutGroup>().enabled = false;
    }

    public virtual void PutData(List<string> data, string selectedItem, UnityAction<string> callback) {
        List<CustomDropdown.Item> items = new List<CustomDropdown.Item>();
        foreach (string d in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = d
            };
            items.Add(item);
        }
        PutData(items, selectedItem, callback);
    }

    public void PutData(List<CustomDropdown.Item> items, string selectedItem, UnityAction<string> callback) {
        Dropdown.dropdownItems.Clear();
        Dropdown.selectedItemIndex = 0;
        foreach (CustomDropdown.Item item in items) {
            if (callback != null) {
                if (item.OnItemSelection == null) {
                    item.OnItemSelection = new UnityEvent();
                }
                item.OnItemSelection.AddListener(() => callback(item.itemName));
            }

            Dropdown.dropdownItems.Add(item);
            if (item.itemName == selectedItem) {
                Dropdown.selectedItemIndex = Dropdown.dropdownItems.Count - 1;
            }
        }

        SetLoading(false);
        if (Dropdown == null)
            return; // e.g. when object is destroyed before init completed
        if (Dropdown.dropdownItems.Count > 0) {
            
            Dropdown.SetupDropdown();
            NoOption.gameObject.SetActive(false);
        } else {
            Dropdown.gameObject.SetActive(false);
            NoOption.gameObject.SetActive(true);
        }
    }

    private void Update() {
        if (!Trigger.activeSelf) {
            LayoutGroupToBeDisabled.enabled = true;
            enabled = false;
        }
    }

    public string GetName() {
        return Label.text;
    }

    public void SetValue(object value) {
        for (int i = 0; i < Dropdown.dropdownItems.Count; ++i) {
            if (Dropdown.dropdownItems[i].itemName == value.ToString()) {
                Dropdown.selectedItemIndex = i;
                Dropdown.selectedText.text = Dropdown.dropdownItems[i].itemName;
            }
        }
    }

    public void SetDarkMode(bool dark) {
        if (dark) {
            Label.color = Color.black;
            NoOption.color = Color.black;
        } else {
            Label.color = Color.white;
            NoOption.color = Color.white;
        }
    }

    public string GetCurrentType() {
        return Type;
    }

    public Transform GetTransform() {
        return transform;
    }
}
