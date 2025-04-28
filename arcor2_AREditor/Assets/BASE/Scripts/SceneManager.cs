using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using UnityEngine.Events;

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
        /// Invoked when robot and EE are selected (can be opened robot stepping menu)
        /// </summary>
        public event EventHandler OnRobotSelected;
        /// <summary>
        /// Contains metainfo about scene (id, name, modified etc) without info about objects and services
        /// </summary>
        public Scene SceneMeta = null;
        /// <summary>
        /// Holds all action objects in scene
        /// </summary>
        public Dictionary<string, ActionObject> ActionObjects = new();
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
        /// Prefab for action object without pose
        /// </summary>
        public GameObject ActionObjectNoPosePrefab;
        /// <summary>
        /// Prefab for collision object
        /// </summary>
        public GameObject CollisionObjectPrefab;
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
        /// Flag which indicates whether scene update event should be trigered during update
        /// </summary>
        private bool updateScene = false;

        public event EventHandler<SceneStateEventArgs> OnSceneStateEvent;

        public IRobot SelectedRobot;

        public string SelectedArmId;

        public string SelectCreatedActionObject;
        public bool OpenTransformMenuOnCreatedObject;



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
        /// Defines if scene was started on server - e.g. if all robots and other action objects
        /// are instantioned and are ready
        /// </summary>
        public bool SceneStarted {
            get;
            private set;
        } = false;

        public RobotEE SelectedEndEffector {
            get;
            set;
        }

        /// <summary>
        /// Creates scene from given json
        /// </summary>
        /// <param name="scene">Json describing scene.</param>
        /// <param name="loadResources">Indicates if resources should be loaded from server.</param>
        /// <param name="customCollisionModels">Allows to override collision models with different ones. Usable e.g. for
        /// project running screen.</param>
        /// <returns>True if scene successfully created, false otherwise</returns>
        public async Task<bool> CreateScene(Scene scene, bool loadResources, CollisionModels customCollisionModels = null) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (SceneMeta != null)
                return false;
            SelectorMenu.Instance.Clear();
            SetSceneMeta(DataHelper.SceneToBareScene(scene));
            this.loadResources = loadResources;
            LoadSettings();

            UpdateActionObjects(scene, customCollisionModels);

            if (scene.Modified == DateTime.MinValue) { //new scene, never saved
                sceneChanged = true;
            } else if (scene.IntModified == DateTime.MinValue) {
                sceneChanged = false;
            } else {
                sceneChanged = scene.IntModified > scene.Modified;
            }
            Valid = true;
            OnLoadScene?.Invoke(this, EventArgs.Empty);
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
            SelectorMenu.Instance.SelectorItems.Clear();
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
            SceneMeta.Description = scene.Description;
            SceneMeta.IntModified = scene.IntModified;
            SceneMeta.Modified = scene.Modified;
            SceneMeta.Name = scene.Name;
        }

        /// <summary>
        /// Gets scene metadata.
        /// </summary>
        /// <returns></returns>
        public Scene GetScene() {
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
            if (updateScene) {
                SceneChanged = true;
                updateScene = false;
            }
        }

        /// <summary>
        /// Initialization of scene manager
        /// </summary>
        private void Start() {
            CommunicationManager.Instance.Client.RobotEndEffectorUpdated += CommunicationManager.SafeEventHandler<RobotEndEffectorUpdatedEventArgs>(RobotEefUpdated);
            CommunicationManager.Instance.Client.RobotJointsUpdated += CommunicationManager.SafeEventHandler<RobotJointsUpdatedEventArgs>(RobotJointsUpdated);
            CommunicationManager.Instance.Client.SceneBaseUpdated += CommunicationManager.SafeEventHandler<BareSceneEventArgs>(OnSceneBaseUpdated);
            CommunicationManager.Instance.Client.SceneState += CommunicationManager.SafeEventHandler<SceneStateEventArgs>(OnSceneState);

            CommunicationManager.Instance.Client.ProjectOverrideAdded += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideAddedOrUpdated);
            CommunicationManager.Instance.Client.ProjectOverrideUpdated += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideAddedOrUpdated);
            CommunicationManager.Instance.Client.ProjectOverrideRemoved += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideRemoved);
        }

        private void OnOverrideRemoved(object sender, ParameterEventArgs args) {
            try {
                ActionObject actionObject = GetActionObject(args.ParentId);
                actionObject.Overrides.Remove(args.Data.Name);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);

            }
        }

        private void OnOverrideAddedOrUpdated(object sender, ParameterEventArgs args) {

            try {
                ActionObject actionObject = GetActionObject(args.ParentId);
                if (actionObject.TryGetParameterMetadata(args.Data.Name, out ParameterMeta parameterMeta)) {
                    Parameter p = new(parameterMeta, args.Data.Value);
                    actionObject.Overrides[args.Data.Name] = p;
                }

            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);

            }

        }

        private async void OnSceneState(object sender, SceneStateEventArgs args) {
            switch (args.Data.State) {
                case SceneStateData.StateEnum.Starting:
                    GameManager.Instance.ShowLoadingScreen("Going online...");
                    OnSceneStateEvent?.Invoke(this, args); // needs to be rethrown to ensure all subscribers has updated data
                    break;
                case SceneStateData.StateEnum.Stopping:
                    SceneStarted = false;
                    GameManager.Instance.ShowLoadingScreen("Going offline...");
                    if (!string.IsNullOrEmpty(args.Data.Message)) {
                        Notifications.Instance.ShowNotification("Scene service failed", args.Data.Message);
                    }
                    OnSceneStateEvent?.Invoke(this, args); // needs to be rethrown to ensure all subscribers has updated data
                    break;
                case SceneStateData.StateEnum.Started:
                    StartCoroutine(WaitUntillSceneValid(() => OnSceneStarted(args)));
                    break;
                case SceneStateData.StateEnum.Stopped:
                    SceneStarted = false;
                    GameManager.Instance.HideLoadingScreen();
                    SelectedRobot = null;
                    SelectedArmId = null;
                    SelectedEndEffector = null;
                    OnSceneStateEvent?.Invoke(this, args); // needs to be rethrown to ensure all subscribers has updated data
                    if (RobotsEEVisible)
                        OnHideRobotsEE?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private IEnumerator WaitUntillSceneValid(UnityAction callback) {
            yield return new WaitUntil(() => Valid);
            callback();
        }

        private async void OnSceneStarted(SceneStateEventArgs args) {
            SceneStarted = true;
            if (RobotsEEVisible)
                OnShowRobotsEE?.Invoke(this, EventArgs.Empty);
            RegisterRobotsForEvent(true, RegisterForRobotEventRequestArgs.WhatEnum.Joints);
            string selectedRobotID = PlayerPrefsHelper.LoadString(SceneMeta.Id + "/selectedRobotId", null);
            SelectedArmId = PlayerPrefsHelper.LoadString(SceneMeta.Id + "/selectedRobotArmId", null);
            string selectedEndEffectorId = PlayerPrefsHelper.LoadString(SceneMeta.Id + "/selectedEndEffectorId", null);
            await SelectRobotAndEE(selectedRobotID, SelectedArmId, selectedEndEffectorId);
            GameManager.Instance.HideLoadingScreen();
            OnSceneStateEvent?.Invoke(this, args); // needs to be rethrown to ensure all subscribers has updated data
        }

        public async Task SelectRobotAndEE(string robotId, string armId, string eeId) {
            if (!string.IsNullOrEmpty(robotId)) {
                try {
                    IRobot robot = GetRobot(robotId);
                    if (!string.IsNullOrEmpty(eeId)) {
                        try {
                            SelectRobotAndEE(await robot.GetEE(eeId, armId));
                        } catch (ItemNotFoundException ex) {
                            PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedEndEffectorId", null);
                            PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotArmId", null);
                            Debug.LogError(ex);
                        }
                    }
                } catch (ItemNotFoundException ex) {
                    PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotId", null);
                    Debug.LogError(ex);
                }
            } else {
                SelectRobotAndEE(null);
            }
        }

        public void SelectRobotAndEE(RobotEE endEffector) {
            if (endEffector == null) {
                SelectedArmId = null;
                SelectedRobot = null;
                SelectedEndEffector = null;
                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotId", null);
                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotArmId", null);
                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedEndEffectorId", null);
            } else {
                try {
                    SelectedArmId = endEffector.ARMId;
                    SelectedRobot = GetRobot(endEffector.Robot.GetId());
                    SelectedEndEffector = endEffector;
                } catch (ItemNotFoundException ex) {
                    PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotId", null);
                    Debug.LogError(ex);
                }

                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotId", SelectedRobot.GetId());
                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedRobotArmId", SelectedArmId);
                PlayerPrefsHelper.SaveString(SceneMeta.Id + "/selectedEndEffectorId", SelectedEndEffector.EEId);
            }

            OnRobotSelected(this, EventArgs.Empty);
        }

        public bool IsRobotSelected() {
            return SelectedRobot != null && !string.IsNullOrEmpty(SelectedArmId);
        }

        public bool IsRobotAndEESelected() {
            return IsRobotSelected() && SelectedEndEffector != null;
        }

        /// <summary>
        /// Register or unregister to/from subsription of joints or end effectors pose of each robot in the scene.
        /// </summary>
        /// <param name="send">To subscribe or to unsubscribe</param>
        /// <param name="what">Pose of end effectors or joints</param>
        public void RegisterRobotsForEvent(bool send, RegisterForRobotEventRequestArgs.WhatEnum what) {
            foreach (IRobot robot in GetRobots()) {
                CommunicationManager.Instance.Client.RegisterForRobotEventAsync(new RegisterForRobotEventRequestArgs(robot.GetId(), what, send));
            }
        }

        private void OnSceneBaseUpdated(object sender, BareSceneEventArgs args) {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor) {
                SetSceneMeta(args.Data);
                updateScene = true;
            }
        }

        /// <summary>
        /// Updates robot model based on recieved joints.
        /// </summary>
        /// <param name="sender">Who invoked event.</param>
        /// <param name="args">Robot joints data</param>
        public async void RobotJointsUpdated(object sender, RobotJointsUpdatedEventArgs args) {
            // if initializing or deinitializing scene OR scene is not started, dont update robot joints
            if (!Valid || !SceneStarted)
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
        public async void RobotEefUpdated(object sender, RobotEndEffectorUpdatedEventArgs args) {
            if (!RobotsEEVisible || !Valid) {
                return;
            }
            foreach (RobotEefDataEefPose eefPose in args.Data.EndEffectors) {
                try {
                    IRobot robot = GetRobot(args.Data.RobotId);
                    RobotEE ee = await robot.GetEE(eefPose.EndEffectorId, eefPose.ArmId);
                    ee.UpdatePosition(TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(eefPose.Pose.Position)),
                        TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(eefPose.Pose.Orientation)));
                } catch (ItemNotFoundException) {
                    continue;
                }

            }
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
            RobotsEEVisible = true;
            if (SceneStarted) {
                OnShowRobotsEE?.Invoke(this, EventArgs.Empty);
            } else {
                Notifications.Instance.ShowToastMessage("End effectors will be shown after going online");
            }

            PlayerPrefsHelper.SaveBool("scene/" + SceneMeta.Id + "/RobotsEEVisibility", true);
            return true;
        }

        /// <summary>
        /// Hides end effectors and unregister from EE positions and robot joints subscription
        /// </summary>
        public void HideRobotsEE() {
            Debug.LogError("hide");
            RobotsEEVisible = false;
            OnHideRobotsEE?.Invoke(this, EventArgs.Empty);
            PlayerPrefsHelper.SaveBool("scene/" + SceneMeta.Id + "/RobotsEEVisibility", false);
        }

        /// <summary>
        /// Loads selected setings from player prefs
        /// </summary>
        internal void LoadSettings() {
            ActionObjectsVisibility = PlayerPrefsHelper.LoadFloat("AOVisibility" + (VRModeManager.Instance.VRModeON ? "VR" : "AR"), VRModeManager.Instance.VRModeON ? 1f : 0f);
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
            GameObject tmpGo = new();
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
        public ActionObject SpawnActionObject(SceneObject sceneObject, CollisionModels customCollisionModels = null) {
            if (!ActionsManager.Instance.ActionObjectsMetadata.TryGetValue(sceneObject.Type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);
            } else if (aom.CollisionObject) {
                obj = Instantiate(CollisionObjectPrefab, ActionObjectsSpawn.transform);
            } else if (aom.HasPose) {
                obj = Instantiate(ActionObjectPrefab, ActionObjectsSpawn.transform);
            } else {
                obj = Instantiate(ActionObjectNoPosePrefab, ActionObjectsSpawn.transform);
            }
            ActionObject actionObject = obj.GetComponent<ActionObject>();
            actionObject.InitActionObject(sceneObject, obj.transform.localPosition, obj.transform.localRotation, aom, customCollisionModels);

            // Add the Action Object into scene reference
            ActionObjects.Add(sceneObject.Id, actionObject);
            actionObject.SetVisibility(ActionObjectsVisibility);
            actionObject.ActionObjectUpdate(sceneObject);
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

        public string GetFreeObjectTypeName(string objectTypeName) {
            int i = 1;
            bool hasFreeName;
            string freeName = objectTypeName;
            do {
                hasFreeName = true;
                if (ActionsManager.Instance.ActionObjectsMetadata.ContainsKey(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ToUnderscoreCase(objectTypeName) + "_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public string GetFreeSceneName(string sceneName) {
            int i = 1;
            bool hasFreeName;
            string freeName = sceneName;
            do {
                hasFreeName = true;
                try {
                    GameManager.Instance.GetSceneId(freeName);
                    hasFreeName = false;
                    freeName = sceneName + "_" + i++.ToString();
                } catch (Arcor2ConnectionException) {

                }

            } while (!hasFreeName);

            return freeName;
        }

        /// <summary>
        /// Returns all robots in scene
        /// </summary>
        /// <returns></returns>
        public List<IRobot> GetRobots() {
            List<IRobot> robots = new();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsRobot()) {
                    robots.Add((RobotActionObject) actionObject);
                }
            }
            return robots;
        }

        public List<ActionObject> GetCameras() {
            List<ActionObject> cameras = new();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsCamera()) {
                    cameras.Add(actionObject);
                }
            }
            return cameras;
        }

        public List<string> GetCamerasIds() {
            List<string> cameraIds = new();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.IsCamera()) {
                    cameraIds.Add(actionObject.Data.Id);
                }
            }
            return cameraIds;
        }

        public List<string> GetCamerasNames() {
            List<string> camerasNames = new();
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
            updateScene = true;
        }

        /// <summary>
        /// Adds action object to scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        /// <returns></returns>
        public void SceneObjectAdded(SceneObject sceneObject) {
            ActionObject actionObject = SpawnActionObject(sceneObject);
            if (!string.IsNullOrEmpty(SelectCreatedActionObject) && sceneObject.Name.Contains(SelectCreatedActionObject)) {
                if (ActionObjectPickerMenu.Instance.IsVisible())
                    AREditorResources.Instance.LeftMenuScene.AddButtonClick();
                SelectorMenu.Instance.SetSelectedObject(actionObject.SelectorItem, true);
                if (!actionObject.ActionObjectMetadata.HasPose)
                    SelectorMenu.Instance.BottomButtons.SelectButton(SelectorMenu.Instance.BottomButtons.Buttons[2], true);
            }
            if (OpenTransformMenuOnCreatedObject) {
                AREditorResources.Instance.LeftMenuScene.SetActiveSubmenu(LeftMenuSelection.Utility);
                AREditorResources.Instance.LeftMenuScene.MoveClick();
            }
            SelectCreatedActionObject = "";
            OpenTransformMenuOnCreatedObject = false;
            updateScene = true;
        }

        /// <summary>
        /// Removes action object from scene
        /// </summary>
        /// <param name="sceneObject">Description of action object</param>
        public void SceneObjectRemoved(SceneObject sceneObject) {
            ActionObject actionObject = GetActionObject(sceneObject.Id);
            if (actionObject != null) {
                ActionObjects.Remove(sceneObject.Id);
                actionObject.DeleteActionObject();
            } else {
                Debug.LogError("Object " + sceneObject.Name + "(" + sceneObject.Id + ") not found");
            }
            updateScene = true;
        }

        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        /// <param name="scene">Scene description</param>
        /// <param name="customCollisionModels">Allows to override action object collision model</param>
        /// <returns></returns>
        public void UpdateActionObjects(Scene scene, CollisionModels customCollisionModels = null) {
            List<string> currentAO = new();
            foreach (SceneObject aoSwagger in scene.Objects) {
                ActionObject actionObject = SpawnActionObject(aoSwagger, customCollisionModels);
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
            Notifications.Instance.ShowToastMessage("Scene saved successfully.");
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
                if (actionObject != null)
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
            if (ActionObjects.TryGetValue(id, out ActionObject actionObject))
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
        /// Enables all action objects
        /// </summary>
        public void EnableAllActionObjects(bool enable, bool includingRobots = true) {
            foreach (ActionObject ao in ActionObjects.Values) {
                if (!includingRobots && ao.IsRobot())
                    continue;
                ao.Enable(enable);
            }
        }

        public void EnableAllRobots(bool enable) {
            foreach (ActionObject ao in ActionObjects.Values) {
                if (ao.IsRobot())
                    ao.Enable(enable);
            }
        }

        public List<ActionObject> GetAllActionObjectsWithoutPose() {
            List<ActionObject> objects = new();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (!actionObject.ActionObjectMetadata.HasPose && actionObject.gameObject.activeSelf) {
                    objects.Add(actionObject);
                }
            }
            return objects;
        }

        public async Task<List<RobotEE>> GetAllRobotsEEs() {
            List<RobotEE> eeList = new();
            foreach (ActionObject ao in ActionObjects.Values) {
                if (ao.IsRobot())
                    eeList.AddRange(await ((IRobot) ao).GetAllEE());
            }
            return eeList;
        }

        public List<ActionObject> GetAllObjectsOfType(string type) {
            return ActionObjects.Values.Where(obj => obj.ActionObjectMetadata.Type == type).ToList();
        }



        #endregion

    }
}

