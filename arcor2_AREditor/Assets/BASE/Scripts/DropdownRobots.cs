using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using System.Linq;
using System.Threading.Tasks;
using Base;
using System;

public class DropdownRobots : MonoBehaviour
{
    public DropdownParameter Dropdown;

    /// <summary>
    /// Initialize dropdown with list of robots in scene
    /// </summary>
    /// <param name="callback">Function to call when item is selected. Will pass robot_id</param>
    /// <param name="withEEOnly">Only puts robots with at least one end effector</param>
    public Task Init(UnityAction<string> callback, bool withEEOnly) {
        return Init(callback, withEEOnly, null);
    }

    internal async Task Init(UnityAction<string> callback, bool withEEOnly, string selectedRobotName) {
        List<string> robotNames = new List<string>();

        if (!withEEOnly) {
            foreach (IRobot robot in Base.SceneManager.Instance.GetRobots()) {
                robotNames.Add(robot.GetName());
                
            }
        } else if (withEEOnly && SceneManager.Instance.SceneStarted) {
            foreach (IRobot robot in Base.SceneManager.Instance.GetRobots()) {
                List<string> endEffectors = await robot.GetEndEffectorIds();
                if (endEffectors.Count > 0) {
                    robotNames.Add(robot.GetName());
                }
            }
        } else {
            return;
        }
        Init(robotNames, callback, selectedRobotName);
    }

    public void Init(List<string> robotNames, UnityAction<string> callback, string selectedRobotName = null) {
        Dropdown.Dropdown.dropdownItems.Clear();
        int selectedItemIndex = 0;
        foreach (string robotName in robotNames) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = robotName                
            };
            if (robotName == selectedRobotName)
                selectedItemIndex = Dropdown.Dropdown.dropdownItems.Count;
            if (callback != null) {
                if (item.OnItemSelection == null)
                    item.OnItemSelection = new UnityEvent();
                item.OnItemSelection.AddListener(() => callback(robotName));
            }
            Dropdown.Dropdown.dropdownItems.Add(item);
        }
        if (Dropdown.Dropdown.dropdownItems.Count > 0) {
            Dropdown.Dropdown.selectedItemIndex = selectedItemIndex;
            Dropdown.Dropdown.SetupDropdown();
        }
    }
}
