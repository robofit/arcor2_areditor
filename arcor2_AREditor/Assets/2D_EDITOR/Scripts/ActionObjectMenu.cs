using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using Base;

public class ActionObjectMenu : MonoBehaviour {
    public ActionObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab;
    public CustomDropdown RobotsList, EndEffectorList;
    public Button NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject RobotsListsBlock, UpdatePositionBlockMesh, UpdatePositionBlockVO;

    private int currentFocusPoint = -1;

    public void SaveID(string new_id) {
        CurrentObject.UpdateId(new_id);
    }

    public async void DeleteIO() {
        IO.Swagger.Model.RemoveFromSceneResponse response = await Base.GameManager.Instance.RemoveFromScene(CurrentObject.Data.Id);
        if (!response.Result) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to remove object " + CurrentObject.Data.Id, response.Messages[0]);
            return;
        }
        CurrentObject = null;
        GetComponent<SimpleSideMenu>().Close();
    }

    public void UpdateMenu() {
        if (currentFocusPoint >= 0)
            return;
        if (CurrentObject.ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(true);
            RobotsListsBlock.SetActive(true);
        } else if (CurrentObject.ActionObjectMetadata.ObjectModel != null) {
            UpdatePositionBlockVO.SetActive(true);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(true);
        } else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
        }

        RobotsList.GetComponent<DropdownRobots>().Init(OnRobotChanged);

        if (RobotsList.dropdownItems.Count > 0)
            OnRobotChanged(RobotsList.selectedText.text);
        else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
        }
        FocusObjectDoneButton.interactable = false;
        NextButton.interactable = false;
        PreviousButton.interactable = false;
    }

    private void OnRobotChanged(string robot_id) {
        EndEffectorList.dropdownItems.Clear();
        EndEffectorList.GetComponent<DropdownEndEffectors>().Init(robot_id);
    }
    

    public void UpdateObjectPosition() {
        if (RobotsList.dropdownItems.Count == 0 || EndEffectorList.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.Data.Id,
            RobotsList.selectedText.text, EndEffectorList.selectedText.text);
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
        if (RobotsList.dropdownItems.Count == 0 || EndEffectorList.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        try {
            await Base.GameManager.Instance.StartObjectFocusing(CurrentObject.Data.Id,
                RobotsList.selectedText.text, EndEffectorList.selectedText.text);
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
            await Base.GameManager.Instance.SavePosition(CurrentObject.Data.Id, currentFocusPoint);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to save current position", ex.Message);
        }

        
    }

    public async void FocusObjectDone() {
        try {
            await Base.GameManager.Instance.FocusObjectDone(CurrentObject.Data.Id);
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

}
