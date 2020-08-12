using System.Linq;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

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

    public DropdownParameter PositionRobotsList, JointsRobotsList, PositionEndEffectorList;

    public GameObject OrientationsDynamicList, JointsDynamicList;

    [SerializeField]
    private ConfirmationDialog ConfirmationDialog;

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

    public void ShowUpdatePositionConfirmationDialog() {
        ConfirmationDialog.Open("Update position",
                                "Do you want to update position of action point " + CurrentActionPoint.Data.Name,
                                () => UpdateActionPointPosition(),
                                () => ConfirmationDialog.Close());
    }

    private async void UpdateActionPointPosition() {
        try {
            string robotId = SceneManager.Instance.RobotNameToId(PositionRobotsList.Dropdown.selectedText.text);
            string endEffectorId = PositionEndEffectorList.Dropdown.selectedText.text;

            await WebsocketManager.Instance.UpdateActionPointUsingRobot(CurrentActionPoint.Data.Id, robotId, endEffectorId);
            ConfirmationDialog.Close();
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Update position failed", "");
        }
    }

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
        try {
            name = CurrentActionPoint.GetFreeOrientationName();
            await WebsocketManager.Instance.AddActionPointOrientation(CurrentActionPoint.Data.Id, new Orientation(), name);
            UpdateMenu();
            //todo open detail of the new orientation?
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }
}
