using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackingManager : Singleton<TrackingManager> {

    public ARPlaneManager ARPlaneManager;
    public ARPointCloudManager ARPointCloudManager;
    public ARTrackedImageManager ARTrackedImageManager;
    public ARAnchorManager ARAnchorManager;

    public VideoPlayerImage TrackingLostAnimation;

    public Material PlaneTransparentMaterial;
    public Material PlaneOpaqueMaterial;

    public event EventHandler<FloatEventArgs> NewLowestPlanePosition;

    private float lowestPlanePosition = 0f;

    private bool planesAndPointCloudsActive = true;
    private bool planesTransparent = true;

    private Coroutine trackingFailureNotify;
    private Coroutine trackingAnchorFailureNotify;
    private Transform mainCamera;

    private int i = 0;

    public float AnchorTrackingLostTimeout = 20f;
    private Coroutine trackingAnchorFailureTimeout;

    /// <summary>
    /// Info about the device tracking status.
    /// </summary>
    private DeviceTrackingStatus deviceTrackingStatus;

    /// <summary>
    /// Info about the world anchor tracking status.
    /// </summary>
    private AnchorTrackingStatus anchorTrackingStatus;

    public enum TrackingQuality {
        NOT_TRACKING = 0,
        POOR_QUALITY = 1,
        GOOD_QUALITY = 2
    }

    public enum DeviceTrackingStatus {
        Tracking = 0,
        WaitingForAnchor = 1,
        InsufficientLight = 2,
        InsufficientFeatures = 3,
        ExcessiveMotion = 4,
        UnknownFailure = 5,
        NotTracking = 6
    }

    public enum AnchorTrackingStatus {
        Tracking = 0,
        NotTracking = 1,
        NotCalibrated = 2
    }

    private void Start() {
#if UNITY_ANDROID && AR_ON
        // We want to display notifications about tracking only when the camera feed is on screen (only in project or scene edit).
        GameManager.Instance.OnOpenProjectEditor += StartTrackingNotifications;
        GameManager.Instance.OnOpenSceneEditor += StartTrackingNotifications;
        GameManager.Instance.OnRunPackage += StartTrackingNotifications;

        // We want to stop notifications everywhere else.
        GameManager.Instance.OnCloseProject += StopTrackingNotifications;
        GameManager.Instance.OnCloseScene += StopTrackingNotifications;
        GameManager.Instance.OnStopPackage += StopTrackingNotifications;

        CalibrationManager.Instance.OnARCalibrated += OnARCalibrated;

        ARPlaneManager.planesChanged += OnPlanesChanged;
        ARPointCloudManager.pointCloudsChanged += OnPointCloudChanged;

        mainCamera = Camera.main.transform;
        //ARTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        deviceTrackingStatus = DeviceTrackingStatus.NotTracking;
        anchorTrackingStatus = AnchorTrackingStatus.NotCalibrated;
#endif       
    }

    private void OnARCalibrated(object sender, CalibrationEventArgs args) {
        anchorTrackingStatus = args.Calibrated ? anchorTrackingStatus : AnchorTrackingStatus.NotCalibrated;
    }

    /// <summary>
    /// Called when anchors of the ARAnchorManager are changed (not tracking, tracking, added, etc.)
    /// </summary>
    /// <param name="obj"></param>
    private void OnAnchorsChanged(ARAnchorsChangedEventArgs obj) {
        // TODO: check if it is working for cloud anchors
        if (CalibrationManager.Instance.Calibrated) {
            if (!CalibrationManager.Instance.UsingCloudAnchors) {
                //if (obj.updated[obj.updated.IndexOf(CalibrationManager.Instance.WorldAnchorLocal)].trackingState == TrackingState.Tracking) {
                switch (CalibrationManager.Instance.WorldAnchorLocal.trackingState) {
                    case TrackingState.Tracking:
                        if (anchorTrackingStatus != AnchorTrackingStatus.Tracking) {
                            // cancel previously invoked tracking failure notification
                            StopTrackingFailureNotifications();
                            StopTrackingFailureTimeout();

                            anchorTrackingStatus = AnchorTrackingStatus.Tracking;
                            if (deviceTrackingStatus == DeviceTrackingStatus.WaitingForAnchor) {
                                Notifications.Instance.ShowNotification("Tracking state", "Session Tracking");
                                GameManager.Instance.SceneSetActive(true);
                                deviceTrackingStatus = DeviceTrackingStatus.Tracking;
                                StopTrackingFailureNotifications(true);
                            }
                        }
                        break;
                    case TrackingState.Limited:
                    case TrackingState.None:
                        // cancel previously invoked tracking failure notification
                        StopTrackingFailureNotifications();

                        trackingAnchorFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost!", "Look around your workspace.", 9f, anchorTrackingFailure: true));
                        TrackingLostAnimation.PlayVideo();
                        anchorTrackingStatus = AnchorTrackingStatus.NotTracking;
                        GameManager.Instance.SceneSetActive(false);
                        if (trackingAnchorFailureTimeout == null) {
                            trackingAnchorFailureTimeout = StartCoroutine(TrackingFailureTimeout(AnchorTrackingLostTimeout));
                        }
                        break;
                }
            }
        } else {
            anchorTrackingStatus = AnchorTrackingStatus.NotCalibrated;
        }
    }

    private void StopTrackingFailureNotifications(bool stopVideoOverride = false) {
        // cancel previously invoked tracking failure notification
        if (trackingAnchorFailureNotify != null) {
            StopCoroutine(trackingAnchorFailureNotify);
            // stop the video only if device is tracking and isn't in any state that demands playing the video (insufficient features and excessive motion)
            if (!(deviceTrackingStatus == DeviceTrackingStatus.InsufficientFeatures || deviceTrackingStatus == DeviceTrackingStatus.ExcessiveMotion)
                || stopVideoOverride) {
                TrackingLostAnimation.StopVideo();
            }
            trackingAnchorFailureNotify = null;
        }
    }

    private void StopTrackingFailureTimeout() {
        if (trackingAnchorFailureTimeout != null) {
            StopCoroutine(trackingAnchorFailureTimeout);
            trackingAnchorFailureTimeout = null;
        }
    }

    //private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs obj) {
    //    if (i > 1000) {
    //        foreach (ARTrackedImage image in obj.updated) {
    //            Quaternion rot = Quaternion.Inverse(mainCamera.transform.rotation) * transform.rotation;
    //            Debug.Log("Tracked image " + image.trackableId.ToString() +
    //                " has world position: " + image.transform.position.ToString("F8") +
    //                " , rotation in quaternions: " + image.transform.rotation.ToString("F8") +
    //                " and rotation in euler: " + image.transform.rotation.eulerAngles.ToString("F8") + "\n" +
    //                " Position relative to camera: " + mainCamera.InverseTransformPoint(image.transform.position).ToString("F8") +
    //                " , rotation in quaternions relative to camera: " + rot.ToString("F8") +
    //                " and rotation in euler relative to camera: " + rot.eulerAngles.ToString("F8"));
    //        }
    //        i = 0;
    //    } else {
    //        i += 1;
    //    }
    //}

    private void OnPointCloudChanged(ARPointCloudChangedEventArgs obj) {
        DisplayPointClouds(planesAndPointCloudsActive, obj.added);
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs obj) {
        DisplayPlanes(planesAndPointCloudsActive, obj.added);
        SetLowestPlane();
    }

    private void SetLowestPlane() {
        float lowestPlanePos = 0f;
        foreach (ARPlane plane in ARPlaneManager.trackables) {
            if (plane.gameObject.transform.position.y < lowestPlanePos) {
                lowestPlanePos = plane.gameObject.transform.position.y;
            }
        }
        if (lowestPlanePos < lowestPlanePosition) {
            NewLowestPlanePosition?.Invoke(this, new FloatEventArgs(lowestPlanePos));
            lowestPlanePosition = lowestPlanePos;
        }
    }

    /// <summary>
    /// Called when openning the scene/project.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StartTrackingNotifications(object sender, EventArgs e) {
        ARSession.stateChanged += ARSessionStateChanged;
        ARAnchorManager.anchorsChanged += OnAnchorsChanged;
    }

    /// <summary>
    /// Called when closing the scene/project.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StopTrackingNotifications(object sender, EventArgs e) {
        ARSession.stateChanged -= ARSessionStateChanged;
        ARAnchorManager.anchorsChanged -= OnAnchorsChanged;
        if (trackingFailureNotify != null) {
            StopCoroutine(trackingFailureNotify);
            trackingFailureNotify = null;
        }
        if (trackingAnchorFailureNotify != null) {
            StopCoroutine(trackingAnchorFailureNotify);
            trackingAnchorFailureNotify = null;
        }
        TrackingLostAnimation.StopVideo();
    }

    /// <summary>
    /// Called when AR tracking changes. On event ARSession.stateChanged
    /// </summary>
    /// <param name="sessionState"></param>
    private void ARSessionStateChanged(ARSessionStateChangedEventArgs sessionState) {
        // cancel previously invoked tracking failure notification
        if (trackingFailureNotify != null) {
            StopCoroutine(trackingFailureNotify);
            // stop the video only if world anchor is tracking
            if (anchorTrackingStatus == AnchorTrackingStatus.Tracking) {
                TrackingLostAnimation.StopVideo();
            }
            trackingFailureNotify = null;
        }

        switch (sessionState.state) {
            case ARSessionState.Unsupported:
                Notifications.Instance.ShowNotification("Tracking not supported", "This device does not support ARCore!");
                break;
            case ARSessionState.None:
            case ARSessionState.CheckingAvailability:
            case ARSessionState.NeedsInstall:
            case ARSessionState.Installing:
            case ARSessionState.Ready:
            case ARSessionState.SessionInitializing:
                switch (ARSession.notTrackingReason) {
                    case NotTrackingReason.None:
                        // tracking should work normally
                        break;
                    case NotTrackingReason.InsufficientLight:
                        trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to insufficient light!", "Enlight your environment.", 9f));
                        deviceTrackingStatus = DeviceTrackingStatus.InsufficientLight;
                        GameManager.Instance.SceneSetActive(false);
                        break;
                    case NotTrackingReason.InsufficientFeatures:
                        trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to insufficient features!", "Try to move the device slowly around your environment.", 9f));
                        TrackingLostAnimation.PlayVideo();
                        deviceTrackingStatus = DeviceTrackingStatus.InsufficientFeatures;
                        GameManager.Instance.SceneSetActive(false);
                        break;
                    case NotTrackingReason.ExcessiveMotion:
                        trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost due to excessive motion!", "You are moving the device too fast.", 9f));
                        TrackingLostAnimation.PlayVideo();
                        deviceTrackingStatus = DeviceTrackingStatus.ExcessiveMotion;
                        GameManager.Instance.SceneSetActive(false);
                        break;
                    case NotTrackingReason.Initializing:
                    case NotTrackingReason.Relocalizing:
                    case NotTrackingReason.Unsupported:
                        trackingFailureNotify = StartCoroutine(TrackingFailureNotify("Tracking lost!", "Reason: " + ARSession.notTrackingReason.ToString(), 9f));
                        deviceTrackingStatus = DeviceTrackingStatus.UnknownFailure;
                        GameManager.Instance.SceneSetActive(false);
                        break;
                }
                break;
            case ARSessionState.SessionTracking:
                // Check if world anchor is tracking
                if (anchorTrackingStatus == AnchorTrackingStatus.Tracking) {
                    // Anchor and device is tracking normally
                    Notifications.Instance.ShowNotification("Tracking state", "Session Tracking");
                    deviceTrackingStatus = DeviceTrackingStatus.Tracking;
                    GameManager.Instance.SceneSetActive(true);
                    StopTrackingFailureNotifications(true);
                } else {
                    deviceTrackingStatus = DeviceTrackingStatus.WaitingForAnchor;
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
    private IEnumerator TrackingFailureNotify(string title, string text, float repeatRate, int repeatCount = -1, bool anchorTrackingFailure = false) {
        int repeat = repeatCount;
        while (repeatCount == -1 ? true : repeat >= 0) {
            repeat -= 1;
            if (anchorTrackingFailure) {
                // display the anchor tracking failure notification only if there are not any other notifications with higher priority
                if (trackingFailureNotify == null) {
                    Notifications.Instance.ShowNotification(title, text);
                }
            } else {
                Notifications.Instance.ShowNotification(title, text);
            }
            yield return new WaitForSeconds(repeatRate);
        }
    }

    /// <summary>
    /// Starts the timeout for the system to recover. If it wont recover within the given timeout, recalibration will be requested.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    private IEnumerator TrackingFailureTimeout(float timeout) {
        yield return new WaitForSeconds(timeout);
        StopTrackingFailureNotifications(true);

        Notifications.Instance.ShowNotification("Tracking lost timeout!", "System will be calibrated.");

        // Make request for recalibration
        CalibrationManager.Instance.Recalibrate(true);
    }

    public TrackingQuality GetTrackingQuality() {
        int featurePoints = 0;

        if (ARSession.state != ARSessionState.SessionTracking) {
            return TrackingQuality.NOT_TRACKING;
        }

        // TODO Decide how to resolve pointCloud
        foreach (ARPointCloud pointCloud in ARPointCloudManager.trackables) {
            featurePoints += pointCloud.identifiers?.Length == null ? 0 : (int) pointCloud.identifiers?.Length;
        }

        //Notifications.Instance.ShowNotification("Tracking quality", "Feature points: " + featurePoints + " Planes: " + ARPlaneManager.trackables.count);
        //Debug.Log("Tracking quality" + " Feature points: " + featurePoints + " Planes: " + ARPlaneManager.trackables.count);

        // Need to have at least one plane and more than zero feature points
        if (ARPlaneManager.trackables.count >= 1 && featurePoints > 0) {
            return TrackingQuality.GOOD_QUALITY;
        }

        return TrackingQuality.POOR_QUALITY;
    }

    public void DisplayPlanesAndPointClouds(bool active) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        planesAndPointCloudsActive = active;
        DisplayPlanes(active);
        DisplayPointClouds(active);
#endif
    }

    public void DisplayPlanes(bool active) {
        foreach (ARPlane plane in ARPlaneManager.trackables) {
            plane.gameObject.SetActive(active);
        }
    }

    public void DisplayPlanes(bool active, List<ARPlane> addedPlanes) {
        foreach (ARPlane plane in addedPlanes) {
            plane.gameObject.SetActive(active);
            // for newly added planes, we have to set their transparency based on if in VR or AR
            ChangePlaneTransparency(planesTransparent, plane.GetComponent<Renderer>());
        }
    }

    public void DisplayPointClouds(bool active) {
        foreach (ARPointCloud pointCloud in ARPointCloudManager.trackables) {
            pointCloud.gameObject.SetActive(active);
        }
    }

    public void DisplayPointClouds(bool active, List<ARPointCloud> addedPointClouds) {
        foreach (ARPointCloud pointCloud in addedPointClouds) {
            pointCloud.gameObject.SetActive(active);
        }
    }

    public void ChangePlaneTransparency(bool transparent) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        planesTransparent = transparent;
        foreach (ARPlane plane in ARPlaneManager.trackables) {
            ChangePlaneTransparency(transparent, plane.GetComponent<Renderer>());
        }
#endif
    }

    private void ChangePlaneTransparency(bool transparent, Renderer planeRenderer) {
        if (transparent) {
            planeRenderer.sharedMaterials = new Material[1] { PlaneTransparentMaterial };
        } else {
            planeRenderer.sharedMaterials = new Material[1] { PlaneOpaqueMaterial };
        }
    }

    public bool IsDeviceTracking() {
        return deviceTrackingStatus == DeviceTrackingStatus.Tracking && anchorTrackingStatus == AnchorTrackingStatus.Tracking;
    }
}
