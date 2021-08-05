using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
using IO.Swagger.Model;
using static Base.AREditorEventArgs;
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
using Google.XR.ARCoreExtensions;
#endif


public class CalibrationManager : Singleton<CalibrationManager> {

    public enum MarkerDetectionState {
        Success,
        Failure,
        Processing
    }
//#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
//    public ARCoreExtensions ARCoreExt;
//#endif
    public ARSessionOrigin ARSessionOrigin;
    public ARAnchorManager ARAnchorManager;
    public ARPlaneManager ARPlaneManager;
    public ARRaycastManager ARRaycastManager;
    public ARTrackedImageManager ARTrackedImageManager;
    public ARPointCloudManager ARPointCloudManager;
    public ARCameraManager ARCameraManager;
    public Transform ARCamera;
    public GameObject WorldAnchorPrefab;
    
    public VideoPlayerImage TrackingLostAnimation;

    [HideInInspector]
    public bool UsingCloudAnchors = false;

    [HideInInspector]
    public ARAnchor WorldAnchorLocal;

#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
    [HideInInspector]
    public ARCloudAnchor WorldAnchorCloud;
#endif

    [HideInInspector]
    public bool Calibrated {
        private set;
        get;
    }

    public event CalibrationEventHandler OnARCalibrated;

    public delegate void ARRecalibrateEventHandler(object sender, EventArgs args);
    public event ARRecalibrateEventHandler OnARRecalibrate;

    private bool activateTrackableMarkers = false;

    [HideInInspector]
    public GameObject worldAnchorVis;

    public GameObject MarkerPositionGameObject;

    public Transform AROfsset;

    private bool firstFrameReceived = false;
    private XRCameraConfiguration? cameraConfiguration;
    private Matrix4x4? displayMatrix;
    private Matrix4x4? projectionMatrix;

    private Matrix4x4 ARCameraTransformMatrix;

    public bool UsingServerCalibration {
        get;
        private set;
    }
    private Coroutine localCalibration = null;
    private Coroutine autoCalibration = null;

    private MarkerDetectionState markerDetectionState;

    private float anchorQuality = 0f;
    private float recalibrateTime = 2f;
    private int imageNum = 1;

    private bool AutoRecalibration = true;

    private Texture2D m_Texture;

    public float AutoRecalibrateTime = 120f;

    private void Awake() {
        UsingServerCalibration = PlayerPrefsHelper.LoadBool("UseServerCalibration", true);
        UpdateAutoCalibTime(float.Parse(PlayerPrefsHelper.LoadString("/autoCalib/recalibrationTime", "120")));
    }

    public void UseServerCalibration(bool useServer) {
        if (useServer) {
            RunServerAutoCalibration();
        } else {
            RunLocalARFoundationCalibration();
        }
    }


    private void Start() {
#if UNITY_STANDALONE || !AR_ON
        Calibrated = true;
        OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorLocal.gameObject));
#endif
    }



#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
    private void OnEnable() {
        //GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        ARTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
        ARCameraManager.frameReceived += FrameReceived;

        GameManager.Instance.OnOpenSceneEditor += OnOpenAR;
        GameManager.Instance.OnOpenProjectEditor += OnOpenAR;
        GameManager.Instance.OnRunPackage += OnOpenAR;
    }

    private void OnDisable() {
        ARCameraManager.frameReceived -= FrameReceived;

        GameManager.Instance.OnOpenSceneEditor -= OnOpenAR;
        GameManager.Instance.OnOpenProjectEditor -= OnOpenAR;
        GameManager.Instance.OnRunPackage -= OnOpenAR;
    }

    private void OnOpenAR(object sender, EventArgs e) {
        if (Calibrated) {
            if (UsingServerCalibration) {
                WorldAnchorLocal.GetComponent<RecalibrateUsingServer>().CreateSelectorItem();
            } else {
                WorldAnchorLocal.GetComponent<Recalibrate>().CreateSelectorItem();
            }
        } else {
            if (!UsingServerCalibration) {
                foreach (ARTrackedImage anchorCube in ARTrackedImageManager.trackables) {
                    CreateAnchor anchor = anchorCube.GetComponent<CreateAnchor>();
                    if (anchor != null) {
                        anchor.CreateSelectorItem();
                    }
                }
            }
        }
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj) {
        ActivateTrackableMarkers(activateTrackableMarkers);
    }
