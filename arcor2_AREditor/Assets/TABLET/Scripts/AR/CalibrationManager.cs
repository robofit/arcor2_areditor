using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if (UNITY_ANDROID || UNITY_IOS)
using Google.XR.ARCoreExtensions;
#endif


public class CalibrationManager : Singleton<CalibrationManager> {

    public ARAnchorManager ARAnchorManager;
    public ARPlaneManager ARPlaneManager;
    public ARRaycastManager ARRaycastManager;
    public ARTrackedImageManager ARTrackedImageManager;
    public ARPointCloudManager ARPointCloudManager;
    public Transform ARCamera;
    public GameObject WorldAnchorPrefab;

    public VideoPlayerImage TrackingLostAnimation;

    [HideInInspector]
    public bool UsingCloudAnchors = false;

    [HideInInspector]
    public ARAnchor WorldAnchorLocal;

#if (UNITY_ANDROID || UNITY_IOS)
    [HideInInspector]
    public ARCloudAnchor WorldAnchorCloud;
#endif

    [HideInInspector]
    public bool Calibrated = false;

    public delegate void ARCalibratedEventHandler(object sender, GameObjectEventArgs args);
    public event ARCalibratedEventHandler OnARCalibrated;

    public delegate void ARRecalibrateEventHandler(object sender, EventArgs args);
    public event ARRecalibrateEventHandler OnARRecalibrate;

    private bool activateTrackableMarkers = false;
    public GameObject worldAnchorVis;

#if UNITY_STANDALONE || UNITY_EDITOR
    private void Start() {
        Calibrated = true;
        
    }

#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private void OnEnable() {
        GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        ARTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj) {
        ActivateTrackableMarkers(activateTrackableMarkers);
        if (obj.added.Count > 0 || obj.removed.Count > 0)
            SelectorMenu.Instance.ForceUpdateMenus();
    }
#endif

    /// <summary>
    /// Immediately creates local anchor after detected marker intersects detected plane beneath it.
    /// Cloud anchor is created afterwards, but it takes some time. When it is finished, scene will be attached to it.
    /// Called if user clicks on the calibration cube displayed over detected marker.
    /// </summary>
    /// <param name="tf"></param>
    public void CreateAnchor(Transform tf) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR

        // try to raycast straight down to intersect closest plane
        List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        if (ARRaycastManager.Raycast(new Ray(tf.position, Vector3.down), raycastHits, TrackableType.PlaneWithinPolygon)) {

            // remove all old local anchors, if there are some (in case we are recalibrating)
            RemoveLocalWorldAnchor();
            RemoveCloudWorldAnchor();

            Pose hitPose = raycastHits[0].pose;
            TrackableId hitPlaneId = raycastHits[0].trackableId;
            ARPlane plane = ARPlaneManager.GetPlane(hitPlaneId);

            // set temporary world anchor
            WorldAnchorLocal = ARAnchorManager.AttachAnchor(plane,
                new Pose(hitPose.position, Quaternion.FromToRotation(tf.up, plane.normal) * tf.rotation));
            // immediately attach scene to local anchor (after cloud anchor is created, scene will be attached to it)
            AttachScene(WorldAnchorLocal.gameObject);

            // Create cloud anchor
            if (Settings.Instance.UseCloudAnchors) {
                WorldAnchorCloud = ARAnchorManager.HostCloudAnchor(WorldAnchorLocal);

                StartCoroutine(HostCloudAnchor());
            } else {
                Calibrated = true;
                UsingCloudAnchors = false;
                OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorLocal.gameObject));
                Notifications.Instance.ShowNotification("Calibration successful", "");
                worldAnchorVis = null;
                ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
            }

            GameManager.Instance.Scene.SetActive(true);
                
            SelectorMenu.Instance.ForceUpdateMenus();

            ActivateTrackableMarkers(false);
        }
        // if there is no plane beneath detected marker then display notification about unsufficient tracking
        else {
            Notifications.Instance.ShowNotification("Calibration error", "Plane beneath calibration marker is not detected");
            //Play animation for moving with the device
            TrackingLostAnimation.PlayVideo(5f);
        }
#endif
    }

    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        HideCurrentWorldAnchor();
        ActivateTrackableMarkers(true);
        Calibrated = false;
        OnARRecalibrate(this, new EventArgs());
        GameManager.Instance.Scene.SetActive(false);
