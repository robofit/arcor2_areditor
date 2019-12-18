using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;

public class DropdownRobots : MonoBehaviour
{

    public void Init(UnityAction<string> callback) {
        GetComponent<CustomDropdown>().dropdownItems.Clear();
        foreach (string robot_id in Base.ActionsManager.Instance.GetRobots()) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = robot_id
            };
            if (callback != null) {
                if (item.OnItemSelection == null)
                    item.OnItemSelection = new UnityEvent();
                item.OnItemSelection.AddListener(() => callback(robot_id));
            }   
            GetComponent<CustomDropdown>().dropdownItems.Add(item);
        }
        if (GetComponent<CustomDropdown>().dropdownItems.Count > 0)
            GetComponent<CustomDropdown>().SetupDropdown();
    }
}
