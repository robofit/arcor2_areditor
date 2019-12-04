using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;

public class ActionObjectMenu : MonoBehaviour {
    public GameObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab;
    public CustomDropdown RobotsList, EndEffectorList;
    public Button NextButton, PreviousButton, FocusObjectDoneButton, StartObjectFocusingButton, SavePositionButton;
    public TMPro.TMP_Text CurrentPointLabel;
    public GameObject RobotsListsBlock, UpdatePositionBlockMesh, UpdatePositionBlockVO;

    private int currentFocusPoint = -1;

    public void SaveID(string new_id) {
        CurrentObject.GetComponent<Base.ActionObject>().Data.Id = new_id;
        Base.GameManager.Instance.UpdateScene();
    }

    public async void DeleteIO() {
        IO.Swagger.Model.RemoveFromSceneResponse response = await Base.GameManager.Instance.RemoveFromScene(CurrentObject.GetComponent<Base.ActionObject>().Data.Id);
        if (!response.Result) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to remove object " + CurrentObject.GetComponent<Base.ActionObject>().Data.Id, response.Messages[0]);
            return;
        }
        CurrentObject = null;
        GetComponent<SimpleSideMenu>().Close();
    }

    public void UpdateMenu() {
        if (currentFocusPoint >= 0)
            return;
        if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(true);
            RobotsListsBlock.SetActive(true);
        } else if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel != null) {
            UpdatePositionBlockVO.SetActive(true);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(true);
        } else {
            UpdatePositionBlockVO.SetActive(false);
            UpdatePositionBlockMesh.SetActive(false);
            RobotsListsBlock.SetActive(false);
        }

        RobotsList.transform.parent.GetComponent<DropdownParameter>().PutData(Base.ActionsManager.Instance.GetRobots(), "",
            () => OnRobotChanged(RobotsList.selectedText.text));

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

        /*Dropdown dropdown, endEffectorDropdown;
        if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
            dropdown = robotsList.GetComponent<Dropdown>();
            endEffectorDropdown = endEffectorList.GetComponent<Dropdown>();
        } else if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel != null) {
            dropdown = robotsListVO.GetComponent<Dropdown>();
            endEffectorDropdown = endEffectorListVO.GetComponent<Dropdown>();
        } else {
            dropdown = null;
            endEffectorDropdown = null;
        }
        if (dropdown != null) {
            dropdown.options.Clear();
            dropdown.captionText.text = "";
            foreach (Base.ActionObject actionObject in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    Dropdown.OptionData option = new Dropdown.OptionData {
                        text = actionObject.Data.Id
                    };
                    dropdown.options.Add(option);
                }
            }
            dropdown.value = 0;
        }

        if (dropdown?.options.Count > 0) {
            dropdown.captionText.text = dropdown.options[dropdown.value].text;
            if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
                EnableFocusControls();
                UpdatePositionBlockMesh.SetActive(true);
                UpdatePositionBlockVO.SetActive(false);
            } else if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel != null) {
                UpdatePositionBlockMesh.SetActive(false);
                UpdatePositionBlockVO.SetActive(true);
            } else {
                UpdatePositionBlockMesh.SetActive(false);
                UpdatePositionBlockVO.SetActive(false);
            }

        } else {
            
            DisableFocusControls();
            UpdatePositionBlockMesh.SetActive(false);
            UpdatePositionBlockVO.SetActive(false);
        }

        /*
        endEffectorDropdown.options.Clear();
        endEffectorDropdown.captionText.text = "EE_Big";
        endEffectorDropdown.value = 0;
        endEffectorDropdown.options.Add(new Dropdown.OptionData {
            text = "EE_Big"
        });
        endEffectorDropdown.options.Add(new Dropdown.OptionData {
            text = "EE_Small"
        });*/

        //ActionsManager.Instance.ActionObjectMetadata
        //if (CurrentObject.GetComponent<ActionObject2D>().Data)*/
    }

    private void OnRobotChanged(string robot_id) {
        UpdateEndEffectorList(robot_id);
    }

    public void UpdateEndEffectorList(Base.ActionObject robot) {
        EndEffectorList.dropdownItems.Clear();
        EndEffectorList.transform.parent.GetComponent<DropdownParameter>().PutData(robot.EndEffectors, "", null);
    }

    public void UpdateEndEffectorList(string robot_id) {
        foreach (Base.ActionObject actionObject in Base.GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
            if (actionObject.Data.Id == robot_id) {
                UpdateEndEffectorList(actionObject);
            }
        }
    }

    public void UpdateObjectPosition() {
        if (RobotsList.dropdownItems.Count == 0 || EndEffectorList.dropdownItems.Count == 0) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update object position", "No robot or end effector available");
            return;
        }
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>().Data.Id,
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
            await Base.GameManager.Instance.StartObjectFocusing(CurrentObject.GetComponent<Base.ActionObject>().Data.Id,
                RobotsList.selectedText.text, EndEffectorList.selectedText.text);
            currentFocusPoint = 0;
            UpdateCurrentPointLabel();
            GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = false;
            GetComponent<SimpleSideMenu>().overlayCloseOnPressed = false;
            FocusObjectDoneButton.interactable = true;
            if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count > 1) {
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
            await Base.GameManager.Instance.SavePosition(CurrentObject.GetComponent<Base.ActionObject>().Data.Id, currentFocusPoint);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to save current position", ex.Message);
        }

        
    }

    public async void FocusObjectDone() {
        try {
            await Base.GameManager.Instance.FocusObjectDone(CurrentObject.GetComponent<Base.ActionObject>().Data.Id);
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
        currentFocusPoint = Math.Min(currentFocusPoint + 1, CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1);
        PreviousButton.interactable = true;
        if (currentFocusPoint == CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count - 1) {
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
        CurrentPointLabel.text = "Point " + (currentFocusPoint + 1) + " out of " + CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.ObjectModel.Mesh.FocusPoints.Count.ToString();
    }

}
