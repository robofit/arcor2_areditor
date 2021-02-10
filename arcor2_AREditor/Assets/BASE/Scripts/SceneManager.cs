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
using WebSocketSharp;

namespace Base {
   
    /// <summary>
    /// Takes care of currently opened scene
    /// </summary>
    public class SceneManager : Singleton<SceneManager> {
        /// <summary>
        /// Invoked when new scene loaded
        /// </summary>
        public event EventHandler OnLoadScene;
        /// <summary>
        /// Invoked when scene chagned
        /// </summary>
        public event EventHandler OnSceneChanged;
        /// <summary>
        /// Invoked when scene save status has changed
        /// </summary>
        public event EventHandler OnSceneSavedStatusChanged;
        /// <summary>
        /// Invoked when scene saved
        /// </summary>
        public event EventHandler OnSceneSaved;
        /// <summary>
        /// Invoked when robor should show their EE pose
        /// </summary>
        public event EventHandler OnShowRobotsEE;
        /// <summary>
        /// Invoked when robots should hide their EE pose
        /// </summary>
        public event EventHandler OnHideRobotsEE;
        /// <summary>
        /// Contains metainfo about scene (id, name, modified etc) without info about objects and services
        /// </summary>
        public Scene SceneMeta = null;
        /// <summary>
        /// Holds all action objects in scene
        /// </summary>
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        /// <summary>
        /// Spawn point for new action objects. Typically scene origin.
        /// </summary>
        public GameObject ActionObjectsSpawn;
        /// <summary>
        /// Origin (0,0,0) of scene.
        /// </summary>
        public GameObject SceneOrigin;
        /// <summary>
        /// Prefab for robot action object
        /// </summary>        
        public GameObject RobotPrefab;
        /// <summary>
        /// Prefab for action object
        /// </summary>
        public GameObject ActionObjectPrefab;
        /// <summary>
        /// Object which is currently selected in scene
        /// </summary>
        public GameObject CurrentlySelectedObject;
        /// <summary>
        /// Manager taking care of connections between action points and action objects
        /// </summary>
        public LineConnectionsManager AOToAPConnectionsManager;
        /// <summary>
        /// Prefab of connectino between action point and action object
        /// </summary>
        public GameObject LineConnectionPrefab;
        /// <summary>
        /// Prefab for robot end effector object
        /// </summary>
        public GameObject RobotEEPrefab;
        /// <summary>
        /// Indicates whether or not scene was changed since last save
        /// </summary>
        private bool sceneChanged = false;
        /// <summary>
        /// ??? Dane?
        /// </summary>
        private bool sceneActive = true;
        /// <summary>
        /// Indicates if action objects should be interactable in scene (if they should response to clicks)
        /// </summary>
        public bool ActionObjectsInteractive;
        /// <summary>
        /// Indicates visibility of action objects in scene
        /// </summary>
        public float ActionObjectsVisibility;
        /// <summary>
        /// Indicates if robots end effector should be visible
        /// </summary>
        public bool RobotsEEVisible {
            get;
            private set;
        }
        /// <summary>
        /// Indicates if resources (e.g. end effectors for robot) should be loaded when scene created.
        /// </summary>
        private bool loadResources = false;

        /// <summary>
        /// Defines if scene was started on server - e.g. if all robots and other action objects
        /// are instantioned and are ready
        /// </summary>
        public bool SceneStarted = false;

        public event AREditorEventArgs.SceneStateHandler OnSceneStateEvent;

