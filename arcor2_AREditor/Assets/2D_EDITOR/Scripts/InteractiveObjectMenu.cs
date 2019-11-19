using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;

public class InteractiveObjectMenu : MonoBehaviour {
    public GameObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab, robotsList, endEffectorList, StartObjectFocusingButton,
        SavePositionButton, CurrentPointLabel, NextButton, PreviousButton, FocusObjectDoneButton, UpdatePositionBlockMesh, UpdatePositionBlockVO, robotsListVO, endEffectorListVO;

    int currentFocusPoint = -1;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        Base.GameManager.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>());

    }

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
        Dropdown dropdown, endEffectorDropdown;
        if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
            dropdown = robotsList.GetComponent<Dropdown>();
            endEffectorDropdown = endEffectorList.GetComponent<Dropdown>();
        } else if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model != null) {
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
            if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model?.Type == IO.Swagger.Model.ObjectModel.TypeEnum.Mesh) {
                EnableFocusControls();
                UpdatePositionBlockMesh.SetActive(true);
                UpdatePositionBlockVO.SetActive(false);
            } else if (CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model != null) {
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
        //if (CurrentObject.GetComponent<ActionObject2D>().Data)
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

    public void UpdateActionPointPosition() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown dropdownEE = endEffectorList.GetComponent<Dropdown>();
        Base.GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }

    public async void StartObjectFocusing() {
        Dropdown robotList = robotsList.GetComponent<Dropdown>();
        Dropdown eeList = endEffectorList.GetComponent<Dropdown>();
        string robotId = robotList.options[robotList.value].text;
        string endEffector = eeList.options[eeList.value].text;
        IO.Swagger.Model.FocusObjectStartResponse response = await Base.GameManager.Instance.StartObjectFocusing(CurrentObject.GetComponent<Base.ActionObject>().Data.Id, robotId, endEffector);
        if (response.Result) {
            currentFocusPoint = 0;
            UpdateCurrentPointLabel();
            GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = false;
            GetComponent<SimpleSideMenu>().overlayCloseOnPressed = false;
        } else {
            CurrentPointLabel.GetComponent<Text>().text = "";
            GUIHelpers2D.Instance.ShowNotification("Failed to start object focusing: " + (response.Messages.Count > 0 ? response.Messages[0] : ""));
            GetComponent<SimpleSideMenu>().handleToggleStateOnPressed = true;
            GetComponent<SimpleSideMenu>().overlayCloseOnPressed = true;
        }

    }

    public void SavePosition() {
        if (currentFocusPoint < 0)
            return;
        Base.GameManager.Instance.SavePosition(CurrentObject.GetComponent<Base.ActionObject>().Data.Id, currentFocusPoint);
    }

    public void FocusObjectDone() {
        Base.GameManager.Instance.FocusObjectDone(CurrentObject.GetComponent<Base.ActionObject>().Data.Id);
        CurrentPointLabel.GetComponent<Text>().text = "";
    }

    public void NextPoint() {
        currentFocusPoint = Math.Min(currentFocusPoint + 1, CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count - 1);
        if (currentFocusPoint == CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count - 1) {
            NextButton.GetComponent<Button>().interactable = false;
        } else {
            NextButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    public void PreviousPoint() {
        currentFocusPoint = Math.Max(currentFocusPoint - 1, 0);
        if (currentFocusPoint == 0) {
            PreviousButton.GetComponent<Button>().interactable = false;
        } else {
            PreviousButton.GetComponent<Button>().interactable = true;
        }
        UpdateCurrentPointLabel();
    }

    private void UpdateCurrentPointLabel() {
        CurrentPointLabel.GetComponent<Text>().text = "Point " + (currentFocusPoint + 1) + " out of " + CurrentObject.GetComponent<Base.ActionObject>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count.ToString();
    }

}
