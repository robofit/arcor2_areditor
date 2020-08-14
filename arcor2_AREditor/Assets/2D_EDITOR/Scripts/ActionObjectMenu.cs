using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using Base;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotArgs;
using System.Collections.Generic;

[RequireComponent(typeof(SimpleSideMenu))]
public class ActionObjectMenu : MonoBehaviour, IMenu {
    public Base.ActionObject CurrentObject;
    [SerializeField]
    private TMPro.TMP_Text objectName;
    public DropdownParameter RobotsList, EndEffectorList, PivotList;
    public Button NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject RobotsListsBlock, UpdatePositionBlockMesh, UpdatePositionBlockVO;
    public Slider VisibilitySlider;
    public InputDialog InputDialog;

    public SwitchComponent ShowModelSwitch;

    public ConfirmationDialog ConfirmationDialog;

    private int currentFocusPoint = -1;

    private GameObject model;

    private SimpleSideMenu menu;

    private void Start() {
        menu = GetComponent<SimpleSideMenu>();
        Debug.Assert(objectName != null);
        Debug.Assert(RobotsList != null);
        Debug.Assert(EndEffectorList != null);
        Debug.Assert(NextButton != null);
        Debug.Assert(PreviousButton != null);
        Debug.Assert(FocusObjectDoneButton != null);
        Debug.Assert(StartObjectFocusingButton != null);
        Debug.Assert(SavePositionButton != null);
        Debug.Assert(CurrentPointLabel != null);
        Debug.Assert(RobotsListsBlock != null);
        Debug.Assert(UpdatePositionBlockMesh != null);
        Debug.Assert(UpdatePositionBlockVO != null);
        Debug.Assert(VisibilitySlider != null);
        Debug.Assert(InputDialog != null);
        Debug.Assert(ConfirmationDialog != null);

        List<string> pivots = new List<string>();
        foreach (string item in Enum.GetNames(typeof(PivotEnum))) {
            pivots.Add(item);
        }
        PivotList.PutData(pivots, "Middle", OnPivotChanged);
    }


