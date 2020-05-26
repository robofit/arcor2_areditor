using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class DropdownEndEffectors : MonoBehaviour {
    public DropdownParameter Dropdown;


    public void Init(string robot_id) {
        if (robot_id == "") {
            Dropdown.Dropdown.dropdownItems.Clear();
            gameObject.SetActive(false);
            return;
        }        

        foreach (IRobot robot in Base.SceneManager.Instance.GetRobots()) {
            Dropdown.Dropdown.dropdownItems.Clear();
            PutData(robot.GetEndEffectors());
        }
        Base.NotificationsModernUI.Instance.ShowNotification("End effector load failed", "Failed to load end effectors");
    }

    public void PutData(List<string> data) {
        foreach (string ee in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = ee
            };
            Dropdown.Dropdown.dropdownItems.Add(item);
        }
        if (Dropdown.Dropdown.dropdownItems.Count > 0) {
            Dropdown.Dropdown.SetupDropdown();
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }
}
