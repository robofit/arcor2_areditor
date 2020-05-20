using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using RosSharp.Urdf;
using RosSharp.Urdf.Runtime;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using Base;

public class ImportURDF : Singleton<ImportURDF> {

    //public Dictionary<Robot, string> Robots = new Dictionary<Robot, string>();
    public string CurrentUrdfRoot;

    public bool UseUrdfMaterials = false;

    private void Start() {
        ColladaImporter.Instance.OnModelImported += OnColladaModelImported;
    }


    public List<Base.Robot> DownloadAndBuildURDF(string robotUrl) {
        string extractedData = DownloadUrdfFiles(robotUrl);
        //string extractedData = "";

        //yield return StartCoroutine(DownloadUrdfFiles(robotUrl, callback:returnValue => { extractedData = returnValue; }));

        Debug.Log(extractedData);
        DirectoryInfo dir = new DirectoryInfo(extractedData);
        FileInfo[] files = dir.GetFiles("*.urdf", SearchOption.AllDirectories);

        List<Base.Robot> robots = new List<Base.Robot>();
        foreach (FileInfo file in files) {
            robots.Add(ImportUrdfObject(file.FullName));
        }
        //yield return null;
        return robots;
    }

    public string DownloadUrdfFiles(string robotUrl) {
        // TODO Download URDF data files from server
        Debug.Log("Downloading");
        string downloadedData = "C:/Users/bambu/Documents/VUT/ARCOR2_KINALI/URDF_EXAMPLES/KINALI/AUBO-i5_downloaded.zip";
        //string downloadedData = "C:/Users/bambu/Documents/VUT/ARCOR2_KINALI/URDF_EXAMPLES/PR2_downloaded.zip";
        //string downloadedData = "C:/Users/bambu/Documents/VUT/ARCOR2_KINALI/URDF_EXAMPLES/dobot_magician/DOBOT_MAGICIAN_downloaded.zip";

        if (robotUrl != null)
            downloadedData = robotUrl;

        Directory.CreateDirectory(Application.persistentDataPath + "/robots/");
        string persistentDataPathFilename = Application.persistentDataPath + "/robots/" + Path.GetFileName(downloadedData);
        string extractedFolderName = persistentDataPathFilename.Remove(persistentDataPathFilename.Length - Path.GetExtension(persistentDataPathFilename).Length);

        if (!File.Exists(persistentDataPathFilename)) {
            File.Copy(downloadedData, persistentDataPathFilename);
        }
        if (!Directory.Exists(extractedFolderName)) {
            ExtractZipFile(File.ReadAllBytes(persistentDataPathFilename), extractedFolderName);
        }

        //callback(extractedFolderName);
        //yield return null;
        return extractedFolderName;
    }

    public void ExtractZipFile(byte[] zipFileData, string targetDirectory, int bufferSize = 256 * 1024) {
        Debug.Log(targetDirectory);
        Directory.CreateDirectory(targetDirectory);

        using (MemoryStream fileStream = new MemoryStream()) {
            fileStream.Write(zipFileData, 0, zipFileData.Length);
            fileStream.Flush();
            fileStream.Seek(0, SeekOrigin.Begin);

            ZipFile zipFile = new ZipFile(fileStream);

            foreach (ZipEntry entry in zipFile) {
                string directoryName = Path.GetDirectoryName(entry.Name);
                string fileName = Path.GetFileName(entry.Name);

                if (directoryName != string.Empty) {
                    Directory.CreateDirectory(Path.Combine(targetDirectory, directoryName));
                }

                if (fileName != string.Empty) {
                    using (FileStream outputFile = File.Create(Path.Combine(targetDirectory, entry.Name))) {
                        if (entry.Size > 0) {
                            Stream zippedStream = zipFile.GetInputStream(entry);
                            byte[] dataBuffer = new byte[bufferSize];

                            int readBytes;
                            while ((readBytes = zippedStream.Read(dataBuffer, 0, bufferSize)) > 0) {
                                outputFile.Write(dataBuffer, 0, readBytes);
                                outputFile.Flush();
                            }
                        }
                    }
                }
            }
        }
    }
 

    public Base.Robot ImportUrdfObject(string filename) {

        CurrentUrdfRoot = Path.GetDirectoryName(filename);

        UrdfRobot urdfRobot = UrdfRobotExtensionsRuntime.Create(filename, useUrdfMaterials : UseUrdfMaterials);
        urdfRobot.transform.parent = Scene.Instance.RobotsOrigin.transform;
        urdfRobot.transform.localPosition = Vector3.zero;

        urdfRobot.SetRigidbodiesIsKinematic(true);

        Base.Robot baseRobot = urdfRobot.gameObject.AddComponent<Base.Robot>();        

        baseRobot.LoadLinks();
        baseRobot.SetRandomJointAngles();

        return baseRobot;
    }


    private void OnColladaModelImported(object sender, ImportedColladaEventArgs args) {
        Transform importedModel = args.Data.transform;
        Transform placeholderGameObject = importedModel.parent;
        importedModel.SetParent(placeholderGameObject.parent, false);

        //TODO: Temporarily, colliders are added directly to Visuals
        AddColliders(importedModel.gameObject, setConvex:true);

        Destroy(placeholderGameObject.gameObject);

        Base.Robot robot = importedModel.GetComponentInParent<Base.Robot>();
        if (robot != null) {
            robot.AddLinkVisual(importedModel.parent.parent.parent.name, importedModel.parent.gameObject);
        }
    }

    private static void AddColliders(GameObject gameObject, bool setConvex = false) {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters) {
            GameObject child = meshFilter.gameObject;
            MeshCollider meshCollider = child.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;

            meshCollider.convex = setConvex;
        }
    }
}
