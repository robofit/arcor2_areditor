using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using IO.Swagger.Model;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ActionPointAimingMenu : Base.Singleton<ActionPointAimingMenu> {
    public Base.ActionPoint CurrentActionPoint;

    public GameObject PositionExpertModeBlock, PositionRobotPickBlock, OrientationsDynamicList, JointsDynamicList, ContainerPosition, ContainerOrientations, ContainerJoints,
        AddOrientationContainer, AddJointsContainer, OrientationJointsDetailContainer;

    [SerializeField]
    private TMPro.TMP_Text OrientationsListLabel, JointsListLabel;

    [SerializeField]
    private ActionButton OrientationManualDefaultButton;

    [SerializeField]
    private Button AddOrientationUsingRobotButton, AddJointsButton, UpdatePositionUsingRobotBtn;

    [SerializeField]
    private TooltipContent UpdatePositionUsingRobotTooltip, AddOrientationUsingRobotTooltip, AddJointsTooltip;

    public DropdownParameter PositionRobotsList, JointsRobotsList, PositionEndEffectorList;

    public DropdownArms PositionDropdownArms, JointsDropdownArms;

    [SerializeField]
    private ConfirmationDialog confirmationDialog;
    public ConfirmationDialog ConfirmationDialog => confirmationDialog;


    [SerializeField]
    private AddOrientationMenu AddOrientationMenu;

    [SerializeField]
    private AddJointsMenu AddJointsMenu;

    public OrientationJointsDetailMenu OrientationJointsDetailMenu;

    [SerializeField]
    private PositionManualEdit PositionManualEdit;

    public CanvasGroup CanvasGroup;

    private enum StateEnum {
        Position,
        Orientations,
        Joints
    }

    private StateEnum State;

    private void Start() {
        WebsocketManager.Instance.OnActionPointBaseUpdated += OnActionPointBaseUpdated;

        // TODO: subscribe only when menu is opened
        ProjectManager.Instance.OnActionPointOrientationAdded += OnActionPointOrientationAdded;
        ProjectManager.Instance.OnActionPointOrientationBaseUpdated += OnActionPointOrientationBaseUpdated;
        ProjectManager.Instance.OnActionPointOrientationRemoved += OnActionPointOrientationRemoved;

        WebsocketManager.Instance.OnActionPointJointsAdded += OnActionPointJointsAdded;
        WebsocketManager.Instance.OnActionPointJointsBaseUpdated += OnActionPointJointsBaseUpdated;
        WebsocketManager.Instance.OnActionPointJointsUpdated += OnActionPointJointsUpdated;
        WebsocketManager.Instance.OnActionPointJointsRemoved += OnActionPointJointsRemoved;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        UpdateMenu();
    }

    public async Task<bool> Show(Base.ActionPoint actionPoint) {

        CurrentActionPoint = actionPoint;
        if (!await actionPoint.WriteLock(false))
            return false;
        OrientationManualDefaultButton.SetLabel(GameManager.Instance.ExpertMode ? "Manual" : "Default");
        UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        return true;
    }
    public async Task<bool> Show(Base.ActionPoint actionPoint, string preselectedOrientation) {
        if (!await Show(actionPoint))
            return false;

        try {

            OpenDetailMenu(actionPoint.GetOrientation(preselectedOrientation));
            return true;
        } catch (KeyNotFoundException ex) {
            Notifications.Instance.ShowNotification("Unable to open detail menu", ex.Message);
            return false;
        }
    }

    public async Task Hide(bool unlockAP = false) {
        if (unlockAP)
            await CurrentActionPoint.WriteUnlock();
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

    public void SwitchToPosition() {
        AddOrientationContainer.SetActive(false);
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(true);
        State = StateEnum.Position;
    }

    public void SwitchToOrientations() {
        AddOrientationContainer.SetActive(false);
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerPosition.SetActive(false);
        ContainerOrientations.SetActive(true);
        State = StateEnum.Orientations;
    }

    public void SwitchToJoints() {
        AddOrientationContainer.SetActive(false);
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(false);
        ContainerJoints.SetActive(true);
        State = StateEnum.Joints;
    }

    private void OnActionPointJointsUpdated(object sender, RobotJointsEventArgs args) {
        if (!IsVisible() && ContainerJoints.activeInHierarchy)
            return;
        try {
            ActionButton btn = GetButton(args.Data.Id, JointsDynamicList);
            Debug.LogError(btn.GetLabel());
            btn.GetComponent<TooltipContent>().enabled = !args.Data.IsValid;
            btn.transform.parent.GetComponent<ServiceButton>().State = args.Data.IsValid;

        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointBaseUpdated(object sender, BareActionPointEventArgs args) {
        if (!IsVisible())
            return;
        if (CurrentActionPoint == null || args.ActionPoint.Id != CurrentActionPoint.GetId())
            return;
        PositionManualEdit.SetPosition(args.ActionPoint.Position);
        UpdateJointsDynamicList((string) JointsRobotsList.GetValue());  //because of possible invalidation of joints
    }

    private void OnActionPointJointsRemoved(object sender, StringEventArgs args) {
        if (!IsVisible())
            return;
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
        if (!IsVisible())
            return;
        try {
            ActionButton btn = GetButton(args.Data.Id, JointsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointJointsAdded(object sender, RobotJointsEventArgs args) {
        if (!IsVisible() || args.ActionPointId != CurrentActionPoint.GetId())
            return;
        if (SceneManager.Instance.GetRobot(args.Data.RobotId).GetName() == (string) JointsRobotsList.GetValue()) {
            ServiceButton btn = CreateJointsButton(JointsDynamicList.transform, args.Data.Id, args.Data.Name, () => OpenDetailMenu(args.Data), args.Data.IsValid);
            btn.GetComponentInChildren<ActionButton>().Highlight(2f);
        }
    }

    private void OnActionPointOrientationRemoved(object sender, StringEventArgs args) {
        if (!IsVisible())
            return;
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

        if (!IsVisible())
            return;
        try {
            CurrentActionPoint.GetOrientation(args.Data.Id);
            ActionButton btn = GetButton(args.Data.Id, OrientationsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (KeyNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointOrientationAdded(object sender, ActionPointOrientationEventArgs args) {

        if (IsVisible() && CurrentActionPoint.Data.Id == args.ActionPointId) {
            CreateOrientationBtn(args.Data);
            UpdateOrientationsListLabel();
        }
    }

    public async void UpdateMenu() {
        if (CurrentActionPoint == null)
            return;

        CustomDropdown positionRobotsListDropdown = PositionRobotsList.Dropdown;
        positionRobotsListDropdown.dropdownItems.Clear();
        await PositionRobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
        if (!SceneManager.Instance.SceneStarted || positionRobotsListDropdown.dropdownItems.Count == 0) {
            PositionRobotsList.gameObject.SetActive(false);
            PositionDropdownArms.gameObject.SetActive(false);
            PositionEndEffectorList.gameObject.SetActive(false);
            UpdatePositionUsingRobotBtn.interactable = false;
            AddOrientationUsingRobotButton.interactable = false;
        } else {
            PositionRobotsList.gameObject.SetActive(true);
            PositionDropdownArms.gameObject.SetActive(false);
            PositionEndEffectorList.gameObject.SetActive(true);
            UpdatePositionUsingRobotBtn.interactable = true;
            AddOrientationUsingRobotButton.interactable = true;
            OnRobotChanged((string) PositionRobotsList.GetValue());
        }

        PositionExpertModeBlock.SetActive(GameManager.Instance.ExpertMode);
        PositionManualEdit.SetPosition(CurrentActionPoint.Data.Position);


        JointsRobotsList.Dropdown.dropdownItems.Clear();
        await JointsRobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotJointsChanged, false);
        if (JointsRobotsList.Dropdown.dropdownItems.Count > 0) {
            JointsRobotsList.gameObject.SetActive(true);
            JointsDynamicList.SetActive(true);
            if (SceneManager.Instance.SceneStarted) {
                OnRobotJointsChanged((string) JointsRobotsList.GetValue());
                JointsDropdownArms.gameObject.SetActive(true);
            } else {
                JointsDropdownArms.gameObject.SetActive(false);
                UpdateJointsDynamicList((string) JointsRobotsList.GetValue(), null);
            }            
            
        } else {
            JointsRobotsList.gameObject.SetActive(false);
            JointsDropdownArms.gameObject.SetActive(false);
            JointsDynamicList.SetActive(false);
        }
        if (SceneManager.Instance.SceneStarted) {
            AddJointsButton.interactable = true;
        } else {
            AddJointsButton.interactable = false;
        }

        UpdateOrientationsDynamicList();
        this.gameObject.SetActive(true); //so the couroutine can be started for sure
        StartCoroutine(UpdateTooltips());
    }


    private IEnumerator UpdateTooltips() {
        yield return new WaitForSeconds(0.1f); //fixes a bug, when after the first collapsing of collapsable menu there is no tooltip

        const string noRobot = "There is no robot in the scene";
        const string sceneNotStarted = "To add using robot, go online";

        if (!SceneManager.Instance.RobotInScene()) {
            UpdatePositionUsingRobotTooltip.description = noRobot;
            AddOrientationUsingRobotTooltip.description = noRobot;
            AddJointsTooltip.description = noRobot;
            UpdatePositionUsingRobotTooltip.enabled = true;
            AddOrientationUsingRobotTooltip.enabled = true;
            AddJointsTooltip.enabled = true;
            JointsListLabel.text = "To show joints list, add a robot to the scene";
        } else if (!SceneManager.Instance.SceneStarted) {
            UpdatePositionUsingRobotTooltip.description = "To update using robot, go online";
            AddOrientationUsingRobotTooltip.description = sceneNotStarted;
            AddJointsTooltip.description = sceneNotStarted;
            UpdatePositionUsingRobotTooltip.enabled = true;
            AddOrientationUsingRobotTooltip.enabled = true;
            AddJointsTooltip.enabled = true;
            JointsListLabel.text = "List of joints:";
        } else {
            UpdatePositionUsingRobotTooltip.enabled = false;
            AddOrientationUsingRobotTooltip.enabled = false;
            AddJointsTooltip.enabled = false;
            JointsListLabel.text = "List of joints:";
        }
    }

    private async void OnRobotChanged(string robot_name) {

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            //await PositionEndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, OnRobotArmChanged);
            await PositionDropdownArms.Init(robotId, OnRobotArmChanged);

            OnRobotArmChanged(PositionDropdownArms.Dropdown.GetValue().ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

    }

    private async void OnRobotArmChanged(string arm_id) {
        string robotId = null;
        try {
            robotId = SceneManager.Instance.RobotNameToId(PositionRobotsList.GetValue().ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            robotId = null;

        }
        if (string.IsNullOrEmpty(robotId)) {
            Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + PositionRobotsList.GetValue().ToString() + "does not exists");
            return;
        }
        await PositionEndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, arm_id, null);
    }

    public void ShowUpdatePositionConfirmationDialog() {
        confirmationDialog.Open("Update position",
                                "Do you want to update position of action point " + CurrentActionPoint.Data.Name,
                                () => UpdateActionPointPosition(),
                                () => confirmationDialog.Close());
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
                string endEffectorId = PositionEndEffectorList.GetValue().ToString();
                IRobot robot = SceneManager.Instance.GetRobot(robotId);
                string armId = null;
                if (robot.MultiArm())
                    armId = PositionDropdownArms.Dropdown.GetValue().ToString();

                await WebsocketManager.Instance.UpdateActionPointUsingRobot(CurrentActionPoint.GetId(), robotId, endEffectorId, armId);
                confirmationDialog.Close();
            }
            Notifications.Instance.ShowToastMessage("Position updated successfully");
        } catch (RequestFailedException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Update position failed", ex.Message);
        }
    }

    public void OnPositionManualUpdateClick() {
        UpdateActionPointPosition(PositionManualEdit.GetPosition());
    }





    public void UpdateOrientationsDynamicList() {
        foreach (RectTransform o in OrientationsDynamicList.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        foreach (IO.Swagger.Model.NamedOrientation orientation in CurrentActionPoint.GetNamedOrientations()) {
            CreateOrientationBtn(orientation);
        }

        UpdateOrientationsListLabel();
    }

    /// <summary>
    /// Creates button in orientations dynamic list with orientation's arrow highlight on hover
    /// </summary>
    /// <param name="orientation"></param>
    private void CreateOrientationBtn(NamedOrientation orientation) {
        ActionButton orientationButton = CreateBtn(OrientationsDynamicList.transform, orientation.Id, orientation.Name, () => OpenDetailMenu(orientation));

        // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding orientation when hovering over button
        OutlineOnClick orientationOutline = CurrentActionPoint.GetOrientationVisual(orientation.Id).GetComponent<OutlineOnClick>();
        EventTrigger eventTrigger = orientationButton.gameObject.AddComponent<EventTrigger>();
        // Create OnPointerEnter entry
        EventTrigger.Entry onPointerEnter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        onPointerEnter.callback.AddListener((eventData) => orientationOutline.Highlight());
        eventTrigger.triggers.Add(onPointerEnter);

        // Create OnPointerExit entry
        EventTrigger.Entry onPointerExit = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };
        onPointerExit.callback.AddListener((eventData) => orientationOutline.UnHighlight());
        eventTrigger.triggers.Add(onPointerExit);
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
        foreach (ActionButton ab in parent.GetComponentsInChildren<ActionButton>()) {
            if (ab.gameObject.activeSelf) {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Creates button for joints 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="jointsID"></param>
    /// <param name="label"></param>
    /// <param name="callback"></param>
    /// <param name="isValid">State of joints</param>
    /// <returns></returns>
    private ServiceButton CreateJointsButton(Transform parent, string jointsID, string label, UnityAction callback, bool isValid) {
        ServiceButton serviceBtn = Instantiate(Base.GameManager.Instance.ServiceButtonPrefab, parent).GetComponent<ServiceButton>();
        var btn = serviceBtn.GetComponentInChildren<ActionButton>();
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.SetLabel(label);
        btn.ObjectId = jointsID;
        btn.Button.onClick.AddListener(callback);
        serviceBtn.State = isValid;
        btn.GetComponent<TooltipContent>().description = "Invalid";
        if (isValid) {
            btn.GetComponent<TooltipContent>().enabled = false;
        } else {
            btn.GetComponent<TooltipContent>().enabled = true;
        }
        return serviceBtn;
    }

    private ActionButton CreateBtn(Transform parent, string objectId, string label, UnityAction callback) {
        ActionButton btn;
        btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, parent).GetComponent<ActionButton>();
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

    private async void OnRobotJointsChanged(string robot_name) {

        try {
            string robotId = SceneManager.Instance.RobotNameToId(robot_name);
            //await PositionEndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robotId, OnRobotArmChanged);
            await JointsDropdownArms.Init(robotId, UpdateJointsDynamicList);
            object arm_id = JointsDropdownArms.Dropdown.GetValue();
            if (arm_id == null)
                UpdateJointsDynamicList(robotId, null);
            else
                UpdateJointsDynamicList(arm_id.ToString());
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
        }

    }

    public async void UpdateJointsDynamicList(string arm_id) {
        IRobot robot = SceneManager.Instance.GetRobotByName(JointsRobotsList.GetValue().ToString());
        UpdateJointsDynamicList(robot.GetId(), robot.MultiArm() ? arm_id : "");
    }


    public async void UpdateJointsDynamicList(string robot_id, string arm_id) {
        

        try {
            //string robotId = SceneManager.Instance.RobotNameToId(JointsRobotsList.GetValue().ToString());

            foreach (RectTransform o in JointsDynamicList.GetComponentsInChildren<RectTransform>()) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }

            System.Collections.Generic.List<ProjectRobotJoints> joints;
            if (string.IsNullOrEmpty(arm_id))
                joints = CurrentActionPoint.GetAllJoints(true, robot_id).Values.ToList();
            else
                joints = CurrentActionPoint.GetJointsOfArm(robot_id, arm_id, true).Values.ToList();
            foreach (IO.Swagger.Model.ProjectRobotJoints joint in joints) {
                CreateJointsButton(JointsDynamicList.transform, joint.Id, joint.Name, () => OpenDetailMenu(joint), joint.IsValid);
            }
            
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to get robot's ID", "");
            return;
        }
    }


    public async void OpenDetailMenu(ProjectRobotJoints joint) {
        AddOrientationContainer.SetActive(false);
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(false);
        OrientationJointsDetailMenu.ShowMenu(CurrentActionPoint, joint);
    }

    public async void OpenDetailMenu(NamedOrientation orientation) {
        AddOrientationContainer.SetActive(false);
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(false);
        OrientationJointsDetailMenu.ShowMenu(CurrentActionPoint, orientation);
        APOrientation orientationArrow = CurrentActionPoint.GetOrientationVisual(orientation.Id);
        SceneManager.Instance.SetSelectedObject(orientationArrow.gameObject);
        orientationArrow.SendMessage("Select", false);
    }

    /// <summary>
    /// If expert mode is active - opens add orientation side menu in manual mode, otherwise adds default orientation (0,0,0,1)
    /// </summary>
    public void OpenAddOrientationMenuManualDefault() {
        if (GameManager.Instance.ExpertMode) {
            AddJointsContainer.SetActive(false);
            if (OrientationJointsDetailMenu.IsVisible())
                OrientationJointsDetailMenu.HideMenu();
            ContainerJoints.SetActive(false);
            ContainerOrientations.SetActive(false);
            ContainerPosition.SetActive(false);
            AddOrientationMenu.ShowMenu(CurrentActionPoint, true);
        } else {
            AddDefaultOrientation();
        }
    }

    public void OpenAddOrientationMenuUsingRobot() {
        AddJointsContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(false);
        AddOrientationMenu.ShowMenu(CurrentActionPoint, false);
    }

    public void OpenAddJointsMenu(bool manual) {
        AddOrientationContainer.SetActive(false);
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        ContainerJoints.SetActive(false);
        ContainerOrientations.SetActive(false);
        ContainerPosition.SetActive(false);
        AddJointsMenu.ShowMenu(CurrentActionPoint);
    }

    public void CloseOrientationJointsDetailMenu() {
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        switch (State) {
            case StateEnum.Joints:
                SwitchToJoints();
                break;
            case StateEnum.Orientations:
                SwitchToOrientations();
                break;
            case StateEnum.Position:
                SwitchToPosition();
                break;
        }
    }


    public async void AddDefaultOrientation() {
        try {
            name = CurrentActionPoint.GetFreeOrientationName();
            await WebsocketManager.Instance.AddActionPointOrientation(CurrentActionPoint.Data.Id, new Orientation(), name);
            Notifications.Instance.ShowToastMessage("Orientation added successfully");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }


}
