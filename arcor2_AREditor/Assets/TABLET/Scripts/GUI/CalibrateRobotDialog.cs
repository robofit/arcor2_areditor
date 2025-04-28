using System.Collections.Generic;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;

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
                var response = await CommunicationManager.Instance.Client.CalibrateRobotAsync(new CalibrateRobotRequestArgs(robotId, camera.Data.Id, (bool) Switch.GetValue()));
                if (!response.Result) {
                    GameManager.Instance.HideLoadingScreen();
                    Notifications.Instance.ShowNotification("Failed to calibrate robot",
                            string.Join(',', response.Messages));
                }
            } catch (Arcor2ConnectionException ex) {
                GameManager.Instance.HideLoadingScreen();
                Notifications.Instance.ShowNotification("Failed to calibrate robot", ex.Message);
            } finally {
                Close();
            }
        }

    }

}
