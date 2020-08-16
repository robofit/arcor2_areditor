using UnityEngine;
using DanielLochner.Assets.SimpleSideMenu;
using Base;
using IO.Swagger.Model;
using System.Globalization;

[RequireComponent(typeof(SimpleSideMenu))]
public class OrientationJointsDetailMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject OrientationBlock, OrientationExpertModeBlock, JointsBlock, JointsExpertModeBlock, MoveHereBlock;

    public TMPro.TMP_InputField QuaternionX, QuaternionY, QuaternionZ, QuaternionW;

    [SerializeField]
    private TooltipContent buttonTooltip;

    [SerializeField]
    private Button UpdateButton;

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
    }


    public void UpdateMenu() {
        if (isOrientationDetail) {  //orientation
            RobotsList.Dropdown.dropdownItems.Clear();
            RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
            if (RobotsList.Dropdown.dropdownItems.Count > 0) {
                OrientationBlock.SetActive(true);
                MoveHereBlock.SetActive(true);

                UpdateButton.interactable = true;
                buttonTooltip.enabled = false;

                OnRobotChanged((string) RobotsList.GetValue());
            } else {
                OrientationBlock.SetActive(false);
                MoveHereBlock.SetActive(false);

                buttonTooltip.description = "There is no robot to update orientation with";
                buttonTooltip.enabled = true;
                UpdateButton.interactable = false;
            }


            NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
            QuaternionX.text = orientation.Orientation.X.ToString(numberFormatInfo);
            QuaternionY.text = orientation.Orientation.Y.ToString(numberFormatInfo);
            QuaternionZ.text = orientation.Orientation.Z.ToString(numberFormatInfo);
            QuaternionW.text = orientation.Orientation.W.ToString(numberFormatInfo);
        } else { //joints
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

    /// <summary>
    /// Updates values (angles) of joints in expert block
    /// </summary>
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

    public async void OnOrientationSaveClick() {
        try {
            decimal x = decimal.Parse(QuaternionX.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal y = decimal.Parse(QuaternionY.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal z = decimal.Parse(QuaternionZ.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            decimal w = decimal.Parse(QuaternionW.text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            await WebsocketManager.Instance.UpdateActionPointOrientation(new Orientation(w, x, y, z), orientation.Id);
            Notifications.Instance.ShowNotification("Orientation updated", "");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to update orientation", ex.Message);
        }
    }


    public async void UpdateUsingRobot() {
        if (isOrientationDetail)
        {
            try {
                string robotId = SceneManager.Instance.RobotNameToId((string) RobotsList.GetValue());
                await WebsocketManager.Instance.UpdateActionPointOrientationUsingRobot(CurrentActionPoint.Data.Id, robotId, (string) EndEffectorList.GetValue(), orientation.Id);
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
                await WebsocketManager.Instance.UpdateActionPointJoints(joints.RobotId, joints.Id);
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
            //TODO uncomment line below
            //MenuManager.Instance.HideMenu(MenuManager.Instance.OrientationJointsDetailMenu);

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
            description += "joints using robot: " + joints.RobotId;
        }
        ConfirmationDialog.Open(title,
                                description,
                                () => UpdateUsingRobot(),
                                () => ConfirmationDialog.Close());
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
        try {
            RobotName.text = SceneManager.Instance.GetRobot(joints.RobotId).GetName();
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification(ex.Message, "");
        }
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
