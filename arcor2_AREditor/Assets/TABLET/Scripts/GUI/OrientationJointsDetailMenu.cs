using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;
using Base;
using IO.Swagger.Model;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SimpleSideMenu))]
public class OrientationJointsDetailMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject OrientationBlock, OrientationExpertModeBlock, JointsBlock, JointsExpertModeBlock, InvalidJointsLabel;

    public OrientationManualEdit OrientationManualEdit;

    public Slider SpeedSlider;

    [SerializeField]
    private TooltipContent updateButtonTooltip, manualOrientationEditTooltip, manualJointsEditTooltip, moveRobotTooltip, moveModelTooltip;

    [SerializeField]
    private Button UpdateButton, ManualOrientationEditButton, moveRobotButton, moveModelButton;

    [SerializeField]
    private TMPro.TMP_InputField DetailName; //name of current orientation/joints
    [SerializeField]
    private TMPro.TMP_Text RobotName; //name of robot - only for joints


    public DropdownParameter RobotsList, EndEffectorList; //only for orientation

    public GameObject JointsDynamicList;

    public ConfirmationDialog ConfirmationDialog;

    private SimpleSideMenu SideMenu;
    private NamedOrientation orientation;
    private ProjectRobotJoints joints;
    private bool isOrientationDetail; //true for orientation, false for joints

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
        WebsocketManager.Instance.OnActionPointOrientationUpdated += OnActionPointOrientationUpdated;
        WebsocketManager.Instance.OnActionPointJointsUpdated += OnActionPointJointsUpdated;
    }

    private void OnActionPointJointsUpdated(object sender, RobotJointsEventArgs args) {
        if (joints != null && joints.Id == args.Data.Id) {
            joints = args.Data;
            UpdateMenu();
        }
    }

    private void OnActionPointOrientationUpdated(object sender, ActionPointOrientationEventArgs args) {
         if (orientation != null && orientation.Id == args.Data.Id) {
            orientation = args.Data;
            UpdateMenu();
         }
     }


    public async void UpdateMenu() {
        if (isOrientationDetail) {  //orientation

            DetailName.text = orientation.Name;

            RobotsList.Dropdown.dropdownItems.Clear();
            await RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);

            if (SceneManager.Instance.SceneStarted && SceneManager.Instance.RobotInScene()) {
                OrientationBlock.SetActive(true);
                EnableButtons(true);
                OnRobotChanged((string) RobotsList.GetValue());
            } else if (!SceneManager.Instance.RobotInScene()) {
                OrientationBlock.SetActive(false);

                updateButtonTooltip.description = "There is no robot to update orientation with";
                moveRobotTooltip.description = "There is no robot";
                moveModelTooltip.description = moveRobotTooltip.description;
                EnableButtons(false);
            } else { //scene not started
                OrientationBlock.SetActive(false);

                updateButtonTooltip.description = "Scene is not started";
                moveRobotTooltip.description = updateButtonTooltip.description;
                moveModelTooltip.description = updateButtonTooltip.description;
                EnableButtons(false);
            }

            OrientationManualEdit.SetOrientation(orientation.Orientation);
            ValidateFieldsOrientation();
        } else { //joints
            if (!SceneManager.Instance.SceneStarted) {
                updateButtonTooltip.description = "Scene is not started";
                moveRobotTooltip.description = updateButtonTooltip.description;
                moveModelTooltip.description = updateButtonTooltip.description;
                EnableButtons(false);
            } else {
                EnableButtons(true);
            }
            DetailName.text = joints.Name;
            InvalidJointsLabel.SetActive(!joints.IsValid);

            UpdateJointsValues();

        }
    }

    /// <summary>
    /// Sets interactibility of buttons and enables/disables tooltips
    /// </summary>
    /// <param name="enable">True if buttons should be interactable</param>
    private void EnableButtons(bool enable) {
        updateButtonTooltip.enabled = !enable;
        moveRobotTooltip.enabled = !enable;
        moveModelTooltip.enabled = !enable;
        UpdateButton.interactable = enable;
        moveRobotButton.interactable = enable;
        moveModelButton.interactable = enable;
    }

    private async void OnRobotChanged(string robot_name) {
        EndEffectorList.Dropdown.dropdownItems.Clear();

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            await EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, null);

        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }
    }

    /// <summary>
    /// Updates list of joints in expert block
    /// </summary>
    private void UpdateJointsList() {
        DestroyJointsFields();

        foreach (IO.Swagger.Model.Joint joint in joints.Joints) {
            LabeledInput labeledInput = Instantiate(GameManager.Instance.LabeledFloatInput, JointsDynamicList.transform).GetComponent<LabeledInput>();
            labeledInput.SetLabel(joint.Name, joint.Name);

            NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
            labeledInput.SetValue(joint.Value.ToString(numberFormatInfo));
        }
    }

    private void DestroyJointsFields() {
        foreach (RectTransform o in JointsDynamicList.GetComponentsInChildren<RectTransform>()) {
            if (!o.gameObject.CompareTag("Persistent")) {
                Destroy(o.gameObject);
            }
        }
    }

    /// <summary>
    /// returns LabeledInput from joints dynamic list
    /// </summary>
    /// <param name="name">Name of the joint</param>
    /// <returns></returns>
    private LabeledInput GetJointInput(string name) {
        foreach (LabeledInput jointInput in JointsDynamicList.GetComponentsInChildren<LabeledInput>()) {
            if (jointInput.GetName() == name) {
                return jointInput;
            }
        }
        throw new ItemNotFoundException("Joint input not found");
    }

    /// <summary>
    /// Updates values of joints (angles) in joints dynamic list (expert block)
    /// </summary>
    private void UpdateJointsValues() {
        foreach (IO.Swagger.Model.Joint joint in joints.Joints) {
            LabeledInput labeledInput = GetJointInput(joint.Name);
            NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
            labeledInput.SetValue(joint.Value.ToString(numberFormatInfo));
        }
    }

    public async void OnJointsSaveClick() {
        List<IO.Swagger.Model.Joint> updatedJoints = new List<IO.Swagger.Model.Joint>();
        try {
            foreach (LabeledInput input in JointsDynamicList.GetComponentsInChildren<LabeledInput>()) {
                decimal value = decimal.Parse(input.Input.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                updatedJoints.Add(new IO.Swagger.Model.Joint(input.GetName(), value));
            }

            await WebsocketManager.Instance.UpdateActionPointJoints(joints.Id, updatedJoints);
            Notifications.Instance.ShowToastMessage("Joints updated successfully");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Joints update failed", ex.Message);
            return;
        } catch (Exception ex) { //decimal parsing exceptions
            Notifications.Instance.ShowNotification("Incorrect joint value", ex.Message);
            return;
        }
    }

    public async void OnOrientationSaveClick() {
        try {
            await WebsocketManager.Instance.UpdateActionPointOrientation(OrientationManualEdit.GetOrientation(), orientation.Id);
            Notifications.Instance.ShowToastMessage("Orientation updated successfully");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to update orientation", ex.Message);
        }
    }


    public async void UpdateUsingRobot() {
        if (isOrientationDetail)
        {
            try {
                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                await WebsocketManager.Instance.UpdateActionPointOrientationUsingRobot(robotId, (string) EndEffectorList.GetValue(), orientation.Id);
                Notifications.Instance.ShowToastMessage("Orientation updated successfully");
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed update orientation", ex.Message);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update orientation", ex.Message);
            }
        }
        else //joints
        {
            try {
                await WebsocketManager.Instance.UpdateActionPointJointsUsingRobot(joints.Id);
                Notifications.Instance.ShowToastMessage("Joints updated successfully");
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to update joints", ex.Message);
            }
        }
        ConfirmationDialog.Close();
        UpdateMenu();
    }

    public async void Delete() {
        try {
            if (isOrientationDetail) {
                await WebsocketManager.Instance.RemoveActionPointOrientation(orientation.Id);
            } else {
                await WebsocketManager.Instance.RemoveActionPointJoints(joints.Id);
            }
            ConfirmationDialog.Close();
            Close();

        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed delete orientation/joints", e.Message);
        }
    }

    public void ShowDeleteDialog() {
        string title = isOrientationDetail ? "Delete orientation" : "Delete joints";
        string description = "Do you want to delete " + (isOrientationDetail ? "orientation " : "joints ") + (isOrientationDetail ? orientation.Name : joints.Name) + "?"; 
        ConfirmationDialog.Open(title,
                                description,
                                () => Delete(),
                                () => ConfirmationDialog.Close());
    }

    /// <summary>
    /// Shows confirmation dialog for updating orientation/joints using robot
    /// </summary>
    public void ShowUpdateUsingRobotDialog() {
        string title = isOrientationDetail ? "Update orientation" : "Update joints";
        string description = "Do you want to update ";
        if (isOrientationDetail) {
            description += "orientation using robot: " + (string) RobotsList.GetValue() + " and end effector: " + (string) EndEffectorList.GetValue() + "?";
        } else {
            description += "joints using robot: " + RobotName.text + "?";
        }
        ConfirmationDialog.Open(title,
                                description,
                                () => UpdateUsingRobot(),
                                () => ConfirmationDialog.Close());
    }

    public async void Rename() {
        try {
            string name = DetailName.text;
            
            if (isOrientationDetail) {
                if (name == orientation.Name) {
                    return;
                }
                await WebsocketManager.Instance.RenameActionPointOrientation(orientation.Id, name);
                Notifications.Instance.ShowToastMessage("Orientation renamed successfully");
            } else {
                if (name == joints.Name) {
                    return;
                }
                await WebsocketManager.Instance.RenameActionPointJoints(joints.Id, name);
                Notifications.Instance.ShowToastMessage("Joints renamed successfully");
            }
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to rename orientation/joints", ex.Message);
            UpdateMenu();
        }
    }

    public async void MoveHereRobot() {
        try {
            if (isOrientationDetail) {
                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                await WebsocketManager.Instance.MoveToActionPointOrientation(robotId, (string) EndEffectorList.GetValue(), (decimal) SpeedSlider.value, orientation.Id);
            } else {
                await WebsocketManager.Instance.MoveToActionPointJoints(joints.RobotId, (decimal) SpeedSlider.value, joints.Id);
            }
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
        }
    }

    public async void MoveHereModel() {
        //TODO 
        Notifications.Instance.ShowNotification("Not implemented yet", "");
    }

    public async void ValidateFieldsOrientation() {
        bool interactable = true;

        manualOrientationEditTooltip.description = OrientationManualEdit.ValidateFields();
        if (!string.IsNullOrEmpty(manualOrientationEditTooltip.description)) {
            interactable = false;
        }

        manualOrientationEditTooltip.enabled = !interactable;
        ManualOrientationEditButton.interactable = interactable;
    }


    public void Close() {
        CurrentActionPoint.GetGameObject().SendMessage("Select", false);
        if (!isOrientationDetail) {
            DestroyJointsFields();
        }
        SideMenu.Close();
    }


    public void ShowMenu(Base.ActionPoint currentActionPoint, NamedOrientation orientation) {
        this.orientation = orientation;
        this.isOrientationDetail = true;

        ShowMenu(currentActionPoint);
    }

    public void ShowMenu(Base.ActionPoint currentActionPoint, ProjectRobotJoints joints) {
        this.joints = joints;
        isOrientationDetail = false;
        try {
            RobotName.text = SceneManager.Instance.GetRobot(joints.RobotId).GetName();
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification(ex.Message, "");
        }
        UpdateJointsList();
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
