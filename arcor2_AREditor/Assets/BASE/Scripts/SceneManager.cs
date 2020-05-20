using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace Base {

    public class RobotUrdfArgs : EventArgs {
        public string RobotType {
            get; set;
        }

        public string Path {
            get; set;
        }

        public RobotUrdfArgs(string path, string robotType) {
            RobotType = robotType;
            Path = path;
        }
    }


    public class SceneManager : Singleton<SceneManager> {

        public delegate void RobotUrdfEventHandler(object sender, RobotUrdfArgs args);

        public IO.Swagger.Model.Scene Scene = null;

       // string == IO.Swagger.Model.Scene Data.Id
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        
        public GameObject ActionObjectsSpawn, SceneOrigin;

        
        public GameObject RobotPrefab, ActionObjectPrefab;

        public GameObject CurrentlySelectedObject;

        public LineConnectionsManager AOToAPConnectionsManager;
        public GameObject LineConnectionPrefab, RobotEEPrefab;

        private bool sceneActive = true;

        public bool ActionObjectsInteractive, ActionObjectsVisible;

        public bool RobotsEEVisible {
            get;
            private set;
        }

        
        private Dictionary<string, RobotEE> EndEffectors = new Dictionary<string, RobotEE>();

        private Dictionary<string, List<string>> robotsWithEndEffector = new Dictionary<string, List<string>>();


        public event EventHandler OnLoadScene;
        public event RobotUrdfEventHandler OnUrdfReady;

        private bool loadResources = false;



        /// <summary>
        /// Creates project from given json
        /// </summary>
        /// <param name="project"></param>
        public async Task<bool> CreateScene(IO.Swagger.Model.Scene scene, bool loadResources, CollisionModels customCollisionModels = null) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (Scene != null)
                return false;
           
            Scene = scene;
            this.loadResources = loadResources;
            LoadSettings();
            bool success = await UpdateScene(scene, customCollisionModels);
            
            if (success) {
                OnLoadScene?.Invoke(this, EventArgs.Empty);
            }

            // TODO - do this when robot is added to scene
            foreach (KeyValuePair<string, RobotMeta> robotMeta in ActionsManager.Instance.RobotsMeta) {
                if (!string.IsNullOrEmpty(robotMeta.Value.UrdfPackageFilename)) {
                    StartCoroutine(DownloadUrdfPackage(robotMeta.Value.UrdfPackageFilename, robotMeta.Key));
                }
            }
            return success;
        }

        /// <summary>
        /// Updates project from given json
        /// </summary>
        /// <param name="project"></param>
        public async Task<bool> UpdateScene(IO.Swagger.Model.Scene scene, CollisionModels customCollisionModels = null) {
            if (scene.Id != Scene.Id)
                return false;
            Scene = scene;
            await UpdateActionObjects(customCollisionModels);
            await UpdateServices();
            return true;
        }

        public bool DestroyScene() {
            RemoveActionObjects();
            Scene = null;
            return true;
        }





        private IEnumerator DownloadUrdfPackage(string fileName, string robotType) {
            string uri = WebsocketManager.Instance.GetServerDomain() + ":6780/urdf/" + fileName;
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
                        OnUrdfReady?.Invoke(this, new RobotUrdfArgs(urdfDictionary, robotType));
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



        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor &&
                GameManager.Instance.SceneInteractable &&
                GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Normal) {
                if (!sceneActive && (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(true);
                    sceneActive = true;
                } else if (sceneActive && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            } else {
                if (sceneActive) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            }
            
            



           
        }

        private void Start() {
            SceneManager.Instance.OnLoadScene += OnSceneLoaded;
            WebsocketManager.Instance.OnRobotEefUpdated += RobotEefUpdated;
        }


        private void RobotEefUpdated(object sender, RobotEefUpdatedEventArgs args) {
            if (!RobotsEEVisible) {
                CleanRobotEE();
                return;
            }
            foreach (EefPose eefPose in args.Data.EndEffectors) {
                if (!EndEffectors.TryGetValue(args.Data.RobotId + "/" + eefPose.EndEffectorId, out RobotEE robotEE)) {
                    robotEE = Instantiate(RobotEEPrefab, transform).GetComponent<RobotEE>();
                    robotEE.SetEEName(args.Data.RobotId, eefPose.EndEffectorId);
                    EndEffectors.Add(ProjectManager.Instance.GetActionProvider(args.Data.RobotId).GetProviderName() + "/" + eefPose.EndEffectorId, robotEE);
                }
                robotEE.transform.localPosition = TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(eefPose.Pose.Position));
                robotEE.transform.localRotation = TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(eefPose.Pose.Orientation));

            }
        }

        private void OnSceneLoaded(object sender, EventArgs e) {
            CleanRobotEE();
            if (RobotsEEVisible) {
                ShowRobotsEE();
            }
        }

        private void CleanRobotEE() {
            foreach (KeyValuePair<string, RobotEE> ee in EndEffectors) {
                Destroy(ee.Value.gameObject);
            }
            EndEffectors.Clear();
        }


        public void ShowRobotsEE() {
            robotsWithEndEffector = GetAllRobotsWithEndEffectors();
            RobotsEEVisible = true;
            foreach (var robot in robotsWithEndEffector) {
                if (robot.Value.Count > 0)
                    WebsocketManager.Instance.RegisterForRobotEvent(robot.Key, true, RegisterForRobotEventArgs.WhatEnum.Eefpose);
            }
            //InvokeRepeating("UpdateEndEffectors", 1, 0.5f);
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/RobotsEEVisibility", true);
            
        }

        public async void HideRobotsEE() {
            RobotsEEVisible = false;
            foreach (var robot in robotsWithEndEffector) {
                if (robot.Value.Count > 0)
                    await WebsocketManager.Instance.RegisterForRobotEvent(robot.Key, false, RegisterForRobotEventArgs.WhatEnum.Eefpose);
            }
            CleanRobotEE();
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/RobotsEEVisibility", false);
        }

        
        internal void LoadSettings() {
            ActionObjectsVisible = PlayerPrefsHelper.LoadBool("scene/" + Scene.Id + "/AOVisibility", true);
            ActionObjectsInteractive = PlayerPrefsHelper.LoadBool("scene/" + Scene.Id + "/AOInteractivity", true);
            RobotsEEVisible = PlayerPrefsHelper.LoadBool("scene/" + Scene.Id + "/RobotsEEVisibility", true);
        }


        /// <summary>
        /// Deactivates or activates all action objects in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionObjectsForGizmo(bool activate) {
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("Default");
                }
            }
        }

        

         public void SetSelectedObject(GameObject obj) {
            if (CurrentlySelectedObject != null) {
                CurrentlySelectedObject.SendMessage("Deselect");
            }
            if (obj != null) {
                obj.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
            }
            CurrentlySelectedObject = obj;
        }

        public void SceneBaseUpdated(IO.Swagger.Model.Scene scene) {

            Scene.Desc = scene.Desc;
            Scene.Modified = scene.Modified;
            Scene.Name = scene.Name;
        }

        

        #region ACTION_OBJECTS

        public async Task<ActionObject> SpawnActionObject(string id, string type, CollisionModels customCollisionModels = null) {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);
            } else {
                obj = Instantiate(ActionObjectPrefab, ActionObjectsSpawn.transform);
            }
            ActionObject actionObject = obj.GetComponentInChildren<ActionObject>();
            actionObject.InitActionObject(id, type, obj.transform.localPosition, obj.transform.localRotation, id, aom, customCollisionModels);

            // Add the Action Object into scene reference
            ActionObjects.Add(id, actionObject);
            if (loadResources && aom.Robot) {
                await actionObject.LoadEndEffectors();
            }

            return actionObject;
        }
        public static string ToUnderscoreCase(string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public string GetFreeAOName(string ioType) {
            int i = 1;
            bool hasFreeName;
            string freeName = ToUnderscoreCase(ioType);
            do {
                hasFreeName = true;
                if (ActionObjectsContainName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ToUnderscoreCase(ioType) + "_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public Dictionary<string, List<string>> GetAllRobotsWithEndEffectors() {
            Dictionary<string, List<string>> robotsWithEndEffectors = new Dictionary<string, List<string>>();
            foreach (ActionObject robot in GetActionObjectsRobots()) {
                robotsWithEndEffectors[robot.Data.Id] = new List<string>();
                foreach (string ee in robot.GetEndEffectors()) {
                    robotsWithEndEffectors[robot.Data.Id].Add(ee);
                }
            }
            foreach (Service service in Base.ActionsManager.Instance.ServicesData.Values) {
                if (service.Metadata.Robot) {
                    foreach (string robot in service.GetRobotsNames()) {
                        if (!robotsWithEndEffectors.ContainsKey(robot)) {
                            robotsWithEndEffectors[robot] = new List<string>();
                            foreach (string ee in service.GetEndEffectors(robot)) {
                                robotsWithEndEffectors[robot].Add(ee);
                            }
                        }
                    }
                }
            }
            return robotsWithEndEffectors;
        }

        public List<ActionObject> GetActionObjectsRobots() {
            List<ActionObject> robots = new List<ActionObject>();

            foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    robots.Add(actionObject);
                }
            }
            return robots;
        }



        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        public async Task UpdateActionObjects(CollisionModels customCollisionModels = null) {
            List<string> currentAO = new List<string>();
            foreach (IO.Swagger.Model.SceneObject aoSwagger in Scene.Objects) {
                ActionObject actionObject = await SpawnActionObject(aoSwagger.Id, aoSwagger.Type, customCollisionModels);
                actionObject.ActionObjectUpdate(aoSwagger, ActionObjectsVisible, ActionObjectsInteractive);
                currentAO.Add(aoSwagger.Id);
            }

        }

        /// <summary>
        /// Updates all services from scene data.  
        /// Only called when whole scene arrived, i.e. when client is connected or scene is opened, so all service needs to be added.
        /// </summary>
        public async Task UpdateServices() {
            ActionsManager.Instance.ClearServices(); //just to be sure
            foreach (IO.Swagger.Model.SceneService service in Scene.Services) {
                await ActionsManager.Instance.AddService(service, loadResources);
            }
        }

        public ActionObject GetNextActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string next;
            if (index + 1 < ActionObjects.Keys.Count)
                next = keys[index + 1];
            else
                next = keys[0];
            if (!ActionObjects.TryGetValue(next, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetPreviousActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string previous;
            if (index - 1 > -1)
                previous = keys[index - 1];
            else
                previous = keys[keys.Count - 1];
            if (!ActionObjects.TryGetValue(previous, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetFirstActionObject() {
            if (ActionObjects.Count == 0) {
                return null;
            }
            return ActionObjects.First().Value;
        }

        /// <summary>
        /// Shows action objects models
        /// </summary>
        public void ShowActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Show();
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/AOVisibility", true);
        }

        /// <summary>
        /// Hides action objects models
        /// </summary>
        public void HideActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Hide();
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/AOVisibility", false);
        }

         /// <summary>
        /// Sets whether action objects should react to user inputs (i.e. enables/disables colliders)
        /// </summary>
        public void SetActionObjectsInteractivity(bool interactivity) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetInteractivity(interactivity);
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/AOInteractivity", interactivity);
        }


        /// <summary>
        /// Destroys and removes references to all action objects in the scene.
        /// </summary>
        public void RemoveActionObjects() {
            foreach (string actionObjectId in ActionObjects.Keys.ToList<string>()) {
                RemoveActionObject(actionObjectId);
            }
            // just to make sure that none reference left
            ActionObjects.Clear();
        }

        /// <summary>
        /// Destroys and removes references to action object of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveActionObject(string Id) {
            try {
                ActionObjects[Id].DeleteActionObject();
            } catch (NullReferenceException e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Finds action object by ID or throws KeyNotFoundException.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionObject GetActionObject(string id) {
            if (ActionObjects.TryGetValue(id, out Base.ActionObject actionObject))
                return actionObject;
            throw new KeyNotFoundException("Action object not found");
        }

        public bool TryGetActionObjectByName(string name, out ActionObject actionObjectOut) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.GetName() == name) {
                    actionObjectOut = actionObject;
                    return true;
                }   
            }
            actionObjectOut = null;
            return false;
        }

        public bool ActionObjectsContainName(string name) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.Data.Name == name) {
                    return true;
                }
            }
            return false;
        }

        

        #endregion

    }
}

