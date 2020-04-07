using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;

public class CalibrationManager : Singleton<CalibrationManager> {

    public ARAnchorManager ARAnchorManager;
    public ARPlaneManager ARPlaneManager;
    public ARRaycastManager ARRaycastManager;
    public ARTrackedImageManager ARTrackedImageManager;
    public ARPointCloudManager ARPointCloudManager;
    public GameObject WorldAnchorPrefab;

    [HideInInspector]
    public ARAnchor WorldAnchorLocal;

    [HideInInspector]
    public ARCloudAnchor WorldAnchorCloud;

    private bool calibrated = false;
    private bool activateTrackableMarkers = false;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private void OnEnable() {
        GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        ARTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj) {
        ActivateTrackableMarkers(activateTrackableMarkers);
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
        // remove all old local anchors, if there are some (in case we are recalibrating)
        RemoveLocalWorldAnchor();
        RemoveCloudWorldAnchor();

        // TODO check if there is plane that is very close to detected object
        // try to raycast straight down to intersect closest plane
        List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        if (ARRaycastManager.Raycast(new Ray(tf.position, Vector3.down), raycastHits, TrackableType.PlaneWithinPolygon)) {
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
            }
        }
        ActivateTrackableMarkers(false);
#endif
    }

    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        ActivateTrackableMarkers(true);
        calibrated = false;
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
            GameObject worldAnchorVis = Instantiate(WorldAnchorPrefab, Vector3.zero, Quaternion.identity);
            worldAnchorVis.transform.SetParent(WorldAnchorCloud.transform, false);
            AttachScene(WorldAnchorCloud.gameObject);

            // remove temporary local anchor
            RemoveLocalWorldAnchor();

            Notifications.Instance.ShowNotification("Cloud anchor created", WorldAnchorCloud.cloudAnchorState.ToString() + " ID: " + WorldAnchorCloud.cloudAnchorId);

            calibrated = true;
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

    private void AttachScene(GameObject worldAnchor, bool initLocalAnchor = false) {
        if (initLocalAnchor) {
            WorldAnchorLocal = ARAnchorManager.AddAnchor(new Pose(Camera.main.transform.position, Camera.main.transform.rotation));
            Scene.Instance.transform.parent = WorldAnchorLocal.gameObject.transform;
        } else if(worldAnchor != null) {
            Scene.Instance.transform.parent = worldAnchor.transform;
        }

        Scene.Instance.transform.localPosition = Vector3.zero;
        Scene.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        Scene.Instance.transform.localEulerAngles = Vector3.zero;
    }

    private void DetachScene() {
        Scene.Instance.transform.parent = null;
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
                                         GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor);

        //TODO Play animation, let the user move around the workspace for atleast 10 seconds
        Notifications.Instance.ShowNotification("Calibrating", "Move the device around your workspace");
        //yield return new WaitForSeconds(10f);

        // Check how many features and planes the tracking has detected
        yield return new WaitUntil(() => TrackingManager.Instance.GetTrackingQuality() == TrackingManager.TrackingQuality.GOOD_QUALITY);

        if (Settings.Instance.UseCloudAnchors) {
            // Try to load cloud anchor defined by ID saved in PlayerPrefs
            if (LoadCloudAnchor()) {
                // if anchor exist in the cloud, wait for it to be fully loaded
                yield return new WaitUntil(() => WorldAnchorCloud.cloudAnchorState == CloudAnchorState.Success);

                GameObject worldAnchorVis = Instantiate(WorldAnchorPrefab, Vector3.zero, Quaternion.identity);
                worldAnchorVis.transform.SetParent(WorldAnchorCloud.transform, false);
                AttachScene(WorldAnchorCloud.gameObject);

                // disactivate marker tracking, because anchor is loaded from the cloud
                ActivateTrackableMarkers(false);

                Notifications.Instance.ShowNotification("Cloud anchor loaded", "Cloud anchor loaded sucessfully");

                calibrated = true;
            }
            //TODO If anchor is not present in the system, play animation to manually calibrate by clicking on marker
            else {
                calibrated = false;
                Notifications.Instance.ShowNotification("Cloud anchor does not exist", "Please calibrate manually by clicking on the cube displayed on your marker");

                AttachScene(null, initLocalAnchor: true);

                ActivateTrackableMarkers(true);
            }
        } else {
            calibrated = false;
            Notifications.Instance.ShowNotification("Cloud anchors are disabled", "Please calibrate manually by clicking on the cube displayed on your marker");

            AttachScene(null, initLocalAnchor: true);

            ActivateTrackableMarkers(true);
        }

        yield return null;
    }
    
    private void ConnectedToServer(object sender, Base.StringEventArgs e) {
        if (!calibrated) {
            StartCoroutine(Calibrate());
        }
    }

    private void ActivateTrackableMarkers(bool active) {
        activateTrackableMarkers = active;
        foreach (ARTrackedImage trackedImg in ARTrackedImageManager.trackables) {
            trackedImg.gameObject.SetActive(active);
        }
    }
    
#endif
}
