using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class ActionPointMenu : MonoBehaviour, IMenu {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;
    public GameObject UpdatePoseBlock, UpdateJointsBlock;
    public TMPro.TMP_Text NoOrientation, NoJoints, ActionObjectType;
    [SerializeField]
    private TMPro.TMP_InputField topText;

    [SerializeField]
    private GameObject dynamicContent, updatePositionButton,
        CollapsablePrefab, scrollableContent, AddOrientationDialog, UpdatePositionToggle,
        UpdatePositionBlock, AddJointsDialog;

    public FocusConfirmationDialog FocusConfirmationDialog;

    public DropdownParameter robotsList, endEffectorList, orientationsList, JointsList;

    public AddNewActionDialog AddNewActionDialog;


    public async void CreatePuck(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.Init(actionProvider, actionProvider.GetActionMetadata(action_id), CurrentActionPoint);
        AddNewActionDialog.WindowManager.OpenWindow();
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.UpdateId(new_id);
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
        SetHeader(actionPoint.Data.Id);
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
                
                if (btnGO.GetComponent<TooltipContent>().tooltipObject == null) {
                    btnGO.GetComponent<TooltipContent>().tooltipObject = Base.GameManager.Instance.Tooltip;
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
        

   public void UpdateJoints(string robot_id) {
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        jointsDropdown.dropdownItems.Clear();
        foreach (string joints in CurrentActionPoint.GetJoints(true, robot_id).Keys) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = joints
            };
            jointsDropdown.dropdownItems.Add(item);
        }
        if (jointsDropdown.dropdownItems.Count > 0) {
            NoJoints.gameObject.SetActive(false);
            jointsDropdown.gameObject.SetActive(true);
            jointsDropdown.SetupDropdown();
        } else {
            jointsDropdown.gameObject.SetActive(false);
            NoJoints.gameObject.SetActive(true);
        }
    }

    

    public void DeleteAP() {
        if (CurrentActionPoint == null)
            return;
        CurrentActionPoint.GetComponent<Base.ActionPoint>().DeleteAP();
        MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
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
        AddOrientationDialog.GetComponent<AddOrientationDialog>().ap = CurrentActionPoint;
        AddOrientationDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowAddJointsDialog() {
        AddJointsDialog.GetComponent<AddJointsDialog>().ap = CurrentActionPoint;
        AddJointsDialog.GetComponent<AddJointsDialog>().RobotId = robotsList.Dropdown.selectedText.text;
        AddJointsDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void FocusJoints() {
        CustomDropdown robotsListDropdown = robotsList.Dropdown;
        CustomDropdown jointsDropdown = JointsList.Dropdown;
        if (jointsDropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", "");
            return;
        }
        try {
            Base.GameManager.Instance.UpdateActionPointJoints(CurrentActionPoint.Data.Id, robotsListDropdown.selectedText.text, jointsDropdown.selectedText.text);
            Base.NotificationsModernUI.Instance.ShowNotification("Joints updated sucessfully", "");
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update joints", ex.Message);
        }

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
        FocusConfirmationDialog.Init();
        FocusConfirmationDialog.WindowManager.OpenWindow();
    }

    public void SetHeader(string header) {
        topText.text = header;
    }
}
