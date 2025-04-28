using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using Base;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using Mesh = Arcor2.ClientSdk.Communication.OpenApi.Models.Mesh;

public class MeshImporter : Singleton<MeshImporter> {

    public delegate void ImportedMeshEventHandler(object sender, ImportedMeshEventArgs args);
    public event ImportedMeshEventHandler OnMeshImported;

    /// <summary>
    /// Dictionary of all urdf robot source file names (e.g. dobot-magician.zip) and bool value indicating, whether download of these source files is in progress.
    /// (fileName, downloadInProgress)
    /// </summary>
    private Dictionary<string, bool> meshSources = new();

    /// <summary>
    /// Loads mesh - takes care of downloading, updating and importing mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="aoId">ID of action object which is asociated with the mesh</param>
    public void LoadModel(Mesh mesh, string aoId) {
        if (CheckIfNewerRobotModelExists(mesh.Id, mesh.AssetId)) {
            StartCoroutine(DownloadMesh(mesh.Id, mesh.AssetId, aoId));
        } else {
            StartCoroutine(ImportMeshWhenReady(string.Format("{0}/meshes/{1}/{2}", Application.persistentDataPath, mesh.Id, mesh.AssetId), aoId, mesh.AssetId));
        }
    }

    private IEnumerator ImportMeshWhenReady(string path, string aoId, string fileName) {
        yield return new WaitUntil(() => meshSources[fileName] == false);
        ImportMesh(path, aoId);
    }

    /// <summary>
    /// Imports model of type DAE, FBX, OBJ, GLTF2, STL, PLY, 3MF into placeholder object, which is returned immediately.
    /// After the model itself is imported, the OnMeshImported action is triggered.
    /// </summary>
    /// <param name="path">Full path of the model to be imported.</param>
    /// <param name="aoId">ID of action object which is asociated with mesh</param>
    /// <returns></returns>
    private void ImportMesh(string path, string aoId) {

        GameObject loadedObject = new("ImportedMeshObject");
        if (Path.GetExtension(path).ToLower() == ".dae") {
            //Debug.LogError("importing dae mesh name: " + path);
            StreamReader reader = File.OpenText(path);
            string daeFile = reader.ReadToEnd();

            // Requires Simple Collada asset from Unity Asset Store: https://assetstore.unity.com/packages/tools/input-management/simple-collada-19579
            // Supports: DAE
            StartCoroutine(ColladaImporter.Instance.ImportAsync(daeFile, Quaternion.identity, Vector3.one, Vector3.zero,
                delegate (GameObject loadedGameObject) {
                    OnMeshImported?.Invoke(this, new ImportedMeshEventArgs(loadedGameObject, aoId));
                },
                loadedObject));

        } else {
            // Requires Trilib 2 asset from Unity Asset Store: https://assetstore.unity.com/packages/tools/modeling/trilib-2-model-loading-package-157548
            // Supports: FBX, OBJ, GLTF2, STL, PLY, 3MF
            AssetLoaderOptions assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            AssetLoader.LoadModelFromFile(path, null, delegate (AssetLoaderContext assetLoaderContext) {
                if (Path.GetExtension(path).ToLower() == ".stl") {
                    assetLoaderContext.RootGameObject.transform.Rotate(0f, 90f, 0f);
                }

                OnMeshImported?.Invoke(this, new ImportedMeshEventArgs(assetLoaderContext.WrapperGameObject, aoId));
            }, null, assetLoaderOptions: assetLoaderOptions, onError: OnModelLoadError, wrapperGameObject: loadedObject);
        }
    }

