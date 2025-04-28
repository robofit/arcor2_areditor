using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ActionPoint = Base.ActionPoint;
using Joint = Arcor2.ClientSdk.Communication.OpenApi.Models.Joint;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class OrientationJointsDetailMenu : MonoBehaviour, IMenu {
    public ActionPoint CurrentActionPoint;

    public GameObject OrientationExpertModeBlock, JointsBlock, JointsExpertModeBlock, InvalidJointsLabel;

    public OrientationManualEdit OrientationManualEdit;

    public Slider SpeedSlider;

    [SerializeField]
    private TooltipContent updateButtonTooltip, manualOrientationEditTooltip, manualJointsEditTooltip, moveRobotTooltip, moveModelTooltip;

    [SerializeField]
    private Button UpdateButton, ManualOrientationEditButton, moveRobotButton, moveModelButton;

    [SerializeField]
    private TMP_InputField DetailName; //name of current orientation/joints
    [SerializeField]
    private TMP_Text RobotName, ArmName; //name of robot and arm - only for joints

    public SwitchComponent SafeMove;

    public GameObject JointsDynamicList;

    public ConfirmationDialog ConfirmationDialog;
    public NamedOrientation orientation;
    public ProjectRobotJoints joints;
    private bool isOrientationDetail; //true for orientation, false for joints

    //visibility of robot model backup: Dictionary<robotID,visibilityValue>
    private Dictionary<string, float> robotVisibilityBackup = new();


    private void Start() {
        CommunicationManager.Instance.Client.OrientationUpdated += CommunicationManager.SafeEventHandler<OrientationEventArgs>(OnActionPointOrientationUpdated);
        CommunicationManager.Instance.Client.JointsUpdated += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsUpdated);
    }

    private void OnActionPointJointsUpdated(object sender, JointsEventArgs args) {
        if (!gameObject.activeInHierarchy)
            return;
        if (joints != null && joints.Id == args.Data.Id) {
            joints = args.Data;
            UpdateMenu();
            MoveHereModel();
        }
    }

    private void OnActionPointOrientationUpdated(object sender, OrientationEventArgs args) {
        if (!gameObject.activeInHierarchy)
            return;
        if (orientation != null && orientation.Id == args.Data.Id) {
            orientation = args.Data;
            UpdateMenu();
        }
    }


    public async void UpdateMenu() {
        if (isOrientationDetail) {  //orientation

            DetailName.text = orientation.Name;


            if (SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotAndEESelected()) {
                EnableButtons(true);
            } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
                updateButtonTooltip.description = "There is no selected robot to update orientation with";
                moveRobotTooltip.description = "There is no selected robot";
                moveModelTooltip.description = moveRobotTooltip.description;
                EnableButtons(false);
            } else { //scene not started

                updateButtonTooltip.description = "Not available when offline";
                moveRobotTooltip.description = updateButtonTooltip.description;
                moveModelTooltip.description = updateButtonTooltip.description;
                EnableButtons(false);
            }

            OrientationManualEdit.SetOrientation(orientation.Orientation);
            ValidateFieldsOrientation();
        } else { //joints
            if (!SceneManager.Instance.SceneStarted) {
                updateButtonTooltip.description = "Not available when offline";
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

    /// <summary>
    /// Updates list of joints in expert block
    /// </summary>
    private void UpdateJointsList() {
        DestroyJointsFields();

        foreach (Joint joint in joints.Joints) {
            LabeledInput labeledInput = Instantiate(GameManager.Instance.LabeledFloatInput, JointsDynamicList.transform).GetComponent<LabeledInput>();
            labeledInput.SetLabel(joint.Name, joint.Name);

            NumberFormatInfo numberFormatInfo = new();
            numberFormatInfo.NumberDecimalSeparator = ".";
            labeledInput.SetValue(joint.Value.ToString(numberFormatInfo));
            //labeledInput.Input.placeholder.color = Color.white;
            // text object of TMP input cannot be accessed directly
            //labeledInput.Input.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
            //      .First(c => c.gameObject.name == "Text").color = Color.white;
            labeledInput.SetDarkMode(false);
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
        foreach (Joint joint in joints.Joints) {
            LabeledInput labeledInput = GetJointInput(joint.Name);
            NumberFormatInfo numberFormatInfo = new();
            numberFormatInfo.NumberDecimalSeparator = ".";
            labeledInput.SetValue(joint.Value.ToString(numberFormatInfo));
        }
    }

    public async void OnJointsSaveClick() {
        List<Joint> updatedJoints = new();
        try {
            foreach (LabeledInput input in JointsDynamicList.GetComponentsInChildren<LabeledInput>()) {
                decimal value = decimal.Parse(input.Input.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                updatedJoints.Add(new Joint(input.GetName(), value));
            }

            var response = await CommunicationManager.Instance.Client.UpdateActionPointJointsAsync(new UpdateActionPointJointsRequestArgs(joints.Id, updatedJoints));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Joints update failed", string.Join(',', response.Messages));
                return;
            }
            Notifications.Instance.ShowToastMessage("Joints updated successfully");
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Joints update failed", ex.Message);
            return;
        } catch (Exception ex) { //decimal parsing exceptions
            Notifications.Instance.ShowNotification("Incorrect joint value", ex.Message);
            return;
        }
    }

    public async void OnOrientationSaveClick() {
        try {
            var response = await CommunicationManager.Instance.Client.UpdateActionPointOrientationAsync(new UpdateActionPointOrientationRequestArgs(orientation.Id, OrientationManualEdit.GetOrientation()));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to update orientation", string.Join(',', response.Messages));
                return;
            }
            Notifications.Instance.ShowToastMessage("Orientation updated successfully");
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to update orientation", ex.Message);
        }
    }


    public async void UpdateUsingRobot() {
        if (isOrientationDetail) {
            try {
                string armId = null;
                if (SceneManager.Instance.SelectedRobot.MultiArm())
                    armId = SceneManager.Instance.SelectedArmId;
                var response = await CommunicationManager.Instance.Client.UpdateActionPointOrientationUsingRobotAsync(
                    new UpdateActionPointOrientationUsingRobotRequestArgs(orientation.Id,
                        new RobotArg(SceneManager.Instance.SelectedRobot.GetId(),
                            SceneManager.Instance.SelectedEndEffector.GetName(), armId)));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to update orientation", string.Join(',', response.Messages));
                    return;
                }
                Notifications.Instance.ShowToastMessage("Orientation updated successfully");
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed update orientation", ex.Message);
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to update orientation", ex.Message);
            }
        } else //joints
          {
            try {
                var response = await CommunicationManager.Instance.Client.UpdateActionPointJointsUsingRobotAsync(
                    new UpdateActionPointJointsUsingRobotRequestArgs(joints.Id));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to update joints", string.Join(',', response.Messages));
                    return;
                }
                Notifications.Instance.ShowToastMessage("Joints updated successfully");
            } catch (Arcor2ConnectionException ex) {
                Notifications.Instance.ShowNotification("Failed to update joints", ex.Message);
            }
        }
        ConfirmationDialog.Close();
        UpdateMenu();
    }

    public async void Delete() {
        try {
            if (isOrientationDetail) {
                var response = await CommunicationManager.Instance.Client.RemoveActionPointOrientationAsync(
                    new RemoveActionPointOrientationRequestArgs(orientation.Id), false);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to delete orientation",
                        string.Join(',', response.Messages));
                    return;
                }
            } else {
                var response = await CommunicationManager.Instance.Client.RemoveActionPointJointsAsync(
                    new RemoveActionPointJointsRequestArgs(joints.Id));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to delete joints",
                        string.Join(',', response.Messages));
                    return;
                }
            }

            ConfirmationDialog.Close();
            HideMenu();

        } catch (Arcor2ConnectionException e) {
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
            description += "orientation using robot: " + SceneManager.Instance.SelectedRobot.GetName() + " and end effector: " + SceneManager.Instance.SelectedEndEffector.GetName() + "?";
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
                var response = await CommunicationManager.Instance.Client.RenameActionPointOrientationAsync(new RenameActionPointOrientationRequestArgs(orientation.Id, name));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to rename orientation", string.Join(',', response.Messages));
                    return;
                }
                Notifications.Instance.ShowToastMessage("Orientation renamed successfully");
            } else {
                if (name == joints.Name) {
                    return;
                }
                var response = await CommunicationManager.Instance.Client.RenameActionPointJointsAsync(new RenameActionPointJointsRequestArgs(joints.Id, name));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to rename joints", string.Join(',', response.Messages));
                    return;
                }
                Notifications.Instance.ShowToastMessage("Joints renamed successfully");
            }
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to rename orientation/joints", ex.Message);
            UpdateMenu();
        }
    }

    public async void MoveHereRobot() {
        try {
            if (isOrientationDetail) {
                string armId = null;
                if (SceneManager.Instance.SelectedRobot.MultiArm())
                    armId = SceneManager.Instance.SelectedArmId;

                var response = await CommunicationManager.Instance.Client.MoveToActionPointAsync(new MoveToActionPointRequestArgs(
                    joints.RobotId,
                    endEffectorId: SceneManager.Instance.SelectedEndEffector.GetName(),
                    speed: (decimal) SpeedSlider.value,
                    orientationId: orientation.Id,
                    safe: (bool) SafeMove.GetValue(),
                    armId: joints.ArmId
                ));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to move robot", string.Join(',', response.Messages));
                }
            } else {
                var response = await CommunicationManager.Instance.Client.MoveToActionPointAsync(new MoveToActionPointRequestArgs(
                    joints.RobotId,
                    (decimal) SpeedSlider.value,
                    jointsId: joints.Id,
                    safe: (bool) SafeMove.GetValue(),
                    armId: joints.ArmId
                    ));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to move robot", string.Join(',', response.Messages));
                }
            }
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to move robot", ex.Message);
        }
    }

    public async void MoveHereModel(bool avoid_collision = true) {
        List<Joint> modelJoints; //joints to move the model to
        string robotId;

        if (isOrientationDetail) {
            try {
                Pose pose = new(orientation: orientation.Orientation, position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(CurrentActionPoint.transform.position)));
                List<Joint> startJoints = SceneManager.Instance.SelectedRobot.GetJoints();

                var response = await CommunicationManager.Instance.Client.InverseKinematicsAsync(
                    new InverseKinematicsRequestArgs(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), pose, startJoints, true));
                if (!response.Result) {
                    if (avoid_collision)
                        MoveHereModel(false);
                    else
                        Notifications.Instance.ShowNotification("Unable to move here model", string.Join(',', response.Messages));
                    return;
                }

                modelJoints = response.Data;
                await PrepareRobotModel(SceneManager.Instance.SelectedRobot.GetId(), false);
                if (!avoid_collision) {
                    Notifications.Instance.ShowNotification("The model is in a collision with other object!", "");
                }
            } catch (ItemNotFoundException ex) {
                Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
                return;
            } catch (Arcor2ConnectionException ex) {
                if (avoid_collision) //if this is first call, try it again without avoiding collisions
                    MoveHereModel(false);
                else
                    Notifications.Instance.ShowNotification("Unable to move here model", ex.Message);
                return;
            }

        } else { //joints menu
            modelJoints = joints.Joints;
            robotId = joints.RobotId;
        }

        foreach (Joint joint in modelJoints) {
            SceneManager.Instance.SelectedRobot.SetJointValue(joint.Name, (float) joint.Value);
        }
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

    /// <summary>
    /// Prepares robot model for setting position (joints angles) different to the position of real robot.
    /// Takes care of: (un)registering to joints stream, grey color and visibility
    /// </summary>
    /// <param name="robotID"></param>
    /// <param name="shadowRealRobot">Should model shadow position of real robot?</param>
    /// <returns></returns>
    private async Task PrepareRobotModel(string robotID, bool shadowRealRobot) {
        if (shadowRealRobot) {
            robotVisibilityBackup.TryGetValue(robotID, out float originalVisibility);
            SceneManager.Instance.GetActionObject(robotID).SetVisibility(originalVisibility);
        } else {
            if (!robotVisibilityBackup.TryGetValue(robotID, out _)) {
                robotVisibilityBackup.Add(robotID, SceneManager.Instance.GetActionObject(robotID).GetVisibility());
                SceneManager.Instance.GetActionObject(robotID).SetVisibility(1f);
            }
        }

        if (SceneManager.Instance.SceneStarted) {
            var response = await CommunicationManager.Instance.Client.RegisterForRobotEventAsync(new RegisterForRobotEventRequestArgs(robotID, RegisterForRobotEventRequestArgs.WhatEnum.Joints, shadowRealRobot));
            if (!response.Result) {
                // Nothing?
            }

            SceneManager.Instance.GetRobot(robotID).SetGrey(!shadowRealRobot, true);
            SceneManager.Instance.GetActionObject(robotID).SetInteractivity(shadowRealRobot);
        } else { //is possible only for joints, not orientation
            foreach (Joint joint in joints.Joints) { //set default angles of joints
                SceneManager.Instance.GetRobot(joints.RobotId).SetJointValue(joint.Name, (float) 0f);
            }
        }

    }

    public async void ShowMenu(ActionPoint currentActionPoint, NamedOrientation orientation) {
        CurrentActionPoint = currentActionPoint;
        if (!await currentActionPoint.GetOrientationVisual(orientation.Id).WriteLock(false)) {
            CurrentActionPoint = null;
            return;
        }

        this.orientation = orientation;
        isOrientationDetail = true;

        ShowMenu(currentActionPoint);
    }

    public async void ShowMenu(ActionPoint currentActionPoint, ProjectRobotJoints joints) {
        CurrentActionPoint = currentActionPoint;
        this.joints = joints;
        isOrientationDetail = false;
        try {
            RobotName.text = SceneManager.Instance.GetRobot(joints.RobotId).GetName();
            ArmName.text = joints.ArmId;
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification(ex.Message, "");
        }

        await PrepareRobotModel(joints.RobotId, false);

        MoveHereModel();
        UpdateJointsList();
        ShowMenu(currentActionPoint);
    }

    private void ShowMenu(ActionPoint actionPoint) {
        CurrentActionPoint = actionPoint;

        OrientationExpertModeBlock.SetActive(isOrientationDetail && GameManager.Instance.ExpertMode);
        JointsBlock.SetActive(!isOrientationDetail);
        JointsExpertModeBlock.SetActive(!isOrientationDetail && GameManager.Instance.ExpertMode);

        UpdateMenu();
        gameObject.SetActive(true);
    }

    public async void HideMenu() {
        if (CurrentActionPoint == null)
            return;
        gameObject.SetActive(false);

        if (!isOrientationDetail) {
            DestroyJointsFields();
        }

        foreach (KeyValuePair<string, float> rv in robotVisibilityBackup) {
            await PrepareRobotModel(rv.Key, true);
        }
        robotVisibilityBackup.Clear();

        CurrentActionPoint = null;
    }

    public bool IsVisible() {
        return gameObject.activeInHierarchy;
    }
}