        public bool Valid = false;
        /// <summary>
        /// Public setter for sceneChanged property. Invokes OnSceneChanged event with each change and
        /// OnSceneSavedStatusChanged when sceneChanged value differs from original value (i.e. when scene
        /// was not changed and now it is and vice versa)
        /// </summary>
        public bool SceneChanged {
            get => sceneChanged;
            set {
                bool origVal = SceneChanged;
                sceneChanged = value;
                if (!Valid)
                    return;
                OnSceneChanged?.Invoke(this, EventArgs.Empty);
                if (origVal != value) {
                    OnSceneSavedStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Creates scene from given json
        /// </summary>
        /// <param name="scene">Json describing scene.</param>
        /// <param name="loadResources">Indicates if resources should be loaded from server.</param>
        /// <param name="customCollisionModels">Allows to override collision models with different ones. Usable e.g. for
        /// project running screen.</param>
        /// <returns>True if scene successfully created, false otherwise</returns>
        public async Task<bool> CreateScene(IO.Swagger.Model.Scene scene, bool loadResources, CollisionModels customCollisionModels = null) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            
            if (SceneMeta != null)
                return false;
            SetSceneMeta(DataHelper.SceneToBareScene(scene));            
            this.loadResources = loadResources;
            LoadSettings();
            
            UpdateActionObjects(scene, customCollisionModels);
            sceneChanged = scene.Modified == DateTime.MinValue;
            try {
                await WebsocketManager.Instance.StopScene(true);
                WebsocketManager.Instance.InvokeSceneStateEvent(new SceneStateData(message: "", state: SceneStateData.StateEnum.Started));
            } catch (RequestFailedException) {
                WebsocketManager.Instance.InvokeSceneStateEvent(new SceneStateData(message: "", state: SceneStateData.StateEnum.Stopped));
            }
            OnLoadScene?.Invoke(this, EventArgs.Empty);
            Valid = true;
            return true;
        }

        /// <summary>
        /// Destroys scene and all objects
        /// </summary>
        /// <returns>True if scene successfully destroyed, false otherwise</returns>
        public bool DestroyScene() {
            SceneStarted = false;
            Valid = false;
            RemoveActionObjects();
            SceneMeta = null;
            return true;
        }

        /// <summary>
        /// Sets scene metadata
        /// </summary>
        /// <param name="scene">Scene metadata</param>
        public void SetSceneMeta(BareScene scene) {
            if (SceneMeta == null) {
                SceneMeta = new Scene(id: "", name: "");
            }
            SceneMeta.Id = scene.Id;
            SceneMeta.Desc = scene.Desc;
            SceneMeta.IntModified = scene.IntModified;
            SceneMeta.Modified = scene.Modified;
            SceneMeta.Name = scene.Name;
        }

        /// <summary>
        /// Gets scene metadata.
        /// </summary>
        /// <returns></returns>
        public IO.Swagger.Model.Scene GetScene() {
            if (SceneMeta == null)
                return null;
            Scene scene = SceneMeta;
            scene.Objects = new List<SceneObject>();
            foreach (ActionObject o in ActionObjects.Values) {
                scene.Objects.Add(o.Data);
            }
            return scene;
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

        /// <summary>
        /// Initialization of scene manager
        /// </summary>
        private void Start() {
            OnLoadScene += OnSceneLoaded;
            WebsocketManager.Instance.OnRobotEefUpdated += RobotEefUpdated;
            WebsocketManager.Instance.OnRobotJointsUpdated += RobotJointsUpdated;
            WebsocketManager.Instance.OnSceneBaseUpdated += OnSceneBaseUpdated;
            WebsocketManager.Instance.OnSceneStateEvent += OnSceneState;

            WebsocketManager.Instance.OnOverrideAdded += OnOverrideAddedOrUpdated;
            WebsocketManager.Instance.OnOverrideUpdated += OnOverrideAddedOrUpdated;
            WebsocketManager.Instance.OnOverrideBaseUpdated += OnOverrideAddedOrUpdated;
            WebsocketManager.Instance.OnOverrideRemoved += OnOverrideRemoved;
        }

        private void OnOverrideRemoved(object sender, ParameterEventArgs args) {
            try {
                ActionObject actionObject = GetActionObject(args.ObjectId);
                actionObject.Overrides.Remove(args.Parameter.Name);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);

            }
        }

        private void OnOverrideAddedOrUpdated(object sender, ParameterEventArgs args) {

            try {
                ActionObject actionObject = GetActionObject(args.ObjectId);
                if (actionObject.TryGetParameterMetadata(args.Parameter.Type, out ParameterMeta parameterMeta)) {
                    Parameter p = new Parameter(parameterMeta, args.Parameter.Value);
                    actionObject.Overrides[args.Parameter.Name] = p;
                }
                
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                
            }
            
        }

        private void OnSceneState(object sender, SceneStateEventArgs args) {
            switch (args.Event.State) {
                case SceneStateData.StateEnum.Starting:
                    GameManager.Instance.ShowLoadingScreen("Starting scene...");
                    break;
                case SceneStateData.StateEnum.Stopping:
                    SceneStarted = false;
                    GameManager.Instance.ShowLoadingScreen("Stopping scene...");
                    if (!args.Event.Message.IsNullOrEmpty()) {
                        Notifications.Instance.ShowNotification("Scene service failed", args.Event.Message);
                    }
                    OnHideRobotsEE?.Invoke(this, EventArgs.Empty);
                    foreach (IRobot robot in GetRobots()) {
                        robot.SetGrey(true);
                        foreach (var joint in robot.GetJoints()) { //set default angles of joints
                            robot.SetJointValue(joint.Name, 0f);
                        }
                    }
                    break;
                case SceneStateData.StateEnum.Started:
                    SceneStarted = true;
                    if (RobotsEEVisible)
                        OnShowRobotsEE?.Invoke(this, EventArgs.Empty);
                    RegisterRobotsForEvent(true, RegisterForRobotEventRequestArgs.WhatEnum.Joints);
                    foreach (IRobot robot in GetRobots()) {
                        robot.SetGrey(false);
                    }
                    GameManager.Instance.HideLoadingScreen();
                    break;
                case SceneStateData.StateEnum.Stopped:
                    SceneStarted = false;
                    GameManager.Instance.HideLoadingScreen();
                    break;
            }
            // needs to be rethrown to ensure all subscribers has updated data
            OnSceneStateEvent?.Invoke(this, args);
        }

        private void InitScene() {

        }

        /// <summary>
        /// Register or unregister to/from subsription of joints or end effectors pose of each robot in the scene.
        /// </summary>
        /// <param name="send">To subscribe or to unsubscribe</param>
        /// <param name="what">Pose of end effectors or joints</param>
        public void RegisterRobotsForEvent(bool send, RegisterForRobotEventRequestArgs.WhatEnum what) {
            foreach (IRobot robot in GetRobots()) {
                WebsocketManager.Instance.RegisterForRobotEvent(robot.GetId(), send, what);
            }
        }

        private void OnSceneBaseUpdated(object sender, BareSceneEventArgs args) {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor) {
                SetSceneMeta(args.Scene);
                sceneChanged = true;
            }
        }

        /// <summary>
        /// Updates robot model based on recieved joints.
        /// </summary>
        /// <param name="sender">Who invoked event.</param>
        /// <param name="args">Robot joints data</param>
        private async void RobotJointsUpdated(object sender, RobotJointsUpdatedEventArgs args) {
            // if initializing or deinitializing scene, dont update robot joints
            if (!Valid)
                return;
            try {
                IRobot robot = GetRobot(args.Data.RobotId);
                robot.SetJointValue(args.Data.Joints);
            } catch (ItemNotFoundException) {
                
            }
                        
        }

        /// <summary>
        /// Updates end effector poses in scene based on recieved poses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Robot ee data</param>
        private void RobotEefUpdated(object sender, RobotEefUpdatedEventArgs args) {
            if (!RobotsEEVisible) {
                return;
            }
            foreach (RobotEefDataEefPose eefPose in args.Data.EndEffectors) {
                try {
                    IRobot robot = GetRobot(args.Data.RobotId);
                    RobotEE ee = robot.GetEE(eefPose.EndEffectorId);
                    ee.UpdatePosition(TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(eefPose.Pose.Position)),
                        TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(eefPose.Pose.Orientation)));
                } catch (ItemNotFoundException) {
                    continue;
                }
                
            }
        }

