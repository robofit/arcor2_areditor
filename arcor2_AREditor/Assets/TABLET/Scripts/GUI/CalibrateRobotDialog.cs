using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class CalibrateRobotDialog : Dialog {

    public DropdownParameter Dropdown;
    public SwitchComponent Switch;
    private string robotId;

    public void Init(List<string> cameraNames, string robotId) {
        if (cameraNames.Count == 0) {
            Notifications.Instance.ShowNotification("Calibration failed", "Could not calibrate robot wihtout camera");
            Close();
        }
        Switch.Switch.isOn = false;
        Dropdown.PutData(cameraNames, "", null);
        this.robotId = robotId;
    }



    public async new Task Confirm() {
        string cameraName = (string) Dropdown.GetValue();
        if (SceneManager.Instance.TryGetActionObjectByName(cameraName, out ActionObject camera)) {
            try {
                await WebsocketManager.Instance.CalibrateRobot(robotId, camera.Data.Id, (bool) Switch.GetValue());
                ToastMessage.Instance.ShowMessage("Robot calibrated", 5);
            } catch (RequestFailedException ex) {
                Notifications.Instance.ShowNotification("Failed to calibrate robot", ex.Message);
            } finally {
                Close();
            }
        }      
        
    }

}
