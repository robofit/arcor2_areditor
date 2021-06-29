using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using Base;
using UnityEngine.Events;
using System.Threading.Tasks;

public class DropdownEndEffectors : MonoBehaviour {
    public DropdownParameter Dropdown;


    public async Task Init(string robotId, string arm_id, UnityAction<string> onChangeCallback) {
        if (!SceneManager.Instance.SceneStarted || string.IsNullOrEmpty(robotId)) {
            Dropdown.Dropdown.dropdownItems.Clear();
            gameObject.SetActive(false);
            return;
        }
        try {
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            Dropdown.Dropdown.dropdownItems.Clear();
            PutData(await robot.GetEndEffectorIds(arm_id), onChangeCallback);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Base.NotificationsModernUI.Instance.ShowNotification("End effector load failed", "Failed to load end effectors, try again later");
        }
        
        
    }

    public void PutData(List<string> data, UnityAction<string> onChangeCallback) {
        foreach (string ee in data) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = ee
            };
            if (onChangeCallback != null) {
                if (item.OnItemSelection == null) {
                    item.OnItemSelection = new UnityEvent();
                }
                item.OnItemSelection.AddListener(() => onChangeCallback(ee));
            }
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
