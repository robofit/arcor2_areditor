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
    public GameObject Items;


    
    public object GetValue() {
        throw new System.NotImplementedException();
    }

    public void SetLabel(string label) {
        Label.text = label;
    }

    public void Init() {
        Dropdown.listParent = transform.parent;
        
    }

    public void OnClick() {
        transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        GetComponent<HorizontalLayoutGroup>().enabled = false;

    }

    public void PutData(List<string> data, string selectedItem, UnityAction callback) {
        Dropdown.dropdownItems.Clear();
        foreach (string d in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = d
            };
            if (item.OnItemSelection == null) {
                item.OnItemSelection = new UnityEvent();
            }
            item.OnItemSelection.AddListener(callback);
            Dropdown.dropdownItems.Add(item);
            if (d == selectedItem) {
                Dropdown.selectedItemIndex = Dropdown.dropdownItems.Count - 1;
            }
        }

        Dropdown.SetupDropdown();
        
       
    }

}
