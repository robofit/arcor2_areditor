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
    public GameObject Items, LoadingObject;
    public bool Loading;

    public void SetLoading(bool loading) {
        this.Loading = loading;
        if (loading) {
            Dropdown.gameObject.SetActive(false);
            LoadingObject.SetActive(true);
        } else {
            Dropdown.gameObject.SetActive(true);
            LoadingObject.SetActive(false);
        }
    }
    
    public object GetValue() {
        throw new System.NotImplementedException();
    }

    public void SetLabel(string label, string description) {
        Label.text = label;
        if (Label.GetComponent<TooltipContent>().tooltipObject == null) {
            Label.GetComponent<TooltipContent>().tooltipObject = Base.GameManager.Instance.Tooltip;
        }
        if (Label.GetComponent<TooltipContent>().descriptionText == null) {
            Label.GetComponent<TooltipContent>().descriptionText = Base.GameManager.Instance.Text;
        }
        Label.GetComponent<TooltipContent>().description = description;
    }

    public void Init() {
        Dropdown.listParent = transform.parent;
        this.Loading = false;
    }

    public void OnClick() {
        transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        GetComponent<HorizontalLayoutGroup>().enabled = false;

    }

    public void DisableLayoutSelf() {
        GetComponent<HorizontalLayoutGroup>().enabled = false;
    }

    public void PutData(List<string> data, string selectedItem, UnityAction callback) {
        Dropdown.dropdownItems.Clear();
        foreach (string d in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = d
            };
            if (callback != null) {
                if (item.OnItemSelection == null) {
                    item.OnItemSelection = new UnityEvent();
                }
                item.OnItemSelection.AddListener(callback);
            }
            
            Dropdown.dropdownItems.Add(item);
            if (d == selectedItem) {
                Dropdown.selectedItemIndex = Dropdown.dropdownItems.Count - 1;
            }
        }
        SetLoading(false);
        if (Dropdown.dropdownItems.Count > 0) {
            Dropdown.SetupDropdown();
            Debug.LogError("asdfasdf: " + Dropdown.selectedText.text);
        } else {
            Dropdown.gameObject.SetActive(false);
        }
        
       
    }

}
