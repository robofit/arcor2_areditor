using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackingManager : Singleton<TrackingManager> {

    public ARPlaneManager ARPlaneManager;
    public ARPointCloudManager ARPointCloudManager;

    private bool planesAndFeaturesActive = true;

    //private float counter = 5f;

    public enum TrackingQuality {
        NOT_TRACKING = 0,
        POOR_QUALITY = 1,
        GOOD_QUALITY = 2
    }

    private void Start() {
        // We want to display notifications about tracking only when the camera feed is on screen (only in project or scene edit).
        GameManager.Instance.OnOpenProjectEditor += StartTrackingNotifications;
        GameManager.Instance.OnOpenSceneEditor += StartTrackingNotifications;

        // We want to stop notifications everywhere else.
        GameManager.Instance.OnCloseProject += StopTrackingNotifications;
        GameManager.Instance.OnCloseScene += StopTrackingNotifications;

        ARPlaneManager.planesChanged += OnPlanesChanged;
        ARPointCloudManager.pointCloudsChanged += OnPointCloudChanged;
    }

    private void OnPointCloudChanged(ARPointCloudChangedEventArgs obj) {
        DisplayPlanesAndFeatures(planesAndFeaturesActive);
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs obj) {
        DisplayPlanesAndFeatures(planesAndFeaturesActive);
    }

    //private void Update() {

    //    counter -= Time.deltaTime;

    //    if (counter <= 0f) {
    //        GetTrackingQuality();
    //        counter = 5f;
    //    }
    //}

    private void StartTrackingNotifications(object sender, EventArgs e) {
        ARSession.stateChanged += ARSessionStateChanged;
        Notifications.Instance.ShowNotification("Tracking state", ARSession.state.ToString());
    }

    private void StopTrackingNotifications(object sender, EventArgs e) {
        ARSession.stateChanged -= ARSessionStateChanged;
        CancelInvoke("TrackingFailureNotify");
    }

    private void ARSessionStateChanged(ARSessionStateChangedEventArgs sessionState) {
        // cancel previously invoked tracking failure notification
        CancelInvoke("TrackingFailureNotify");

        switch (sessionState.state) {
            case ARSessionState.Unsupported:
                Notifications.Instance.ShowNotification("Tracking not supported", "This device does not support ARCore!");
                break;
            default:
                Notifications.Instance.ShowNotification("Tracking state", sessionState.state.ToString());
                if (sessionState.state != ARSessionState.SessionTracking) {
                    NotTrackingReason trackingFailureReason = ARSession.notTrackingReason;
                    if (trackingFailureReason != NotTrackingReason.None) {
                        Notifications.Instance.ShowNotification("Tracking lost!", "Reason: " + trackingFailureReason.ToString());
                        // notify user ever 9 seconds about tracking failure
                        InvokeRepeating("TrackingFailureNotify", 9f, 9f);
                    }
                }
                break;
        }
    }

    private void TrackingFailureNotify() {
        Notifications.Instance.ShowNotification("Tracking lost!", "Reason: " + ARSession.notTrackingReason.ToString());
    }

    public TrackingQuality GetTrackingQuality() {
        int featurePoints = 0;

        if (ARSession.state != ARSessionState.SessionTracking) {
            return TrackingQuality.NOT_TRACKING;
        }

        // TODO Decide how to resolve pointCloud
        foreach (ARPointCloud pointCloud in ARPointCloudManager.trackables) {
            featurePoints += (pointCloud.identifiers?.Length == null ? 0 : (int) pointCloud.identifiers?.Length);
        }

        //Notifications.Instance.ShowNotification("Tracking quality", "Feature points: " + featurePoints + " Planes: " + ARPlaneManager.trackables.count);
        Debug.Log("Tracking quality" + " Feature points: " + featurePoints + " Planes: " + ARPlaneManager.trackables.count);

        // Need to have at least one plane and more than zero feature points
        if (ARPlaneManager.trackables.count >= 1 && featurePoints > 0) {
            return TrackingQuality.GOOD_QUALITY;
        }

        return TrackingQuality.POOR_QUALITY;
    }

    public void DisplayPlanesAndFeatures(bool active) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        planesAndFeaturesActive = active;

        foreach (ARPlane plane in ARPlaneManager.trackables) {
            plane.gameObject.SetActive(active);
        }

        foreach (ARPointCloud pointCloud in ARPointCloudManager.trackables) {
            pointCloud.gameObject.SetActive(active);
        }
#endif
    }

}