#endif

    private void OnApplicationPause(bool pause) {
        if (UsingServerCalibration) {
            // Application was paused, suspended
            if (pause) {
                if (autoCalibration != null) {
                    StopCoroutine(autoCalibration);
                    autoCalibration = null;
                }
            } else {
                if (autoCalibration == null) {
                    autoCalibration = StartCoroutine(AutoCalibrate());
                }
            }
        }
    }

    public void Recalibrate(bool startAutoCalibrationProcess = false, bool showNotification = false) {
        if (UsingServerCalibration) {
            if (startAutoCalibrationProcess) {
                RecalibrateUsingServerAuto();
            } else {
                RecalibrateUsingServer(inverse: true, showNotification: showNotification);
            }
        } else {
            RecalibrateUsingARFoundation();
        }
    }

#region Calibration using ARFoundation

    private void RunLocalARFoundationCalibration() {
        #if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        // If we used server calibration before, remove all server anchors and prepare for local calibration
        if (UsingServerCalibration) {
            //Debug.Log("Local: Was using server calibration");
            Calibrated = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
            if (autoCalibration != null) {
                //Debug.Log("Local: Stopping autoCalib");
                StopCoroutine(autoCalibration);
                autoCalibration = null;
            }
            RemoveLocalWorldAnchor();
            GameManager.Instance.SceneSetActive(false);
        }

        ARTrackedImageManager.enabled = true;

        if (!Calibrated) {
            //Debug.Log("Local: not calibrated");
            // stop previously running calibration coroutine
            if (localCalibration != null) {
                //Debug.Log("Local: Stopping localCalib");
                StopCoroutine(localCalibration);
            }
            localCalibration = StartCoroutine(Calibrate());
        }
        
        UsingServerCalibration = false;
#endif
    }

    public void EnableAutoReCalibration(bool active) {
        AutoRecalibration = active;
        if (UsingServerCalibration) {
            if (autoCalibration != null) {
                StopCoroutine(autoCalibration);
            }
            autoCalibration = StartCoroutine(AutoCalibrate());
            //Debug.Log("AUTO RECALIBRATION " + AutoRecalibration);
        }
    }

    private IEnumerator Calibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
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

                // disactivate marker tracking, because anchor is loaded from the cloud
                ActivateTrackableMarkers(false);

                Notifications.Instance.ShowNotification("Cloud anchor loaded", "Cloud anchor loaded sucessfully");

                Calibrated = true;
                UsingCloudAnchors = true;
                OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorCloud.gameObject));
                Notifications.Instance.ShowNotification("Calibration successful", "");
                GameManager.Instance.SceneSetActive(true);
            }
            //TODO If anchor is not present in the system, play animation to manually calibrate by clicking on marker
            else {
                Calibrated = false;
                OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
                Notifications.Instance.ShowNotification("Calibrate by clicking on the Calibration cube", "Cloud anchor does not exist. Please calibrate manually by clicking on the cube displayed on your marker");

                //AttachScene(null, initLocalAnchor: true);

                ActivateTrackableMarkers(true);
            }
        } else {
            Calibrated = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
            Notifications.Instance.ShowNotification("Calibrate by clicking on the Calibration cube", "Please calibrate manually by clicking on the cube displayed on your marker");

            //AttachScene(null, initLocalAnchor: true);

            ActivateTrackableMarkers(true);
        }
