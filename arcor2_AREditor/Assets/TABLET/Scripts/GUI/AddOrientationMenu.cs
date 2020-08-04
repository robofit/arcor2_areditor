using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class AddOrientationMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput, QuaternionX, QuaternionY, QuaternionZ, QuaternionW;
    public DropdownParameter RobotsList, EndEffectorList;
    public GameObject LiteModeBlock, ExpertModeBlock;
    public bool ExpertModeBool;


    [SerializeField]
    private Button CreateNewOrientation;

    [SerializeField]
    private TooltipContent buttonTooltip;

    [SerializeField]
    private ActionPointAimingMenu ActionPointAimingMenu;

    private SimpleSideMenu SideMenu;

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
    }

  

    public void UpdateMenu() {
        //ActionPointName.text = CurrentActionPoint.Data.Name;

        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (robotsListDropdown.dropdownItems.Count > 0) {
            OnRobotChanged((string) RobotsList.GetValue());
        }

        ValidateFields();
    }

    /// <summary>
    /// updates EndEffectorList on selected robot change
    /// </summary>
    /// <param name="robot_name">Newly selected robot's name</param>
    private void OnRobotChanged(string robot_name) {
        EndEffectorList.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);
            if (EndEffectorList.Dropdown.dropdownItems.Count == 0) {
                //todo nedovolit create?
            }
        }
        catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

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

        if (ExpertModeBool) {
            if (interactable) {
                if (string.IsNullOrEmpty(QuaternionX.text) || string.IsNullOrEmpty(QuaternionY.text) || string.IsNullOrEmpty(QuaternionZ.text) || string.IsNullOrEmpty(QuaternionW.text)) {
                    interactable = false;
                    buttonTooltip.description = "All quaternion values are required";
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

    public async void AddOrientation()
    {
        Debug.Assert(CurrentActionPoint != null);

        string name = NameInput.text;
        IO.Swagger.Model.Orientation orientation = null;

        if (ExpertModeBool) {
                orientation = new IO.Swagger.Model.Orientation(Convert.ToDecimal(QuaternionW.text), Convert.ToDecimal(QuaternionX.text), Convert.ToDecimal(QuaternionY.text), Convert.ToDecimal(QuaternionZ.text));
        }
        else {
            if (CurrentActionPoint.Parent != null) {
                orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(Quaternion.Inverse(CurrentActionPoint.Parent.GetTransform().rotation)));
            }
        }
        
        bool success = await Base.GameManager.Instance.AddActionPointOrientation(CurrentActionPoint, orientation, name);

        if (success) {
            //TODO: after adding this menu to menuManager uncomment line below - should delete old values after creating new orientation
            //MenuManager.Instance.AddOrientationMenu.Close();
            Close();
            //todo open detail of the new orientation
        }
    }

    public void ShowMenu(Base.ActionPoint actionPoint, string preselectedOrientation = null) {
        CurrentActionPoint = actionPoint;
        ExpertModeBlock.SetActive(ExpertModeBool);
        LiteModeBlock.SetActive(!ExpertModeBool);
        UpdateMenu();
        SideMenu.Open();
    }

    public void Close() {
        ActionPointAimingMenu.UpdateMenu();
        SideMenu.Close();
    }
}
