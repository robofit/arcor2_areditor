using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class DropdownEndEffectors : MonoBehaviour {
    public CustomDropdown Dropdown;


    public void Init(string robot_id) {
        if (robot_id == "") {
            Dropdown.dropdownItems.Clear();
            gameObject.SetActive(false);
            return;
        }
        foreach (Base.ActionObject actionObject in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
            if (actionObject.Data.Id == robot_id) {
                UpdateEndEffectorList(actionObject);
                return;
            }
        }
        //not found in objects, try services
        foreach (Base.Service s in Base.ActionsManager.Instance.ServicesData.Values) {
            if (!s.IsRobot()) {
                continue;
            }
            if (s.Robots.ContainsKey(robot_id)) {
                UpdateEndEffectorList(s, robot_id);
                return;
            }
        }
        Base.NotificationsModernUI.Instance.ShowNotification("End effector load failed", "Failed to load end effectors");
    }

    public void UpdateEndEffectorList(Base.ActionObject robot) {
        Dropdown.dropdownItems.Clear();
        PutData(robot.EndEffectors);
    }

    public void UpdateEndEffectorList(Base.Service service, string robot_id) {
        Dropdown.dropdownItems.Clear();
        PutData(service.GetEndEffectors(robot_id));
    }

    public void PutData(List<string> data) {
        foreach (string ee in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = ee
            };
            GetComponent<CustomDropdown>().dropdownItems.Add(item);
        }
        if (GetComponent<CustomDropdown>().dropdownItems.Count > 0) {
            GetComponent<CustomDropdown>().SetupDropdown();
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }
}