#endif
        yield return null;
    }

    /// <summary>
    /// Immediately creates local anchor after detected marker intersects detected plane beneath it.
    /// Cloud anchor is created afterwards, but it takes some time. When it is finished, scene will be attached to it.
    /// Called if user clicks on the calibration cube displayed over detected marker.
    /// </summary>
    /// <param name="tf"></param>
    public void CreateAnchor(Transform tf) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        ARPlane plane = null;
        UnityEngine.Pose hitPose = new UnityEngine.Pose();

        // try to raycast straight down to intersect closest plane
        List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        if (ARRaycastManager.Raycast(new Ray(tf.position, Vector3.down), raycastHits, TrackableType.PlaneWithinPolygon)) {
            hitPose = raycastHits[0].pose;
            TrackableId hitPlaneId = raycastHits[0].trackableId;
            plane = ARPlaneManager.GetPlane(hitPlaneId);
        }

        // remove all old local anchors, if there are some (in case we are recalibrating)
        RemoveLocalWorldAnchor();
        RemoveCloudWorldAnchor();

        // set temporary world anchor
        //WorldAnchorLocal = ARAnchorManager.AttachAnchor(plane,
        //    new Pose(hitPose.position, Quaternion.FromToRotation(tf.up, plane.normal) * tf.rotation));

        //WorldAnchorLocal = ARAnchorManager.AddAnchor(new UnityEngine.Pose(hitPose != new UnityEngine.Pose() ? hitPose.position : tf.position,
        //    plane != null ? Quaternion.FromToRotation(tf.up, plane.normal) * tf.rotation : tf.rotation));

        WorldAnchorLocal = ARAnchorManager.AddAnchor(new UnityEngine.Pose(tf.position,
            plane != null ? Quaternion.FromToRotation(tf.up, plane.normal) * tf.rotation : tf.rotation));

        // immediately attach scene to local anchor (after cloud anchor is created, scene will be attached to it)
        AttachScene(WorldAnchorLocal.gameObject);

        // Create cloud anchor
        if (Settings.Instance.UseCloudAnchors) {
            WorldAnchorCloud = ARAnchorManager.HostCloudAnchor(WorldAnchorLocal);
            StartCoroutine(HostCloudAnchor());
        } else {
            Calibrated = true;
            UsingCloudAnchors = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorLocal.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            worldAnchorVis = null;
            ActivateCalibrationElements((bool) MainSettingsMenu.Instance.CalibrationElements.GetValue());
        }

        GameManager.Instance.SceneSetActive(true);
        ActivateTrackableMarkers(false);
#endif
    }

    public void RecalibrateUsingARFoundation() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        HideCurrentWorldAnchor();
        ActivateTrackableMarkers(true);
        Calibrated = false;
        OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
        OnARRecalibrate?.Invoke(this, new EventArgs());
        GameManager.Instance.SceneSetActive(false);
#endif
    }


#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
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
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorCloud.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            ActivateCalibrationElements((bool) MainSettingsMenu.Instance.CalibrationElements.GetValue());
            GameManager.Instance.SceneSetActive(true);
        } else {
            Notifications.Instance.ShowNotification("Cloud anchor error", WorldAnchorCloud.cloudAnchorState.ToString());
            Debug.LogError("Cloud anchor error: " + WorldAnchorCloud.cloudAnchorState);
        }
    }

    private void RemoveLocalWorldAnchor() {
        if (WorldAnchorLocal != null) {
            DetachScene();
            worldAnchorVis = null;
            Destroy(WorldAnchorLocal.gameObject);
        }
    }

    private void RemoveCloudWorldAnchor() {
        if (WorldAnchorCloud != null) {
            DetachScene();
            worldAnchorVis = null;
            Destroy(WorldAnchorCloud.gameObject);
        }
    }

    private void HideCurrentWorldAnchor() {
        if (WorldAnchorLocal != null) {
            if (!worldAnchorVis) {
                worldAnchorVis = Helper.FindComponentInChildWithTag<Transform>(WorldAnchorLocal.gameObject, "world_anchor").gameObject;
            }
            worldAnchorVis.SetActive(false);
            WorldAnchorLocal.GetComponent<Recalibrate>().Enable(false);
        }
        if (WorldAnchorCloud != null) {
            if (!worldAnchorVis) {
                worldAnchorVis = Helper.FindComponentInChildWithTag<Transform>(WorldAnchorCloud.gameObject, "world_anchor").gameObject;
            }
            worldAnchorVis.SetActive(false);
            WorldAnchorCloud.GetComponent<Recalibrate>().Enable(false);
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
        if (Calibrated) {
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
                WorldAnchorLocal.GetComponent<InteractiveObject>().Enable(active);

                // If calib cube should be hidden, check if it is not selected, otherwise deselect it from selector menu
                if (!active) {
                    if (WorldAnchorLocal.GetComponent<InteractiveObject>() == SelectorMenu.Instance.GetSelectedObject()) {
                        SelectorMenu.Instance.DeselectObject();
                    }
                }
            }
        }
    }