#endif
    }


#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private IEnumerator HostCloudAnchor() {
        // Wait until the anchor is fully uploaded to the cloud
        yield return new WaitWhile(() => WorldAnchorCloud.cloudAnchorState == CloudAnchorState.None ||
                                         WorldAnchorCloud.cloudAnchorState == CloudAnchorState.TaskInProgress);
        if (WorldAnchorCloud.cloudAnchorState == CloudAnchorState.Success) {
            // save its ID to PlayerPrefs
            PlayerPrefs.SetString("cloud_anchor_id", WorldAnchorCloud.cloudAnchorId);
            PlayerPrefs.Save();

            // create new calibration cube representing cloud anchor and attach scene to it
            worldAnchorVis = Instantiate(WorldAnchorPrefab, Vector3.zero, Quaternion.identity);
            worldAnchorVis.transform.SetParent(WorldAnchorCloud.transform, false);
            AttachScene(WorldAnchorCloud.gameObject);

            // remove temporary local anchor
            RemoveLocalWorldAnchor();

            Notifications.Instance.ShowNotification("Cloud anchor created", WorldAnchorCloud.cloudAnchorState.ToString() + " ID: " + WorldAnchorCloud.cloudAnchorId);

            Calibrated = true;
            UsingCloudAnchors = true;
            OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorCloud.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
            GameManager.Instance.Scene.SetActive(true);
                
                SelectorMenu.Instance.ForceUpdateMenus();
        } else {
            Notifications.Instance.ShowNotification("Cloud anchor error", WorldAnchorCloud.cloudAnchorState.ToString());
            Debug.LogError("Cloud anchor error: " + WorldAnchorCloud.cloudAnchorState);
        }
    }

    private void RemoveLocalWorldAnchor() {
        if (WorldAnchorLocal != null) {
            DetachScene();
            ARAnchorManager.RemoveAnchor(WorldAnchorLocal);
        }
    }

    private void RemoveCloudWorldAnchor() {
        if (WorldAnchorCloud != null) {
            DetachScene();
            Destroy(WorldAnchorCloud.gameObject);
        }
    }

    private void HideCurrentWorldAnchor() {
        if (WorldAnchorLocal != null) {
            WorldAnchorLocal.gameObject.SetActive(false);
            SelectorMenu.Instance.ForceUpdateMenus();
            Debug.LogError("HideCurrentWorldAnchor");
        }
        if (WorldAnchorCloud != null) {
            WorldAnchorCloud.gameObject.SetActive(false);
            SelectorMenu.Instance.ForceUpdateMenus();
            Debug.LogError("HideCurrentWorldAnchor");
        }
    }

    private void AttachScene(GameObject worldAnchor) {
    //private void AttachScene(GameObject worldAnchor, bool initLocalAnchor = false) {
        //if (initLocalAnchor) {
        //    WorldAnchorLocal = ARAnchorManager.AddAnchor(new Pose(Camera.main.transform.position, Camera.main.transform.rotation));
        //    GameManager.Instance.Scene.transform.parent = WorldAnchorLocal.gameObject.transform;
        //} else
        if(worldAnchor != null) {
            GameManager.Instance.Scene.transform.parent = worldAnchor.transform;
        }

        Vector3 offset = PlayerPrefsHelper.LoadVector3("/marker_offset", Vector3.zero);

        GameManager.Instance.Scene.transform.localPosition = offset;
        GameManager.Instance.Scene.transform.localScale = new Vector3(1f, 1f, 1f);
        GameManager.Instance.Scene.transform.localEulerAngles = Vector3.zero;
    }

    public void UpdateMarkerOffset(Vector3 offset) {
        PlayerPrefsHelper.SaveVector3("/marker_offset", offset);
        GameManager.Instance.Scene.transform.localPosition = offset;
    }

    private void DetachScene() {
        GameManager.Instance.Scene.transform.parent = null;
    }

    private bool LoadCloudAnchor() {
        string cloudAnchorId = PlayerPrefs.GetString("cloud_anchor_id");
        if (cloudAnchorId != null) {
            WorldAnchorCloud = ARAnchorManager.ResolveCloudAnchorId(cloudAnchorId);

            if (WorldAnchorCloud == null) {
                Notifications.Instance.ShowNotification("Cloud anchor fail", "Cloud anchor couldn't be loaded");
                Debug.Log("Cloud anchor fail: Cloud anchor couldn't be loaded " + cloudAnchorId);
                return false;    
            }

            return true;
        }
        return false;
    }

    private IEnumerator Calibrate() {
        // Do nothing while in the MainScreen (just track feature points, planes, etc. as user moves unintentionally with the device)
        yield return new WaitUntil(() => GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor ||
                                         GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor ||
                                         GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning);

        Notifications.Instance.ShowNotification("Calibrating", "Move the device around your workspace");
        TrackingLostAnimation.PlayVideo();
        //yield return new WaitForSeconds(10f);

        // Check how many features and planes the tracking has detected
        yield return new WaitUntil(() => TrackingManager.Instance.GetTrackingQuality() == TrackingManager.TrackingQuality.GOOD_QUALITY);
        TrackingLostAnimation.StopVideo();

        if (Settings.Instance.UseCloudAnchors) {
            // Try to load cloud anchor defined by ID saved in PlayerPrefs
            if (LoadCloudAnchor()) {
                // if anchor exist in the cloud, wait for it to be fully loaded
                yield return new WaitUntil(() => WorldAnchorCloud.cloudAnchorState == CloudAnchorState.Success);

                worldAnchorVis = Instantiate(WorldAnchorPrefab, Vector3.zero, Quaternion.identity);
                worldAnchorVis.transform.SetParent(WorldAnchorCloud.transform, false);
                AttachScene(WorldAnchorCloud.gameObject);
                Debug.LogError("Calibrate");

                // disactivate marker tracking, because anchor is loaded from the cloud
                ActivateTrackableMarkers(false);

                Notifications.Instance.ShowNotification("Cloud anchor loaded", "Cloud anchor loaded sucessfully");

                Calibrated = true;
                UsingCloudAnchors = true;
                OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorCloud.gameObject));
                Notifications.Instance.ShowNotification("Calibration successful", "");
                GameManager.Instance.Scene.SetActive(true);
                
                SelectorMenu.Instance.ForceUpdateMenus();
            }
            //TODO If anchor is not present in the system, play animation to manually calibrate by clicking on marker
            else {
                Calibrated = false;
                Notifications.Instance.ShowNotification("Cloud anchor does not exist", "Please calibrate manually by clicking on the cube displayed on your marker");

                //AttachScene(null, initLocalAnchor: true);

                ActivateTrackableMarkers(true);
            }
        } else {
            Calibrated = false;
            Notifications.Instance.ShowNotification("Cloud anchors are disabled", "Please calibrate manually by clicking on the cube displayed on your marker");

            //AttachScene(null, initLocalAnchor: true);

            ActivateTrackableMarkers(true);
        }

        yield return null;
    }
    
    private void ConnectedToServer(object sender, Base.StringEventArgs e) {
        if (!Calibrated) {
            StartCoroutine(Calibrate());
        }
    }

    private void ActivateTrackableMarkers(bool active) {
        activateTrackableMarkers = active;
        foreach (ARTrackedImage trackedImg in ARTrackedImageManager.trackables) {
            // Control if camera is 5 cm from the marker cube, if so clip the cube and don't display it.
            // Fixes the situation when user detects the marker but won't click on it, when he closes the scene and reopens,
            // marker cube stays positioned at the camera position (transforms should be the same).
            if (Vector3.Distance(trackedImg.transform.position, ARCamera.position) <= 0.05f) {
                trackedImg.gameObject.SetActive(false);
            } else {
                bool wasActive = trackedImg.gameObject.activeSelf;                
                trackedImg.gameObject.SetActive(active);
                
            }
        }
    }

    public void ActivateCalibrationElements(bool active) {
        if (worldAnchorVis == null) {
            foreach (Transform child in WorldAnchorLocal.transform) {
                if (child.tag == "world_anchor") {
                    worldAnchorVis = child.gameObject;
                    break;
                }
            }
        }

        if (worldAnchorVis != null) {
            worldAnchorVis.SetActive(active);
            SelectorMenu.Instance.ForceUpdateMenus();
            Debug.LogError("ActivateCalibrationElements");
        }
    }
#endif
}
