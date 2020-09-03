using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(SimpleSideMenu))]
public class ActionPointAimingMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject JointsBlock, PositionBlock, PositionExpertModeBlock, PositionLiteModeBlock, PositionRobotPickBlock;

    [SerializeField]
    private TMPro.TMP_Text ActionPointName, OrientationsListLabel, JointsListLabel;

    [SerializeField]
    private ActionButton OrientationManualDefaultButton;

    [SerializeField]
    private Button AddOrientationUsingRobotButton;

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

    [SerializeField]
    private PositionManualEdit PositionManualEdit;

    private SimpleSideMenu SideMenu;

    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
        WebsocketManager.Instance.OnActionPointUpdated += OnActionPointUpdated;
        WebsocketManager.Instance.OnActionPointBaseUpdated += OnActionPointBaseUpdated;

        // TODO: subscribe only when menu is opened
        WebsocketManager.Instance.OnActionPointOrientationAdded += OnActionPointOrientationAdded;
        WebsocketManager.Instance.OnActionPointOrientationBaseUpdated += OnActionPointOrientationBaseUpdated;
        WebsocketManager.Instance.OnActionPointOrientationRemoved += OnActionPointOrientationRemoved;

        WebsocketManager.Instance.OnActionPointJointsAdded += OnActionPointJointsAdded;
        WebsocketManager.Instance.OnActionPointJointsBaseUpdated += OnActionPointJointsBaseUpdated;
        WebsocketManager.Instance.OnActionPointJointsRemoved += OnActionPointJointsRemoved;
    }

    private void OnActionPointBaseUpdated(object sender, BareActionPointEventArgs args) {
        if (CurrentActionPoint == null || args.ActionPoint.Id != CurrentActionPoint.GetId())
            return;
        PositionManualEdit.SetPosition(args.ActionPoint.Position);
    }

    private void OnActionPointJointsRemoved(object sender, StringEventArgs args) {
        try {
            ActionButton btn = GetButton(args.Data, JointsDynamicList);
            btn.gameObject.SetActive(false);
            Destroy(btn.gameObject);
        } catch (ItemNotFoundException) {
            // not currently opened action point
            Debug.LogError(args.Data);
        }
    }

    private void OnActionPointJointsBaseUpdated(object sender, RobotJointsEventArgs args) {
        try {
            ActionButton btn = GetButton(args.Data.Id, JointsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointJointsAdded(object sender, RobotJointsEventArgs args) {
        if (args.ActionPointId != CurrentActionPoint.GetId())
            return;
        if (SceneManager.Instance.GetRobot(args.Data.RobotId).GetName() == (string) JointsRobotsList.GetValue()) {
            CreateBtn(JointsDynamicList.transform, args.Data.Id, args.Data.Name, () => OpenDetailMenu(args.Data)).Highlight(2f);
        }
    }

    private void OnActionPointOrientationRemoved(object sender, StringEventArgs args) {
        try {
            ActionButton btn = GetButton(args.Data, OrientationsDynamicList);
            btn.gameObject.SetActive(false);
            Destroy(btn.gameObject);
            UpdateOrientationsListLabel();
        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointOrientationBaseUpdated(object sender, ActionPointOrientationEventArgs args) {
        try {
            CurrentActionPoint.GetOrientation(args.Data.Id);
            ActionButton btn = GetButton(args.Data.Id, OrientationsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (KeyNotFoundException) {
            // not currently opened action point
        }        
    }

    private void OnActionPointOrientationAdded(object sender, ActionPointOrientationEventArgs args) {
        if (CurrentActionPoint.Data.Id == args.ActionPointId) {
            CreateBtn(OrientationsDynamicList.transform, args.Data.Id, args.Data.Name, () => OpenDetailMenu(args.Data)).Highlight(2f);
            UpdateOrientationsListLabel();
        }
    }

    private void OnActionPointUpdated(object sender, ProjectActionPointEventArgs args) {
        if (CurrentActionPoint == null || args.ActionPoint.Id != CurrentActionPoint.GetId())
            return;
        ActionPointName.text = args.ActionPoint.Name;
    }
    
    public void UpdateMenu() {
        ActionPointName.text = CurrentActionPoint.Data.Name;

        CustomDropdown positionRobotsListDropdown = PositionRobotsList.Dropdown;
        positionRobotsListDropdown.dropdownItems.Clear();
        PositionRobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (positionRobotsListDropdown.dropdownItems.Count == 0) {
            PositionBlock.SetActive(GameManager.Instance.ExpertMode);
            PositionExpertModeBlock.SetActive(GameManager.Instance.ExpertMode);
            PositionLiteModeBlock.SetActive(false);
            AddOrientationUsingRobotButton.interactable = false;
        } else {
            PositionBlock.SetActive(true);
            PositionExpertModeBlock.SetActive(GameManager.Instance.ExpertMode);
            PositionLiteModeBlock.SetActive(true);
            OnRobotChanged((string) PositionRobotsList.GetValue());
            AddOrientationUsingRobotButton.interactable = true;
        }

        PositionManualEdit.SetPosition(CurrentActionPoint.Data.Position);


        JointsRobotsList.Dropdown.dropdownItems.Clear();
        JointsRobotsList.gameObject.GetComponent<DropdownRobots>().Init(UpdateJointsDynamicList, false);
        if (JointsRobotsList.Dropdown.dropdownItems.Count > 0) {
            JointsBlock.SetActive(true);
            UpdateJointsDynamicList((string) JointsRobotsList.GetValue());
        } else {
            JointsBlock.SetActive(false);
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

    /// <summary>
    /// Sets new action point position - using robot if no position is passed
    /// </summary>
    /// <param name="position">New position to set</param>
    private async void UpdateActionPointPosition(Position position = null) {
        try {
            if (position != null) {
                await WebsocketManager.Instance.UpdateActionPointPosition(CurrentActionPoint.GetId(), position);
            } else {
                string robotId = SceneManager.Instance.RobotNameToId(PositionRobotsList.Dropdown.selectedText.text);
                string endEffectorId = PositionEndEffectorList.Dropdown.selectedText.text;

                await WebsocketManager.Instance.UpdateActionPointUsingRobot(CurrentActionPoint.GetId(), robotId, endEffectorId);
                ConfirmationDialog.Close();
            }
            
        } catch (RequestFailedException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Update position failed", ex.Message);
        }
    }

    public void OnPositionManualUpdateClick() {
        UpdateActionPointPosition(PositionManualEdit.GetPosition());
    }

    public void ShowMenu(Base.ActionPoint actionPoint) {
        CurrentActionPoint = actionPoint;
        OrientationManualDefaultButton.SetLabel(GameManager.Instance.ExpertMode ? "Manual" : "Default");
        UpdateMenu();
        SideMenu.Open();
    }

    public void ShowMenu(Base.ActionPoint actionPoint, string preselectedOrientation) {
        ShowMenu(actionPoint);

        try {
            OpenDetailMenu(actionPoint.GetOrientation(preselectedOrientation));
        } catch (KeyNotFoundException ex) {
            Notifications.Instance.ShowNotification("Unable to open detail menu", ex.Message);
        }
    }

    public void Close() {
        SideMenu.Close();
    }

    public void UpdateOrientationsDynamicList() {
        foreach (RectTransform o in OrientationsDynamicList.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        foreach (IO.Swagger.Model.NamedOrientation orientation in CurrentActionPoint.GetNamedOrientations()) {
            CreateBtn(OrientationsDynamicList.transform, orientation.Id, orientation.Name, () => OpenDetailMenu(orientation));
        }
        UpdateOrientationsListLabel();
    }

    private ActionButton GetButton(string id, GameObject parent) {
        foreach (ActionButton ab in parent.GetComponentsInChildren<ActionButton>()) {
            if (ab.ObjectId == id) {
                return ab;
            }
        }
        throw new ItemNotFoundException("Button not found");
    }

    /// <summary>
    /// Returns true if parent transform contains any child of type ActionButton
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    private bool ContainActiveButton(Transform parent) {
        foreach (ActionButton ab in OrientationsDynamicList.GetComponentsInChildren<ActionButton>()) {
            if (ab.gameObject.activeSelf) {
                return true;
            }
        }
        return false;
    }

    private ActionButton CreateBtn(Transform parent, string objectId, string label, UnityAction callback) {
        ActionButton btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, parent).GetComponent<ActionButton>();
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.SetLabel(label);
        btn.ObjectId = objectId;
        btn.Button.onClick.AddListener(callback);
        return btn;
    }

    private void UpdateOrientationsListLabel() {
        if (ContainActiveButton(OrientationsDynamicList.transform)) {
            OrientationsListLabel.text = "List of orientations:";
        } else {
            OrientationsListLabel.text = "There is no orientation yet.";
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

            System.Collections.Generic.List<ProjectRobotJoints> joints = CurrentActionPoint.GetAllJoints(true, robotId).Values.ToList();
            foreach (IO.Swagger.Model.ProjectRobotJoints joint in joints) {
                CreateBtn(JointsDynamicList.transform, joint.Id, joint.Name, () => OpenDetailMenu(joint));
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
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }
}