#endif

    #endregion




    #region Calibration using ARServer

    public void UpdateAutoCalibTime(float time) {
        AutoRecalibrateTime = time;
    }

    private void RunServerAutoCalibration() {
        #if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        // If we used local calibration before, remove all local anchors and prepare for server calibration
        if (!UsingServerCalibration) {
            //Debug.Log("Server: Was using local calibration");
            Calibrated = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
            if (localCalibration != null) {
                //Debug.Log("Server: Stopping local calibration coroutine");
                StopCoroutine(localCalibration);
                localCalibration = null;
            }
            RemoveLocalWorldAnchor();
            GameManager.Instance.SceneSetActive(false);
        }

        ARTrackedImageManager.enabled = false;

        if (!Calibrated) {
            //Debug.Log("Server: not calibrated");
            // stop previously running calibration coroutine
            if (autoCalibration != null) {
                //Debug.Log("Server: Stopping autocalib");
                StopCoroutine(autoCalibration);
            }
            autoCalibration = StartCoroutine(AutoCalibrate());
        }

        UsingServerCalibration = true;
#endif
    }

    private IEnumerator AutoCalibrate() {
        // Do nothing while in the MainScreen (just track feature points, planes, etc. as user moves unintentionally with the device)
        yield return new WaitUntil(() => GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor ||
                                         GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor ||
                                         GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning);

        if (!Calibrated) {
            Notifications.Instance.ShowNotification("Calibrating", "Move the device around your workspace");
            TrackingLostAnimation.PlayVideo();
            //yield return new WaitForSeconds(10f);

            // Check how many features and planes the tracking has detected
            yield return new WaitUntil(() => TrackingManager.Instance.GetTrackingQuality() == TrackingManager.TrackingQuality.GOOD_QUALITY);
            TrackingLostAnimation.StopVideo();

            Calibrated = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
            Notifications.Instance.ShowNotification("Locate as much markers as possible", "Calibration will be done automatically");
        }

        while (!Calibrated) {
            
            markerDetectionState = MarkerDetectionState.Processing;
            bool calibrated = false;
            yield return CalibrateUsingServerAsync(success => {
                calibrated = success;
                markerDetectionState = success ? MarkerDetectionState.Success : MarkerDetectionState.Failure;
            }, inverse: true, force:true);

            Calibrated = calibrated;

            yield return new WaitForSeconds(0.5f);
        }
        OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorLocal.gameObject));

        // Main autocalibration loop
        while (Application.isFocused) {
            // Do nothing while in the MainScreen (just track feature points, planes, etc. as user moves unintentionally with the device)
            yield return new WaitUntil(() => (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor ||
                                             GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor ||
                                             GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning) &&
                                             TrackingManager.Instance.IsDeviceTracking());

            if (AutoRecalibration) {
                markerDetectionState = MarkerDetectionState.Processing;
                yield return CalibrateUsingServerAsync(success => {
                    if (success) {
                        markerDetectionState = MarkerDetectionState.Success;
                        recalibrateTime = 2f;
                    } else {
                        markerDetectionState = MarkerDetectionState.Failure;
                        if (anchorQuality > 0) {
                            anchorQuality -= 0.05f;
                        }
                        //if (recalibrateTime > 1f) {
                        //    recalibrateTime -= 10f;
                        //}
                    }
                }, inverse: true, autoCalibrate: true);

            //Debug.Log("Current quality: " + anchorQuality);
            }

            yield return new WaitForSeconds(AutoRecalibrateTime);
        }
    }

    private void FrameReceived(ARCameraFrameEventArgs arCameraArgs) {
        if (!firstFrameReceived) {
            firstFrameReceived = true;
            
            displayMatrix = arCameraArgs.displayMatrix;
            projectionMatrix = arCameraArgs.projectionMatrix;
            if (ARCameraManager.descriptor.supportsCameraConfigurations) {
                cameraConfiguration = ARCameraManager.currentConfiguration;
                Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
                Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);
            }
        }
    }

    public IEnumerator CalibrateUsingServerAsync(Action<bool> callback = null, bool inverse = false, bool autoCalibrate = false, bool force = false, bool showNotification = false) {
        Debug.Log("Calibrating using server");

        if (!ARCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)) {
            //Debug.LogError("Did not get the intrinsics");
            callback?.Invoke(false);
            yield break;
        }

        // Get information about the device camera image.
        if (ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
                        // If successful, launch a coroutine that waits for the image
            // to be ready, then apply it to a texture.
            //StartCoroutine(ProcessImage(image, cameraIntrinsics, inverse));
            yield return ProcessImage(image, cameraIntrinsics, success => {
                callback?.Invoke(success);
            }, inverse, autoCalibrate, force:force, showNotification:showNotification);

            // It's safe to dispose the image before the async operation completes.
            image.Dispose();
        } else {
            callback?.Invoke(false);
        }
    }

    private IEnumerator ProcessImage(XRCpuImage image, XRCameraIntrinsics cameraIntrinsics, Action<bool> callback = null, bool inverse = false, bool autoCalibrate = false, bool force = false, bool showNotification = false) {
        // Get ARCamera Transform Matrix
        ARCameraTransformMatrix = Matrix4x4.TRS(ARCamera.position, ARCamera.rotation, ARCamera.localScale);

        // Create the async conversion request.
        var request = image.ConvertAsync(new XRCpuImage.ConversionParams {
            // Use the full image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width, image.height),

            // Color image format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the Y axis.
            transformation = XRCpuImage.Transformation.MirrorY
        });

        // Wait for the conversion to complete.
        while (!request.status.IsDone())
            yield return null;

        // Check status to see if the conversion completed successfully.
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready) {
            // Something went wrong.
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            // Dispose even if there is an error.
            request.Dispose();

            callback?.Invoke(false);
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();

        // Create a texture if necessary.
        //Texture2D
            m_Texture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false);

        // Copy the image data into the texture.
        m_Texture.LoadRawTextureData(rawData);
        m_Texture.Apply();

        // Need to dispose the request to delete resources associated
        // with the request, including the raw data.
        request.Dispose();

        string imageString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(m_Texture.EncodeToJPG());

        //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/image" + imageNum + ".jpg", m_Texture.EncodeToJPG());
        //imageNum++;

        //Debug.Log("Image size: " + request.conversionParams.outputDimensions.x + " x " + request.conversionParams.outputDimensions.y);
        //Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
        //Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);


        CameraParameters cameraParams = new CameraParameters(cx: (decimal) cameraIntrinsics.principalPoint.x,
                                                     cy: (decimal) cameraIntrinsics.principalPoint.y,
                                                     distCoefs: new List<decimal>() { 0, 0, 0, 0 },
                                                     fx: (decimal) cameraIntrinsics.focalLength.x,
                                                     fy: (decimal) cameraIntrinsics.focalLength.y);
        //Debug.Log(cameraParams.ToString());

        if (inverse) {
            GetMarkerPosition(cameraParams, imageString, autoCalibrate:autoCalibrate, force:force, showNotification:showNotification);            
        } else {
            //GetCameraPosition(cameraParams, imageString, autoCalibrate);
        }

        yield return new WaitWhile(() => markerDetectionState == MarkerDetectionState.Processing);
        if (markerDetectionState == MarkerDetectionState.Success) {
            callback?.Invoke(true);
        } else if (markerDetectionState == MarkerDetectionState.Failure) {
            callback?.Invoke(false);
        }

        //GetMarkerCornersPosition(cameraParams, imageString);
    }

    public async Task GetMarkerCornersPosition(CameraParameters cameraParams, string image) {
        try {
            List<IO.Swagger.Model.MarkerCorners> markerCorners = await WebsocketManager.Instance.GetMarkerCorners(cameraParams, image);
            foreach (MarkerCorners marker in markerCorners) {
                foreach (Corner corner in marker.Corners) {
                    
                    //Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3((1920f - ((float) corner.X * 3f)), (float) corner.Y * 2.25f, 0.5f));
                    Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3((float) corner.X, (float) corner.Y, 0.5f));
                    position = displayMatrix.Value.MultiplyVector(position);
                    GameObject cornerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cornerGO.transform.position = position;
                    cornerGO.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                }
            }
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to get marker corners.", ex.Message);
        }
    }

    public async void GetMarkerPosition(CameraParameters cameraParams, string image, bool autoCalibrate = false, bool force = false, bool showNotification = false) {
        try {
            // receive cameraPose from server
            IO.Swagger.Model.EstimatedPose markerEstimatedPose = await WebsocketManager.Instance.GetCameraPose(cameraParams, image, inverse:true);

            if ((float)markerEstimatedPose.Quality > anchorQuality || force) {
                anchorQuality = (float)markerEstimatedPose.Quality;

                
                //Notifications.Instance.ShowNotification("Marker quality", markerEstimatedPose.Quality.ToString());

                IO.Swagger.Model.Pose markerPose = markerEstimatedPose.Pose;

                Vector3 markerPositionReceived = new Vector3((float) markerPose.Position.X, (float) markerPose.Position.Y, (float) markerPose.Position.Z);
                Quaternion markerRotationReceived = new Quaternion((float) markerPose.Orientation.X, (float) markerPose.Orientation.Y,
                                                                    (float) markerPose.Orientation.Z, (float) markerPose.Orientation.W);

                Matrix4x4 markerMatrix = AdjustMatrixByScreenOrientation(Matrix4x4.TRS(markerPositionReceived, markerRotationReceived, Vector3.one));

                Vector3 markerPosition = TransformConvertor.OpenCVToUnity(TransformConvertor.GetPositionFromMatrix(markerMatrix));
                Quaternion markerRotation = TransformConvertor.OpenCVToUnity(TransformConvertor.GetQuaternionFromMatrix(markerMatrix));

                // Marker Position
                GameObject marker = Instantiate(MarkerPositionGameObject);
                marker.transform.localPosition = ARCameraTransformMatrix.MultiplyPoint3x4(markerPosition); //ARCamera.TransformPoint(markerPosition);
                marker.transform.localRotation = TransformConvertor.GetQuaternionFromMatrix(ARCameraTransformMatrix) * markerRotation; //ARCamera.transform.rotation * markerRotation; 
                marker.transform.localScale = new Vector3(1f, 1f, 1f);

                CreateLocalAnchor(marker);


                //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/image" + imageNum + ".jpg", m_Texture.EncodeToJPG());
                //imageNum++;

                // Transformation Inversion to get Camera Position
                //markerMatrix = Matrix4x4.TRS(markerPosition, markerRotation, Vector3.one); // create translation, rotation and scaling matrix
                //Matrix4x4 cameraMatrix = markerMatrix.inverse; // inverse to get marker rotation matrix
                //cameraMatrix.SetColumn(3, Vector4.zero); // set translation column to zeros
                //Vector3 cameraPos = cameraMatrix.MultiplyPoint3x4(markerPosition); // transform cameraPosition by marker matrix
                //cameraPos = -1 * cameraPos;

                //if (WorldAnchorLocal != null) {
                //    // Camera Position
                //    GameObject camera = Instantiate(MarkerPositionGameObject, WorldAnchorLocal.transform);
                //    camera.transform.localPosition = cameraPos;
                //    camera.transform.localRotation = Quaternion.LookRotation(cameraMatrix.GetColumn(2), cameraMatrix.GetColumn(1));
                //    camera.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                //}

                //Notifications.Instance.ShowNotification("Marker position", "GetCameraPose inverse true");

                markerDetectionState = MarkerDetectionState.Success;
                if (showNotification) {
                    Notifications.Instance.ShowNotification("Calibration successful", "");
                }
            } else {
                markerDetectionState = MarkerDetectionState.Failure;
                if (showNotification) {
                    Notifications.Instance.ShowNotification("No markers visible", "Find some markers and try again.");
                }
            }            

        } catch (RequestFailedException ex) {
            markerDetectionState = MarkerDetectionState.Failure;
            Debug.Log("No markers visible");
            if (showNotification) {
                Notifications.Instance.ShowNotification("No markers visible", "Find some markers and try again.");
            }
        }
    }

    private Matrix4x4 AdjustMatrixByScreenOrientation(Matrix4x4 m) {
        float rotZ = 0;
        switch (Screen.orientation) {
            case ScreenOrientation.Portrait:
                rotZ = 90;
                break;
            case ScreenOrientation.LandscapeLeft:
                rotZ = 180;
                break;
            case ScreenOrientation.LandscapeRight:
                rotZ = 0;
                break;
            case ScreenOrientation.PortraitUpsideDown:
                rotZ = -90;
                break;
        }
#if UNITY_EDITOR
        rotZ = 180;
#endif
        Matrix4x4 screenRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotZ));
        return screenRotation * m;
    }


    //public async void GetCameraPosition(CameraParameters cameraParams, string image, bool autoCalibrate = false) {
    //    try {
    //        // receive cameraPose from server
    //        IO.Swagger.Model.EstimatedPose cameraEstimatedPose = await WebsocketManager.Instance.GetCameraPose(cameraParams, image, inverse:false);
    //        IO.Swagger.Model.Pose cameraPose = cameraEstimatedPose.Pose;

    //        Vector3 cameraPositionReceived = new Vector3((float) cameraPose.Position.X, (float) cameraPose.Position.Y, (float) cameraPose.Position.Z);
    //        Quaternion cameraRotationReceived = new Quaternion((float) cameraPose.Orientation.X, (float) cameraPose.Orientation.Y,
    //                                                            (float) cameraPose.Orientation.Z, (float) cameraPose.Orientation.W);

    //        //Matrix4x4 cameraMatrix = AdjustMatrixByScreenOrientation(Matrix4x4.TRS(cameraPositionReceived, cameraRotationReceived, Vector3.one));
    //        Matrix4x4 cameraMatrix = Matrix4x4.TRS(cameraPositionReceived, cameraRotationReceived, Vector3.one);

    //        Vector3 cameraPosition = TransformConvertor.OpenCVToUnity(TransformConvertor.GetPositionFromMatrix(cameraMatrix));
    //        Quaternion cameraRotation = TransformConvertor.OpenCVToUnity(TransformConvertor.GetQuaternionFromMatrix(cameraMatrix));


    //        // Transformation Inversion to get Marker Position
    //        cameraMatrix = Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one); // create translation, rotation and scaling matrix
    //        Matrix4x4 markerMatrix = cameraMatrix.inverse; // inverse to get marker rotation matrix
    //        markerMatrix.SetColumn(3, Vector4.zero); // set translation column to zeros
    //        Vector3 markerPos = markerMatrix.MultiplyPoint3x4(cameraPosition); // transform cameraPosition by marker matrix
    //        markerPos = -1 * markerPos;

    //        markerMatrix = AdjustMatrixByScreenOrientation(Matrix4x4.TRS(markerPos, TransformConvertor.GetQuaternionFromMatrix(markerMatrix), Vector3.one));

    //        // Marker Position
    //        GameObject marker = Instantiate(MarkerPositionGameObject); // create marker gameobject as child of the camera
    //        marker.transform.localPosition = ARCameraTransformMatrix.MultiplyPoint3x4(TransformConvertor.GetPositionFromMatrix(markerMatrix)); //ARCamera.TransformPoint(markerPos);
    //        marker.transform.localRotation = TransformConvertor.GetQuaternionFromMatrix(ARCameraTransformMatrix) * TransformConvertor.GetQuaternionFromMatrix(markerMatrix); //ARCamera.transform.rotation * Quaternion.LookRotation(markerMatrix.GetColumn(2), markerMatrix.GetColumn(1)); // get quaternion from rotation matrix
    //        marker.transform.localScale = new Vector3(1f, 1f, 1f);

    //        CreateLocalAnchor(marker);

    //        //if (WorldAnchorLocal != null) {
    //        //    // Camera Position
    //        //    GameObject camera = Instantiate(MarkerPositionGameObject, WorldAnchorLocal.transform);        
    //        //    camera.transform.localPosition = cameraPosition;
    //        //    camera.transform.localRotation = cameraRotation;
    //        //    camera.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    //        //}

    //        //Notifications.Instance.ShowNotification("Camera position", "GetCameraPose inverse false");

    //        markerDetectionState = MarkerDetectionState.Success;

    //    } catch (RequestFailedException ex) {
    //        markerDetectionState = MarkerDetectionState.Failure;
    //        Debug.Log("No markers visible");
    //        Notifications.Instance.ShowNotification("No markers visible", ex.Message);
    //    }
    //}

    public void CreateLocalAnchor(GameObject marker) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        // remove all old local anchors, if there are some (in case we are recalibrating)
        RemoveLocalWorldAnchor();

        //FeatureMapQuality mapQuality = ARAnchorManager.EstimateFeatureMapQualityForHosting(new UnityEngine.Pose(marker.transform.position, marker.transform.rotation));
        //Debug.Log("FeatureMapQuality: " + mapQuality);
        //Notifications.Instance.ShowNotification("FeatureMapQuality", mapQuality.ToString());

        WorldAnchorLocal = marker.AddComponent<ARAnchor>();
        // immediately attach scene to local anchor (after cloud anchor is created, scene will be attached to it)
        AttachScene(WorldAnchorLocal.gameObject);
        GameManager.Instance.Scene.transform.localEulerAngles = new Vector3(0f, 90f, 90f);

        // Create cloud anchor
        if (Settings.Instance.UseCloudAnchors) {
            WorldAnchorCloud = ARAnchorManager.HostCloudAnchor(WorldAnchorLocal);
            StartCoroutine(HostCloudAnchor());
        } else {
            Calibrated = true;
            UsingCloudAnchors = false;
            OnARCalibrated?.Invoke(this, new CalibrationEventArgs(true, WorldAnchorLocal.gameObject));
            //Notifications.Instance.ShowNotification("Calibration successful", "");
            worldAnchorVis = null;
            ActivateCalibrationElements((bool) MainSettingsMenu.Instance.CalibrationElements.GetValue());
        }

        GameManager.Instance.SceneSetActive(true);
        ActivateTrackableMarkers(false);
