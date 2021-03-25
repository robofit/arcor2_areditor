using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class RobotSelectorDialog : Dialog {

    public DropdownRobots DropdownRobots;
    public DropdownEndEffectors DropdownEndEffectors;
    private string robotId, eeId;

    public async override void Open() {
        robotId = PlayerPrefsHelper.LoadString(SceneManager.Instance.SceneMeta.Id + "/selectedRobotId", null);
        eeId = PlayerPrefsHelper.LoadString(SceneManager.Instance.SceneMeta.Id + "/selectedEndEffectorId", null);
        Debug.LogError(robotId);
        Debug.LogError(eeId);
        if (!SceneManager.Instance.SceneStarted) {
            Notifications.Instance.ShowNotification("Failed to open robot selector", "Scene offline");
            return;
        }
        base.Open();
        string robotName = null;
        try {
            IRobot robot = SceneManager.Instance.GetRobot(robotId);
            robotName = robot.GetName();
        } catch (ItemNotFoundException ex) {
            robotName = null;
        }
        await DropdownRobots.Init(SelectRobot, false, robotName);
        if (DropdownRobots.Dropdown.Dropdown.dropdownItems.Count > 0) {
            SelectRobot(DropdownRobots.Dropdown.GetValue().ToString());
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

    public void SelectEE(string endEffectorId) {
        eeId = endEffectorId;
    }

    public override void Confirm() {
        PlayerPrefsHelper.SaveString(SceneManager.Instance.SceneMeta.Id + "/selectedRobotId", robotId);
        PlayerPrefsHelper.SaveString(SceneManager.Instance.SceneMeta.Id + "/selectedEndEffectorId", eeId);
        SceneManager.Instance.SelectRobotAndEE(robotId, eeId);
        Close();
    }

    public override void Close() {
        Close(true);
    }

    public void Close(bool setActiveSubmenu) {
        //LeftMenu.Instance.UpdateVisibility();
        SelectorMenu.Instance.gameObject.SetActive(true);

        base.Close();

        //LeftMenu.Instance.UpdateVisibility();

        if (setActiveSubmenu) {
            //LeftMenu.Instance.SetActiveSubmenu(LeftMenuSelection.Robot);
        }
    }
}

