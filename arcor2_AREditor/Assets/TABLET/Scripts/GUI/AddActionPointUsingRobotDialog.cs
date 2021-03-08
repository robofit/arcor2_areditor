using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddActionPointUsingRobotDialog : Dialog
{
    public DropdownParameter DropdownRobots;
    public DropdownParameter DropdownEndEffectors;
    public CustomInputField Name;

    public Button ConfirmBtn, CancelBtn;

    public override void Confirm() {
        throw new System.NotImplementedException();
    }

    public string GetName() {
        return Name.inputText.text;
    }

    public string GetRobotId() {
        return SceneManager.Instance.RobotNameToId((string) DropdownRobots.GetValue());
    }

    public string GetEEId() {
        return (string) DropdownEndEffectors.GetValue();
    }

    public async Task Open(string inputValue, UnityAction confirmationCallback, UnityAction cancelCallback) {
        DropdownRobots.Dropdown.dropdownItems.Clear();
        await DropdownRobots.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (DropdownRobots.Dropdown.dropdownItems.Count > 0) {
            OnRobotChanged((string) DropdownRobots.GetValue());
        } else {
            Notifications.Instance.ShowNotification("Failed to add AP", "There is no robot with EE in the scene");
            return;
        }
        Name.inputText.text = inputValue;

        ConfirmBtn.onClick.RemoveAllListeners();
        ConfirmBtn.onClick.AddListener(confirmationCallback);
        CancelBtn.onClick.AddListener(cancelCallback);
        Open();
    }

    /// <summary>
    /// updates EndEffectorList on selected robot change
    /// </summary>
    /// <param name="robot_name">Newly selected robot's name</param>
    private async void OnRobotChanged(string robot_name) {
        DropdownEndEffectors.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            await DropdownEndEffectors.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

    }
}
