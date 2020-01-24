using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class ActionPointMenu : MonoBehaviour {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;
    public GameObject UpdatePoseBlock, UpdateJointsBlock;
    public TMPro.TMP_Text NoOrientation, NoJoints;

    [SerializeField]
    private GameObject dynamicContent, topText, interactiveObjectType, robotsList, updatePositionButton, endEffectorList,
        CollapsablePrefab, orientationsList, scrollableContent, AddOrientationDialog, FocusConfirmationDialog, UpdatePositionToggle,
        UpdatePositionBlock, JointsList, AddJointsDialog;

    public void CreatePuck(string action_id, IActionProvider actionProvider) {
        Base.GameManager.Instance.SpawnPuck(action_id, CurrentActionPoint, true, actionProvider);
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.GetComponent<Base.ActionPoint>().Data.Id = new_id;
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
        topText.GetComponentInChildren<Text>().text = actionPoint.Data.Id;
        interactiveObjectType.GetComponent<Text>().text = actionPoint.ActionObject.GetComponent<Base.ActionObject>().Data.Type;

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
        CustomDropdown robotsListDropdown = robotsList.GetComponent<CustomDropdown>();
        robotsListDropdown.dropdownItems.Clear();

        robotsListDropdown.GetComponent<DropdownRobots>().Init(OnRobotChanged);
        if (robotsListDropdown.dropdownItems.Count == 0) {
            UpdatePositionBlock.SetActive(false);
        } else {
            OnRobotChanged(robotsListDropdown.selectedText.text);
            UpdatePositionBlock.SetActive(true);
        }
        UpdateJoints();
        UpdateOrientations();


    }

    public void UpdateOrientations() {
        CustomDropdown orientationDropdown = orientationsList.GetComponent<CustomDropdown>();
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
        

   public void UpdateJoints() {
        CustomDropdown jointsDropdown = JointsList.GetComponent<CustomDropdown>();
        jointsDropdown.dropdownItems.Clear();
        foreach (string joints in CurrentActionPoint.GetJoints(true).Keys) {
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
        endEffectorList.GetComponent<CustomDropdown>().dropdownItems.Clear();
        endEffectorList.GetComponent<DropdownEndEffectors>().Init(robot_id);
        if (endEffectorList.GetComponent<CustomDropdown>().dropdownItems.Count == 0) {
            UpdatePoseBlock.SetActive(false);
            UpdateJointsBlock.SetActive(true);
        } else {
            UpdatePoseBlock.SetActive(true);
            UpdateJointsBlock.SetActive(false);
        }
    }

   

    public void ShowAddOrientationDialog() {
        AddOrientationDialog.GetComponent<AddOrientationDialog>().ap = CurrentActionPoint;
        AddOrientationDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowAddJointsDialog() {
        AddJointsDialog.GetComponent<AddJointsDialog>().ap = CurrentActionPoint;
        AddJointsDialog.GetComponent<AddJointsDialog>().RobotId = robotsList.GetComponent<CustomDropdown>().selectedText.text;
        AddJointsDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void FocusJoints() {
        CustomDropdown robotsListDropdown = robotsList.GetComponent<CustomDropdown>();
        CustomDropdown jointsDropdown = JointsList.GetComponent<CustomDropdown>();
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
        if (robotsList.GetComponent<CustomDropdown>().dropdownItems.Count == 0 ||
            endEffectorList.GetComponent<CustomDropdown>().dropdownItems.Count == 0 ||
            orientationsList.GetComponent<CustomDropdown>().dropdownItems.Count == 0) {
            Base.Notifications.Instance.ShowNotification("Failed to update orientation.", "Something is not selected");
            return;
        }
        CustomDropdown robotsListDropdown = robotsList.GetComponent<CustomDropdown>();
        CustomDropdown endEffectorDropdown = endEffectorList.GetComponent<CustomDropdown>();
        CustomDropdown orientationDropdown = orientationsList.GetComponent<CustomDropdown>();
        if (endEffectorDropdown.dropdownItems.Count == 0) {
            FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().EndEffectorId = "";
        } else {
            FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().EndEffectorId = endEffectorDropdown.selectedText.text;
        }
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().RobotId = robotsListDropdown.selectedText.text;

        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().OrientationId = orientationDropdown.selectedText.text;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().UpdatePosition = UpdatePositionToggle.GetComponent<Toggle>().isOn;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().ActionPointId = CurrentActionPoint.Data.Id;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().Init();
        FocusConfirmationDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }
}
