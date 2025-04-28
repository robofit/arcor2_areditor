using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using TMPro;
using UnityEngine;
using Joint = Arcor2.ClientSdk.Communication.OpenApi.Models.Joint;

namespace Base {

    [RequireComponent(typeof(OutlineOnClick))]
    [RequireComponent(typeof(Target))]
    public class RobotActionObject : ActionObject, IRobot {

        public TextMeshPro ActionObjectName;
        public GameObject RobotPlaceholderPrefab;
        public GameObject LockIcon;

        private OutlineOnClick outlineOnClick;

        public bool ResourcesLoaded = false;

        [SerializeField]
        private GameObject EEOrigin;

        private bool eeVisible = false;

        public RobotModel RobotModel {
            get; private set;
        }
        public bool manipulationStarted {
            get;
        }
        public bool updatePose {
            get;
        }

        private bool robotVisible = false;

        private Dictionary<string, List<RobotEE>> EndEffectors = new();

        private GameObject RobotPlaceholder;

        private List<Renderer> robotRenderers = new();
        private List<Collider> robotColliders = new();

        private bool transparent = false;
        private bool ghost = false;

        private Shader standardShader;
        private Shader ghostShader;
        private Shader transparentShader;

        private bool jointStateSubscribeIsValid = true;
        private bool modelLoading = false;

        private bool loadingEndEffectors = false;

        private bool isGreyColorForced;

        public RobotMeta RobotMeta;

        protected override void Start() {
            base.Start();
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning && SceneManager.Instance.RobotsEEVisible && SceneManager.Instance.SceneStarted) {
                _ = EnableVisualisationOfEE();
            }
            SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
        }

        // ONDESTROY CANNOT BE USED BECAUSE OF ITS DELAYED CALL - it causes mess when directly creating project from scene
        private void OnDestroy() {
            SceneManager.Instance.OnSceneStateEvent -= OnSceneStateEvent;
        }

        private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
            UpdateColor();
            if (HasUrdf() && RobotModel != null)
                SetDefaultJoints();

