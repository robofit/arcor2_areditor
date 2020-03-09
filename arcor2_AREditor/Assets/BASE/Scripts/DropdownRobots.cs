using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;

public class DropdownRobots : MonoBehaviour
{
    public CustomDropdown Dropdown;
    public void Init(UnityAction<string> callback) {
        Dropdown.dropdownItems.Clear();
        foreach (string robot_id in Base.ActionsManager.Instance.GetRobots()) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = robot_id
            };
            if (callback != null) {
                if (item.OnItemSelection == null)
                    item.OnItemSelection = new UnityEvent();
                item.OnItemSelection.AddListener(() => callback(robot_id));
            }
            Dropdown.dropdownItems.Add(item);
        }
        if (Dropdown.dropdownItems.Count > 0)
            Dropdown.SetupDropdown();
    }
}
