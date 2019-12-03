using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AddOrientationDialog : MonoBehaviour
{
    public GameObject NewOrientationName;
    public Base.ActionPoint ap;
    public void AddOrientation() {
        if (ap == null) {
            Base.NotificationsModernUI.Instance.ShowNotification("System error", "Action point not set!");
            return;
        }

        IO.Swagger.Model.NamedOrientation orientation = new IO.Swagger.Model.NamedOrientation(id: NewOrientationName.GetComponent<TMPro.TMP_InputField>().text, orientation: new IO.Swagger.Model.Orientation(w: 1, x: 0, y: 0, z: 0));
        if (ap.GetPoses().ContainsKey(orientation.Id)) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed", "Orientation named " + orientation.Id + " already exists");
            return;
        }
        ap.Data.Orientations.Add(orientation);
        Base.GameManager.Instance.UpdateProject();
        NewOrientationName.GetComponent<TMPro.TMP_InputField>().text = "";
        GetComponent<ModalWindowManager>().CloseWindow();
    }
}
