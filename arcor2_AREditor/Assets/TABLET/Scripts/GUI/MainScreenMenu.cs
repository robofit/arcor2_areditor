using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MainScreenMenu : MonoBehaviour
{
    public TMPro.TMP_Text ConnectionString;

    public Toggle CloudAnchorToggle;
    public Toggle ServerCalibrationToggle;

    public GameObject EditorSettingsAutoCalibToggle;

    private void Start() {
        Debug.Assert(ConnectionString != null);
        CloudAnchorToggle.isOn = Settings.Instance.UseCloudAnchors;
        Base.GameManager.Instance.OnConnectedToServer += OnConnectedToServer;

        bool useServerCalib = PlayerPrefsHelper.LoadBool("UseServerCalibration", true);
        // If the toggle is unchanged, we need to manually call the UseServerCalibration function.
        // If the toggle has changed, the function will be called automatically. So we need to avoid calling it twice.
        if ((ServerCalibrationToggle.isOn && useServerCalib) || (!ServerCalibrationToggle.isOn && !useServerCalib)) {
            ServerCalibrationToggle.isOn = useServerCalib;
            UseServerCalibration(useServerCalib);
        } else {
            ServerCalibrationToggle.isOn = useServerCalib;
        }
    }

    private void OnConnectedToServer(object sender, Base.StringEventArgs eventArgs) {
        ConnectionString.text = eventArgs.Data;
    }

    public void UseServerCalibration(bool useServer) {
        CalibrationManager.Instance.UseServerCalibration(useServer);
        EditorSettingsAutoCalibToggle.gameObject.SetActive(useServer);
    }

    private void OnDestroy() {
        PlayerPrefsHelper.SaveBool("UseServerCalibration", ServerCalibrationToggle.isOn);
    }
}
