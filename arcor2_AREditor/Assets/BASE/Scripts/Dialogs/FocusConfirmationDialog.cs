using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class FocusConfirmationDialog : MonoBehaviour
{
    public string RobotId, EndEffectorId, OrientationId, OrientationName, ActionPointId, ActionPointName;
    public bool UpdatePosition;
    public TMPro.TMP_Text SettingsText;
    public ModalWindowManager WindowManager;

    public void Init() {
        SettingsText.text = "Robot: " + RobotId +
            "\nEnd effector: " + EndEffectorId +
            "\nOrientation: " + OrientationName +
            "\nAction point: " + ActionPointName +
            "\nUpdate position: " + UpdatePosition.ToString();

    }

    public void UpdatePositionOrientation() {
        try {
            if (EndEffectorId == "") {
                Base.GameManager.Instance.UpdateActionPointJoints(RobotId, OrientationId);
            } else {
                if (UpdatePosition)
                    Base.GameManager.Instance.UpdateActionPointPositionUsingRobot(ActionPointId, RobotId, EndEffectorId);
                Base.GameManager.Instance.UpdateActionPointOrientationUsingRobot(ActionPointId, RobotId, EndEffectorId, OrientationId);
            }
            GetComponent<ModalWindowManager>().CloseWindow();
            MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update", ex.Message);
        }
    }
}
