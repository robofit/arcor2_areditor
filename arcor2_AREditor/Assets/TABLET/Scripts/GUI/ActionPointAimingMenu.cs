using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System.Linq;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleSideMenu;
using Base;
using System;
using Packages.Rider.Editor.UnitTesting;
using OrbCreationExtensions;
using UnityEngine.Events;
using IO.Swagger.Model;

[RequireComponent(typeof(SimpleSideMenu))]
public class ActionPointAimingMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject JointsExpertModeBlock, JointsLiteModeBlock;

    [SerializeField]
    private TMPro.TMP_Text ActionPointName;

    [SerializeField]
    private TooltipContent buttonTooltip;

    [SerializeField]
    private Button UpdatePositionButton;

    [SerializeField]
    private ActionButton OrientationManualDefaultButton;

    public FocusConfirmationDialog FocusConfirmationDialog;

    public DropdownParameter PositionRobotsList, JointsRobotsList, PositionEndEffectorList;
    public DropdownParameterJoints JointsList;

    public GameObject OrientationsDynamicList, JointsDynamicList; //todo [SerializeField] ?? - je to tak v ActionPointMenu.cs

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private AddOrientationMenu AddOrientationMenu;

    [SerializeField]
    private AddJointsMenu AddJointsMenu;

    [SerializeField]
    private OrientationJointsDetailMenu OrientationJointsDetailMenu;

    private SimpleSideMenu SideMenu;


    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
        ProjectManager.Instance.OnActionPointUpdated += OnActionPointUpdated;
    }

    
    private void OnActionPointUpdated(object sender, ActionPointUpdatedEventArgs args) {
        if (CurrentActionPoint != null && CurrentActionPoint.Equals(args.Data)) {
            UpdateMenu();
            
        }
    }
    
    public void UpdateMenu(string preselectedOrientation = null) {
        ActionPointName.text = CurrentActionPoint.Data.Name;

        /*
        DropdownParameter[] robotsLists = { PositionRobotsList, JointsRobotsList };
        UnityAction<string>[] onChangeMethods = { OnRobotChanged, UpdateJointsDynamicList };

        for (int i = 0; i < robotsLists.Length; i++) {
            DropdownParameter robotsList = robotsLists[i];
            robotsList.Dropdown.dropdownItems.Clear();

            robotsList.gameObject.GetComponent<DropdownRobots>().Init(onChangeMethods[i], false);
            if (robotsList.Dropdown.dropdownItems.Count == 0) {
                UpdatePositionBlock.SetActive(false);
            } else {
                onChangeMethods[i]((string) robotsList.GetValue());
                UpdatePositionBlock.SetActive(true);
            }

        }
    */

        CustomDropdown positionRobotsListDropdown = PositionRobotsList.Dropdown;
        positionRobotsListDropdown.dropdownItems.Clear();
        PositionRobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (positionRobotsListDropdown.dropdownItems.Count == 0) {

            buttonTooltip.description = "There is no robot to update position with";
            buttonTooltip.enabled = true;
            UpdatePositionButton.interactable = false;

        } else {
            buttonTooltip.enabled = false;
            UpdatePositionButton.interactable = true;
            OnRobotChanged((string) PositionRobotsList.GetValue());
        }

        JointsRobotsList.Dropdown.dropdownItems.Clear();
        JointsRobotsList.gameObject.GetComponent<DropdownRobots>().Init(UpdateJointsDynamicList, false);
        if (JointsRobotsList.Dropdown.dropdownItems.Count > 0) {
            UpdateJointsDynamicList((string) JointsRobotsList.GetValue());
        }

        UpdateOrientationsDynamicList();
    }

    /*
    public void UpdateJoints(string robot_id, string selectedJoints = null) {
        if (robot_id == null)
            return;
        CustomDropdown jointsDropdown = JointsList.Dropdown;

        JointsList.PutData(CurrentActionPoint.GetAllJoints(true, robot_id).Values.ToList(), selectedJoints, null, CurrentActionPoint.Data.Name);

        if (jointsDropdown.dropdownItems.Count > 0) {
            NoJoints.gameObject.SetActive(false);
            JointsList.gameObject.SetActive(true);
            UpdateJointsBtn.interactable = true;
        } else {
            JointsList.gameObject.SetActive(false);
            NoJoints.gameObject.SetActive(true);
            UpdateJointsBtn.interactable = false;
        }
    }
    */
    private void OnRobotChanged(string robot_name) {
        PositionEndEffectorList.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            PositionEndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);

        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

    }

    /*
    public void ShowAddOrientationDialog() {
        inputDialog.Open("Create new named orientation",
                         "Please set name of the new orientation",
                         "Name",
                         CurrentActionPoint.GetFreeOrientationName(),
                         () => AddOrientation(inputDialog.GetValue(), (string) RobotsList.GetValue()),
                         () => inputDialog.Close());
    }
    */
    /*
    public async void AddOrientation(string name, string robotName) {
        Debug.Assert(CurrentActionPoint != null);
        IRobot robot;
        try {
            robot = SceneManager.Instance.GetRobotByName(robotName);
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add orientation", "Could not found robot called: " + robotName);
            Debug.LogError(ex);
            return;
        }


        if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            Notifications.Instance.ShowNotification("Failed to add orientation", "There already exists orientation or joints with name " + name);
            return;
        }
        IO.Swagger.Model.Orientation orientation = new IO.Swagger.Model.Orientation();
        if (CurrentActionPoint.Parent != null) {
            orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(Quaternion.Inverse(CurrentActionPoint.Parent.GetTransform().rotation)));
        }
        preselectedOrientation = name;
        bool successOrientation = await Base.GameManager.Instance.AddActionPointOrientation(CurrentActionPoint, orientation, name);
        bool successJoints = await Base.GameManager.Instance.AddActionPointJoints(CurrentActionPoint, name, robot.GetId());
        if (successOrientation && successJoints) {
            inputDialog.Close();
        } else {
            preselectedOrientation = null;
        }
    }
    */
    /*
    public void ShowAddJointsDialog() {
        inputDialog.Open("Create new joints configuration",
                         "Please set name of the new joints configuration",
                         "Name",
                         CurrentActionPoint.GetFreeJointsName(),
                         () => AddJoints(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }
    */
    /*
    public async void AddJoints(string name) {
        Debug.Assert(CurrentActionPoint != null);
        preselectedJoints = name;
        bool success = await Base.GameManager.Instance.AddActionPointJoints(CurrentActionPoint, name, (string) RobotsList.GetValue());
        if (success) {
            inputDialog.Close();
        } else {
            preselectedJoints = null;
        }
    }
    */
    /*
    public void FocusJoints() {
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        if (jointsDropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", "");
            return;
        }
        try {
            preselectedJoints = name;
            string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
            Base.GameManager.Instance.UpdateActionPointJoints(robotId, (string) JointsList.GetValue());
            Base.NotificationsModernUI.Instance.ShowNotification("Joints updated sucessfully", "");


        } catch (Exception ex) when (ex is Base.RequestFailedException || ex is ItemNotFoundException) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", ex.Message);
            preselectedJoints = null;
        }

    }
    */

    public void ShowUpdatePositionConfirmationDialog() {
        try {
            string robotId = SceneManager.Instance.RobotNameToId(PositionRobotsList.Dropdown.selectedText.text);
            string endEffectorId = PositionEndEffectorList.Dropdown.selectedText.text;

            //TODO show the confirmation dialog
            Base.GameManager.Instance.UpdateActionPointPositionUsingRobot(CurrentActionPoint.Data.Id, robotId, endEffectorId);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Update position failed", "");
        }
    }

    /*
    public void ShowFocusConfirmationDialog() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 ||
            PositionEndEffectorList.Dropdown.dropdownItems.Count == 0 ||
            OrientationsList.Dropdown.dropdownItems.Count == 0) {
            Base.Notifications.Instance.ShowNotification("Failed to update orientation.", "Something is not selected");
            return;
        }
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        CustomDropdown endEffectorDropdown = PositionEndEffectorList.Dropdown;
        CustomDropdown orientationDropdown = OrientationsList.Dropdown;
        if (endEffectorDropdown.dropdownItems.Count == 0) {
            FocusConfirmationDialog.EndEffectorId = "";
        } else {
            FocusConfirmationDialog.EndEffectorId = endEffectorDropdown.selectedText.text;
        }
        FocusConfirmationDialog.RobotName = robotsListDropdown.selectedText.text;

        FocusConfirmationDialog.OrientationId = CurrentActionPoint.GetNamedOrientationByName(orientationDropdown.selectedText.text).Id;
        FocusConfirmationDialog.JointsId = CurrentActionPoint.GetJointsByName(orientationDropdown.selectedText.text).Id;
        FocusConfirmationDialog.OrientationName = orientationDropdown.selectedText.text;
        FocusConfirmationDialog.UpdatePosition = UpdatePositionToggle.GetComponent<Toggle>().isOn;
        FocusConfirmationDialog.ActionPointId = CurrentActionPoint.Data.Id;
        FocusConfirmationDialog.ActionPointName = CurrentActionPoint.Data.Name;
        if (FocusConfirmationDialog.Init())
            FocusConfirmationDialog.WindowManager.OpenWindow();
    }
    */

    public void ShowMenu(Base.ActionPoint actionPoint, string preselectedOrientation = null) {
        CurrentActionPoint = actionPoint;
        JointsExpertModeBlock.SetActive(GameManager.Instance.ExpertMode);
        JointsLiteModeBlock.SetActive(!GameManager.Instance.ExpertMode);
        OrientationManualDefaultButton.SetLabel(GameManager.Instance.ExpertMode ? "Manual" : "Default");
        UpdateMenu(preselectedOrientation);
        SideMenu.Open();
    }

    public void Close() {
        SideMenu.Close();
    }

    public void UpdateMenu() {
        UpdateMenu(null);
    }

    public void UpdateOrientationsDynamicList() {
        foreach (RectTransform o in OrientationsDynamicList.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }

        foreach (IO.Swagger.Model.NamedOrientation orientation in CurrentActionPoint.GetNamedOrientations()) {
            ActionButton btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, OrientationsDynamicList.transform).GetComponent<ActionButton>();
            btn.transform.localScale = new Vector3(1, 1, 1);
            btn.SetLabel(orientation.Name);

            btn.Button.onClick.AddListener(() => OpenDetailMenu(orientation));
        }
    }


    public void UpdateJointsDynamicList(string robotName) {
        if (robotName == null)
            return;

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robotName);

            foreach (RectTransform o in JointsDynamicList.GetComponentsInChildren<RectTransform>()) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }

            foreach (IO.Swagger.Model.ProjectRobotJoints joint in CurrentActionPoint.GetAllJoints(true, robotId).Values.ToList()) {
                ActionButton btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, JointsDynamicList.transform).GetComponent<ActionButton>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetLabel(joint.Name);

                btn.Button.onClick.AddListener(() => OpenDetailMenu(joint));
            }
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to get robot's ID", "");
            return;
        }
    }

    private void OpenDetailMenu(ProjectRobotJoints joint) {
        OrientationJointsDetailMenu.ShowMenu(CurrentActionPoint, joint);
    }

    private void OpenDetailMenu(NamedOrientation orientation) {
        OrientationJointsDetailMenu.ShowMenu(CurrentActionPoint, orientation);
    }

    /*
public void UpdateJoints(string robot_id, string selectedJoints = null) {
   if (robot_id == null)
       return;
   CustomDropdown jointsDropdown = JointsList.Dropdown;

   JointsList.PutData(CurrentActionPoint.GetAllJoints(true, robot_id).Values.ToList(), selectedJoints, null, CurrentActionPoint.Data.Name);

   if (jointsDropdown.dropdownItems.Count > 0) {
       NoJoints.gameObject.SetActive(false);
       JointsList.gameObject.SetActive(true);
       UpdateJointsBtn.interactable = true;
   } else {
       JointsList.gameObject.SetActive(false);
       NoJoints.gameObject.SetActive(true);
       UpdateJointsBtn.interactable = false;
   }
}*/

    /// <summary>
    /// If expert mode is active - opens add orientation side menu in manual mode, otherwise adds default orientation (0,0,0,1)
    /// </summary>
    public void OpenAddOrientationMenuManualDefault() {
        if (GameManager.Instance.ExpertMode) {
            AddOrientationMenu.ShowMenu(CurrentActionPoint, true);
        } else {
            AddDefaultOrientation();
        }
    }

    public void OpenAddOrientationMenuUsingRobot() {
        AddOrientationMenu.ShowMenu(CurrentActionPoint, false);
    }

    public void OpenAddJointsMenu(bool manual) {
        AddJointsMenu.ShowMenu(CurrentActionPoint, manual);
    }

    public async void AddDefaultOrientation() {
        Debug.Assert(CurrentActionPoint != null);

        IO.Swagger.Model.Orientation orientation = new Orientation();
        name = CurrentActionPoint.GetFreeOrientationName();

        bool success = await Base.GameManager.Instance.AddActionPointOrientation(CurrentActionPoint, orientation, name);

        if (success) {
            //todo open detail of the new orientation?
            UpdateMenu();

        }
    }
}
