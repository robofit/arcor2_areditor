using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class DropdownParameter : MonoBehaviour, IActionParameter {

    public TMPro.TMP_Text Label;
    public CustomDropdown Dropdown;
    public GameObject LoadingObject;
    public bool Loading;
    public VerticalLayoutGroup LayoutGroupToBeDisabled;
    public GameObject Trigger, CanvasRoot;

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
        if (Label.GetComponent<TooltipContent>().tooltipRect == null) {
            Label.GetComponent<TooltipContent>().tooltipRect = Base.GameManager.Instance.Tooltip;
        }
        if (Label.GetComponent<TooltipContent>().descriptionText == null) {
            Label.GetComponent<TooltipContent>().descriptionText = Base.GameManager.Instance.Text;
        }
        Label.GetComponent<TooltipContent>().description = description;
    }

    public void Init(VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot, bool enableIcons = false) {
        
        Dropdown.listParent = canvasRoot.transform;
        CanvasRoot = canvasRoot;
        Dropdown.enableIcon = enableIcons;
        Dropdown.selectedImage.gameObject.SetActive(enableIcons);
        
        LayoutGroupToBeDisabled = layoutGroupToBeDisabled;
       
        this.Loading = false;
    }

    public void OnClick() {
        gameObject.GetComponent<HorizontalLayoutGroup>().enabled = false;
        LayoutGroupToBeDisabled.enabled = false;
        enabled = true;
    }

    public void DisableLayoutSelf() {
        gameObject.GetComponent<HorizontalLayoutGroup>().enabled = false;
    }

    public virtual void PutData(List<string> data, string selectedItem, UnityAction callback) {
        List<CustomDropdown.Item> items = new List<CustomDropdown.Item>();
        foreach (string d in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = d
            };
            items.Add(item);
        }
        PutData(items, selectedItem, callback);
    }

    public void PutData(List<CustomDropdown.Item> items, string selectedItem, UnityAction callback) {
        Dropdown.dropdownItems.Clear();
        foreach (CustomDropdown.Item item in items) {
            if (callback != null) {
                if (item.OnItemSelection == null) {
                    item.OnItemSelection = new UnityEvent();
                }
                item.OnItemSelection.AddListener(callback);
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
        } else {
            Dropdown.gameObject.SetActive(false);
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
        throw new NotImplementedException();
    }
}
