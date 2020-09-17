using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class AddJointsMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;
    public DropdownParameter RobotsList;


    [SerializeField]
    private Button CreateNewJoints;

    [SerializeField]
    private TooltipContent buttonTooltip;

    private SimpleSideMenu SideMenu;


    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
    }


    public void UpdateMenu() {
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init((string x) =>{ }, false);

        ValidateFields();
    }

    public async void ValidateFields() {
        bool interactable = true;
        name = NameInput.text;

        if (string.IsNullOrEmpty(name)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        } else if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + name;
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
            await Base.WebsocketManager.Instance.AddActionPointJoints(CurrentActionPoint.Data.Id, robot.GetId(), name);
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
        SideMenu.Open();
    }

    public void Close() {
        SideMenu.Close();
    }
}
