using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddJointsMenu : MonoBehaviour {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;
    public DropdownParameter RobotsList;
    public DropdownArms DropdownArms;


    [SerializeField]
    private Button CreateNewJoints;

    [SerializeField]
    private TooltipContent buttonTooltip;

    private string jointsName;

    public async void UpdateMenu() {
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        await RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, false);
        OnRobotChanged(RobotsList.GetValue().ToString());
        ValidateFields();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="robot_name"></param>
    private async void OnRobotChanged(string robot_name) {
        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            if (string.IsNullOrEmpty(robotId)) {
                Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + RobotsList.GetValue().ToString() + "does not exists");
                return;
            }
            await DropdownArms.Init(robotId, null);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
        }
    }

    public async void ValidateFields() {
        bool interactable = true;
        jointsName = NameInput.text;

        if (string.IsNullOrEmpty(jointsName)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        } else if (CurrentActionPoint.OrientationNameExist(jointsName) || CurrentActionPoint.JointsNameExist(jointsName)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + jointsName;
            interactable = false;
        }

        if (interactable) {
            if (RobotsList.Dropdown.dropdownItems.Count == 0) {
                interactable = false;
                buttonTooltip.description = "There is no robot to be used";
            }
        }
        
        buttonTooltip.enabled = !interactable;
        CreateNewJoints.interactable = interactable;
    }

    public async void AddJoints() {
        string robotName = (string) RobotsList.GetValue();

        IRobot robot;
        try {
            robot = SceneManager.Instance.GetRobotByName(robotName);
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add joints", "Could not found robot called: " + robotName);
            Debug.LogError(ex);
            return;
        }

        Debug.Assert(CurrentActionPoint != null);
        try {
            string armId = null;
            if (robot.MultiArm())
                armId = DropdownArms.Dropdown.GetValue().ToString();
            jointsName = NameInput.text;
            await Base.WebsocketManager.Instance.AddActionPointJoints(CurrentActionPoint.Data.Id, robot.GetId(), jointsName, armId);
            Notifications.Instance.ShowToastMessage("Joints added successfully");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add joints", ex.Message);
            return;
        }
        Close();
        
    }

    /// <summary>
    /// Opens menu for adding joints
    /// </summary>
    /// <param name="actionPoint"></param>
    public void ShowMenu(Base.ActionPoint actionPoint) {
        CurrentActionPoint = actionPoint;
        NameInput.text = CurrentActionPoint.GetFreeOrientationName();

        UpdateMenu();
        gameObject.SetActive(true);
    }

    public void Close() {
        ActionPointAimingMenu.Instance.SwitchToJoints();
    }
}