            if (args.Data.State == SceneStateData.StateEnum.Stopped) {
                HideRobotEE();
            }
        }


        private void OnDisable() {
            SceneManager.Instance.OnShowRobotsEE -= OnShowRobotsEE;
            SceneManager.Instance.OnHideRobotsEE -= OnHideRobotsEE;
            SceneManager.Instance.OnSceneStateEvent -= OnSceneStateEvent;
        }

        private void OnEnable() {
            SceneManager.Instance.OnShowRobotsEE += OnShowRobotsEE;
            SceneManager.Instance.OnHideRobotsEE += OnHideRobotsEE;
            SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
        }

        private void OnShowRobotsEE(object sender, EventArgs e) {
            _ = EnableVisualisationOfEE();
        }

        private void OnHideRobotsEE(object sender, EventArgs e) {
            _ = DisableVisualisationOfEE();
        }

        protected override async void Update() {
            base.Update();
        }

        private async void UpdatePose() {
            try {
                var response = await CommunicationManager.Instance.Client.UpdateActionObjectPoseAsync(new UpdateObjectPoseRequestArgs(Data.Id, GetPose()));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to update action object pose", string.Join(",", response.Messages));
                    ResetPosition();
                }
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to update action object pose", e.Message);
                ResetPosition();
            }
        }

        public void ShowRobotEE() {
            foreach (List<RobotEE> eeList in EndEffectors.Values)
                foreach (RobotEE ee in eeList) {
                    ee.gameObject.SetActive(true);
                }
        }

        public void HideRobotEE() {
            foreach (List<RobotEE> eeList in EndEffectors.Values) {
                foreach (RobotEE ee in eeList) {
                    try {
                        ee.gameObject.SetActive(false);
                    } catch (Exception ex) when (ex is NullReferenceException || ex is MissingReferenceException) {
                        continue;
                    }
                }

            }
        }

        public async Task DisableVisualisationOfEE() {
            if (!eeVisible)
                return;
            eeVisible = false;
            if (EndEffectors.Count > 0) {
                await CommunicationManager.Instance.Client.RegisterForRobotEventAsync(new RegisterForRobotEventRequestArgs(GetId(), RegisterForRobotEventRequestArgs.WhatEnum.EefPose, false));
                HideRobotEE();
            }
        }


        public async Task EnableVisualisationOfEE() {
            if (eeVisible)
                return;
            eeVisible = true;
            if (!ResourcesLoaded)
                await LoadResources();
            if (EndEffectors.Count > 0) {
                await CommunicationManager.Instance.Client.RegisterForRobotEventAsync(new RegisterForRobotEventRequestArgs(GetId(), RegisterForRobotEventRequestArgs.WhatEnum.EefPose, true));
                ShowRobotEE();
            }
        }


        public async override void InitActionObject(SceneObject sceneObject, Vector3 position, Quaternion orientation, ActionObjectMetadata actionObjectMetadata, CollisionModels customCollisionModels = null, bool loadResources = true) {
            base.InitActionObject(sceneObject, position, orientation, actionObjectMetadata);

            // if there should be an urdf robot model
            if (ActionsManager.Instance.RobotsMeta.TryGetValue(sceneObject.Type, out RobotMeta robotMeta)) {
                RobotMeta = robotMeta;
                if (!string.IsNullOrEmpty(robotMeta.UrdfPackageFilename)) {
                    // Get the robot model, if it returns null, the robot will be loading itself
                    RobotModel = UrdfManager.Instance.GetRobotModelInstance(robotMeta.Type, robotMeta.UrdfPackageFilename);

                    if (RobotModel != null) {
                        RobotModelLoaded();
                    } else {
                        // Robot is not loaded yet, let's wait for it to be loaded
                        UrdfManager.Instance.OnRobotUrdfModelLoaded += OnRobotModelLoaded;
                        modelLoading = true;
                    }
                }
            }

            ResourcesLoaded = false;
        }

        private void OnRobotModelLoaded(object sender, RobotUrdfModelArgs args) {

            // check if the robot of the type we need was loaded
            if (args.RobotType == Data.Type) {
                // if so, lets ask UrdfManager for the robot model
                RobotModel = UrdfManager.Instance.GetRobotModelInstance(Data.Type);

                RobotModelLoaded();

                // if robot is loaded, unsubscribe from UrdfManager event
                UrdfManager.Instance.OnRobotUrdfModelLoaded -= OnRobotModelLoaded;
                modelLoading = false;
            }
        }

        private async void RobotModelLoaded() {
            RobotModel.RobotModelGameObject.transform.parent = transform;
            RobotModel.RobotModelGameObject.transform.localPosition = Vector3.zero;
            RobotModel.RobotModelGameObject.transform.localRotation = Quaternion.identity;

            // retarget OnClickCollider target to receive OnClick events
            foreach (OnClickCollider onCLick in RobotModel.RobotModelGameObject.GetComponentsInChildren<OnClickCollider>(true)) {
                onCLick.Target = gameObject;
            }

            RobotModel.SetActiveAllVisuals(true);

            bool outlineWasHighlighted = outlineOnClick.Highlighted;

            outlineOnClick.UnHighlight();
            outlineOnClick.ClearRenderers();
            RobotPlaceholder.SetActive(false);
            Destroy(RobotPlaceholder);

            robotColliders.Clear();
            robotRenderers.Clear();
            robotRenderers.AddRange(RobotModel.RobotModelGameObject.GetComponentsInChildren<Renderer>(true));
            robotColliders.AddRange(RobotModel.RobotModelGameObject.GetComponentsInChildren<Collider>(true));
            outlineOnClick.InitRenderers(robotRenderers);
            outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.TwoPassShader;
            outlineOnClick.InitMaterials();

            SetOutlineSizeBasedOnScale();

            SetVisibility(visibility, true);

            SetDefaultJoints();

            Target target = GetComponent<Target>();
            if (target != null) {
                target.ChangeTarget(RobotModel.RobotModelGameObject);
            }

            if (outlineWasHighlighted) {
                outlineOnClick.Highlight();
                if (SelectorMenu.Instance.ManuallySelected) {
                    DisplayOffscreenIndicator(true);
                }
            }

            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning || GameManager.Instance.GetGameState() != GameManager.GameStateEnum.LoadingPackage)
                await CommunicationManager.Instance.Client.RegisterForRobotEventAsync(new RegisterForRobotEventRequestArgs(GetId(), RegisterForRobotEventRequestArgs.WhatEnum.Joints, true));
        }

        private void SetOutlineSizeBasedOnScale() {
            float robotScale = 0f;
            foreach (RobotLink link in RobotModel.Links.Values) {
                robotScale = link.LinkScale;
                if (!link.IsBaseLink && robotScale != 0) {
                    break;
                }
            }
            outlineOnClick.CompensateOutlineByModelScale(robotScale);
        }

        private void SetDefaultJoints() {
            foreach (var joint in RobotModel.Joints) {
                SetJointValue(joint.Key, 0f);
            }
        }

        public override Vector3 GetScenePosition() {
            return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Pose.Position));
        }

        public override void SetScenePosition(Vector3 position) {
            Data.Pose.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
        }

        public override Quaternion GetSceneOrientation() {
            return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Pose.Orientation));
        }

        public override void SetSceneOrientation(Quaternion orientation) {
            Data.Pose.Orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation));
        }

        public override void Show() {
            robotVisible = true;
            SetVisibility(1);
        }

        public override void Hide() {
            robotVisible = false;
            SetVisibility(0);
        }

        public override void SetInteractivity(bool interactive) {
            foreach (Collider collider in robotColliders) {
                collider.enabled = interactive;
            }
        }

        public override void SetVisibility(float value, bool forceShaderChange = false) {
            base.SetVisibility(value);

            if (standardShader == null) {
                standardShader = Shader.Find("Standard");
            }

            if (ghostShader == null) {
                ghostShader = Shader.Find("Custom/Ghost");
            }

            if (transparentShader == null) {
                transparentShader = Shader.Find("Transparent/Diffuse");
            }

            // Set opaque shader
            if (value >= 1) {
                transparent = false;
                ghost = false;
                foreach (Renderer renderer in robotRenderers) {
                    // Robot has its outline active, we need to select second material,
                    // (first is mask, second is object material, third is outline)
                    if (renderer.materials.Length == 3) {
                        renderer.materials[1].shader = standardShader;
                    } else {
                        renderer.material.shader = standardShader;
                    }
                }
            }
            // Set transparent shader
            else if (value <= 0.1) {
                ghost = false;
                if (forceShaderChange || !transparent) {
                    foreach (Renderer renderer in robotRenderers) {
                        // Robot has its outline active, we need to select second material,
                        // (first is mask, second is object material, third is outline)
                        if (renderer.materials.Length == 3) {
                            renderer.materials[1].shader = transparentShader;
                        } else {
                            renderer.material.shader = transparentShader;
                        }

                        Material mat;
                        if (renderer.materials.Length == 3) {
                            mat = renderer.materials[1];
                        } else {
                            mat = renderer.material;
                        }
                        Color color = mat.color;
                        color.a = 0f;
                        mat.color = color;
                    }
                    transparent = true;
                }
            } else {
                transparent = false;
                if (forceShaderChange || !ghost) {
                    foreach (Renderer renderer in robotRenderers) {
                        if (renderer.materials.Length == 3) {
                            renderer.materials[1].shader = ghostShader;
                        } else {
                            renderer.material.shader = ghostShader;
                        }
                    }
                    ghost = true;
                }
                // set alpha of the material
                foreach (Renderer renderer in robotRenderers) {
                    Material mat;
                    if (renderer.materials.Length == 3) {
                        mat = renderer.materials[1];
                    } else {
                        mat = renderer.material;
                    }
                    Color color = mat.color;
                    color.a = value;
                    mat.color = color;
                }
            }

            UpdateColor();
        }

        public void EnableStandardShader() {
            foreach (Renderer i in robotRenderers) {
                if (i.materials.Length == 3) {
                    i.materials[1].shader = Shader.Find("Standard");
                } else {
                    i.material.shader = Shader.Find("Standard");
                }
            }
        }

        public void EnableInvalidShader() {
            foreach (Renderer i in robotRenderers) {
                if (i.materials.Length == 3) {
                    i.materials[1].shader = Shader.Find("InvalidPose");
                } else {
                    i.material.shader = Shader.Find("InvalidPose");
                }
            }
        }


        public async Task<List<string>> GetEndEffectorIds(string arm_id = null) {
            await LoadResources();
            List<string> result = new();
            if (string.IsNullOrEmpty(arm_id)) {
                foreach (List<RobotEE> eeList in EndEffectors.Values) {
                    foreach (RobotEE ee in eeList) {
                        result.Add(ee.EEId);
                    }
                }
            } else if (EndEffectors.ContainsKey(arm_id)) {
                foreach (RobotEE ee in EndEffectors[arm_id]) {
                    result.Add(ee.EEId);
                }
            } else {
                throw new KeyNotFoundException($"Robot {GetName()} does not contain arm {arm_id}");
            }

            return result;
        }

        private async Task LoadResources() {
            if (!ResourcesLoaded) {
                ResourcesLoaded = await LoadEndEffectorsAndArms();
            }
        }

        private Task<bool> WaitUntilResourcesReady() {
            return Task.Run(() => {
                while (true) {
                    if (ResourcesLoaded) {
                        return true;
                    } else if (!loadingEndEffectors) {
                        return false;
                    } else {
                        Thread.Sleep(10);
                    }
                }
            });

        }

        public async Task<bool> LoadEndEffectorsAndArms() {
            if (!SceneManager.Instance.Valid) {
                Debug.LogError("SceneManager instance not valid");
                return false;
            }
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning) {
                loadingEndEffectors = false;
                GameManager.Instance.HideLoadingScreen();
                return true;
            }
            if (loadingEndEffectors) {
                await WaitUntilResourcesReady();
                return true;
            } else {
                loadingEndEffectors = true;
            }
            GameManager.Instance.ShowLoadingScreen("Loading end effectors of robot " + Data.Name);
            try {
                Dictionary<string, List<string>> endEffectors = new();

                if (RobotMeta.MultiArm) {
                    List<string> arms;
                    var response = await CommunicationManager.Instance.Client.GetRobotArmsAsync(new GetRobotArmsRequestArgs(Data.Id));
                    if (!response.Result) {
                        Debug.LogError(string.Join(',', response.Messages));
                        Notifications.Instance.ShowNotification("Failed to load end effectors", string.Join(',', response.Messages));
                        return false;
                    } else {
                        arms = response.Data;
                    }

                    foreach (string arm in arms) {
                        var responseEef = await CommunicationManager.Instance.Client.GetRobotEndEffectorsAsync(new GetEndEffectorsRequestArgs(Data.Id, arm));
                        if (!responseEef.Result) {
                            Debug.LogError(string.Join(',', responseEef.Messages));
                            Notifications.Instance.ShowNotification("Failed to load end effectors", string.Join(',', responseEef.Messages));
                            return false;
                        } else {
                            endEffectors[arm] = responseEef.Data;
                        }
                    }
                } else {
                    var response = await CommunicationManager.Instance.Client.GetRobotEndEffectorsAsync(new GetEndEffectorsRequestArgs(Data.Id));
                    if (!response.Result) {
                        Debug.LogError(string.Join(',', response.Messages));
                        Notifications.Instance.ShowNotification("Failed to load end effectors", string.Join(',', response.Messages));
                        return false;
                    } else {
                        endEffectors["default"] = response.Data;
                    }
                }
                foreach (KeyValuePair<string, List<string>> eeList in endEffectors) {
                    foreach (string eeId in eeList.Value) {
                        CreateEndEffector(eeList.Key, eeId);
                    }
                }

                return true;
            } catch (Arcor2ConnectionException ex) {
                Debug.LogError(ex.Message);
                Notifications.Instance.ShowNotification("Failed to load end effectors", ex.Message);
                return false;
            } finally {
                loadingEndEffectors = false;
                GameManager.Instance.HideLoadingScreen();
            }
        }

        private RobotEE CreateEndEffector(string armId, string eeId) {
            RobotEE ee = Instantiate(SceneManager.Instance.RobotEEPrefab, EEOrigin.transform).GetComponent<RobotEE>();
            ee.InitEE(this, armId, eeId);
            ee.gameObject.SetActive(false);
            if (!EndEffectors.ContainsKey(armId)) {
                EndEffectors.Add(armId, new List<RobotEE>());
            }
            EndEffectors[armId].Add(ee);
            return ee;
        }

        public override void CreateModel(CollisionModels customCollisionModels = null) {
            RobotPlaceholder = Instantiate(RobotPlaceholderPrefab, transform);
            RobotPlaceholder.transform.parent = transform;
            RobotPlaceholder.transform.localPosition = Vector3.zero;
            RobotPlaceholder.transform.localPosition = Vector3.zero;

            RobotPlaceholder.GetComponent<OnClickCollider>().Target = gameObject;

            robotColliders.Clear();
            robotRenderers.Clear();
            robotRenderers.AddRange(RobotPlaceholder.GetComponentsInChildren<Renderer>());
            robotColliders.AddRange(RobotPlaceholder.GetComponentsInChildren<Collider>());
            Colliders = robotColliders;
            outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
            outlineOnClick.InitRenderers(robotRenderers);
            outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.TwoPassShader;
            Target target = GetComponent<Target>();
            if (target != null) {
                target.ChangeTarget(RobotPlaceholder);
            }
        }

        public override GameObject GetModelCopy() {
            if (RobotModel?.RobotModelGameObject != null)
                return Instantiate(RobotModel.RobotModelGameObject);
            else
                return Instantiate(RobotPlaceholder);
        }

        public bool HasUrdf() {
            if (ActionsManager.Instance.RobotsMeta.TryGetValue(Data.Type, out RobotMeta robotMeta)) {
                return !string.IsNullOrEmpty(robotMeta.UrdfPackageFilename);
            }
            return false;
        }

        public override void OnHoverStart() {
            if (!enabled)
                return;
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
                GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionObject &&
                GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
                if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
                    if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning)
                        return;
                } else {
                    return;
                }
            }
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor &&
                GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor &&
                GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning) {
                return;
            }
            ActionObjectName.gameObject.SetActive(true);
            outlineOnClick.Highlight();
            if (SelectorMenu.Instance.ManuallySelected) {
                DisplayOffscreenIndicator(true);
            }
        }

        public override void OnHoverEnd() {
            ActionObjectName.gameObject.SetActive(false);
            outlineOnClick.UnHighlight();
            DisplayOffscreenIndicator(false);
        }

        public override void UpdateObjectName(string newUserId) {
            base.UpdateObjectName(newUserId);
            ActionObjectName.text = newUserId;
        }

        public override void ActionObjectUpdate(SceneObject actionObjectSwagger) {
            base.ActionObjectUpdate(actionObjectSwagger);
            ActionObjectName.text = actionObjectSwagger.Name;
            // update label on each end effector
            foreach (List<RobotEE> arm in EndEffectors.Values) {
                foreach (RobotEE ee in arm)
                    ee.UpdateLabel();
            }
            ResetPosition();
        }

        public async Task<RobotEE> GetEE(string ee_id, string arm_id) {
            bool packageRunning = GameManager.Instance.GetGameState() == GameManager.GameStateEnum.PackageRunning ||
                GameManager.Instance.GetGameState() == GameManager.GameStateEnum.LoadingPackage;
            if (!packageRunning && !ResourcesLoaded) {
                await LoadResources();
            }

            string realArmId = arm_id;
            if (!MultiArm())
                realArmId = "default";

            if (!EndEffectors.ContainsKey(realArmId)) {
                if (packageRunning) {
                    EndEffectors.Add(realArmId, new List<RobotEE>());
                } else {
                    throw new ItemNotFoundException($"Robot {GetName()} does not have arm {realArmId}");
                }
            }
            foreach (RobotEE ee in EndEffectors[realArmId])
                if (ee.EEId == ee_id)
                    return ee;
            if (packageRunning) {
                return CreateEndEffector(realArmId, ee_id);
            }
            throw new ItemNotFoundException("End effector with ID " + ee_id + " not found for " + GetName());
        }

        /// <summary>
        /// Sets value of joints specified in List joints. Firstly checks if joint names are really equal or not.
        /// If some joint name is not correct, method will not allow to set the joints nor to check if they are valid, unless option forceJointsValidCheck is set to true.
        /// </summary>
        /// <param name="joints">List of joints with new angle values.</param>
        /// <param name="angle_in_degrees">Whether the joint angle is in degrees.</param>
        /// <param name="forceJointsValidCheck">If true, check for valid joint names will be called even if previous one failed.</param>
        public void SetJointValue(List<Joint> joints, bool angle_in_degrees = false, bool forceJointsValidCheck = false) {
            if (RobotModel != null && (jointStateSubscribeIsValid || forceJointsValidCheck)) {
                if (CheckJointsAreValid(joints)) {
                    foreach (Joint joint in joints) {
                        SetJointValue(joint.Name, (float) joint.Value);
                    }
                    jointStateSubscribeIsValid = true;
                } else {
                    Notifications.Instance.ShowNotification("Wrong joint names received!", "Unregistering joint state receiving for robot " + RobotModel.RobotType + ". Joints has to be named same as in urdf.");
                    jointStateSubscribeIsValid = false;
                }
            }
        }

        /// <summary>
        /// Checks if the joint names in joints corresponds to the joint names in RobotModel.
        /// </summary>
        /// <param name="joints"></param>
        /// <returns>True if joints have equal names, false if not.</returns>
        public bool CheckJointsAreValid(List<Joint> joints) {
            if (RobotModel != null) {
                List<string> receivedJoints = new();
                foreach (Joint joint in joints) {
                    receivedJoints.Add(joint.Name);
                }

                foreach (string jointName in RobotModel.Joints.Keys) {
                    receivedJoints.Remove(jointName);
                }

                if (receivedJoints.Count != 0) {
                    Debug.LogError("Received wrong joints: " + string.Join(",", joints) + " .. but expected: " + string.Join(",", RobotModel.GetJoints()));
                    Notifications.Instance.ShowNotification("Received wrong joints!", "Received:" + string.Join(",", joints) + ".. but expected: " + string.Join(",", RobotModel.GetJoints()));
                    return false;
                } else {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the value of individual joint.
        /// </summary>
        /// <param name="name">Joint name.</param>
        /// <param name="angle">Joint angle (in radians by default).</param>
        /// <param name="angle_in_degrees">Whether the joint angle is in degrees.</param>
        public void SetJointValue(string name, float angle, bool angle_in_degrees = false) {
            RobotModel?.SetJointAngle(name, angle, angle_in_degrees);
        }

        public List<Joint> GetJoints() {
            if (RobotModel == null) {
                // if urdf model is still loading, return empty joint list
                if (modelLoading) {
                    return new List<Joint>();
                } else {
                    throw new Arcor2ConnectionException("Model not found for this robot.");
                }
            } else
                return RobotModel.GetJoints();
        }

        public override void DeleteActionObject() {
            UnloadRobotModel();
            UrdfManager.Instance.OnRobotUrdfModelLoaded -= OnRobotModelLoaded;
            modelLoading = false;
            base.DeleteActionObject();
        }

        private void UnloadRobotModel() {
            // if RobotModel was present, lets return it to the UrdfManager robotModel pool
            if (RobotModel != null) {
                if (UrdfManager.Instance != null) {
                    // remove every outlines on the robot
                    outlineOnClick.UnHighlight();
                    UrdfManager.Instance.ReturnRobotModelInstace(RobotModel);
                }
            }
        }

        /// <summary>
        /// Sets grey color of robot model (indicates that model is not in position of real robot)
        /// </summary>
        /// <param name="grey">True for setting grey, false for standard state.</param>
        public void SetGrey(bool grey, bool force = false) {
            isGreyColorForced = force && grey;
            if (force) {
                UpdateColor();
                return;
            }

            if (grey) {
                foreach (Renderer renderer in robotRenderers) {
                    foreach (Material mat in renderer.materials) {
                        mat.SetColor("_EmissionColor", new Color(0.2f, 0.05f, 0.05f));
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            } else {
                foreach (Renderer renderer in robotRenderers) {
                    foreach (Material mat in renderer.materials) {
                        mat.SetColor("_EmissionColor", new Color(0.2f, 0.05f, 0.05f, 0f));
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        public override void OpenMenu() {
            _ = ActionObjectMenu.Instance.Show(this, false);
        }

        public override bool HasMenu() {
            return true;
        }

        public override async void StartManipulation() {
            throw new NotImplementedException();
        }

        public async Task<List<RobotEE>> GetAllEE() {
            await LoadResources();
            List<RobotEE> eeList = new();
            foreach (List<RobotEE> ee in EndEffectors.Values)
                eeList.AddRange(ee);
            return eeList;
        }

        public override string GetObjectTypeName() {
            return "Robot";
        }

        public override void UpdateColor() {
            if (!HasUrdf())
                return;

            SetGrey(!SceneManager.Instance.SceneStarted || IsLockedByOtherUser || isGreyColorForced);
        }

        public override void OnObjectLocked(string owner) {
            base.OnObjectLocked(owner);
            if (IsLockedByOtherUser) {
                ActionObjectName.text = GetLockedText();
                LockIcon.SetActive(true);
            }
        }

        public override void OnObjectUnlocked() {
            base.OnObjectUnlocked();
            ActionObjectName.text = GetName();
            LockIcon.SetActive(false);
        }

        public async Task<List<string>> GetArmsIds() {
            await LoadResources();
            return EndEffectors.Keys.ToList();
        }

        public bool MultiArm() {
            return RobotMeta.MultiArm;
        }

        public override void CloseMenu() {
            ActionObjectMenu.Instance.Hide();
        }

        public override void EnableVisual(bool enable) {
            if (RobotModel != null)
                RobotModel.RobotModelGameObject.SetActive(enable);
        }

        string IRobot.LockOwner() {
            return LockOwner;
        }

        public override void UpdateModel() {
            return;
        }

        public override async Task<RequestResult> Movable() {
            RequestResult result = await base.Movable();
            if (result.Success && SceneManager.Instance.SceneStarted) {
                result.Success = false;
                result.Message = "Robot could only be manipulated when scene is offline.";
            }
            return result;
        }

        public InteractiveObject GetInteractiveObject() {
            return this;
        }
    }
}
