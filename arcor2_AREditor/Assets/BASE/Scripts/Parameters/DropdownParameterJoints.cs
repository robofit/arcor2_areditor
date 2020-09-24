using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using System.Linq;
using TMPro;
using Base;

public class DropdownParameterJoints : DropdownParameter
{

    public Sprite ValidIcon, InvalidIcon;

    public void PutData(Dictionary<string, bool> data, string selectedItem, UnityAction<string> callback) {
        List<CustomDropdown.Item> items = new List<CustomDropdown.Item>();
        foreach (KeyValuePair<string, bool> d in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = d.Key
            };
            items.Add(item);
        }
        PutData(items, selectedItem, callback);


    }

    public override object GetValue() {
        string apName;
        string value = (string) base.GetValue();
        if (value == null)
            return null;

        apName = value.Split('.').First();
        try {
            Base.ActionPoint actionPoint = Base.ProjectManager.Instance.GetactionpointByName(apName);
            return actionPoint.GetJointsByName(value.Split('.').Last()).Id;
        } catch (KeyNotFoundException ex) {
            Debug.LogError(ex);
            Debug.LogError(value);
            return null;
        }
        
        
    }
}
