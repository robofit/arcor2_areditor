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
    public bool Calibrated = false;

    public delegate void ARCalibratedEventHandler(object sender, GameObjectEventArgs args);
    public event ARCalibratedEventHandler OnARCalibrated;

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
            OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorLocal.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            worldAnchorVis = null;
            ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
        }

        GameManager.Instance.SceneSetActive(true);
        ActivateTrackableMarkers(false);
#endif
    }

    public void Recalibrate() {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        HideCurrentWorldAnchor();
        ActivateTrackableMarkers(true);
        Calibrated = false;
        OnARRecalibrate(this, new EventArgs());
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
            OnARCalibrated?.Invoke(this, new GameObjectEventArgs(WorldAnchorCloud.gameObject));
            Notifications.Instance.ShowNotification("Calibration successful", "");
            ActivateCalibrationElements(ControlBoxManager.Instance.CalibrationElementsToggle.isOn);
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
                GameManager.Instance.SceneSetActive(true);
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
            projectionMatrix = arCameraArgs.projectionMatrix;
            if (ARCameraManager.descriptor.supportsCameraConfigurations) {
                cameraConfiguration = ARCameraManager.currentConfiguration;
                Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
                Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);
            }
        }
    }

    public void CalibrateUsingServerAsync(bool inverse = false) {
        Debug.Log("Calibrating using server async");

        if (!ARCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
            return;

        // Get information about the device camera image.
        if (ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
            // If successful, launch a coroutine that waits for the image
            // to be ready, then apply it to a texture.
            StartCoroutine(ProcessImage(image, cameraIntrinsics, inverse));

            // It's safe to dispose the image before the async operation completes.
            image.Dispose();
        }
    }

    IEnumerator ProcessImage(XRCpuImage image, XRCameraIntrinsics cameraIntrinsics, bool inverse = false) {
        Debug.Log("Processing image");

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
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();

        // Create a texture if necessary.
        Texture2D m_Texture = new Texture2D(
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

        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/image.jpg", m_Texture.EncodeToJPG());

        Debug.Log("Image size: " + request.conversionParams.outputDimensions.x + " x " + request.conversionParams.outputDimensions.y);
        Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
        Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);


        CameraParameters cameraParams = new CameraParameters(cx: (decimal) cameraIntrinsics.principalPoint.x,
                                                     cy: (decimal) cameraIntrinsics.principalPoint.y,
                                                     distCoefs: new List<decimal>() { 0, 0, 0, 0 },
                                                     fx: (decimal) cameraIntrinsics.focalLength.x,
                                                     fy: (decimal) cameraIntrinsics.focalLength.y);
        Debug.Log(cameraParams.ToString());

        if (inverse) {
            GetMarkerPosition(cameraParams, imageString);
        } else {
            GetCameraPosition(cameraParams, imageString);
        }

        //GetMarkerCornersPosition(cameraParams, imageString);
    }

    //public unsafe void CalibrateUsingServer() {
    //    Debug.Log("Calibrating using server");
    //    if (!ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    //        return;
    //    Debug.Log("Got latest CPU image");
    //    if (!ARCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
    //        return;
    //    Debug.Log("Got intrinsics");
    //    var conversionParams = new XRCpuImage.ConversionParams {
    //        // Get the entire image.
    //        inputRect = new RectInt(0, 0, image.width, image.height),

    //        // Downsample by 2.
    //        outputDimensions = new Vector2Int(image.width, image.height),

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
    //    try {
    //        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
    //    } catch (InvalidOperationException e) {
    //        Debug.LogError(e);
    //    }

    //    // The image was converted to RGBA32 format and written into the provided buffer
    //    // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
    //    image.Dispose();

    //    // At this point, you can process the image, pass it to a computer vision algorithm, etc.
    //    // In this example, you apply it to a texture to visualize it.

    //    // You've got the data; let's put it into a texture so you can visualize it.
    //    Texture2D texture = new Texture2D(
    //        conversionParams.outputDimensions.x,
    //        conversionParams.outputDimensions.y,
    //        conversionParams.outputFormat,
    //        false);
    //    Debug.Log("Image size: " + conversionParams.outputDimensions.x + " x " + conversionParams.outputDimensions.y);
    //    Debug.Log("Camera Resolution: " + cameraConfiguration.Value);
    //    Debug.Log("Camera width: " + cameraConfiguration.Value.width + " height: " + cameraConfiguration.Value.height + " framerate: " + cameraConfiguration.Value.framerate);

    //    texture.LoadRawTextureData(buffer);
    //    texture.Apply();

    //    // Done with your temporary data, so you can dispose it.
    //    buffer.Dispose();

    //    string imageString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(texture.EncodeToJPG());


    //    CameraParameters cameraParams = new CameraParameters(cx: (decimal) cameraIntrinsics.principalPoint.x,
    //                                                         cy: (decimal) cameraIntrinsics.principalPoint.y,
    //                                                         distCoefs: new List<decimal>() { 0, 0, 0, 0 },
    //                                                         fx: (decimal) cameraIntrinsics.focalLength.x,
    //                                                         fy: (decimal) cameraIntrinsics.focalLength.y);

    //    GetCameraPosition(cameraParams, imageString);
    //    GetMarkerCornersPosition(cameraParams, imageString);
    //}

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

    public async Task GetMarkerPosition(CameraParameters cameraParams, string image) {
        try {            
            // receive cameraPose from server
            IO.Swagger.Model.EstimatedPose markerEstimatedPose = await WebsocketManager.Instance.GetCameraPose(cameraParams, image, inverse:true);
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

            Notifications.Instance.ShowNotification("Marker position", "GetCameraPose inverse true");

        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
        }
    }

    private Matrix4x4 AdjustMatrixByScreenOrientation(Matrix4x4 m) {
        Debug.Log(Screen.orientation);
        Notifications.Instance.ShowNotification(Screen.orientation.ToString(), "");
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


    public async Task GetCameraPosition(CameraParameters cameraParams, string image) {
        try {
            // receive cameraPose from server
            IO.Swagger.Model.EstimatedPose cameraEstimatedPose = await WebsocketManager.Instance.GetCameraPose(cameraParams, image, inverse:false);
            IO.Swagger.Model.Pose cameraPose = cameraEstimatedPose.Pose;

            Vector3 cameraPositionReceived = new Vector3((float) cameraPose.Position.X, (float) cameraPose.Position.Y, (float) cameraPose.Position.Z);
            Quaternion cameraRotationReceived = new Quaternion((float) cameraPose.Orientation.X, (float) cameraPose.Orientation.Y,
                                                                (float) cameraPose.Orientation.Z, (float) cameraPose.Orientation.W);

            //Matrix4x4 cameraMatrix = AdjustMatrixByScreenOrientation(Matrix4x4.TRS(cameraPositionReceived, cameraRotationReceived, Vector3.one));
            Matrix4x4 cameraMatrix = Matrix4x4.TRS(cameraPositionReceived, cameraRotationReceived, Vector3.one);

            Vector3 cameraPosition = TransformConvertor.OpenCVToUnity(TransformConvertor.GetPositionFromMatrix(cameraMatrix));
            Quaternion cameraRotation = TransformConvertor.OpenCVToUnity(TransformConvertor.GetQuaternionFromMatrix(cameraMatrix));


            // Transformation Inversion to get Marker Position
            cameraMatrix = Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one); // create translation, rotation and scaling matrix
            Matrix4x4 markerMatrix = cameraMatrix.inverse; // inverse to get marker rotation matrix
            markerMatrix.SetColumn(3, Vector4.zero); // set translation column to zeros
            Vector3 markerPos = markerMatrix.MultiplyPoint3x4(cameraPosition); // transform cameraPosition by marker matrix
            markerPos = -1 * markerPos;

            markerMatrix = AdjustMatrixByScreenOrientation(Matrix4x4.TRS(markerPos, TransformConvertor.GetQuaternionFromMatrix(markerMatrix), Vector3.one));

            // Marker Position
            GameObject marker = Instantiate(MarkerPositionGameObject); // create marker gameobject as child of the camera
            marker.transform.localPosition = ARCameraTransformMatrix.MultiplyPoint3x4(TransformConvertor.GetPositionFromMatrix(markerMatrix)); //ARCamera.TransformPoint(markerPos);
            marker.transform.localRotation = TransformConvertor.GetQuaternionFromMatrix(ARCameraTransformMatrix) * TransformConvertor.GetQuaternionFromMatrix(markerMatrix); //ARCamera.transform.rotation * Quaternion.LookRotation(markerMatrix.GetColumn(2), markerMatrix.GetColumn(1)); // get quaternion from rotation matrix
            marker.transform.localScale = new Vector3(1f, 1f, 1f);

            CreateLocalAnchor(marker);

            //if (WorldAnchorLocal != null) {
            //    // Camera Position
            //    GameObject camera = Instantiate(MarkerPositionGameObject, WorldAnchorLocal.transform);        
            //    camera.transform.localPosition = cameraPosition;
            //    camera.transform.localRotation = cameraRotation;
            //    camera.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            //}

            Notifications.Instance.ShowNotification("Camera position", "GetCameraPose inverse false");

        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to calibrate camera", ex.Message);
        }
    }

    public void CreateLocalAnchor(GameObject marker) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        // remove all old local anchors, if there are some (in case we are recalibrating)
        RemoveLocalWorldAnchor();

        WorldAnchorLocal = marker.AddComponent<ARAnchor>();
        // immediately attach scene to local anchor (after cloud anchor is created, scene will be attached to it)
        AttachScene(WorldAnchorLocal.gameObject);
        //GameManager.Instance.Scene.transform.localEulerAngles = new Vector3(0f, 90f, 90f);

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

        GameManager.Instance.SceneSetActive(true);
        ActivateTrackableMarkers(false);
#endif
    }

    public void RecalibrateUsingServer(bool inverse = false) {
#if (UNITY_ANDROID || UNITY_IOS) && AR_ON
        Calibrated = false;
        OnARRecalibrate(this, new EventArgs());
        CalibrateUsingServerAsync(inverse);
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
}
