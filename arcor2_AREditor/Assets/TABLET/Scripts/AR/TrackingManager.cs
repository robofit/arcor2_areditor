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

    public VideoPlayerImage TrackingLostAnimation;

    public Material PlaneTransparentMaterial;
    public Material PlaneOpaqueMaterial;

    private bool planesAndFeaturesActive = true;

    private Coroutine trackingFailureNotify;

    //private float counter = 5f;

    public enum TrackingQuality {
        NOT_TRACKING = 0,
        POOR_QUALITY = 1,
        GOOD_QUALITY = 2
    }

    private void Start() {
#if UNITY_ANDROID && !UNITY_EDITOR
        // We want to display notifications about tracking only when the camera feed is on screen (only in project or scene edit).
        GameManager.Instance.OnOpenProjectEditor += StartTrackingNotifications;
        GameManager.Instance.OnOpenSceneEditor += StartTrackingNotifications;
        GameManager.Instance.OnRunPackage += StartTrackingNotifications;

        // We want to stop notifications everywhere else.
        GameManager.Instance.OnCloseProject += StopTrackingNotifications;
        GameManager.Instance.OnCloseScene += StopTrackingNotifications;

        ARPlaneManager.planesChanged += OnPlanesChanged;
        ARPointCloudManager.pointCloudsChanged += OnPointCloudChanged;
#endif
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
        //Notifications.Instance.ShowNotification("Tracking state", ARSession.state.ToString());
    }

    private void StopTrackingNotifications(object sender, EventArgs e) {
        ARSession.stateChanged -= ARSessionStateChanged;
        if (trackingFailureNotify != null) {
            StopCoroutine(trackingFailureNotify);
        }
    }

    private void ARSessionStateChanged(ARSessionStateChangedEventArgs sessionState) {
        // cancel previously invoked tracking failure notification
        if (trackingFailureNotify != null) {
            StopCoroutine(trackingFailureNotify);
            TrackingLostAnimation.StopVideo();
            trackingFailureNotify = null;
        }

        switch (sessionState.state) {
            case ARSessionState.Unsupported:
                Notifications.Instance.ShowNotification("Tracking not supported", "This device does not support ARCore!");
                break;
            default:
                Notifications.Instance.ShowNotification("Tracking state", sessionState.state.ToString());
                if (sessionState.state != ARSessionState.SessionTracking) {
                    NotTrackingReason trackingFailureReason = ARSession.notTrackingReason;
                    switch (trackingFailureReason) {
                        case NotTrackingReason.InsufficientFeatures:
                            trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to insufficient features!", "Try to move the device slowly around your environment.", 9f));
                            TrackingLostAnimation.PlayVideo();
                            break;
                        case NotTrackingReason.ExcessiveMotion:
                            trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to excessive motion!", "You are moving the device too fast.", 9f));
                            TrackingLostAnimation.PlayVideo();
                            break;
                        case NotTrackingReason.InsufficientLight:
                            trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to insufficient light!", "Enlight your environment.", 9f));
                            break;
                        case NotTrackingReason.None:
                            // ingnore notification when tracking was lost for no reason
                            break;
                        default:
                            Notifications.Instance.ShowNotification("Tracking lost!", "Reason: " + trackingFailureReason.ToString());
                            // notify user ever 9 seconds about tracking failure
                            trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost!", "Reason: " + trackingFailureReason.ToString(), 9f));
                            break;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Displays user notification on display. If repeatCount is set to -1, notifications will keep displaying until Coroutine is stopped manually.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="text"></param>
    /// <param name="repeatRate"></param>
    /// <param name="repeatCount"></param>
    /// <returns></returns>
    private IEnumerator TrackingFailureNotify(string title, string text, float repeatRate, int repeatCount = -1) {
        int repeat = repeatCount;
        while (repeatCount == -1 ? true : repeat >= 0) {
            repeat -= 1;
            Notifications.Instance.ShowNotification(title, text);
            yield return new WaitForSeconds(repeatRate);
        }
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

    public void ChangePlaneTransparency(bool transparent) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        foreach (ARPlane plane in ARPlaneManager.trackables) {
            Renderer ren = plane.GetComponent<Renderer>();
            if (transparent) {
                ren.sharedMaterials = new Material[1] { PlaneTransparentMaterial };
            } else {
                ren.sharedMaterials = new Material[1] { PlaneOpaqueMaterial };
            }
        }
#endif
    }

}
