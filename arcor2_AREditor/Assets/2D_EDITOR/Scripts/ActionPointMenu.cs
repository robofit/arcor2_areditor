using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class ActionPointMenu : MonoBehaviour {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;

    [SerializeField]
    private GameObject dynamicContent, topText, interactiveObjectType, robotsList, updatePositionButton, endEffectorList,
        CollapsablePrefab, orientationsList, scrollableContent, AddOrientationDialog, FocusConfirmationDialog, UpdatePositionToggle,
        UpdatePositionBlock;

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
        orientationDropdown.SetupDropdown();
    }

    

    public void DeleteAP() {
        if (CurrentActionPoint == null)
            return;
        CurrentActionPoint.GetComponent<Base.ActionPoint>().DeleteAP();
    }

   

    private void OnRobotChanged(string robot_id) {
        Debug.LogError(robot_id);
        endEffectorList.GetComponent<CustomDropdown>().dropdownItems.Clear();
        endEffectorList.GetComponent<DropdownEndEffectors>().Init(robot_id);
    }

   

    public void ShowAddOrientationDialog() {
        AddOrientationDialog.GetComponent<AddOrientationDialog>().ap = CurrentActionPoint;
        AddOrientationDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }

    public void ShowFocusConfirmationDialog() {
        CustomDropdown robotsListDropdown = robotsList.GetComponent<CustomDropdown>();
        CustomDropdown endEffectorDropdown = endEffectorList.GetComponent<CustomDropdown>();
        CustomDropdown orientationDropdown = orientationsList.GetComponent<CustomDropdown>();
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().EndEffectorId = endEffectorDropdown.selectedText.text;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().RobotId = robotsListDropdown.selectedText.text;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().OrientationId = orientationDropdown.selectedText.text;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().UpdatePosition = UpdatePositionToggle.GetComponent<Toggle>().isOn;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().ActionPointId = CurrentActionPoint.Data.Id;
        FocusConfirmationDialog.GetComponent<FocusConfirmationDialog>().Init();
        FocusConfirmationDialog.GetComponent<ModalWindowManager>().OpenWindow();
    }
}
