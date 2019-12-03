using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class FocusConfirmationDialog : MonoBehaviour
{
    public string RobotId, EndEffectorId, OrientationId, ActionPointId;
    public bool UpdatePosition;
    public TMPro.TMP_Text SettingsText;

    public void Init() {
        SettingsText.text = "Robot: " + RobotId +
            "\nEnd effector: " + EndEffectorId +
            "\nOrientation: " + OrientationId +
            "\nAction point: " + ActionPointId +
            "\nUpdate position: " + UpdatePosition.ToString();

    }

    public void UpdatePositionOrientation() {
        try {
            Base.GameManager.Instance.UpdateActionPointPosition(ActionPointId, RobotId, EndEffectorId, OrientationId, UpdatePosition);
            GetComponent<ModalWindowManager>().CloseWindow();
        } catch (Base.RequestFailedException ex) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to update position", ex.Message);
        }
    }
}
