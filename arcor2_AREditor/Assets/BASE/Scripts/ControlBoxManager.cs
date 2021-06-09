using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.UI;

public class ControlBoxManager : Singleton<ControlBoxManager> {

    [SerializeField]
    private InputDialog InputDialog;
    [SerializeField]

    public Toggle TrackablesToggle;
    public Toggle ConnectionsToggle;
    public Toggle VRModeToggle;
    public Toggle CalibrationElementsToggle;
    public Toggle AutoCalibToggle;

    public AddActionPointUsingRobotDialog AddActionPointUsingRobotDialog;

    private ManualTooltip calibrationElementsTooltip;
    private ManualTooltip autoCalibTooltip;



    private void Start() {
#if UNITY_ANDROID && AR_ON
        TrackablesToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_display_trackables", false);
        CalibrationElementsToggle.interactable = false;
        CalibrationElementsToggle.isOn = true;
        calibrationElementsTooltip = CalibrationElementsToggle.GetComponent<ManualTooltip>();
        calibrationElementsTooltip.DisplayAlternativeDescription = true;


        bool useAutoCalib = PlayerPrefsHelper.LoadBool("control_box_autoCalib", true);

        autoCalibTooltip = AutoCalibToggle.GetComponent<ManualTooltip>();
        autoCalibTooltip.DisplayAlternativeDescription = useAutoCalib;

        // If the toggle is unchanged, we need to manually call the EnableAutoReCalibration function.
        // If the toggle has changed, the function will be called automatically. So we need to avoid calling it twice.
        if ((AutoCalibToggle.isOn && useAutoCalib) || (!AutoCalibToggle.isOn && !useAutoCalib)) {
            AutoCalibToggle.isOn = useAutoCalib;
            EnableAutoReCalibration(useAutoCalib);
        } else {
            AutoCalibToggle.isOn = useAutoCalib;
        }

#endif
        ConnectionsToggle.isOn = PlayerPrefsHelper.LoadBool("control_box_display_connections", true);
        GameManager.Instance.OnGameStateChanged += GameStateChanged;
    }


    private void OnEnable() {
        CalibrationManager.Instance.OnARCalibrated += OnARCalibrated;
        CalibrationManager.Instance.OnARRecalibrate += OnARRecalibrate;
    }

    private void OnDisable() {
        CalibrationManager.Instance.OnARCalibrated -= OnARCalibrated;
        CalibrationManager.Instance.OnARRecalibrate -= OnARRecalibrate;
    }

    

    /// <summary>
    /// Triggered when the system calibrates = anchor is created (either when user clicks on calibration cube or when system loads the cloud anchor).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnARCalibrated(object sender, CalibrationEventArgs args) {
#if UNITY_ANDROID && AR_ON
        // Activate toggle to enable hiding/displaying calibration cube
        CalibrationElementsToggle.interactable = true;
        calibrationElementsTooltip.DisplayAlternativeDescription = false;
#endif
    }


    private void OnARRecalibrate(object sender, EventArgs args) {
#if UNITY_ANDROID && AR_ON
        // Disactivate toggle to disable hiding/displaying calibration cube
        CalibrationElementsToggle.interactable = false;
        calibrationElementsTooltip.DisplayAlternativeDescription = true;
#endif
    }


    /// <summary>
    /// Called when the user tries to click on the show/hide toggle before the system is calibrated.
    /// </summary>
    public void OnCalibrationElementsToggleClick() {
        if (!CalibrationManager.Instance.Calibrated) {
            if (CalibrationManager.Instance.UsingServerCalibration) {
                Notifications.Instance.ShowNotification("System is not calibrated", "Please locate the visual marker and wait for the calibration to complete automatically.");
            } else {
                Notifications.Instance.ShowNotification("System is not calibrated", "Please locate the visual marker, wait for the calibration cube to show up and click on it, in order to calibrate the system.");
            }
        }
    }

    private void GameStateChanged(object sender, GameStateEventArgs args) {
        ConnectionsToggle.gameObject.SetActive(false);
        switch (args.Data) {

            case GameManager.GameStateEnum.ProjectEditor:
                
                ConnectionsToggle.gameObject.SetActive(true);
                break;
            case GameManager.GameStateEnum.SceneEditor:
                
                break;
            case GameManager.GameStateEnum.PackageRunning:
                ConnectionsToggle.gameObject.SetActive(true);
                break;
        }

    }

    public void DisplayTrackables(bool active) {
#if UNITY_ANDROID && AR_ON
        TrackingManager.Instance.DisplayPlanesAndPointClouds(active);
#endif
    }

    public void DisplayCalibrationElements(bool active) {
#if UNITY_ANDROID && AR_ON
        CalibrationManager.Instance.ActivateCalibrationElements(active);
#endif
    }

    public void ToggleVRMode(bool active) {
        if (active) {
            VRModeManager.Instance.EnableVRMode();
        } else {
            VRModeManager.Instance.DisableVRMode();
        }
    }

    public void ShowEditorSettingsMenu() {
        MenuManager.Instance.ShowMenu(MenuManager.Instance.EditorSettingsMenu);
    }

    public void DisplayConnections(bool active) {
        ConnectionManagerArcoro.Instance.DisplayConnections(active);
    }

    public void EnableAutoReCalibration(bool active) {
#if UNITY_ANDROID && AR_ON
        autoCalibTooltip.DisplayAlternativeDescription = active;
        CalibrationManager.Instance.EnableAutoReCalibration(active);
#endif
    }

    private void OnDestroy() {
#if UNITY_ANDROID && AR_ON
        PlayerPrefsHelper.SaveBool("control_box_display_trackables", TrackablesToggle.isOn);
        PlayerPrefsHelper.SaveBool("control_box_autoCalib", AutoCalibToggle.isOn);
#endif
        PlayerPrefsHelper.SaveBool("control_box_display_connections", ConnectionsToggle.isOn);
    }
}