#endif
    }

    public void RecalibrateUsingServer(bool inverse = false, bool showNotification = false) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        //Calibrated = false;
        //OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
        //OnARRecalibrate?.Invoke(this, new EventArgs());
        StartCoroutine(CalibrateUsingServerAsync(inverse: inverse, force: true, showNotification: showNotification));
#endif
    }

    public void RecalibrateUsingServerAuto() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        //Calibrated = false;
        //OnARCalibrated?.Invoke(this, new CalibrationEventArgs(false, null));
        //OnARRecalibrate?.Invoke(this, new EventArgs());

        if (autoCalibration != null) {
            //Debug.Log("Server: Stopping autocalib");
            StopCoroutine(autoCalibration);
        }
        autoCalibration = StartCoroutine(AutoCalibrate());
#endif
    }

    public async Task ReceiveImageFromCamera() {
        // EXAMPLE OF LOADING IMAGE FROM CAMERA ON SERVER
        string image = await WebsocketManager.Instance.GetCameraColorImage("ID");
        //Debug.Log("RECEIVED Image " + image);
        byte[] bytes = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(image);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        texture.Apply();
        //File.WriteAllBytes(Application.persistentDataPath + "/images/Received.jpg", texture.EncodeToJPG());
        //File.WriteAllBytes(Application.persistentDataPath + "/images/Received.jpg", bytes);
    }

#endregion

}