    /// <summary>
    /// Downloads mesh and saves it into persistent storage
    /// </summary> 
    /// <param name="meshId"></param>
    /// <param name="uri">Where should be the mesh download from.</param>
    /// <param name="aoId">ID of action object which is asociated with mesh</param>
    /// <returns></returns>
    private IEnumerator DownloadMesh(string meshId, string fileName, string aoId) {

        //Debug.LogError("MESH: download started");
        string uri = MainSettingsMenu.Instance.GetAssetFileURI(fileName);
        Debug.LogError(uri);
        using (UnityWebRequest www = UnityWebRequest.Get(uri)) {
            // Request and wait for the desired page.
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError) {
                //Debug.LogError(www.error + " (" + uri + ")");
                Notifications.Instance.ShowNotification("Failed to download mesh", www.error);
            } else {
                string meshDirectory = string.Format("{0}/meshes/{1}", Application.persistentDataPath, meshId);
                Directory.CreateDirectory(meshDirectory);
                string savePath = string.Format("{0}/{1}", meshDirectory, fileName);
                File.WriteAllBytes(savePath, www.downloadHandler.data);
                meshSources[fileName] = false;

                //Debug.LogError("MESH: download finished");
                //if the mesh is zipped, extract it
                if (Path.GetExtension(savePath).ToLower() == ".zip") {
                    string meshUnzipDirectory = string.Format("{0}/{1}", meshDirectory, "mesh");
                    try {
                        Directory.Delete(meshUnzipDirectory, true);
                    } catch (DirectoryNotFoundException) {
                        // ok, nothing to delete..
                    }
                    try {
                        ZipFile.ExtractToDirectory(savePath, meshUnzipDirectory);
                        OnMeshDownloaded(meshUnzipDirectory, meshId, aoId);
                    } catch (Exception ex) when (ex is ArgumentException ||
                                                    ex is ArgumentNullException ||
                                                    ex is DirectoryNotFoundException ||
                                                    ex is PathTooLongException ||
                                                    ex is IOException ||
                                                    ex is FileNotFoundException ||
                                                    ex is InvalidDataException ||
                                                    ex is UnauthorizedAccessException) {
                        Debug.LogError(ex);
                        Notifications.Instance.ShowNotification("Failed to extract mesh", "");
                    }
                } else { //not *.zip
                    OnMeshDownloaded(meshId, savePath, aoId);
                }
            }
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="meshID"></param>
    /// <param name="path">Full path to mesh, except zip files - then it is path to extracted directory</param>
    /// <param name="aoId">Id of action object which is asociated with mesh</param>
    private void OnMeshDownloaded(string meshID, string path, string aoId) {
        if (Path.GetExtension(path).ToLower() == ".zip") {
            //todo find the object file
        } else {
            ImportMesh(path, aoId);
        }
    }

    private string GetPathToMesh(string meshId) {
        if (Path.GetExtension(meshId).ToLower() == ".zip") {
            string path = string.Format("{0}/meshes/{1}/mesh/", Application.persistentDataPath, meshId);
            string[] extensions = { "dae", "fbx", "obj", "gltf2", "stl", "ply", "3mf" };
            string[] files = { };
            foreach (var extension in extensions) {
                files = Directory.GetFiles(path, "*." + extension);
                if (files.Length > 0)
                    return files[0];
            }
            throw new FileNotFoundException();
        } else {
            return string.Format("{0}/meshes/{1}/{1}", Application.persistentDataPath, meshId);
        }
    }

    private void OnModelLoadError(IContextualizedError obj) {
        Notifications.Instance.ShowNotification("Unable to show mesh ", obj.GetInnerException().Message);
        Debug.LogError(obj.GetInnerException().Message);
    }

    /// <summary>
    /// Checks that newer version of mesh exists on the server.
    /// Returns true if so or if there is no downloaded zip file with the robot model at all,
    /// false if downloaded zip file is already at its newest version. 
    /// </summary>
    /// <param name="meshId"></param>
    /// <param name="uri">Where the mesh should be downloaded from</param>
    /// <returns></returns>
    public bool CheckIfNewerRobotModelExists(string meshId, string fileName) {

        // at the moment, project service could not provide lastModified property for meshes and URDFs, so it has to be downloaded every time..
        if (meshSources.TryGetValue(fileName, out bool downloadInProgress)) {
            if (downloadInProgress) {
                // download is in progress, return false so the urdf file won't download again
                return false;
            } else {
                // return true and start downloading
                meshSources[fileName] = true;
                return true;
            }
        } else {
            // Create the entry in RoboModelsSources and set downloadProgress to true and start downloading
            meshSources.Add(fileName, true);
            return true;
        }

        //Debug.LogError("mesh: Checking if newer  mesh exists " + meshId);
        FileInfo meshFileInfo = new(Application.persistentDataPath + "/meshes/" + meshId + "/" + fileName);
        if (!meshFileInfo.Exists) {
            //Debug.LogError("mesh: mesh file " + meshId + " has to be downloaded.");
            // Check whether downloading can be started and start it, if so.
            return CanIDownload(meshId);
        }

        string uri = MainSettingsMenu.Instance.GetAssetFileURI(fileName);

        DateTime downloadedZipLastModified = meshFileInfo.LastWriteTime;
        try {
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
            HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            if (DateTime.Compare(downloadedZipLastModified, httpWebResponse.LastModified) < 0) {
                //Debug.LogError("mesh: Newer version is present on the server.");
                httpWebResponse.Close();
                // Check whether downloading can be started and start it, if so.
                return CanIDownload(meshId);
            } else {
                // There is no need to download anything, lets return false
                //Debug.LogError("mesh: Downloaded version is already the latest one.");
                httpWebResponse.Close();
                return false;
            }
        } catch (WebException ex) {
            //Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to get robot model", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Checks whether someone else is not downloading specified mesh in given meshId. If not, return true and start downloading.
    /// </summary>
    /// <param name="meshId"></param>
    /// <returns></returns>
    private bool CanIDownload(string meshId) {
        // If RobotModelsSources has entry, lets check if download of the urdf is in progress
        if (meshSources.TryGetValue(meshId, out bool downloadInProgress)) {
            if (downloadInProgress) {
                // download is in progress, return false so the urdf file won't download again
                return false;
            } else {
                // return true and start downloading
                meshSources[meshId] = true;
                return true;
            }
        } else {
            // Create the entry in RoboModelsSources and set downloadProgress to true and start downloading
            meshSources.Add(meshId, true);
            return true;
        }
    }
}


/// <summary>
/// Used when model is imported.
/// </summary>
public class ImportedMeshEventArgs : EventArgs {
    /// <summary>
    /// Imported GameObject.
    /// </summary>
    public GameObject RootGameObject {
        get; set;
    }

    public string Name {
        get; set;
    }

    public ImportedMeshEventArgs(GameObject gameObject, string name) {
        RootGameObject = gameObject;
        Name = name;
    }
}

