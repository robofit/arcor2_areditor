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
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
using Google.XR.ARCoreExtensions;
#endif


public class CalibrationManager : Singleton<CalibrationManager> {

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
    public bool Calibrated = false;

    public delegate void ARCalibratedEventHandler(object sender, GameObjectEventArgs args);
    public event ARCalibratedEventHandler OnARCalibrated;

    public delegate void ARRecalibrateEventHandler(object sender, EventArgs args);
    public event ARRecalibrateEventHandler OnARRecalibrate;

    private bool activateTrackableMarkers = false;

    [HideInInspector]
    public GameObject worldAnchorVis;

    public GameObject MarkerPositionGameObject;

    private bool firstFrameReceived = false;
    private XRCameraConfiguration? cameraConfiguration;
    private Matrix4x4? displayMatrix;

#if UNITY_STANDALONE || !AR_ON
    private void Start() {
        Calibrated = true;
        
    }
#endif

#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
    private void OnEnable() {
        GameManager.Instance.OnConnectedToServer += ConnectedToServer;
        ARTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
        ARCameraManager.frameReceived += FrameReceived;
    }

    private void OnDisable() {
        ARCameraManager.frameReceived -= FrameReceived;
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

        WorldAnchorLocal = ARAnchorManager.AddAnchor(new UnityEngine.Pose(hitPose != new UnityEngine.Pose() ? hitPose.position : tf.position,
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
            OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorLocal.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            worldAnchorVis = null;
            ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
        }

        GameManager.Instance.Scene.SetActive(true);
        ActivateTrackableMarkers(false);
#endif
    }

    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        HideCurrentWorldAnchor();
        ActivateTrackableMarkers(true);
        Calibrated = false;
        OnARRecalibrate(this, new EventArgs());
        GameManager.Instance.Scene.SetActive(false);
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
            OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorCloud.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
            GameManager.Instance.Scene.SetActive(true);
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
            }
            //TODO If anchor is not present in the system, play animation to manually calibrate by clicking on marker
            else {
                Calibrated = false;
                Notifications.Instance.ShowNotification("Calibrate by clicking on the Calibration cube", "Cloud anchor does not exist. Please calibrate manually by clicking on the cube displayed on your marker");

                //AttachScene(null, initLocalAnchor: true);

                ActivateTrackableMarkers(true);
            }
        } else {
            Calibrated = false;
            Notifications.Instance.ShowNotification("Calibrate by clicking on the Calibration cube", "Please calibrate manually by clicking on the cube displayed on your marker");

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
            WorldAnchorLocal.GetComponent<Recalibrate>().Enable(active);
        }
    }
