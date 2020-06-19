using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Model;
using OrbCreationExtensions;
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


    public class ServiceEventArgs : EventArgs {
        public Service Data {
            get; set;
        }

        public ServiceEventArgs(Service data) {
            Data = data;
        }
    }


    public class SceneManager : Singleton<SceneManager> {

        public delegate void ServiceEventHandler(object sender, ServiceEventArgs args);

        public delegate void RobotUrdfEventHandler(object sender, RobotUrdfArgs args);

        public IO.Swagger.Model.Scene Scene = null;

       // string == IO.Swagger.Model.Scene Data.Id
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        private Dictionary<string, Service> servicesData = new Dictionary<string, Service>();


        public GameObject ActionObjectsSpawn, SceneOrigin, EEOrigin;

        
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

        private List<IRobot> robotsWithEndEffector = new List<IRobot>();


        public event EventHandler OnLoadScene;
        public event EventHandler OnSceneChanged;
        public event EventHandler OnSceneSaved;
        public event RobotUrdfEventHandler OnUrdfReady;
        public event ServiceEventHandler OnServicesUpdated;


        private bool loadResources = false;

        public Dictionary<string, Service> ServicesData {
            get => servicesData;
            set => servicesData = value;
        }
        


        /// <summary>
        /// Creates project from given json
        /// </summary>
        /// <param name="project"></param>
        public async Task<bool> CreateScene(IO.Swagger.Model.Scene scene, bool loadResources, GameManager.GameStateEnum requestedGameState, CollisionModels customCollisionModels = null) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (Scene != null)
                return false;
            robotsWithEndEffector.Clear();
            
            Scene = scene;
            this.loadResources = loadResources;
            LoadSettings();
            

            bool success = await UpdateScene(scene, customCollisionModels);
            
            if (success) {
                OnLoadScene?.Invoke(this, EventArgs.Empty);
            }

            // TODO - do this when robot is added to scene
            //foreach (KeyValuePair<string, RobotMeta> robotMeta in ActionsManager.Instance.RobotsMeta) {
            //    if (!string.IsNullOrEmpty(robotMeta.Value.UrdfPackageFilename)) {
            //        StartCoroutine(DownloadUrdfPackage(robotMeta.Value.UrdfPackageFilename, robotMeta.Key));
            //    }
            //}
            //SpawnActionObject("123456789", "DobotMagician");

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
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool DestroyScene() {
            RemoveActionObjects();
            servicesData.Clear();
            Scene = null;
            return true;
        }

        private IEnumerator DownloadUrdfPackage(string fileName, string robotType) {
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

        public void ClearServices() {
            List<string> servicesKeys = servicesData.Keys.ToList();
            foreach (string service in servicesKeys) {
                RemoveService(service);
            }
        }


        public void UpdateService(IO.Swagger.Model.SceneService sceneService) {
            Debug.Assert(ServicesData.ContainsKey(sceneService.Type));
            ServicesData.TryGetValue(sceneService.Type, out Service service);
            service.Data = sceneService;
            OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddService(IO.Swagger.Model.SceneService sceneService, bool loadResources) {
            Debug.Assert(!ServicesData.ContainsKey(sceneService.Type));
            if (ActionsManager.Instance.ServicesMetadata.TryGetValue(sceneService.Type, out ServiceMetadata serviceMetadata)) {
                Service service;
                if (serviceMetadata.Robot)
                    service = new RobotService(sceneService, serviceMetadata);
                else 
                    service = new Service(sceneService, serviceMetadata);
                if (loadResources && service.IsRobot()) {
                    await ((RobotService) service).LoadRobots();
                }
                ServicesData.Add(sceneService.Type, service);
                OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
            }
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveService(string serviceType) {
            Debug.Assert(ServicesData.ContainsKey(serviceType));
            ServicesData.TryGetValue(serviceType, out Service service);
            ServicesData.Remove(serviceType);
            OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
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

        public bool ServiceInScene(string type) {
            return ServicesData.ContainsKey(type);
        }

        public Service GetService(string type) {
            if (ServicesData.TryGetValue(type, out Service sceneService)) {
                return sceneService;
            } else {
                throw new KeyNotFoundException("Service not in scene!");
            }
        }

        private void Start() {
            OnLoadScene += OnSceneLoaded;
            WebsocketManager.Instance.OnRobotEefUpdated += RobotEefUpdated;
            WebsocketManager.Instance.OnRobotJointsUpdated += RobotJointsUpdated;
        }

        private async void RobotJointsUpdated(object sender, RobotJointsUpdatedEventArgs args) {
            if (!RobotsEEVisible) {
                CleanRobotEE();
                return;
            }
            Debug.LogError(args.Data.RobotId);
            foreach (var obj in ActionObjects) {
                Debug.LogError(obj.Key);
            }
            // check if robotId is really a robot
            if (ActionObjects.TryGetValue(args.Data.RobotId, out ActionObject actionObject)) {
               if (actionObject.IsRobot()) {
                    RobotActionObject robot = (RobotActionObject) actionObject;
                    foreach (IO.Swagger.Model.Joint joint in args.Data.Joints) {
                        robot.SetJointAngle(joint.Name, (float) joint.Value);                        
                    }
                } else {
                    Debug.LogError("My robot is not a robot?");
                    Notifications.Instance.SaveLogs();
                }
            } else {
                Debug.LogError("Robot not found!");
                await WebsocketManager.Instance.RegisterForRobotEvent(args.Data.RobotId,
                    false, RegisterForRobotEventArgs.WhatEnum.Joints);
            }
            
        }

        private void RobotEefUpdated(object sender, RobotEefUpdatedEventArgs args) {
            if (!RobotsEEVisible) {
                CleanRobotEE();
                return;
            }
            foreach (EefPose eefPose in args.Data.EndEffectors) {
                if (!EndEffectors.TryGetValue(args.Data.RobotId + "/" + eefPose.EndEffectorId, out RobotEE robotEE)) {
                    robotEE = Instantiate(RobotEEPrefab, EEOrigin.transform).GetComponent<RobotEE>();
                    robotEE.SetEEName(GetRobot(args.Data.RobotId).GetName(), eefPose.EndEffectorId);
                    EndEffectors.Add(args.Data.RobotId + "/" + eefPose.EndEffectorId, robotEE);
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

        /// <summary>
        /// Return true if there is any robot in scene
        /// </summary>
        /// <returns></returns>
        public bool RobotInScene() {
            return GetRobots().Count > 0;
        }

        private void CleanRobotEE() {
            foreach (KeyValuePair<string, RobotEE> ee in EndEffectors) {
                Destroy(ee.Value.gameObject);
            }
            EndEffectors.Clear();
        }


        public async void ShowRobotsEE() {
            CleanRobotEE();
            foreach (IRobot robot in GetRobots()) {
                if (robot.GetEndEffectors().Count > 0) {
                    robotsWithEndEffector.Add(robot);
                }
            }
            RobotsEEVisible = true;
            foreach (IRobot robot in robotsWithEndEffector) {
                await WebsocketManager.Instance.RegisterForRobotEvent(robot.GetId(), true, RegisterForRobotEventArgs.WhatEnum.Eefpose);
                if (robot.HasUrdf())
                    await WebsocketManager.Instance.RegisterForRobotEvent(robot.GetId(), true, RegisterForRobotEventArgs.WhatEnum.Joints);
                    
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/RobotsEEVisibility", true);
            
        }

        public async void HideRobotsEE() {
            RobotsEEVisible = false;
            foreach (IRobot robot in robotsWithEndEffector) {
                await WebsocketManager.Instance.RegisterForRobotEvent(robot.GetId(), false, RegisterForRobotEventArgs.WhatEnum.Eefpose);
                await WebsocketManager.Instance.RegisterForRobotEvent(robot.GetId(), false, RegisterForRobotEventArgs.WhatEnum.Joints);
            }
            robotsWithEndEffector.Clear();
            CleanRobotEE();
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/RobotsEEVisibility", false);
        }

        public RobotEE GetRobotEE(string robotId, string eeId) {
            if (EndEffectors.TryGetValue(robotId + "/" + eeId, out RobotEE robotEE)) {
                return robotEE;
            } else {
                throw new ItemNotFoundException("No ee with id: " + robotId + "/" + eeId);
            }
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

        public Vector3 GetCollisionFreePointAbove(Vector3 originalPosition) {
            Vector3 aboveModel = Vector3.zero;
            Collider[] colliders = Physics.OverlapSphere(originalPosition + aboveModel, 0.025f);
            // to avoid infinite loop
            int i = 0;
            while (colliders.Length > 0 && i < 20) {
                Collider collider = colliders[0];
                aboveModel.y = collider.gameObject.transform.position.y + collider.bounds.extents.y + 0.05f;
                colliders = Physics.OverlapSphere(originalPosition + aboveModel, 0.025f);
                ++i;
            }
            return originalPosition + aboveModel;
        }




        #region ACTION_OBJECTS

        public async Task<ActionObject> SpawnActionObject(string id, string type, CollisionModels customCollisionModels = null) {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                Debug.Log("URDF: spawning RobotActionObject");
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);
            } else {
                obj = Instantiate(ActionObjectPrefab, ActionObjectsSpawn.transform);
            }
            ActionObject actionObject = obj.GetComponent<ActionObject>();
            actionObject.InitActionObject(id, type, obj.transform.localPosition, obj.transform.localRotation, id, aom, customCollisionModels);

            // Add the Action Object into scene reference
            ActionObjects.Add(id, actionObject);

            if (aom.Robot) {
                if (ActionsManager.Instance.RobotsMeta.TryGetValue(type, out RobotMeta robotMeta)) {
                    if (!string.IsNullOrEmpty(robotMeta.UrdfPackageFilename)) {
                        StartCoroutine(DownloadUrdfPackage(robotMeta.UrdfPackageFilename, robotMeta.Type));
                    }
                }

                if (loadResources) {
                    await ((RobotActionObject) actionObject).LoadEndEffectors();
                }
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

        

        public List<ActionObject> GetActionObjectsRobots() {
            List<ActionObject> robots = new List<ActionObject>();

            foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    robots.Add(actionObject);
                }
            }
            return robots;
        }

        public List<IRobot> GetRobots() {
            List<string> robotIds = new List<string>();
            List<IRobot> robots = new List<IRobot>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsRobot()) {
                    robots.Add((RobotActionObject) actionObject);
                    robotIds.Add(actionObject.Data.Id);
                }                    
            }
            foreach (Service service in servicesData.Values) {
                if (service.IsRobot()) {
                    List<Robot> serviceRobots = ((RobotService) service).GetRobots();
                    foreach (Robot robot in serviceRobots) {
                        if (!robotIds.Contains(robot.GetId())) {
                            robots.Add(robot);
                            robotIds.Add(robot.GetId());
                        }
                    }
                }
            }
            return robots;
        }

        public IRobot GetRobot(string robotId) {
            foreach (IRobot robot in GetRobots()) {
                if (robot.GetId() == robotId)
                    return robot;
            }
            throw new ItemNotFoundException("No robot with id: " + robotId);
        }

        public IRobot GetRobotByName(string robotName) {
            foreach (IRobot robot in GetRobots())
                if (robot.GetName() == robotName)
                    return robot;
            throw new ItemNotFoundException("Robot with name " + robotName + " does not exists!");
        }


        /*public List<string> GetRobotsNames() {
            HashSet<string> robots = new HashSet<string>();
            foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    robots.Add(actionObject.Data.Name);
                }
            }
            foreach (Service service in servicesData.Values) {
                if (service.Metadata.Robot) {
                    foreach (string s in service.GetRobotsNames()) {
                        robots.Add(s);
                    }
                }
            }
            return robots.ToList<string>();
        }*/



        public string RobotNameToId(string robotName) {
            return GetRobotByName(robotName).GetId();
        }


        public void SceneObjectUpdated(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                actionObject.ActionObjectUpdate(sceneObject, SceneManager.Instance.ActionObjectsVisible, SceneManager.Instance.ActionObjectsInteractive);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SceneObjectBaseUpdated(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {

            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task SceneObjectAdded(SceneObject sceneObject) {
            ActionObject actionObject = await SpawnActionObject(sceneObject.Id, sceneObject.Type);
            actionObject.ActionObjectUpdate(sceneObject, ActionObjectsVisible, ActionObjectsInteractive);
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }


        public void SceneObjectRemoved(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                ActionObjects.Remove(sceneObject.Id);
                Destroy(actionObject.gameObject);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
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
            ClearServices(); //just to be sure
            foreach (IO.Swagger.Model.SceneService service in Scene.Services) {
                await AddService(service, loadResources);
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

        internal void SceneSaved() {
            OnSceneSaved?.Invoke(this, EventArgs.Empty);
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
            ActionObjectsVisible = true;
        }

        /// <summary>
        /// Hides action objects models
        /// </summary>
        public void HideActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Hide();
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/AOVisibility", false);
            ActionObjectsVisible = false;
        }

         /// <summary>
        /// Sets whether action objects should react to user inputs (i.e. enables/disables colliders)
        /// </summary>
        public void SetActionObjectsInteractivity(bool interactivity) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetInteractivity(interactivity);
            }
            PlayerPrefsHelper.SaveBool("scene/" + Scene.Id + "/AOInteractivity", interactivity);
            ActionObjectsInteractive = interactivity;
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

