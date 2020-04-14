using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class ActionPointMenu : MonoBehaviour, IMenu {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;
    public GameObject UpdatePoseBlock, UpdateJointsBlock;
    public TMPro.TMP_Text NoOrientation, NoJoints, ActionObjectType;
    [SerializeField]
    private TMPro.TMP_Text actionPointName;

    [SerializeField]
    private GameObject dynamicContent, updatePositionButton,
        CollapsablePrefab, scrollableContent, AddOrientationDialog, UpdatePositionToggle,
        UpdatePositionBlock, AddJointsDialog;

    public FocusConfirmationDialog FocusConfirmationDialog;

    public DropdownParameter RobotsList, EndEffectorList, OrientationsList;
    public DropdownParameterJoints JointsList;

    public ConfirmationDialog ConfirmationDialog;

    public AddNewActionDialog AddNewActionDialog;

    [SerializeField]
    private Button LockedBtn, UnlockedBtn, UntieBtn, BackBtn;

    [SerializeField]
    private InputDialog inputDialog;


    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), CurrentActionPoint);
        AddNewActionDialog.WindowManager.OpenWindow();
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.UpdateId(new_id);
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename action point",
                         "Type new name",
                         "New name",
                         CurrentActionPoint.Data.Name,
                         () => RenameActionPoint(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameActionPoint(string newUserId) {
        bool result = await Base.GameManager.Instance.RenameActionPoint(CurrentActionPoint, newUserId);
        if (result) {
            inputDialog.Close();
            actionPointName.text = newUserId;
        }
    }


    public void UpdateMenu() {
        scrollableContent.GetComponent<VerticalLayoutGroup>().enabled = true;

        Base.ActionPoint actionPoint;
        if (CurrentActionPoint == null) {
            return;
        } else {
            actionPoint = CurrentActionPoint.GetComponent<Base.ActionPoint>();
        }

        foreach (RectTransform o in dynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(actionPoint.Data.Name);
        if (actionPoint.Parent != null)
            ActionObjectType.text = actionPoint.Parent.GetName();
        else
            ActionObjectType.text = "Global action point";

        Dictionary<IActionProvider, List<Base.ActionMetadata>> actionsMetadata;
        if (actionPoint.Parent == null) {
            actionsMetadata = Base.ActionsManager.Instance.GetAllFreeActions();
        } else {
            Base.ActionObject parentActionObject = actionPoint.Parent.GetActionObject();
            if (parentActionObject == null)
                actionsMetadata = Base.ActionsManager.Instance.GetAllFreeActions();
            else
                actionsMetadata = Base.ActionsManager.Instance.GetAllActionsOfObject(parentActionObject);
        }

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, dynamicContent.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.Name = keyval.Key.GetProviderName();
            collapsableMenu.Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
                Button btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, collapsableMenu.Content.transform).GetComponent<Button>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.GetComponentInChildren<TMPro.TMP_Text>().text = am.Name;
                TooltipContent btnTooltip = btn.gameObject.AddComponent<TooltipContent>();
                btnTooltip.enabled = am.Description != "";
                
                if (btnTooltip.tooltipRect == null) {
                    btnTooltip.tooltipRect = Base.GameManager.Instance.Tooltip;
                }
                if (btnTooltip.descriptionText == null) {
                    btnTooltip.descriptionText = Base.GameManager.Instance.Text;
                }
                btnTooltip.description = am.Description;
                btn.onClick.AddListener(() => ShowAddNewActionDialog(am.Name, keyval.Key));
            }

        }
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, false);
        if (robotsListDropdown.dropdownItems.Count == 0) {
            UpdatePositionBlock.SetActive(false);
        } else {
            OnRobotChanged((string) RobotsList.GetValue());
            UpdatePositionBlock.SetActive(true);
            
        }
        
        UpdateOrientations();
        UpdateLockedBtns(CurrentActionPoint.Locked);
        if (CurrentActionPoint.Parent == null)
            UntieBtn.interactable = false;
        else
            UntieBtn.interactable = true;
        
    }

    public void UpdateOrientations() {
        CustomDropdown orientationDropdown = OrientationsList.Dropdown;
        orientationDropdown.dropdownItems.Clear();
        foreach (IO.Swagger.Model.NamedOrientation orientation in CurrentActionPoint.GetNamedOrientations()) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = orientation.Name
            };
            orientationDropdown.dropdownItems.Add(item);
        }
        if (orientationDropdown.dropdownItems.Count == 0) {
            OrientationsList.gameObject.SetActive(false);
            NoOrientation.gameObject.SetActive(true);
        } else {
            NoOrientation.gameObject.SetActive(false);
            OrientationsList.gameObject.SetActive(true);
            orientationDropdown.enabled = true;
            orientationDropdown.SetupDropdown();
        }
            
    }
        

   public void UpdateJoints(string robot_id, string selectedJoints=null) {
        if (robot_id == null)
            return;
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        
        JointsList.PutData(CurrentActionPoint.GetAllJoints(true, robot_id).Values.ToList(), selectedJoints, null);

        if (jointsDropdown.dropdownItems.Count > 0) {
            NoJoints.gameObject.SetActive(false);
            JointsList.gameObject.SetActive(true);
        } else {
            JointsList.gameObject.SetActive(false);
            NoJoints.gameObject.SetActive(true);
        }
    }

    

    public async void DeleteAP() {
        Debug.Assert(CurrentActionPoint != null);
        bool success = await Base.GameManager.Instance.RemoveActionPoint(CurrentActionPoint.Data.Id);
        if (success) {
            ConfirmationDialog.Close();
            MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
        }    
    }

   

    public void ShowDeleteAPDialog() {
        ConfirmationDialog.Open("Delete action point",
                                "Do you want to delete action point " + CurrentActionPoint.Data.Name + "?",
                                () => DeleteAP(),
                                () => ConfirmationDialog.Close());
    }

   

    private void OnRobotChanged(string robot_id) {
        EndEffectorList.Dropdown.dropdownItems.Clear();
        EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robot_id);
        if (EndEffectorList.Dropdown.dropdownItems.Count == 0) {
            UpdatePoseBlock.SetActive(false);
            UpdateJointsBlock.SetActive(true);
        } else {
            UpdatePoseBlock.SetActive(true);
            UpdateJointsBlock.SetActive(false);
        }
        UpdateJoints(robot_id);
    }

   

    public void ShowAddOrientationDialog() {
        inputDialog.Open("Create new named orientation",
                         "Please set name of the new orientation",
                         "Name",
                         "",
                         () => AddOrientation(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void AddOrientation(string name) {
        Debug.Assert(CurrentActionPoint != null);
        bool success = await Base.GameManager.Instance.AddActionPointOrientation(CurrentActionPoint, name);
        if (success) {
            inputDialog.Close();
        }
        UpdateOrientations();
    }

    public void ShowAddJointsDialog() {
        inputDialog.Open("Create new joints configuration",
                         "Please set name of the new joints configuration",
                         "Name",
                         "",
                         () => AddJoints(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void AddJoints(string name) {
        Debug.Assert(CurrentActionPoint != null);
        bool success = await Base.GameManager.Instance.AddActionPointJoints(CurrentActionPoint, name, (string) RobotsList.GetValue());
        if (success) {
            inputDialog.Close();
        }
        UpdateJoints((string) RobotsList.GetValue());
    }

    public void FocusJoints() {
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        if (jointsDropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", "");
            return;
        }
        try {
            Base.GameManager.Instance.UpdateActionPointJoints((string) RobotsList.GetValue(), CurrentActionPoint.GetJoints((string) JointsList.GetValue()).Name);
            Base.NotificationsModernUI.Instance.ShowNotification("Joints updated sucessfully", "");
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", ex.Message);
        }
        UpdateJoints((string) RobotsList.GetValue(), (string) JointsList.GetValue());
    }

    public void ShowFocusConfirmationDialog() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 ||
            EndEffectorList.Dropdown.dropdownItems.Count == 0 ||
            OrientationsList.Dropdown.dropdownItems.Count == 0) {
            Base.Notifications.Instance.ShowNotification("Failed to update orientation.", "Something is not selected");
            return;
        }
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        CustomDropdown endEffectorDropdown = EndEffectorList.Dropdown;
        CustomDropdown orientationDropdown = OrientationsList.Dropdown;
        if (endEffectorDropdown.dropdownItems.Count == 0) {
            FocusConfirmationDialog.EndEffectorId = "";
        } else {
            FocusConfirmationDialog.EndEffectorId = endEffectorDropdown.selectedText.text;
        }
        FocusConfirmationDialog.RobotId = robotsListDropdown.selectedText.text;

        FocusConfirmationDialog.OrientationId = CurrentActionPoint.GetNamedOrientationByName(orientationDropdown.selectedText.text).Id;
        FocusConfirmationDialog.OrientationName = orientationDropdown.selectedText.text;
        FocusConfirmationDialog.UpdatePosition = UpdatePositionToggle.GetComponent<Toggle>().isOn;
        FocusConfirmationDialog.ActionPointId = CurrentActionPoint.Data.Id;
        FocusConfirmationDialog.ActionPointName = CurrentActionPoint.Data.Name;
        FocusConfirmationDialog.Init();
        FocusConfirmationDialog.WindowManager.OpenWindow();
    }

    public void SetHeader(string header) {
        actionPointName.text = header;
    }

    public void UpdateLockedBtns(bool locked) {
        LockedBtn.gameObject.SetActive(locked);
        UnlockedBtn.gameObject.SetActive(!locked);
    }

    public void SetLocked(bool locked) {
        CurrentActionPoint.Locked = locked;
        UpdateLockedBtns(locked);
    }

   
    public async void UntieActionPoint() {
        Debug.Assert(CurrentActionPoint != null);
        bool result = await Base.GameManager.Instance.UpdateActionPointParent(CurrentActionPoint, "");
        if (result) {
            ConfirmationDialog.Close();
        }
    }

    public void ShowUntieActionPointDialog() {
        ConfirmationDialog.Open("Untie action point",
                                "Do you want to untie action point " + CurrentActionPoint.Data.Name + "?",
                                () => UntieActionPoint(),
                                () => ConfirmationDialog.Close());
    }

    public void EnableBackButton(bool enable) {
        BackBtn.gameObject.SetActive(enable);
    }

    public void BackToParentMenu() {
        CurrentActionPoint.Parent.ShowMenu();
        Base.Scene.Instance.SetSelectedObject(CurrentActionPoint.Parent.GetGameObject());
        CurrentActionPoint.Parent.GetGameObject().SendMessage("Select");
    }
}
