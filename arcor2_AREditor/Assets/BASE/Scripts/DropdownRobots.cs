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
        foreach (string robotName in Base.ActionsManager.Instance.GetRobotsNames()) {
            if (withEEOnly) {
                if (Base.Scene.Instance.TryGetActionObjectByName(robotName, out Base.ActionObject robot))
                {
                    if (robot.GetEndEffectors().Count > 0) {
                        robotNames.Add(robotName);
                    }

                    //not found in objects, try services
                } else foreach (Base.Service s in Base.ActionsManager.Instance.ServicesData.Values) {
                    
                    if (s.IsRobot() && s.Robots.ContainsKey(robotName)) {
                        if (s.GetEndEffectors(robotName).Count > 0) {
                            robotNames.Add(robotName);
                            break;
                        }
                    }                    
                }

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