#endif

    //private void RegisterFramesReceiving() {
    //    ARCameraManager.frameReceived += OnCameraFrameReceived;
    //}

    //private void UnregisterFramesReceiving() {
    //    ARCameraManager.frameReceived -= OnCameraFrameReceived;
    //}

    //private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
    //    if (!ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    //        return;

    //    var conversionParams = new XRCpuImage.ConversionParams {
    //        // Get the entire image.
    //        inputRect = new RectInt(0, 0, image.width, image.height),

    //        // Downsample by 2.
    //        //outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

    //        // Choose RGBA format.
    //        outputFormat = TextureFormat.RGBA32,

    //        // Flip across the vertical axis (mirror image).
    //        transformation = XRCpuImage.Transformation.MirrorY
    //    };

    //    // See how many bytes you need to store the final image.
    //    int size = image.GetConvertedDataSize(conversionParams);

    //    // Allocate a buffer to store the image.
    //    var buffer = new NativeArray<byte>(size, Allocator.Temp);

    //    // Extract the image data
    //    image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

    //    // The image was converted to RGBA32 format and written into the provided buffer
    //    // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
    //    image.Dispose();

    //    // At this point, you can process the image, pass it to a computer vision algorithm, etc.
    //    // In this example, you apply it to a texture to visualize it.

    //    // You've got the data; let's put it into a texture so you can visualize it.
    //    Texture2D m_Texture = new Texture2D(
    //        conversionParams.outputDimensions.x,
    //        conversionParams.outputDimensions.y,
    //        conversionParams.outputFormat,
    //        false);

    //    m_Texture.LoadRawTextureData(buffer);
    //    m_Texture.Apply();

    //    // Done with your temporary data, so you can dispose it.
    //    buffer.Dispose();
    //}


    private void FrameReceived(ARCameraFrameEventArgs arCameraArgs) {
        if (!firstFrameReceived) {
            firstFrameReceived = true;

            displayMatrix = arCameraArgs.displayMatrix;
            if (ARCameraManager.descriptor.supportsCameraConfigurations) {
                cameraConfiguration = ARCameraManager.currentConfiguration;
                Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
                Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);
            }
        }
    }

    public unsafe void CalibrateUsingServer() {
        if (!ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        if (!ARCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
            return;
        
        var conversionParams = new XRCpuImage.ConversionParams {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width, image.height),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        Texture2D texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);
        Debug.Log("Image size: " + conversionParams.outputDimensions.x + " x " + conversionParams.outputDimensions.y);
        Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
        Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);

        texture.LoadRawTextureData(buffer);
        texture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        string imageString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(texture.EncodeToJPG());


        CameraParameters cameraParams = new CameraParameters(cx: (decimal) cameraIntrinsics.principalPoint.x,
                                                             cy: (decimal) cameraIntrinsics.principalPoint.y,
                                                             distCoefs: new List<decimal>() { 0, 0, 0, 0 },
                                                             fx: (decimal) cameraIntrinsics.focalLength.x,
                                                             fy: (decimal) cameraIntrinsics.focalLength.y);

        //GetCameraPosition(cameraParams, imageString);
        GetMarkerCornersPosition(cameraParams, imageString);     
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


    public async Task GetCameraPosition(CameraParameters cameraParams, string image) {
        try {
            IO.Swagger.Model.Pose cameraPose = await WebsocketManager.Instance.GetCameraPose(cameraParams, image);
            Vector3 cameraPosition = TransformConvertor.ROSToUnity(new Vector3((float) cameraPose.Position.X, (float) cameraPose.Position.Y, (float) cameraPose.Position.Z));
            Quaternion cameraRotation = TransformConvertor.ROSToUnity(new Quaternion((float) cameraPose.Orientation.X, (float) cameraPose.Orientation.Y, (float) cameraPose.Orientation.Z, (float) cameraPose.Orientation.W));
            Vector3 cameraPositionOrig = new Vector3((float) cameraPose.Position.X, (float) cameraPose.Position.Y, (float) cameraPose.Position.Z);
            Quaternion cameraRotationOrig = new Quaternion((float) cameraPose.Orientation.X, (float) cameraPose.Orientation.Y, (float) cameraPose.Orientation.Z, (float) cameraPose.Orientation.W);
            Debug.Log("RECEIVED camera position: " + cameraPositionOrig + " and orientation: " + cameraRotationOrig);
            Debug.Log("RECEIVED camera position transformed: " + cameraPosition + " and orientation: " + cameraRotation);
            Debug.Log("RECEIVED marker position: " + cameraPositionOrig * -1f + " and orientation: " + Quaternion.Inverse(cameraRotationOrig));
            Debug.Log("RECEIVED marker position transformed: " + cameraPosition * -1f + " and orientation: " + Quaternion.Inverse(cameraRotation));

            Matrix4x4 cameraMatrix = Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one);
            Matrix4x4 cameraMatrixROS = Matrix4x4.TRS(cameraPositionOrig, cameraRotationOrig, Vector3.one);
            Matrix4x4 markerMatrix = cameraMatrix.inverse;
            Matrix4x4 markerMatrixROS = cameraMatrixROS.inverse;

            GameObject marker = Instantiate(MarkerPositionGameObject, ARCamera);
            marker.transform.localPosition = markerMatrix.GetColumn(3);
            marker.transform.localRotation = Quaternion.LookRotation(markerMatrix.GetColumn(2), markerMatrix.GetColumn(1));
            marker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            GameObject marker1 = Instantiate(MarkerPositionGameObject, ARCamera);
            marker1.transform.localPosition = TransformConvertor.ROSToUnity(markerMatrixROS.GetColumn(3));
            marker1.transform.localRotation = TransformConvertor.ROSToUnity(Quaternion.LookRotation(markerMatrixROS.GetColumn(2), markerMatrixROS.GetColumn(1)));
            marker1.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);


            //GameObject markerVis = Instantiate(MarkerPositionGameObject, ARCamera);
            //markerVis.transform.localPosition = cameraPosition * -1f;
            //markerVis.transform.localRotation = Quaternion.Inverse(cameraRotation);
            //markerVis.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            //GameObject markerVisOrig = Instantiate(MarkerPositionGameObject, ARCamera);
            //markerVisOrig.transform.localPosition = cameraPositionOrig * -1f;
            //markerVisOrig.transform.localRotation = Quaternion.Inverse(cameraRotationOrig);
            //markerVisOrig.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);


            //GameObject cameraVis = Instantiate(MarkerPositionGameObject, ARCamera);
            //cameraVis.transform.localPosition = cameraPosition;
            //cameraVis.transform.localRotation = cameraRotation;
            //cameraVis.transform.localScale = new Vector3(0.4f, 0.2f, 0.2f);

            //GameObject cameraVisOrig = Instantiate(MarkerPositionGameObject, ARCamera);
            //cameraVisOrig.transform.localPosition = cameraPositionOrig;
            //cameraVisOrig.transform.localRotation = cameraRotationOrig;
            //cameraVisOrig.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);

        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
        }
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
}