    public async void DeleteActionObject() {
        IO.Swagger.Model.RemoveFromSceneResponse response =
            await WebsocketManager.Instance.RemoveFromScene(CurrentObject.Data.Id, false);
        if (!response.Result) {
            Notifications.Instance.ShowNotification("Failed to remove object " + CurrentObject.Data.Name, response.Messages[0]);
            return;
        }
        CurrentObject = null;
        ConfirmationDialog.Close();
        MenuManager.Instance.ActionObjectMenuSceneEditor.Close();
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action object",
                                "Do you want to delete action object " + CurrentObject.Data.Name + "?",
                                () => DeleteActionObject(),
                                () => ConfirmationDialog.Close());
    }

    public void ShowRenameDialog() {
        InputDialog.Open("Rename action object",
                         "",
                         "New name",
                         CurrentObject.Data.Name,
                         () => RenameObject(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public async void RenameObject(string newName) {
        try {
            await WebsocketManager.Instance.RenameObject(CurrentObject.Data.Id, newName);
            InputDialog.Close();
            objectName.text = newName;
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename object", e.Message);
        }
    }


    public void UpdateMenu() {
        if (currentFocusPoint >= 0)
            return;

        if (SceneManager.Instance.RobotInScene()) {
            RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, true);
            string robotId = null;
            try {
                robotId = SceneManager.Instance.RobotNameToId(RobotsList.GetValue().ToString());
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                robotId = null;
            }
            if (string.IsNullOrEmpty(robotId)) {
                Notifications.Instance.ShowNotification("Robot not found", "Robot with name " + RobotsList.GetValue().ToString() + "does not exists");

            } else {
                OnRobotChanged(robotId);
            }            

            if (CurrentObject.ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(true);
                RobotsListsBlock.SetActive(true);
            } else if (CurrentObject.ActionObjectMetadata.ObjectModel != null) {
                UpdatePositionBlockVO.SetActive(true);
                UpdatePositionBlockMesh.SetActive(false);
                RobotsListsBlock.SetActive(true);
                ShowModelSwitch.Interactable = SceneManager.Instance.RobotsEEVisible;
                if (ShowModelSwitch.Interactable && ShowModelSwitch.Switch.isOn) {
                    ShowModelOnEE();
                }
            } else {
                UpdatePositionBlockVO.SetActive(false);
                UpdatePositionBlockMesh.SetActive(false);
                RobotsListsBlock.SetActive(false);
            }

        } else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
        }

        
        FocusObjectDoneButton.interactable = false;
        NextButton.interactable = false;
        PreviousButton.interactable = false;
        objectName.text = CurrentObject.Data.Name;

        VisibilitySlider.value = CurrentObject.GetVisibility() * 100;

        
    }

    private void OnRobotChanged(string robot_id) {
        EndEffectorList.Dropdown.dropdownItems.Clear();
        EndEffectorList.gameObject.GetComponent<DropdownEndEffectors>().Init(robot_id, OnEEChanged);
        UpdateModelOnEE();
    }

    private void OnEEChanged(string eeId) {
        UpdateModelOnEE();
    }

    private void OnPivotChanged(string pivot) {
        UpdateModelOnEE();
    }
       

    public async void UpdateObjectPosition() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 || EndEffectorList.Dropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        PivotEnum pivot = (PivotEnum) Enum.Parse(typeof(PivotEnum), (string) PivotList.GetValue());
        IRobot robot = SceneManager.Instance.GetRobotByName((string) RobotsList.GetValue());
        await WebsocketManager.Instance.UpdateActionObjectPoseUsingRobot(CurrentObject.Data.Id,
                robot.GetId(), (string) EndEffectorList.GetValue(), pivot);
        
    }
         

    private void EnableFocusControls() {
        SavePositionButton.GetComponent<Button>().interactable = true;
        StartObjectFocusingButton.GetComponent<Button>().interactable = true;
        NextButton.GetComponent<Button>().interactable = true;
        PreviousButton.GetComponent<Button>().interactable = true;
        FocusObjectDoneButton.GetComponent<Button>().interactable = true;
    }

    private void DisableFocusControls() {
        SavePositionButton.GetComponent<Button>().interactable = false;
        StartObjectFocusingButton.GetComponent<Button>().interactable = false;
        NextButton.GetComponent<Button>().interactable = false;
        PreviousButton.GetComponent<Button>().interactable = false;
        FocusObjectDoneButton.GetComponent<Button>().interactable = false;
    }

    public async void StartObjectFocusing() {
        if (RobotsList.Dropdown.dropdownItems.Count == 0 || EndEffectorList.Dropdown.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        try {
            await WebsocketManager.Instance.StartObjectFocusing(CurrentObject.Data.Id,
                (string) RobotsList.GetValue(),
                (string) EndEffectorList.GetValue());
            currentFocusPoint = 0;
            UpdateCurrentPointLabel();
            GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = false;
            GetComponent<SimpleSideMenu>().overlayCloseOnPressed = false;
            FocusObjectDoneButton.interactable = true;
            if (CurrentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
                NextButton.interactable = true;
                PreviousButton.interactable = true;
            }
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to start object focusing", ex.Message);
            CurrentPointLabel.text = "";
            currentFocusPoint = -1;
            if (ex.Message == "Focusing already started.") { //TODO HACK! find better solution
                FocusObjectDone();
            }
        }
    }

    public async void SavePosition() {
        if (currentFocusPoint < 0)
            return;
        try {
            await WebsocketManager.Instance.SavePosition(CurrentObject.Data.Id, currentFocusPoint);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to save current position", ex.Message);
        }

        
    }

    public async void FocusObjectDone() {
        try {
            await WebsocketManager.Instance.FocusObjectDone(CurrentObject.Data.Id);
            CurrentPointLabel.text = "";
            GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = true;
            GetComponent<SimpleSideMenu>().overlayCloseOnPressed = true;
            currentFocusPoint = -1;
            FocusObjectDoneButton.interactable = false;
            NextButton.interactable = false;
            PreviousButton.interactable = false;
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to focus object", ex.Message);
        }     
    }

    public void NextPoint() {
        currentFocusPoint = Math.Min(currentFocusPoint + 1, CurrentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1);
        PreviousButton.interactable = true;
        if (currentFocusPoint == CurrentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1) {
            NextButton.GetComponent<Button>().interactable = false;
        } else {
            NextButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    public void PreviousPoint() {
        currentFocusPoint = Math.Max(currentFocusPoint - 1, 0);
        NextButton.interactable = true;
        if (currentFocusPoint == 0) {
            PreviousButton.GetComponent<Button>().interactable = false;
        } else {
            PreviousButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    private void UpdateCurrentPointLabel() {
        CurrentPointLabel.text = "Point " + (currentFocusPoint + 1) + " out of " + CurrentObject.ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count.ToString();
    }


    public void OnVisibilityChange(float value) {
        if (CurrentObject != null)
            CurrentObject.SetVisibility(value / 100f);
    }

    public void ShowModelOnEE() {
        if (model != null)
            HideModelOnEE();
        model = CurrentObject.GetModelCopy();
        if (model == null)
            return;
        UpdateModelOnEE();
    }

    private void UpdateModelOnEE() {
        if (model == null)
            return;
        string robotName = (string) RobotsList.GetValue(), eeId = (string) EndEffectorList.GetValue();
        if (string.IsNullOrEmpty(robotName) || string.IsNullOrEmpty(eeId)) {
            throw new RequestFailedException("Robot or end effector not selected!");
        }
        
        try {
            string robotId = SceneManager.Instance.RobotNameToId(robotName);
            RobotEE ee = SceneManager.Instance.GetRobot(robotId).GetEE(eeId);
            model.transform.parent = ee.gameObject.transform;

            switch ((PivotEnum) Enum.Parse(typeof(PivotEnum), (string) PivotList.GetValue())) {
                case PivotEnum.Top:
                    model.transform.localPosition = new Vector3(0, model.transform.localScale.y / 2, 0);
                    break;
                case PivotEnum.Bottom:
                    model.transform.localPosition = new Vector3(0, -model.transform.localScale.y / 2, 0);
                    break;
                case PivotEnum.Middle:
                    model.transform.localPosition = new Vector3(0, 0, 0);
                    break;
            }
            model.transform.localRotation = new Quaternion(0, 0, 0, 1);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("End-effector position unknown", "Robot did not send position of selected end effector");
            ShowModelSwitch.Switch.isOn = false;
        }
        
    }

    public void HideModelOnEE() {
        if (model != null) {
            Destroy(model);
        }            
        model = null;
    }

    public void OnMenuStateChanged() {
        switch (menu.CurrentState) {
            case SimpleSideMenu.State.Closed:
                HideModelOnEE();
                break;
        }
    }

}
