using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class FocusConfirmationDialog : MonoBehaviour
{
    public string RobotId, EndEffectorId, OrientationId, ActionPointId, ActionPointUserId;
    public bool UpdatePosition;
    public TMPro.TMP_Text SettingsText;
    public ModalWindowManager WindowManager;

    public void Init() {
        SettingsText.text = "Robot: " + RobotId +
            "\nEnd effector: " + EndEffectorId +
            "\nOrientation: " + OrientationId +
            "\nAction point: " + ActionPointUserId +
            "\nUpdate position: " + UpdatePosition.ToString();

    }

    public void UpdatePositionOrientation() {
        try {
            if (EndEffectorId == "") {
                Base.GameManager.Instance.UpdateActionPointJoints(ActionPointId, RobotId, OrientationId);
            } else {
                Base.GameManager.Instance.UpdateActionPointPosition(ActionPointId, RobotId, EndEffectorId, OrientationId, UpdatePosition);
            }
            GetComponent<ModalWindowManager>().CloseWindow();
            MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update", ex.Message);
        }
    }
}
