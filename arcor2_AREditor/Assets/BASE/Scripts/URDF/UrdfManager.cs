using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Base;
using RosSharp.Urdf;
using RosSharp.Urdf.Runtime;
using UnityEngine;
using UnityEngine.Networking;

public class UrdfManager : Singleton<UrdfManager> {

    /// <summary>
    /// Invoked when robot URDF model is fully loaded. Contains type of robot.
    /// Robot model itself needs to be loaded through UrdfManager.Instance.GetRobotModelInstance()
    /// </summary>
    public event AREditorEventArgs.RobotUrdfModelEventHandler OnRobotUrdfModelLoaded;

    /// <summary>
    /// Dictionary of all loaded urdf robot models. Key = type of robot (e.g. magician), Value = list of RobotModels (including its instances).
    /// </summary>
    private Dictionary<string, List<RobotModel>> RobotModels = new Dictionary<string, List<RobotModel>>();
    

    /// <summary>
    /// Downloads URDF package for selected robot and stores them to file.
    /// </summary>
    /// <param name="fileName">Where URDF should be stored.</param>
    /// <param name="robotType">Type of robot.</param>
    /// <returns></returns>
    public IEnumerator DownloadUrdfPackage(string fileName, string robotType) {
        // TODO: Check, if urdf has a new timestamp and needs to be downloaded and rebuilded

        // We will download and build the urdf only if it has not been already downloaded and built
        if (!RobotModels.ContainsKey(robotType)) {
            GameManager.Instance.SetTurboFramerate();

            Debug.Log("URDF: download started");

            string uri = "//" + WebsocketManager.Instance.GetServerDomain() + ":6780/urdf/" + fileName;
            using (UnityWebRequest www = UnityWebRequest.Get(uri)) {
                // Request and wait for the desired page.
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError) {
                    Debug.LogError(www.error + " (" + uri + ")");
                    Notifications.Instance.ShowNotification("Failed to download URDF", www.error);
                } else {
                    string robotDictionary = string.Format("{0}/urdf/{1}/", Application.persistentDataPath, robotType);
                    Directory.CreateDirectory(robotDictionary);
                    string savePath = string.Format("{0}/{1}", robotDictionary, fileName);
                    System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
                    string urdfDictionary = string.Format("{0}/{1}", robotDictionary, "urdf");
                    try {
                        Directory.Delete(urdfDictionary, true);
                    } catch (DirectoryNotFoundException) {
                        // ok, nothing to delete..
                    }

                    try {
                        ZipFile.ExtractToDirectory(savePath, urdfDictionary);
                        Debug.Log("URDF: zip extracted");
                        //OnUrdfReady?.Invoke(this, new RobotUrdfArgs(urdfDictionary, robotType));
                        OnUrdfDownloaded(urdfDictionary, robotType);
                    } catch (Exception ex) when (ex is ArgumentException ||
                                                    ex is ArgumentNullException ||
                                                    ex is DirectoryNotFoundException ||
                                                    ex is PathTooLongException ||
                                                    ex is IOException ||
                                                    ex is FileNotFoundException ||
                                                    ex is InvalidDataException ||
                                                    ex is UnauthorizedAccessException) {
                        Debug.LogError(ex);
                        Notifications.Instance.ShowNotification("Failed to extract URDF", "");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Starts robot model import after the urdf zip was downloaded and extracted.
    /// </summary>
    /// <param name="path">Path to the extracted folder containing urdf files.</param>
    /// <param name="robotType">Type of the robot that will be imported.</param>
    private void OnUrdfDownloaded(string path, string robotType) {
        Debug.Log("URDF: urdf is downloaded and extracted");
        DirectoryInfo dir = new DirectoryInfo(path);

        Debug.Log("URDF: searching directory for urdf file");

        FileInfo[] files = dir.GetFiles("*.urdf", SearchOption.TopDirectoryOnly);

        // if .urdf is missing, try to find .xml file
        if (files.Length == 0) {
            Debug.Log("URDF: searching directory for xml file");

            files = dir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
        }

        // import only first found file
        if (files.Length > 0) {
            Debug.Log("URDF: found file " + files[0].FullName);
            Debug.Log("URDF: starting collada import");

            ImportUrdfObject(files[0].FullName, robotType);

            // subscribe for ColladaImporter event in order to load robot links
            ColladaImporter.Instance.OnModelImported += OnColladaModelImported;
        }
    }


    /// <summary>
    /// Imports URDF based on a given filename. Filename has to contain a full path.
    /// </summary>
    /// <param name="filename">Filename including path to the urdf file.</param>
    private void ImportUrdfObject(string filename, string robotType) {
        UrdfRobot urdfRobot = UrdfRobotExtensionsRuntime.Create(filename, useUrdfMaterials: false);
        urdfRobot.transform.parent = transform;
        urdfRobot.transform.localPosition = Vector3.zero;
        urdfRobot.transform.localEulerAngles = Vector3.zero;

        urdfRobot.SetRigidbodiesIsKinematic(true);

        RobotModel robot = new RobotModel(robotType, urdfRobot.gameObject);
        robot.LoadLinks();

        RobotModels.Add(robotType, new List<RobotModel>() { robot });

        Debug.Log("URDF: robot created (without models yet)");
    }


    /// <summary>
    /// Called upon event ColladaImporter.Instance.OnModelImported, when DAE file is imported.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">Contains imported GameObject.</param>
    private void OnColladaModelImported(object sender, ImportedColladaEventArgs args) {
        Debug.Log("URDF: Collada model imported");
        Transform importedModel = args.Data.transform;

        UrdfRobot[] urdfRobots = importedModel.GetComponentsInParent<UrdfRobot>(true);
        if (urdfRobots != null) {
            UrdfRobot urdfRobot = urdfRobots[0];

            // check if imported model corresponds to this robot
            RobotModel robotModel = GetRobotModel(urdfRobot.gameObject);
            if (robotModel != null) {

                // get rid of the placeholder object (New Game Object)
                Transform placeholderGameObject = importedModel.parent;
                importedModel.SetParent(placeholderGameObject.parent, worldPositionStays: false);

                //TODO: Temporarily, colliders are added directly to Visuals
                AddColliders(importedModel.gameObject, setConvex: true);

                Destroy(placeholderGameObject.gameObject);

                robotModel.SetLinkVisualLoaded(importedModel.parent.parent.parent.name, importedModel.parent.gameObject.GetComponent<UrdfVisual>());

                Debug.Log("URDF: dae model of the link: " + importedModel.parent.parent.parent.name + " imported");
            }
        }
    }

    private void AddColliders(GameObject gameObject, bool setConvex = false) {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters) {
            GameObject child = meshFilter.gameObject;
            MeshCollider meshCollider = child.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;

            meshCollider.convex = setConvex;

            // Add OnClick functionality aswell
            child.AddComponent<OnClickCollider>();
        }
    }

    private RobotModel CopyRobotModel(RobotModel robotToCopy) {
        GameObject robotModelGameObject = Instantiate(robotToCopy.RobotModelGameObject);
        robotModelGameObject.transform.parent = transform;
        robotModelGameObject.transform.localPosition = Vector3.zero;
        robotModelGameObject.transform.localEulerAngles = Vector3.zero;
        RobotModel robot = new RobotModel(robotToCopy.RobotType, robotModelGameObject);

        return robot;
    }

    /// <summary>
    /// Returns RobotModel of specified robotType. If there is no free RobotModel, it will create and return a new instance.
    /// </summary>
    /// <param name="robotType">Type of the robot that will be imported.</param>
    /// <returns></returns>
    public RobotModel GetRobotModelInstance(string robotType) {
        if (RobotModels.TryGetValue(robotType, out List<RobotModel> robotModels)) {
            if (robotModels.Count > 0) {
                foreach (RobotModel robotModel in robotModels) {
                    // if there is some instanced robot model that is not being used, return its instance
                    if (!robotModel.IsBeingUsed) {
                        robotModel.IsBeingUsed = true;
                        return robotModel;
                    }
                }
                // if no free instance of the robot is available, make a new one
                RobotModel robot = CopyRobotModel(robotModels[0]);
                RobotModels[robotType].Add(robot);
                robot.IsBeingUsed = true;
                return robot;
            }

            Debug.LogError("Failed to get robot model instance of type: " + robotType);
        } else {
            Debug.Log("Robot model of type: " + robotType + " is not loaded yet.");
        }

        return null;
    }

    /// <summary>
    /// Returns RobotModel of specified robotType.
    /// </summary>
    /// <param name="robotType">Type of the robot that will be imported.</param>
    /// <returns></returns>
    public RobotModel GetRobotModel(string robotType) {
        if (RobotModels.TryGetValue(robotType, out List<RobotModel> robotModels)) {
            if (robotModels.Count > 0) {
                return robotModels[0];
            }

            Debug.LogError("Failed to get robot model instance of type: " + robotType);
        } else {
            Debug.Log("Robot model of type: " + robotType + " is not loaded yet.");
        }

        return null;
    }

    /// <summary>
    /// Returns RobotModel of specified robotModelGameObject (by comparing references).
    /// </summary>
    /// <param name="robotModelGameObject"></param>
    /// <returns></returns>
    public RobotModel GetRobotModel(GameObject robotModelGameObject) {
        foreach (List<RobotModel> robotModels in RobotModels.Values) {
            foreach (RobotModel robotModel in robotModels) {
                if (ReferenceEquals(robotModel.RobotModelGameObject, robotModelGameObject)) {
                    return robotModel;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Puts specified RobotModel back to the RobotModels dictionary "pool" for further use by another RobotActionObject.
    /// </summary>
    /// <param name="robotModel">RobotModel to be put back to the RobotModels dictionary.</param>
    public void ReturnRobotModelInstace(RobotModel robotModel) {

        robotModel.RobotModelGameObject.transform.parent = transform;
        robotModel.RobotModelGameObject.transform.localPosition = Vector3.zero;
        robotModel.RobotModelGameObject.transform.localEulerAngles = Vector3.zero;

        robotModel.IsBeingUsed = false;
        robotModel.SetActiveAllVisuals(false);

        // retarget OnClickCollider target to receive OnClick events
        foreach (OnClickCollider onCLick in robotModel.RobotModelGameObject.GetComponentsInChildren<OnClickCollider>(true)) {
            onCLick.Target = null;
        }
    }

    /// <summary>
    /// Called from RobotModel when the model is imported and fully loaded.
    /// </summary>
    /// <param name="robotType">Type of the robot that was imported.</param>
    public void RobotModelLoaded(string robotType) {
        GameManager.Instance.SetDefaultFramerate();
        OnRobotUrdfModelLoaded?.Invoke(this, new RobotUrdfModelArgs(robotType));
    }

}
