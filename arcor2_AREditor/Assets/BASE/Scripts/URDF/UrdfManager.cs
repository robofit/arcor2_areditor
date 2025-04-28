using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
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
    public event EventHandler<RobotUrdfModelArgs> OnRobotUrdfModelLoaded;

    /// <summary>
    /// Dictionary of all loaded urdf robot models. Key = type of robot (e.g. magician), Value = list of RobotModels (including its instances).
    /// </summary>
    private Dictionary<string, List<RobotModel>> RobotModels = new();

    /// <summary>
    /// Dictionary of all urdf robot source file names (e.g. dobot-magician.zip) and bool value indicating, whether download of these source files is in progress.
    /// // HACK - at the moment, this indicates if it was already downloaded - remove when lastModified will work on project service
    /// (fileName, downloadInProgress)
    /// </summary>
    private Dictionary<string, bool> RobotModelsSources = new();

    /// <summary>
    /// Downloads URDF package for selected robot and stores them to file.
    /// </summary> 
    /// <param name="robotType">Type of robot.</param>
    /// <param name="fileName">Where URDF should be stored.</param>
    /// <returns></returns>
    public IEnumerator DownloadUrdfPackage(string robotType, string fileName) {
        //GameManager.Instance.SetTurboFramerate();

        //Debug.Log("URDF: download started");
        string uri = MainSettingsMenu.Instance.GetAssetFileURI(fileName);
        UnityWebRequest www;
        try {
            www = UnityWebRequest.Get(uri);
        } catch (WebException ex) {
            Notifications.Instance.ShowNotification("Failed to load robot model", ex.Message);
            yield break;
        }
        // Request and wait for the desired page.
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError) {
            Debug.LogError(www.error + " (" + uri + ")");
            Notifications.Instance.ShowNotification("Failed to download URDF", www.error);
        } else {
            string robotDictionary = string.Format("{0}/urdf/{1}/", Application.persistentDataPath, robotType);
            Directory.CreateDirectory(robotDictionary);
            string savePath = string.Format("{0}/{1}", robotDictionary, fileName);
            File.WriteAllBytes(savePath, www.downloadHandler.data);
            string urdfDictionary = string.Format("{0}/{1}", robotDictionary, "urdf");
            try {
                Directory.Delete(urdfDictionary, true);
            } catch (DirectoryNotFoundException) {
                // ok, nothing to delete..
            }
            try {
                ZipFile.ExtractToDirectory(savePath, urdfDictionary);
                //Debug.Log("URDF: zip extracted");
                // HACK: remove when lastModified will work on project service
                // Set to false to indicate that download is not in progress.
                //RobotModelsSources[fileName] = false;
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

    /// <summary>
    /// Builds specified robotType directly from downloaded urdf.
    /// </summary>
    /// <param name="robotType">Type of robot.</param>
    public void BuildRobotModelFromUrdf(string robotType) {
        string pathToUrdf = Application.persistentDataPath + "/urdf/" + robotType + "/urdf/";
        OnUrdfDownloaded(pathToUrdf, robotType);
    }

    /// <summary>
    /// Starts robot model import after the urdf zip was downloaded and extracted.
    /// </summary>
    /// <param name="path">Path to the extracted folder containing urdf files.</param>
    /// <param name="robotType">Type of the robot that will be imported.</param>
    private void OnUrdfDownloaded(string path, string robotType) {
        //Debug.Log("URDF: urdf is downloaded and extracted");
        DirectoryInfo dir = new(path);

        //Debug.Log("URDF: searching directory for urdf file");

        FileInfo[] files = dir.GetFiles("*.urdf", SearchOption.TopDirectoryOnly);

        // if .urdf is missing, try to find .xml file
        if (files.Length == 0) {
            //Debug.Log("URDF: searching directory for xml file");

            files = dir.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
        }

        // import only first found file
        if (files.Length > 0) {
            //Debug.Log("URDF: found file " + files[0].FullName);
            //Debug.Log("URDF: starting collada import");

            ImportUrdfObject(files[0].FullName, robotType);
        }
    }

    private void OnEnable() {
        // subscribe for UrdfImporter event in order to load robot links
        UrdfAssetImporterRuntime.Instance.OnModelImported += OnModelImported;
    }

    private void OnDisable() {
        // unsubscribe for UrdfImporter event
        if(UrdfAssetImporterRuntime.Instance != null)
            UrdfAssetImporterRuntime.Instance.OnModelImported -= OnModelImported;
    }


    /// <summary>
    /// Imports URDF based on a given filename. Filename has to contain a full path.
    /// <param name="filename">Filename including path to the urdf file.</param>
    /// <param name="robotType">Type of the robot.</param>
    /// </summary>
    private void ImportUrdfObject(string filename, string robotType) {
        UrdfRobot urdfRobot = UrdfRobotExtensionsRuntime.Create(filename, true, true);
        urdfRobot.transform.parent = transform;
        urdfRobot.transform.localPosition = Vector3.zero;
        urdfRobot.transform.localEulerAngles = Vector3.zero;

        urdfRobot.SetRigidbodiesIsKinematic(true);

        RobotModel robot = new(robotType, urdfRobot.gameObject);
        robot.LoadLinks();

        RobotModels[robotType].Add(robot);

        //Debug.Log("URDF: robot created (without models yet)");
    }


    /// <summary>
    /// Called upon event OnModelImported in UrdfAssetImporterRuntime, when 3d model is imported.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">Contains imported GameObject.</param>
    private void OnModelImported(object sender, ImportedModelEventArgs args) {
        //Debug.Log("URDF: model imported");
        Transform importedModel = args.RootGameObject.transform;

        UrdfRobot[] urdfRobots = importedModel.GetComponentsInParent<UrdfRobot>(true);
        if (urdfRobots != null) {
            UrdfRobot urdfRobot = urdfRobots[0];

            // TODO: make sure that this robotModel check really works
            // check if imported model corresponds to this robot
            RobotModel robotModel = GetRobotModel(urdfRobot.gameObject);
            if (robotModel != null) {
                if (args.CollidersOnly) {
                    robotModel.SetLinkCollisionLoaded(importedModel.GetComponentsInParent<UrdfLink>(true)[0].name, importedModel.GetComponentsInParent<UrdfCollision>(true)[0]);
                } else {
                    robotModel.SetLinkVisualLoaded(importedModel.GetComponentsInParent<UrdfLink>(true)[0].name, importedModel.GetComponentsInParent<UrdfVisual>(true)[0]);
                }

                //Debug.Log("URDF: model of the link: " + importedModel.parent.parent.parent.parent.name + " imported");
            }
        }
    }



    /// <summary>
    /// Creates identical copy of specified RobotModel.
    /// </summary>
    /// <param name="robotToCopy">RobotModel to copy.</param>
    /// <returns>Copy of specified RobotModel.</returns>
    private RobotModel CopyRobotModel(RobotModel robotToCopy) {
        GameObject robotModelGameObject = Instantiate(robotToCopy.RobotModelGameObject);
        robotModelGameObject.transform.parent = transform;
        robotModelGameObject.transform.localPosition = Vector3.zero;
        robotModelGameObject.transform.localEulerAngles = Vector3.zero;

        RobotModel robot = new(robotToCopy.RobotType, robotModelGameObject);
        robot.LoadLinks(true);
        robot.RobotLoaded = true;

        return robot;
    }

    /// <summary>
    /// Removes all instanciated robot models of specified robotType.
    /// </summary>
    /// <param name="robotType">Type of the robot that will be removed.</param>
    public void RemoveOldModels(string robotType) {
        if (RobotModels.TryGetValue(robotType, out List<RobotModel> robotModels)) {
            foreach (RobotModel robotModel in robotModels) {
                Destroy(robotModel.RobotModelGameObject);
            }
            RobotModels[robotType].Clear();
        }

    }


    /// <summary>
    /// Checks that newer version of robot model exists on the server.
    /// Returns true if so or if there is no downloaded zip file with the robot model at all,
    /// false if downloaded zip file is already at its newest version. 
    /// </summary>
    /// <param name="fileName">Full path with the urdf zip file containing the robot model.</param>
    /// <param name="robotType">Type of the robot.</param>
    /// <returns></returns>
    public bool CheckIfNewerRobotModelExists(string robotType, string fileName) {
        // HACK - remove once lastModified on project service get working again
        if (RobotModelsSources.TryGetValue(fileName, out bool downloadInProgress)) {
            if (downloadInProgress) {
                // download is in progress, return false so the urdf file won't download again
                return false;
            } else {
                // return true and start downloading
                RobotModelsSources[fileName] = true;
                return true;
            }
        } else {
            // Create the entry in RoboModelsSources and set downloadProgress to true and start downloading
            RobotModelsSources.Add(fileName, true);
            return true;
        }

        // at the moment, project service could not provide lastModified property for meshes and URDFs, so it has to be downloaded every time..
        return true;

        FileInfo urdfFileInfo = new(Application.persistentDataPath + "/urdf/" + robotType + "/" + fileName);
        DateTime downloadedZipLastModified = urdfFileInfo.LastWriteTime;
        if (!urdfFileInfo.Exists) {
            //Debug.Log("URDF: URDF zip file of the robot " + robotType + " has to be downloaded.");

            // Check whether downloading can be started and start it, if so.
            return CanIDownload(fileName);
        }
        string uri = MainSettingsMenu.Instance.GetAssetFileURI(fileName);
        try {
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
            HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            // t1 is earlier than t2 --> newer version of urdf is present on the server
            if (DateTime.Compare(downloadedZipLastModified, httpWebResponse.LastModified) < 0) {
                //Debug.Log("URDF: Newer version is present on the server.");
                httpWebResponse.Close();
                // Check whether downloading can be started and start it, if so.
                return CanIDownload(fileName);
            } else {
                // There is no need to download anything, lets return false
                //Debug.Log("URDF: Downloaded version is already the latest one.");
                httpWebResponse.Close();
                return false;
            }
        } catch (WebException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to get robot model", ex.Message);
            return false;
        }
        
    }

    /// <summary>
    /// Checks whether someone else is not downloading specified urdf in given fileName. If not, return true and start downloading.
    /// </summary>
    /// <param name="fileName">Filename of robotType.</param>
    /// <returns></returns>
    private bool CanIDownload(string fileName) {
        // If RobotModelsSources has entry, lets check if download of the urdf is in progress
        if (RobotModelsSources.TryGetValue(fileName, out bool downloadInProgress)) {
            if (downloadInProgress) {
                // download is in progress, return false so the urdf file won't download again
                return false;
            } else {
                // return true and start downloading
                RobotModelsSources[fileName] = true;
                return true;
            }
        } else {
            // Create the entry in RoboModelsSources and set downloadProgress to true and start downloading
            RobotModelsSources.Add(fileName, true);
            return true;
        }
    }


    /// <summary>
    /// Checks if the robot of specified type exists. Returns true if so, false if not.
    /// </summary>
    /// <param name="robotType">Type of the robot.</param>
    /// <returns></returns>
    public bool CheckIfRobotModelExists(string robotType) {
        return RobotModels.ContainsKey(robotType);
    }

    /// <summary>
    /// Returns RobotModel of specified robotType. If there is no free RobotModel, it will create and return a new instance.
    /// </summary>
    /// <param name="robotType">Type of the robot that will be imported.</param>
    /// <param name="urdfDataPackageFilename">Filename of urdf package for correspondig robot of robotType.
    /// If set, process will check if newer version exists on the server and initiates download.</param>
    /// <returns>Instance of new or existing free RobotModel or null, if the robot is imported yet.</returns>
    public RobotModel GetRobotModelInstance(string robotType, string urdfDataPackageFilename = null) {

        if (RobotModels.ContainsKey(robotType)) {
            if (urdfDataPackageFilename != null ? CheckIfNewerRobotModelExists(robotType, urdfDataPackageFilename) : false) {
                RemoveOldModels(robotType);
                StartCoroutine(DownloadUrdfPackage(robotType, urdfDataPackageFilename));
            }
            else {
                if (RobotModels.TryGetValue(robotType, out List<RobotModel> robotModels)) {
                    if (robotModels.Count > 0) {
                        foreach (RobotModel robotModel in robotModels) {
                            // if there is some instanced robot model that is not being used, return its instance
                            if (!robotModel.IsBeingUsed && robotModel.RobotLoaded) {
                                robotModel.IsBeingUsed = true;

                                //RobotUnused.Remove(robotModel.RobotModelGameObject);
                                //RobotUsed.Add(robotModel.RobotModelGameObject);

                                return robotModel;
                            }
                        }
                        if (robotModels[0].RobotLoaded) {
                            // if no free instance of the robot is available, make a new one
                            RobotModel robotModel = CopyRobotModel(robotModels[0]);
                            RobotModels[robotType].Add(robotModel);
                            robotModel.IsBeingUsed = true;

                            //RobotUnused.Remove(robotModel.RobotModelGameObject);
                            //RobotUsed.Add(robotModel.RobotModelGameObject);

                            return robotModel;
                        }
                    } else {
                        return null;
                    }
                }
            }
        }
        // Robot model does not exist, lets create an entry in RobotModels and initiate downloading process
        else {
            
            RobotModels.Add(robotType, new List<RobotModel>() { });
            
            if (urdfDataPackageFilename != null ? CheckIfNewerRobotModelExists(robotType, urdfDataPackageFilename) : false) {
                RemoveOldModels(robotType);
                StartCoroutine(DownloadUrdfPackage(robotType, urdfDataPackageFilename));
            }
            else {
                BuildRobotModelFromUrdf(robotType);
            }
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
        } else {
            //Debug.Log("URDF: Robot model of type: " + robotType + " is not loaded yet.");
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

        //RobotUsed.Remove(robotModel.RobotModelGameObject);
        //RobotUnused.Add(robotModel.RobotModelGameObject);

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
