using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using UnityEngine.Events;

public class RobotSelectorDialog : Dialog {

    public DropdownRobots DropdownRobots;
    public DropdownEndEffectors DropdownEndEffectors;
    private string robotId, eeId;

    private UnityAction _closeCallback;

    public async Task<bool> Open(UnityAction closeCallback) {
        if (!SceneManager.Instance.RobotInScene()) {
            Notifications.Instance.ShowNotification("Failed to open robot selector menu", "There are no robots in scene");
            closeCallback.Invoke();
            return false;
        }
        
        _closeCallback = closeCallback;
        robotId = PlayerPrefsHelper.LoadString(SceneManager.Instance.SceneMeta.Id + "/selectedRobotId", null);
        eeId = PlayerPrefsHelper.LoadString(SceneManager.Instance.SceneMeta.Id + "/selectedEndEffectorId", null);
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot selector", "Scene offline");
            return false;
        }
        base.Open();
        string robotName = null;
        try {
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            robotName = robot.GetName();
        } catch (ItemNotFoundException ex) {
            robotName = null;
        }
        DropdownRobots.Dropdown.Dropdown.dropdownItems.Clear();
        await DropdownRobots.Init(SelectRobot, false, robotName);
        if (DropdownRobots.Dropdown.Dropdown.dropdownItems.Count > 0) {
            SelectRobot(DropdownRobots.Dropdown.GetValue().ToString());
            return true;
        } else {
            Notifications.Instance.ShowNotification("Failed to open robot selector menu", "There are no robots in scene");
            Close();
            return false;
        }
    }

    public async void SelectRobot(string robotName) {
        
        try {
            IRobot robot = SceneManager.Instance.GetRobotByName(robotName);
            robotId = robot.GetId();
            await DropdownEndEffectors.Init(robot.GetId(), SelectEE);
            if (!string.IsNullOrEmpty(DropdownEndEffectors.Dropdown.GetValue().ToString())) {
                SelectEE(DropdownEndEffectors.Dropdown.GetValue().ToString());
            }
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to select robot", "Robot " + robotName + " does not exists.");
            return;
        }
        
    }

    public bool Opened() {
        return windowManager.isOn;
    }

    public void SelectEE(string endEffectorId) {
        eeId = endEffectorId;
    }

    public async override void Confirm() {
        PlayerPrefsHelper.SaveString(SceneManager.Instance.SceneMeta.Id + "/selectedRobotId", robotId);
        PlayerPrefsHelper.SaveString(SceneManager.Instance.SceneMeta.Id + "/selectedEndEffectorId", eeId);
        await SceneManager.Instance.SelectRobotAndEE(robotId, eeId);
        Close();
    }

    public override void Close() {
        Close(true);
        MenuManager.Instance.LeftMenuProject.UpdateBtns();
    }

    public void Close(bool closeCallbackCall) {
        SelectorMenu.Instance.gameObject.SetActive(true);
        base.Close();
        if (closeCallbackCall) {
            _closeCallback.Invoke();
        }
    }
}

