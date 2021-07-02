using Base;
using DanielLochner.Assets.SimpleSideMenu;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

public class AddOrientationMenu : MonoBehaviour {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;// QuaternionX, QuaternionY, QuaternionZ, QuaternionW;
    public DropdownParameter RobotsList, EndEffectorList;
    public DropdownArms DropdownArms;
    public GameObject LiteModeBlock, ManualModeBlock;
    public bool ManualMode;

    public OrientationManualEdit OrientationManualEdit;

    [SerializeField]
    private Button CreateNewOrientation;

    [SerializeField]
    private TooltipContent buttonTooltip;


    public async void UpdateMenu() {
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        await RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (robotsListDropdown.dropdownItems.Count > 0) {
            OnRobotChanged((string) RobotsList.GetValue());
        }

        ValidateFields();
    }

    /// <summary>
    /// updates EndEffectorList on selected robot change
    /// </summary>
    /// <param name="robot_name">Newly selected robot's name</param>
    private async void OnRobotChanged(string robot_name) {
        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            await DropdownArms.Init(robotId, OnRobotArmChanged);
            OnRobotArmChanged(DropdownArms.Dropdown.GetValue().ToString());
        }
        catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load robot arms", ex.Message);
        }

    }

    private async void OnRobotArmChanged(string arm_id) {
        string robotId;
        try {
            robotId = SceneManager.Instance.RobotNameToId(RobotsList.GetValue().ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            robotId = null;

        }
        if (string.IsNullOrEmpty(robotId)) {
            Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + RobotsList.GetValue().ToString() + "does not exists");
            return;
        }
        await EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, arm_id, null);
    }

    public async void ValidateFields() {
        bool interactable = true;
        name = NameInput.text;

        if (string.IsNullOrEmpty(name)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        }
        else if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + name;
            interactable = false;
        }

        if (ManualMode) {
            if (interactable) {
                buttonTooltip.description = OrientationManualEdit.ValidateFields();
                if (!string.IsNullOrEmpty(buttonTooltip.description)) {
                    interactable = false;
                }
            }
        }
        else {
            if (interactable) {
                if (RobotsList.Dropdown.dropdownItems.Count == 0) {
                    interactable = false;
                    buttonTooltip.description = "There is no robot to be used";
                }
            }
        }
        buttonTooltip.enabled = !interactable;
        CreateNewOrientation.interactable = interactable;
    }

    public async void AddOrientation() {
        Debug.Assert(CurrentActionPoint != null);


        string name = NameInput.text;
        try {

            if (ManualMode) {
                Orientation orientation = OrientationManualEdit.GetOrientation();
                await WebsocketManager.Instance.AddActionPointOrientation(CurrentActionPoint.Data.Id, orientation, name);
            } else { //using robot

                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                IRobot robot = SceneManager.Instance.GetRobot(robotId);
                string armId = null;
                if (robot.MultiArm())
                    armId = DropdownArms.Dropdown.GetValue().ToString();
                await WebsocketManager.Instance.AddActionPointOrientationUsingRobot(CurrentActionPoint.Data.Id, robotId, (string) EndEffectorList.GetValue(), name, armId);
            }
            Close(); //close add menu
            Notifications.Instance.ShowToastMessage("Orientation added successfully");
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }

    public void ShowMenu(Base.ActionPoint actionPoint, bool manualMode) {
        ManualMode = manualMode;
        CurrentActionPoint = actionPoint;

        ManualModeBlock.SetActive(ManualMode);
        LiteModeBlock.SetActive(!ManualMode);

        NameInput.text = CurrentActionPoint.GetFreeOrientationName();
        OrientationManualEdit.SetOrientation(new Orientation());
        UpdateMenu();
        gameObject.SetActive(true);
    }

    public void Close() {
        ActionPointAimingMenu.Instance.SwitchToOrientations();
    }
}
