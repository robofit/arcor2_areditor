using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ActionPoint = Base.ActionPoint;

public class ActionPointAimingMenu : RightMenu<ActionPointAimingMenu> {
    public ActionPoint CurrentActionPoint;

    public GameObject PositionExpertModeBlock, PositionRobotPickBlock, OrientationsDynamicList, JointsDynamicList, ContainerPosition, ContainerOrientations, ContainerJoints,
        AddOrientationContainer, AddJointsContainer, OrientationJointsDetailContainer, RobotPickBlock;

    [SerializeField]
    private TMP_Text OrientationsListLabel, JointsListLabel;

    [SerializeField]
    private ActionButton OrientationManualDefaultButton;

    [SerializeField]
    private ButtonWithTooltip AddOrientationUsingRobotButton, AddJointsButton, UpdatePositionUsingRobotBtn, UpdatePositionUsingRobotBtn2;

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


    private enum StateEnum {
        Position,
        Orientations,
        Joints
    }

    private StateEnum State;

    private void Start() {
        CommunicationManager.Instance.Client.ActionPointBaseUpdated += CommunicationManager.SafeEventHandler<BareActionPointEventArgs>(OnActionPointBaseUpdated);

        // TODO: subscribe only when menu is opened
        ProjectManager.Instance.OnActionPointOrientationAdded += OnActionPointOrientationAdded;
        ProjectManager.Instance.OnActionPointOrientationBaseUpdated += OnActionPointOrientationBaseUpdated;
        ProjectManager.Instance.OnActionPointOrientationRemoved += OnActionPointOrientationRemoved;

        CommunicationManager.Instance.Client.JointsAdded += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsAdded);
        CommunicationManager.Instance.Client.JointsBaseUpdated += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsBaseUpdated);
        CommunicationManager.Instance.Client.JointsUpdated += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsUpdated);
        CommunicationManager.Instance.Client.JointsRemoved += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsRemoved);
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        UpdateMenu();
    }

    public override async Task<bool> Show(InteractiveObject obj, bool lockTree) {

        if (!await base.Show(obj, lockTree))
            return false;

        if (obj is ActionPoint3D actionPoint) {
            CurrentActionPoint = actionPoint;
            OrientationManualDefaultButton.SetLabel(GameManager.Instance.ExpertMode ? "Manual" : "Default");
            UpdateMenu();
            EditorHelper.EnableCanvasGroup(CanvasGroup, true);
            RobotInfoMenu.Instance.Show();
            return true;
        } else {
            return false;
        }

    }
    public async Task<bool> Show(ActionPoint actionPoint, string preselectedOrientation) {
        if (!await Show(actionPoint, true))
            return false;

        try {
            OpenDetailMenu(actionPoint.GetOrientation(preselectedOrientation));
            return true;
        } catch (KeyNotFoundException ex) {
            Notifications.Instance.ShowNotification("Unable to open detail menu", ex.Message);
            return false;
        }
    }

    public async override Task Hide() {
        await base.Hide();
        if (!IsVisible)
            return;
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        RobotInfoMenu.Instance.Hide();
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

    private void OnActionPointJointsUpdated(object sender, JointsEventArgs args) {
        if (!IsVisible && ContainerJoints.activeInHierarchy)
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
        if (!IsVisible)
            return;
        if (CurrentActionPoint == null || args.Data.Id != CurrentActionPoint.GetId())
            return;
        PositionManualEdit.SetPosition(args.Data.Position);
        if (SceneManager.Instance.SceneStarted)
            UpdateJointsDynamicList(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedArmId);  //because of possible invalidation of joints
    }

    private void OnActionPointJointsRemoved(object sender, JointsEventArgs args) {
        if (!IsVisible)
            return;
        try {
            ActionButton btn = GetButton(args.Data.Id, JointsDynamicList);
            btn.gameObject.SetActive(false);
            Destroy(btn.gameObject);
        } catch (ItemNotFoundException) {
            // not currently opened action point
            Debug.LogError(args.Data);
        }
    }

    private void OnActionPointJointsBaseUpdated(object sender, JointsEventArgs args) {
        if (!IsVisible)
            return;
        try {
            ActionButton btn = GetButton(args.Data.Id, JointsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointJointsAdded(object sender, JointsEventArgs args) {
        if (!IsVisible || args.ParentId != CurrentActionPoint.GetId())
            return;
        if (args.Data.RobotId == SceneManager.Instance.SelectedRobot.GetId()) {
            ServiceButton btn = CreateJointsButton(JointsDynamicList.transform, args.Data.Id, args.Data.Name, () => OpenDetailMenu(args.Data), args.Data.IsValid);
            btn.GetComponentInChildren<ActionButton>().Highlight(2f);
        }
    }

    private void OnActionPointOrientationRemoved(object sender, OrientationEventArgs args) {
        if (!IsVisible)
            return;
        try {
            ActionButton btn = GetButton(args.Data.Id, OrientationsDynamicList);
            btn.gameObject.SetActive(false);
            Destroy(btn.gameObject);
            UpdateOrientationsListLabel();
        } catch (ItemNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointOrientationBaseUpdated(object sender, OrientationEventArgs args) {

        if (!IsVisible)
            return;
        try {
            CurrentActionPoint.GetOrientation(args.Data.Id);
            ActionButton btn = GetButton(args.Data.Id, OrientationsDynamicList);
            btn.SetLabel(args.Data.Name);
        } catch (KeyNotFoundException) {
            // not currently opened action point
        }
    }

    private void OnActionPointOrientationAdded(object sender, OrientationEventArgs args) {

        if (IsVisible && CurrentActionPoint.Data.Id == args.ParentId) {
            CreateOrientationBtn(args.Data);
            UpdateOrientationsListLabel();
        }
    }

    public async void UpdateMenu() {
        if (CurrentActionPoint == null)
            return;
        CloseAnySubmenu();

        PositionExpertModeBlock.SetActive(GameManager.Instance.ExpertMode);
        RobotPickBlock.SetActive(!GameManager.Instance.ExpertMode);
        PositionManualEdit.SetPosition(CurrentActionPoint.Data.Position);

        if (SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotSelected()) {
            JointsDynamicList.SetActive(true);
            if (SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotAndEESelected()) {
                UpdateJointsDynamicList(SceneManager.Instance.SelectedArmId);
            } else {
                UpdateJointsDynamicList(SceneManager.Instance.SelectedRobot.GetId(), null);
            }
        } else {
            JointsDynamicList.SetActive(false);
        }


        UpdateOrientationsDynamicList();

        const string noRobot = "There is no robot in the scene";
        const string sceneNotStarted = "To add using robot, go online";
        const string eeNotSelected = "End effector is not selected";
        UpdatePositionUsingRobotBtn.SetInteractivity(SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotAndEESelected());
        UpdatePositionUsingRobotBtn2.SetInteractivity(UpdatePositionUsingRobotBtn.IsInteractive());
        AddOrientationUsingRobotButton.SetInteractivity(SceneManager.Instance.SceneStarted && SceneManager.Instance.IsRobotAndEESelected());
        AddJointsButton.SetInteractivity(SceneManager.Instance.SceneStarted, "Scene not started");

        if (!SceneManager.Instance.RobotInScene()) {
            UpdatePositionUsingRobotBtn.SetInteractivity(false, noRobot);
            UpdatePositionUsingRobotBtn2.SetInteractivity(false, noRobot);
            AddOrientationUsingRobotButton.SetInteractivity(false, noRobot);
            AddJointsButton.SetInteractivity(false, noRobot);
            JointsListLabel.text = "To show joints list, add a robot to the scene";
        } else if (!SceneManager.Instance.SceneStarted) {
            UpdatePositionUsingRobotBtn.SetInteractivity(false, "To update using robot, go online");
            UpdatePositionUsingRobotBtn2.SetInteractivity(false, "To update using robot, go online");
            AddOrientationUsingRobotButton.SetInteractivity(false, sceneNotStarted);
            AddJointsButton.SetInteractivity(false, sceneNotStarted);
            JointsListLabel.text = "List of joints:";
        } else if (!SceneManager.Instance.IsRobotAndEESelected()) {
            UpdatePositionUsingRobotBtn.SetInteractivity(false, eeNotSelected);
            UpdatePositionUsingRobotBtn2.SetInteractivity(false, eeNotSelected);
            AddOrientationUsingRobotButton.SetInteractivity(false, eeNotSelected);
            AddJointsButton.SetInteractivity(SceneManager.Instance.IsRobotSelected(), "Robot is not selected");
            if (SceneManager.Instance.IsRobotSelected())
                JointsListLabel.text = "List of joints:";
            else
                JointsListLabel.text = "To show joints list, select robot";
        } else {
            UpdatePositionUsingRobotBtn.SetInteractivity(true);
            UpdatePositionUsingRobotBtn2.SetInteractivity(true);
            AddOrientationUsingRobotButton.SetInteractivity(true);
            AddJointsButton.SetInteractivity(true);
            JointsListLabel.text = "List of joints:";
        }


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
                var response = await CommunicationManager.Instance.Client.UpdateActionPointPositionAsync(new UpdateActionPointPositionRequestArgs(CurrentActionPoint.GetId(), position));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Update position failed", string.Join(',', response.Messages));
                    return;
                }
            } else {
                string armId = null;
                if (SceneManager.Instance.SelectedRobot.MultiArm())
                    armId = SceneManager.Instance.SelectedArmId;
                var response = await CommunicationManager.Instance.Client.UpdateActionPointUsingRobotAsync(new UpdateActionPointUsingRobotRequestArgs(CurrentActionPoint.GetId(), new RobotArg(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.EEId, armId)));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Update position failed", string.Join(',', response.Messages));
                    return;
                }
                await SceneManager.Instance.SelectedRobot.WriteUnlock();
            }
            Notifications.Instance.ShowToastMessage("Position updated successfully");
        } catch (Arcor2ConnectionException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Update position failed", ex.Message);
        } finally {
            confirmationDialog.Close();
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
        foreach (NamedOrientation orientation in CurrentActionPoint.GetNamedOrientations()) {
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
        EventTrigger.Entry onPointerEnter = new() {
            eventID = EventTriggerType.PointerEnter
        };
        onPointerEnter.callback.AddListener((eventData) => orientationOutline.Highlight());
        eventTrigger.triggers.Add(onPointerEnter);

        // Create OnPointerExit entry
        EventTrigger.Entry onPointerExit = new() {
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
        ServiceButton serviceBtn = Instantiate(GameManager.Instance.ServiceButtonPrefab, parent).GetComponent<ServiceButton>();
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
        btn = Instantiate(GameManager.Instance.ButtonPrefab, parent).GetComponent<ActionButton>();
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


    public void UpdateJointsDynamicList(string arm_id) {
        UpdateJointsDynamicList(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedRobot.MultiArm() ? arm_id : "");
    }


    public void UpdateJointsDynamicList(string robot_id, string arm_id) {


        try {
            foreach (RectTransform o in JointsDynamicList.GetComponentsInChildren<RectTransform>()) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }

            List<ProjectRobotJoints> joints;
            if (string.IsNullOrEmpty(arm_id))
                joints = CurrentActionPoint.GetAllJoints(true, robot_id).Values.ToList();
            else
                joints = CurrentActionPoint.GetJointsOfArm(robot_id, arm_id, true).Values.ToList();
            foreach (ProjectRobotJoints joint in joints) {
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
        CloseAnySubmenu();
    }

    public void CloseAnySubmenu() {
        if (OrientationJointsDetailMenu.IsVisible())
            OrientationJointsDetailMenu.HideMenu();
        AddOrientationMenu.gameObject.SetActive(false);
        AddJointsMenu.gameObject.SetActive(false);
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
            var response = await CommunicationManager.Instance.Client.AddActionPointOrientationAsync(new AddActionPointOrientationRequestArgs(CurrentActionPoint.Data.Id, new Orientation(), name));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to add new orientation", string.Join(',', response.Messages));
                return;
            }

            Notifications.Instance.ShowToastMessage("Orientation added successfully");
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }


}
