using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class DropdownRobots : MonoBehaviour
{
    public DropdownParameter Dropdown;

    /// <summary>
    /// Initialize dropdown with list of robots in scene
    /// </summary>
    /// <param name="callback">Function to call when item is selected. Will pass robot_id</param>
    /// <param name="withEEOnly">Only puts robots with at lease one end effector</param>
    public void Init(UnityAction<string> callback, bool withEEOnly) {
        List<string> robotNames = new List<string>();
        foreach (IRobot robot in Base.SceneManager.Instance.GetRobots()) {
            List<string> endEffectors = robot.GetEndEffectors();
            if (withEEOnly) {
                if (endEffectors.Count > 0) {
                    robotNames.Add(robot.GetName());
                }
            } else {
                robotNames.Add(robot.GetName());
            }            
        }
        Init(robotNames, callback);
    }


    public void Init(List<string> robotNames, UnityAction<string> callback) {
        Dropdown.Dropdown.dropdownItems.Clear();

        foreach (string robotName in robotNames) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = robotName
            };
            if (callback != null) {
                if (item.OnItemSelection == null)
                    item.OnItemSelection = new UnityEvent();
                item.OnItemSelection.AddListener(() => callback(robotName));
            }
            Dropdown.Dropdown.dropdownItems.Add(item);
        }
        if (Dropdown.Dropdown.dropdownItems.Count > 0)
            Dropdown.Dropdown.SetupDropdown();
    }
}
