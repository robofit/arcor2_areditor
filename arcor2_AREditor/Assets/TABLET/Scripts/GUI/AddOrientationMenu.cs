using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ActionPoint = Base.ActionPoint;

public class AddOrientationMenu : MonoBehaviour {
    public ActionPoint CurrentActionPoint;

    public TMP_InputField NameInput;// QuaternionX, QuaternionY, QuaternionZ, QuaternionW;
    public GameObject ManualModeBlock;
    public bool ManualMode;

    public OrientationManualEdit OrientationManualEdit;

    [SerializeField]
    private Button CreateNewOrientation;

    [SerializeField]
    private TooltipContent buttonTooltip;


    public async void UpdateMenu() {

        ValidateFields();
    }

    public async void ValidateFields() {
        bool interactable = true;
        name = NameInput.text;

        if (string.IsNullOrEmpty(name)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        } else if (CurrentActionPoint.OrientationNameExist(name) || CurrentActionPoint.JointsNameExist(name)) {
            buttonTooltip.description = "There already exists orientation or joints with name " + name;
            interactable = false;
        }

        if (ManualMode) {
            if (interactable) {
                buttonTooltip.description = OrientationManualEdit.ValidateFields();
                if (!string.IsNullOrEmpty(buttonTooltip.description)) {
                    interactable = false;
                }
            }
        } else {
            if (interactable) {
                if (!SceneManager.Instance.IsRobotSelected()) {
                    interactable = false;
                    buttonTooltip.description = "There is no robot to be used";
                }
            }
        }
        buttonTooltip.enabled = !interactable;
        CreateNewOrientation.interactable = interactable;
    }

    public async void AddOrientation() {
        Debug.Assert(CurrentActionPoint != null);


        string name = NameInput.text;
        try {

            if (ManualMode) {
                Orientation orientation = OrientationManualEdit.GetOrientation();
                var response = await CommunicationManager.Instance.Client.AddActionPointOrientationAsync(new AddActionPointOrientationRequestArgs(CurrentActionPoint.Data.Id, orientation, name));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to add new orientation", string.Join(',', response.Messages));
                    return;
                }
            } else { //using robot

                string armId = null;
                if (SceneManager.Instance.SelectedRobot.MultiArm())
                    armId = SceneManager.Instance.SelectedArmId;
                var response = await CommunicationManager.Instance.Client.AddActionPointOrientationUsingRobotAsync(new AddActionPointOrientationUsingRobotRequestArgs(CurrentActionPoint.Data.Id, new RobotArg(SceneManager.Instance.SelectedRobot.GetId(), SceneManager.Instance.SelectedEndEffector.GetName(), armId), name));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to add new orientation", string.Join(',', response.Messages));
                    return;
                }
            }
            Close(); //close add menu
            Notifications.Instance.ShowToastMessage("Orientation added successfully");
        } catch (ItemNotFoundException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to add new orientation", ex.Message);
        }
    }

    public void ShowMenu(ActionPoint actionPoint, bool manualMode) {
        ManualMode = manualMode;
        CurrentActionPoint = actionPoint;

        ManualModeBlock.SetActive(ManualMode);

        NameInput.text = CurrentActionPoint.GetFreeOrientationName();
        OrientationManualEdit.SetOrientation(new Orientation());
        UpdateMenu();
        gameObject.SetActive(true);
    }

    public void Close() {
        ActionPointAimingMenu.Instance.SwitchToOrientations();
    }
}
