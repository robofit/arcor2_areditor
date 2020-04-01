using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackingManager : Singleton<TrackingManager> {

    private void Start() {
        // We want to display notifications about tracking only when the camera feed is on screen (only in project or scene edit).
        GameManager.Instance.OnOpenProjectEditor += StartTrackingNotifications;
        GameManager.Instance.OnOpenSceneEditor += StartTrackingNotifications;

        // We want to stop notifications everywhere else.
        GameManager.Instance.OnCloseProject += StopTrackingNotifications;
        GameManager.Instance.OnCloseScene += StopTrackingNotifications;        
    }

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

}
