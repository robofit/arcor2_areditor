using System;
using Base;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddJointsMenu : MonoBehaviour {
    public Base.ActionPoint CurrentActionPoint;

    public TMPro.TMP_InputField NameInput;

    [SerializeField]
    private Button CreateNewJoints;

    [SerializeField]
    private TooltipContent buttonTooltip;

    private string jointsName;

    public async void UpdateMenu() {        
        ValidateFields();
    }


    public async void ValidateFields() {
        bool interactable = true;
        jointsName = NameInput.text;

        if (string.IsNullOrEmpty(jointsName)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        } else if (CurrentActionPoint.OrientationNameExist(jointsName) || CurrentActionPoint.JointsNameExist(jointsName)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + jointsName;
            interactable = false;
        }

        if (interactable) {
            if (!SceneManager.Instance.IsRobotSelected()) {
                interactable = false;
                buttonTooltip.description = "Robot is not selected";
            }
        }
        
        buttonTooltip.enabled = !interactable;
        CreateNewJoints.interactable = interactable;
    }

    public async void AddJoints() {
        
        Debug.Assert(CurrentActionPoint != null);
        try {
            string armId = null;
            if (SceneManager.Instance.SelectedRobot.MultiArm())
                armId = SceneManager.Instance.SelectedArmId;
            jointsName = NameInput.text;
            await Base.WebsocketManager.Instance.AddActionPointJoints(CurrentActionPoint.Data.Id, SceneManager.Instance.SelectedRobot.GetId(), jointsName, SceneManager.Instance.SelectedEndEffector.EEId, armId);
            Notifications.Instance.ShowToastMessage("Joints added successfully");
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to add joints", ex.Message);
            return;
        }
        Close();
        
    }

    /// <summary>
    /// Opens menu for adding joints
    /// </summary>
    /// <param name="actionPoint"></param>
    public void ShowMenu(Base.ActionPoint actionPoint) {
        CurrentActionPoint = actionPoint;
        NameInput.text = CurrentActionPoint.GetFreeOrientationName();

        UpdateMenu();
        gameObject.SetActive(true);
    }

    public void Close() {
        ActionPointAimingMenu.Instance.SwitchToJoints();
    }
}
