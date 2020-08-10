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
public class OrientationJointsDetailMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject OrientationBlock, OrientationExpertModeBlock, JointsBlock, JointsExpertModeBlock;

    [SerializeField]
    private TMPro.TMP_InputField DetailName; //name of current orientation/joints
    [SerializeField]
    private TMPro.TMP_Text RobotName; //name of robot - only for joints


    public DropdownParameter RobotsList, EndEffectorList; //only for orientation

    public GameObject JointsDynamicList; //todo [SerializeField] ?? - je to tak v ActionPointMenu.cs


    private SimpleSideMenu SideMenu;
    private NamedOrientation orientation;
    private ProjectRobotJoints joints;
    private bool isOrientationDetail; //true for orientation, false for joints

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
    }


    public void UpdateMenu() {
        RobotsList.Dropdown.dropdownItems.Clear();
        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        OnRobotChanged((string) RobotsList.GetValue());
        if (!isOrientationDetail) {
           UpdateJointsList();
        }
    }


    private void OnRobotChanged(string robot_name) {
        EndEffectorList.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);

        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }
    }

    public void UpdateOrientationOrJointsUsingRobot() {
        if (isOrientationDetail) {
            try {
                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                GameManager.Instance.UpdateActionPointOrientationUsingRobot(CurrentActionPoint.Data.Id, robotId, (string) EndEffectorList.GetValue(), orientation.Id);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to load end effectors", "");
            }
        }
        else //joints
        {
            GameManager.Instance.UpdateActionPointJoints(joints.RobotId, joints.Id);
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

    public void ShowUpdatePositionConfirmationDialog() {
        CustomDropdown positionRobotsListDropdown = PositionRobotsList.Dropdown;
        Base.Notifications.Instance.ShowNotification("klik na update position", "selcted: " + positionRobotsListDropdown.selectedText.text);
    }

    */

    public void UpdateJointsList() {
        foreach (RectTransform o in JointsDynamicList.GetComponentsInChildren<RectTransform>()) {
            if (!o.gameObject.CompareTag("Persistent")) {
                Destroy(o.gameObject);
            }
        }

        foreach (IO.Swagger.Model.Joint joint in joints.Joints) {
            LabeledInput labeledInput = Instantiate(GameManager.Instance.LabeledFloatInput, JointsDynamicList.transform).GetComponent<LabeledInput>();
            labeledInput.SetLabel(joint.Name, joint.Name);
            labeledInput.SetValue(joint.Value);
        }
    }

    public void OnJointsSaveClick() {
        //TODO
    }

    public void OnOrientationSaveClick() {
        //TODO
    }

    public void OnDeleteClick() {
        //TODO
    }

    public void Rename() {
        //TODO
    }


    public void Close() {
        SideMenu.Close();
    }


    public void ShowMenu(Base.ActionPoint currentActionPoint, NamedOrientation orientation) {
        this.orientation = orientation;
        this.isOrientationDetail = true;
        DetailName.text = orientation.Name;
        ShowMenu(currentActionPoint);
    }

    public void ShowMenu(Base.ActionPoint currentActionPoint, ProjectRobotJoints joints) {
        this.joints = joints;
        isOrientationDetail = false;
        DetailName.text = joints.Name;
        RobotName.text = joints.RobotId;
        ShowMenu(currentActionPoint);
    }

    private void ShowMenu(Base.ActionPoint actionPoint) {
        CurrentActionPoint = actionPoint;

        OrientationBlock.SetActive(isOrientationDetail);
        OrientationExpertModeBlock.SetActive(isOrientationDetail && GameManager.Instance.ExpertMode);
        JointsBlock.SetActive(!isOrientationDetail);
        JointsExpertModeBlock.SetActive(!isOrientationDetail && GameManager.Instance.ExpertMode);



        UpdateMenu();
        SideMenu.Open();
    }
}
