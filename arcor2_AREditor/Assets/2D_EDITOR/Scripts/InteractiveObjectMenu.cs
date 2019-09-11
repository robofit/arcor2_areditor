using UnityEngine;
using UnityEngine.UI;
using System;

public class InteractiveObjectMenu : MonoBehaviour {
    public GameObject CurrentObject;
    [SerializeField]
    private GameObject aPPrefab, robotsList, endEffectorList, StartObjectFocusingButton,
        SavePositionButton, CurrentPointLabel, NextButton, PreviousButton, FocusObjectDoneButton;

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
        GameManager.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>());

    }

    public void SaveID(string new_id) {
        CurrentObject.GetComponent<Base.ActionObject>().Data.Id = new_id;
        GameManager.Instance.UpdateScene();
    }

    public void DeleteIO() {
        CurrentObject.GetComponent<Base.ActionObject>().DeleteIO();
        CurrentObject = null;
    }

    public void UpdateMenu() {
        Dropdown dropdown = robotsList.GetComponent<Dropdown>();
        Dropdown endEffectorDropdown = endEffectorList.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.captionText.text = "";
        foreach (Base.ActionObject actionObject in GameManager.Instance.ActionObjects.GetComponentsInChildren<Base.ActionObject>()) {
            if (actionObject.ActionObjectMetadata.Robot) {
                Dropdown.OptionData option = new Dropdown.OptionData {
                    text = actionObject.Data.Id
                };
                dropdown.options.Add(option);
            }
        }
        dropdown.value = 0;
        if (dropdown.options.Count > 0 && CurrentObject.GetComponent<ActionObject2D>().ActionObjectMetadata.Model?.Type == IO.Swagger.Model.MetaModel3d.TypeEnum.Mesh) {
            endEffectorDropdown.interactable = true;
            dropdown.interactable = true;
            EnableFocusControls();
            dropdown.captionText.text = dropdown.options[dropdown.value].text;
        } else {
            endEffectorDropdown.interactable = false;
            dropdown.interactable = false;            
            DisableFocusControls();
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
        GameManager.Instance.UpdateActionObjectPosition(CurrentObject.GetComponent<Base.ActionObject>(), dropdown.options[dropdown.value].text, dropdownEE.options[dropdownEE.value].text);
    }

    public void StartObjectFocusing() {
        Dropdown robotList = robotsList.GetComponent<Dropdown>();
        Dropdown eeList = endEffectorList.GetComponent<Dropdown>();
        string robotId = robotList.options[robotList.value].text;
        string endEffector = eeList.options[eeList.value].text;
        GameManager.Instance.StartObjectFocusing(CurrentObject.GetComponent<ActionObject2D>().Data.Id, robotId, endEffector);
        currentFocusPoint = 0;
        UpdateCurrentPointLabel();
    }

    public void SavePosition() {
        if (currentFocusPoint < 0)
            return;
        GameManager.Instance.SavePosition(CurrentObject.GetComponent<ActionObject2D>().Data.Id, currentFocusPoint);
    }

    public void FocusObjectDone() {
        GameManager.Instance.FocusObjectDone(CurrentObject.GetComponent<ActionObject2D>().Data.Id);
        CurrentPointLabel.GetComponent<Text>().text = "";
    }

    public void NextPoint() {
        currentFocusPoint = Math.Min(currentFocusPoint + 1, CurrentObject.GetComponent<ActionObject2D>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count - 1);
        if (currentFocusPoint == CurrentObject.GetComponent<ActionObject2D>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count - 1) {
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
        CurrentPointLabel.GetComponent<Text>().text = "Point " + (currentFocusPoint + 1) + " out of " + CurrentObject.GetComponent<ActionObject2D>().ActionObjectMetadata.Model.Mesh.FocusPoints.Count.ToString();
    }

}
