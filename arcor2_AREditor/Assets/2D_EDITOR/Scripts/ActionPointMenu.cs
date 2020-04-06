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

    public DropdownParameter robotsList, endEffectorList, orientationsList;
    public DropdownParameterJoints JointsList;

    public ConfirmationDialog ConfirmationDialog;

    public AddNewActionDialog AddNewActionDialog;

    [SerializeField]
    private Button LockedBtn, UnlockedBtn;

    [SerializeField]
    private InputDialog inputDialog;


    public async void CreatePuck(string action_id, IActionProvider actionProvider) {
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
        ActionObjectType.text = actionPoint.ActionObject.GetComponent<Base.ActionObject>().Data.Type;

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in Base.ActionsManager.Instance.GetAllActionsOfObject(actionPoint.ActionObject.GetComponent<Base.ActionObject>())) {
            GameObject collapsableMenu = Instantiate(CollapsablePrefab, dynamicContent.transform);
            collapsableMenu.GetComponent<CollapsableMenu>().Name = keyval.Key.GetProviderName();
            collapsableMenu.GetComponent<CollapsableMenu>().Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
                GameObject btnGO = Instantiate(Base.GameManager.Instance.ButtonPrefab, collapsableMenu.GetComponent<CollapsableMenu>().Content.transform);
                btnGO.transform.localScale = new Vector3(1, 1, 1);
                Button btn = btnGO.GetComponent<Button>();
                btn.GetComponentInChildren<TMPro.TMP_Text>().text = am.Name;
                btnGO.AddComponent<TooltipContent>();
                btnGO.GetComponent<TooltipContent>().enabled = am.Description != "";
                
                if (btnGO.GetComponent<TooltipContent>().tooltipRect == null) {
                    btnGO.GetComponent<TooltipContent>().tooltipRect = Base.GameManager.Instance.Tooltip;
                }
                if (btnGO.GetComponent<TooltipContent>().descriptionText == null) {
                    btnGO.GetComponent<TooltipContent>().descriptionText = Base.GameManager.Instance.Text;
                }
                btnGO.GetComponent<TooltipContent>().description = am.Description;
                btn.onClick.AddListener(() => CreatePuck(am.Name, keyval.Key));
            }

        }
        CustomDropdown robotsListDropdown = robotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();

        robotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged);
        if (robotsListDropdown.dropdownItems.Count == 0) {
            UpdatePositionBlock.SetActive(false);
        } else {
            OnRobotChanged((string) robotsList.GetValue());
            UpdatePositionBlock.SetActive(true);
            
        }
        
        UpdateOrientations();
        UpdateLockedBtns(CurrentActionPoint.Locked);
        
    }

    public void UpdateOrientations() {
        CustomDropdown orientationDropdown = orientationsList.Dropdown;
        orientationDropdown.dropdownItems.Clear();
        foreach (string orientation in CurrentActionPoint.GetPoses().Keys) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = orientation
            };
            orientationDropdown.dropdownItems.Add(item);
        }
        if (orientationDropdown.dropdownItems.Count == 0) {
            orientationDropdown.gameObject.SetActive(false);
            NoOrientation.gameObject.SetActive(true);
        } else {
            NoOrientation.gameObject.SetActive(false);
            orientationDropdown.enabled = true;
            orientationDropdown.SetupDropdown();
        }
            
    }
        

   public void UpdateJoints(string robot_id, string selectedJoints=null) {
        if (robot_id == null)
            return;
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        
        JointsList.PutData(CurrentActionPoint.GetJoints(true, robot_id).Values.ToList(), selectedJoints, null);

        if (jointsDropdown.dropdownItems.Count > 0) {
            NoJoints.gameObject.SetActive(false);
            jointsDropdown.gameObject.SetActive(true);
        } else {
            jointsDropdown.gameObject.SetActive(false);
            NoJoints.gameObject.SetActive(true);
        }
    }

    

    public void DeleteAP() {
        Debug.Assert(CurrentActionPoint != null);
        CurrentActionPoint.GetComponent<Base.ActionPoint>().DeleteAP();
        MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
    }

   

    public void ShowDeleteAPDialog() {
        ConfirmationDialog.Open("Delete action point",
                                "Do you want to delete action point " + CurrentActionPoint.Data.Id + "?",
                                () => DeleteAP(),
                                () => ConfirmationDialog.Close());
    }

   

    private void OnRobotChanged(string robot_id) {
        endEffectorList.Dropdown.dropdownItems.Clear();
        endEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robot_id);
        if (endEffectorList.Dropdown.dropdownItems.Count == 0) {
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
        bool success = await Base.GameManager.Instance.AddActionPointOrientation(CurrentActionPoint, name);
        if (success) {
            inputDialog.Close();
        }
        UpdateOrientations();
    }

    public void FocusJoints() {
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        if (jointsDropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", "");
            return;
        }
        try {
            Base.GameManager.Instance.UpdateActionPointJoints((string) robotsList.GetValue(), (string) JointsList.GetValue());
            Base.NotificationsModernUI.Instance.ShowNotification("Joints updated sucessfully", "");
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", ex.Message);
        }
        UpdateJoints((string) robotsList.GetValue(), (string) JointsList.GetValue());
    }

    public void ShowFocusConfirmationDialog() {
        if (robotsList.Dropdown.dropdownItems.Count == 0 ||
            endEffectorList.Dropdown.dropdownItems.Count == 0 ||
            orientationsList.Dropdown.dropdownItems.Count == 0) {
            Base.Notifications.Instance.ShowNotification("Failed to update orientation.", "Something is not selected");
            return;
        }
        CustomDropdown robotsListDropdown = robotsList.Dropdown;
        CustomDropdown endEffectorDropdown = endEffectorList.Dropdown;
        CustomDropdown orientationDropdown = orientationsList.Dropdown;
        if (endEffectorDropdown.dropdownItems.Count == 0) {
            FocusConfirmationDialog.EndEffectorId = "";
        } else {
            FocusConfirmationDialog.EndEffectorId = endEffectorDropdown.selectedText.text;
        }
        FocusConfirmationDialog.RobotId = robotsListDropdown.selectedText.text;

        FocusConfirmationDialog.OrientationId = orientationDropdown.selectedText.text;
        FocusConfirmationDialog.UpdatePosition = UpdatePositionToggle.GetComponent<Toggle>().isOn;
        FocusConfirmationDialog.ActionPointId = CurrentActionPoint.Data.Id;
        FocusConfirmationDialog.ActionPointUserId = CurrentActionPoint.Data.Name;
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
        bool result = await Base.GameManager.Instance.UpdateActionPointParent(CurrentActionPoint, null);
        if (result) {
            ConfirmationDialog.Close();
        }
    }

    public void ShowUntieActionPointDialog() {
        ConfirmationDialog.Open("Untie action point",
                                "Do you want to untie action point " + CurrentActionPoint.Data.Id + "?",
                                () => UntieActionPoint(),
                                () => ConfirmationDialog.Close());
    }
}
