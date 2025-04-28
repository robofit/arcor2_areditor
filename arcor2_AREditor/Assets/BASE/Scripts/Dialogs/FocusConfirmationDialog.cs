using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;

public class FocusConfirmationDialog : MonoBehaviour
{
    public string RobotName, ArmId, EndEffectorId, OrientationId, JointsId, OrientationName, ActionPointId, ActionPointName;
    public bool UpdatePosition;
    public TMP_Text SettingsText;
    public ModalWindowManager WindowManager;

    private string RobotId;

    public bool Init() {
        try {
            RobotId = SceneManager.Instance.RobotNameToId(RobotName);
        } catch (ItemNotFoundException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to load end effectors", "");
            return false;
        }

        SettingsText.text = "Robot: " + RobotName +
            "\nArm: " + ArmId +
            "\nEnd effector: " + EndEffectorId +
            "\nOrientation: " + OrientationName +
            "\nAction point: " + ActionPointName +
            "\nUpdate position: " + UpdatePosition.ToString();
        return true;
    }

    public async void UpdatePositionOrientation() {
        try {
            IRobot robot = SceneManager.Instance.GetRobot(RobotId);
            if (!robot.MultiArm())
                ArmId = null;
            if (UpdatePosition)
                GameManager.Instance.UpdateActionPointPositionUsingRobot(ActionPointId, RobotId, EndEffectorId, ArmId);
            
            var responseOrientation = await CommunicationManager.Instance.Client.UpdateActionPointOrientationUsingRobotAsync(new UpdateActionPointOrientationUsingRobotRequestArgs(OrientationId, new RobotArg(RobotId, EndEffectorId, ArmId)));

            var responseJoints = await CommunicationManager.Instance.Client.UpdateActionPointJointsUsingRobotAsync(new UpdateActionPointJointsUsingRobotRequestArgs(JointsId));

            if (!responseOrientation.Result || !responseJoints.Result) {
                Notifications.Instance.ShowNotification("Failed to update", string.Join(',', responseOrientation.Messages));
                return;
            }

            GetComponent<ModalWindowManager>().CloseWindow();
        } catch (Arcor2ConnectionException ex) {
            NotificationsModernUI.Instance.ShowNotification("Failed to update", ex.Message);
        }
    }
}
