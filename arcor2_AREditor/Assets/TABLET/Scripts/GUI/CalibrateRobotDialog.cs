using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class CalibrateRobotDialog : Dialog {

    public DropdownParameter Dropdown;
    public SwitchComponent Switch;
    private string robotId;

    public bool Init(List<string> cameraNames, string robotId) {
        if (cameraNames.Count == 0) {
            Notifications.Instance.ShowNotification("Calibration failed", "Could not calibrate robot wihtout camera");
            Close();
            return false;
        }
        Switch.SetValue(false);
        Dropdown.PutData(cameraNames, "", null);
        this.robotId = robotId;
        return true;
    }



    public async override void Confirm() {
        string cameraName = (string) Dropdown.GetValue();
        if (SceneManager.Instance.TryGetActionObjectByName(cameraName, out ActionObject camera)) {
            try {
                GameManager.Instance.ShowLoadingScreen("Calibrating robot...");
                await WebsocketManager.Instance.CalibrateRobot(robotId, camera.Data.Id, (bool) Switch.GetValue());
            } catch (RequestFailedException ex) {
                GameManager.Instance.HideLoadingScreen();
                Notifications.Instance.ShowNotification("Failed to calibrate robot", ex.Message);
            } finally {
                Close();
            }
        }      
        
    }

}