        /// <summary>
        /// Initialize robots end effectors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSceneLoaded(object sender, EventArgs e) {
            /*if (RobotsEEVisible) {
                ShowRobotsEE();
            }*/
        }

        /// <summary>
        /// Return true if there is any robot in scene
        /// </summary>
        /// <returns></returns>
        public bool RobotInScene() {
            return GetRobots().Count > 0;
        }

        /// <summary>
        /// Registers for end effector poses (and if robot has URDF then for joints values as well) and displays EE positions in scene
        /// </summary>
        /// <param name="robotId">Id of robot which should be registered. If null, all robots in scene are registered.</param>
        public bool ShowRobotsEE() {
            if (!SceneStarted) {
                Notifications.Instance.ShowNotification("Failed to show robots EE", "This can only be done when scene is started");
                return false;
            }
            RobotsEEVisible = true;
            OnShowRobotsEE?.Invoke(this, EventArgs.Empty);
            
            PlayerPrefsHelper.SaveBool("scene/" + SceneMeta.Id + "/RobotsEEVisibility", true);
            return true;
        }

        /// <summary>
        /// Hides end effectors and unregister from EE positions and robot joints subscription
        /// </summary>
        public void HideRobotsEE() {
            RobotsEEVisible = false;
            OnHideRobotsEE?.Invoke(this, EventArgs.Empty);
            PlayerPrefsHelper.SaveBool("scene/" + SceneMeta.Id + "/RobotsEEVisibility", false);
        }

        /// <summary>
        /// Loads selected setings from player prefs
        /// </summary>
        internal void LoadSettings() {
            ActionObjectsVisibility = PlayerPrefsHelper.LoadFloat("AOVisibility" + (VRModeManager.Instance.VRModeON ? "VR" : "AR"), (VRModeManager.Instance.VRModeON ? 1f : 0f));
            ActionObjectsInteractive = PlayerPrefsHelper.LoadBool("scene/" + SceneMeta.Id + "/AOInteractivity", true);
            RobotsEEVisible = PlayerPrefsHelper.LoadBool("scene/" + SceneMeta.Id + "/RobotsEEVisibility", true);
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

        /// <summary>
        /// Sets selected object
        /// </summary>
        /// <param name="obj">Object which is currently selected</param>
        public void SetSelectedObject(GameObject obj) {
            if (CurrentlySelectedObject != null) {
                CurrentlySelectedObject.SendMessage("Deselect");
            }
            if (obj != null) {
                obj.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
            }
            CurrentlySelectedObject = obj;
        }

      
        /// <summary>
        /// Computes point above selected transform which is collision free
        /// </summary>
        /// <param name="transform">Original pose</param>
        /// <param name="bbSize">Size of box where no collision is allowed</param>
        /// <param name="orientation">Orientation of box where no collision is allowed</param>
        /// <returns>Offset from original transform in world coordinates (relative to scene origin)</returns>
        public Vector3 GetCollisionFreePointAbove(Transform transform, Vector3 bbSize, Quaternion orientation) {
            GameObject tmpGo = new GameObject();
            tmpGo.transform.parent = transform;
            tmpGo.transform.localPosition = Vector3.zero;
            tmpGo.transform.localRotation = Quaternion.identity;

            Collider[] colliders = Physics.OverlapBox(transform.position, bbSize, orientation);   //OverlapSphere(tmpGo.transform.position, 0.025f);
            
            // to avoid infinite loop
            int i = 0;
            while (colliders.Length > 0 && i < 40) {
                Collider collider = colliders[0];
                // TODO - depends on the rotation between detected marker and original position of camera, height of collision free point above will be slightly different
                // How to solve this?
                tmpGo.transform.Translate(new Vector3(0, collider.bounds.extents.y, 0), SceneOrigin.transform);
                colliders = Physics.OverlapBox(tmpGo.transform.position, bbSize / 2, orientation);
                ++i;
            }
            return tmpGo.transform.localPosition;
        }


        #region ACTION_OBJECTS
        /// <summary>
        /// Spawns new action object
        /// </summary>
        /// <param name="id">UUID of action object</param>
        /// <param name="type">Action object type</param>
        /// <param name="customCollisionModels">Allows to override collision model of spawned action objects</param>
        /// <returns>Spawned action object</returns>
        public ActionObject SpawnActionObject(string id, string type, CollisionModels customCollisionModels = null) {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                //Debug.Log("URDF: spawning RobotActionObject");
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);

            } else {
                obj = Instantiate(ActionObjectPrefab, ActionObjectsSpawn.transform);
            }
            ActionObject actionObject = obj.GetComponent<ActionObject>();
            actionObject.InitActionObject(id, type, obj.transform.localPosition, obj.transform.localRotation, id, aom, customCollisionModels);

            // Add the Action Object into scene reference
            ActionObjects.Add(id, actionObject);

            

            return actionObject;
        }

        /// <summary>
        /// Transform string to underscore case (e.g. CamelCase to camel_case)
        /// </summary>
        /// <param name="str">String to be transformed</param>
        /// <returns>Underscored string</returns>
        public static string ToUnderscoreCase(string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        /// <summary>
        /// Finds free action object name, based on action object type (e.g. Box, Box_1, Box_2 etc.)
        /// </summary>
        /// <param name="aoType">Type of action object</param>
        /// <returns></returns>
        public string GetFreeAOName(string aoType) {
            int i = 1;
            bool hasFreeName;
            string freeName = ToUnderscoreCase(aoType);
            do {
                hasFreeName = true;
                if (ActionObjectsContainName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ToUnderscoreCase(aoType) + "_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        /// <summary>
        /// Returns all robots in scene
        /// </summary>
        /// <returns></returns>
        public List<IRobot> GetRobots() {
            List<IRobot> robots = new List<IRobot>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsRobot()) {
                    robots.Add((RobotActionObject) actionObject);
                }                    
            }
            return robots;
        }

        public List<ActionObject> GetCameras() {
            List<ActionObject> cameras = new List<ActionObject>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsCamera()) {
                    cameras.Add(actionObject);
                }
            }
            return cameras;
        }

        public List<string> GetCamerasIds() {
            List<string> cameraIds = new List<string>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsCamera()) {
                    cameraIds.Add(actionObject.Data.Id);
                }
            }
            return cameraIds;
        }

        public List<string> GetCamerasNames() {
            List<string> camerasNames = new List<string>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsCamera()) {
                    camerasNames.Add(actionObject.Data.Name);
                }
            }
            return camerasNames;
        }

        /// <summary>
        /// Gets robot based on its ID
        /// </summary>
        /// <param name="robotId">UUID of robot</param>
        /// <returns></returns>
        public IRobot GetRobot(string robotId) {
            foreach (IRobot robot in GetRobots()) {
                if (robot.GetId() == robotId)
                    return robot;
            }
            throw new ItemNotFoundException("No robot with id: " + robotId);
        }

        /// <summary>
        /// Gets robot based on its name
        /// </summary>
        /// <param name="robotName">Human readable name of robot</param>
        /// <returns>Robot</returns>
        public IRobot GetRobotByName(string robotName) {
            foreach (IRobot robot in GetRobots())
                if (robot.GetName() == robotName)
                    return robot;
            throw new ItemNotFoundException("Robot with name " + robotName + " does not exists!");
        }

        /// <summary>
        /// Convers robots name to ID
        /// </summary>
        /// <param name="robotName">Robots name</param>
        /// <returns>Robots ID</returns>
        public string RobotNameToId(string robotName) {
            return GetRobotByName(robotName).GetId();
        }

        /// <summary>
        /// Updates action object in scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        public void SceneObjectUpdated(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                actionObject.ActionObjectUpdate(sceneObject);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            SceneChanged = true;
        }

        /// <summary>
        /// Updates metadata of action object in scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        public void SceneObjectBaseUpdated(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {

            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            SceneChanged = true;
        }

        /// <summary>
        /// Adds action object to scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        /// <returns></returns>
        public void SceneObjectAdded(SceneObject sceneObject) {
            ActionObject actionObject = SpawnActionObject(sceneObject.Id, sceneObject.Type);
            actionObject.ActionObjectUpdate(sceneObject);
            SceneChanged = true;
        }

        /// <summary>
        /// Removes action object from scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        public void SceneObjectRemoved(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                ActionObjects.Remove(sceneObject.Id);
                Destroy(actionObject.gameObject);
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            SceneChanged = true;
        }

        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        /// <param name="scene">Scene description</param>
        /// <param name="customCollisionModels">Allows to override action object collision model</param>
        /// <returns></returns>
        public void UpdateActionObjects(Scene scene, CollisionModels customCollisionModels = null) {
            List<string> currentAO = new List<string>();
            foreach (IO.Swagger.Model.SceneObject aoSwagger in scene.Objects) {
                ActionObject actionObject = SpawnActionObject(aoSwagger.Id, aoSwagger.Type, customCollisionModels);
                actionObject.ActionObjectUpdate(aoSwagger);
                currentAO.Add(aoSwagger.Id);
            }

        }

        /// <summary>
        /// Gets next action object in dictionary. Allows to iterate through all action objects
        /// </summary>
        /// <param name="aoId">Current action object UUID</param>
        /// <returns></returns>
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

        /// <summary>
        /// Invoked when scene was saved
        /// </summary>
        internal void SceneSaved() {
            Base.Notifications.Instance.ShowToastMessage("Scene saved successfully.");
            OnSceneSaved?.Invoke(this, EventArgs.Empty);
            SceneChanged = false;
        }

        /// <summary>
        /// Gets previous action object in dictionary. Allows to iterate through all action objects
        /// </summary>
        /// <param name="aoId">Current action object UUID</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets first action object in dictionary all null if empty
        /// </summary>
        /// <returns></returns>
        public ActionObject GetFirstActionObject() {
            if (ActionObjects.Count == 0) {
                return null;
            }
            return ActionObjects.First().Value;
        }

        public void SetVisibilityActionObjects(float value) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetVisibility(value);
            }
            PlayerPrefsHelper.SaveFloat("AOVisibility" + (VRModeManager.Instance.VRModeON ? "VR" : "AR"), value);
            ActionObjectsVisibility = value;
        }

        /// <summary>
        /// Sets whether action objects should react to user inputs (i.e. enables/disables colliders)
        /// </summary>
        public void SetActionObjectsInteractivity(bool interactivity) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetInteractivity(interactivity);
            }
            PlayerPrefsHelper.SaveBool("scene/" + SceneMeta.Id + "/AOInteractivity", interactivity);
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
        /// <param name="Id">Action object ID</param>
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

        /// <summary>
        /// Tries to get action object based on its human readable name
        /// </summary>
        /// <param name="name">Human readable name</param>
        /// <param name="actionObjectOut">Found action object</param>
        /// <returns>True if object was found, false otherwise</returns>
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

        /// <summary>
        /// Checks if there is action object of given name
        /// </summary>
        /// <param name="name">Human readable name of actio point</param>
        /// <returns>True if action object with given name exists, false otherwise</returns>
        public bool ActionObjectsContainName(string name) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.Data.Name == name) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disables (i.e. greys out) all action objects
        /// </summary>
        public void DisableAllActionObjects() {
            foreach (ActionObject ao in ActionObjects.Values) {
                ao.Disable();
            }
        }

        /// <summary>
        /// Enables all action objects
        /// </summary>
        public void EnableAllActionObjects() {
            foreach (ActionObject ao in ActionObjects.Values) {
                ao.Enable();
            }
        }

        

        #endregion

    }
}

