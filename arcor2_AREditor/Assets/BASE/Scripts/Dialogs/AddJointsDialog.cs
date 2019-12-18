using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AddJointsDialog : MonoBehaviour {
    public GameObject NewJointsName;
    public Base.ActionPoint ap;
    public string RobotId;

    public void AddJoints() {
        if (ap == null) {
            Base.NotificationsModernUI.Instance.ShowNotification("System error", "Action point not set!");
            return;
        }
        string joints_id = NewJointsName.GetComponent<TMPro.TMP_InputField>().text;
        if (ap.GetJoints().ContainsKey(joints_id)) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed", "Joints named " + joints_id + " already exists");
            return;
        }
        IO.Swagger.Model.RobotJoints robotJoints = new IO.Swagger.Model.RobotJoints(id: joints_id, isValid: false, joints: new List<IO.Swagger.Model.Joint>(), RobotId);

        
        ap.Data.RobotJoints.Add(robotJoints);
        Base.GameManager.Instance.UpdateProject();
        NewJointsName.GetComponent<TMPro.TMP_InputField>().text = "";
        GetComponent<ModalWindowManager>().CloseWindow();
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateJoints();
    }
}
