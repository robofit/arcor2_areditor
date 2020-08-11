using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading.Tasks;
using IO.Swagger.Model;
using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using RosSharp.Urdf.Runtime;
using TMPro;
using UnityEngine;

namespace Base {
    public class RobotActionObject : ActionObject, IRobot {
        
        public TextMeshPro ActionObjectName;
        public GameObject RobotPlaceholderPrefab;

        private OutlineOnClick outlineOnClick;

        public Dictionary<string, RobotLink> Links = new Dictionary<string, RobotLink>();
        public Dictionary<string, string> Joints = new Dictionary<string, string>();

        private bool robotLoaded = false;

        public List<string> EndEffectors = new List<string>();
        
        private GameObject RobotPlaceholder;
        private GameObject RobotModel;
        private UrdfRobot UrdfRobot;
        private List<Renderer> robotRenderers = new List<Renderer>();
        private List<Collider> robotColliders = new List<Collider>();

        private bool transparent = false;

        private Shader standardShader;
        private Shader transparentShader;

        protected override void Start() {
            base.Start();
        }

        public override void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata, IO.Swagger.Model.CollisionModels customCollisionModels = null) {
            base.InitActionObject(id, type, position, orientation, uuid, actionObjectMetadata);
            SceneManager.Instance.OnUrdfReady += OnUrdfDownloaded;
            Data.Id = id;
            Data.Type = type;
            SetScenePosition(position);
            SetSceneOrientation(orientation);
            Data.Id = uuid;
            ActionObjectMetadata = actionObjectMetadata;
            CreateModel(customCollisionModels);
            enabled = true;
            SetVisibility(visibility);
        }
        
        private void OnUrdfDownloaded(object sender, RobotUrdfArgs args) {
            Debug.Log("URDF: urdf is downloaded and extracted");
            // check if downloaded urdf contains this robot
            if (ActionObjectMetadata.Type == args.RobotType) {
                DirectoryInfo dir = new DirectoryInfo(args.Path);

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

                    ImportUrdfObject(files[0].FullName);

                    // subscribe for ColladaImporter event in order to load robot links
                    ColladaImporter.Instance.OnModelImported += OnColladaModelImported;
                }
            }
        }

        /// <summary>
        /// Imports URDF based on a given filename. Filename has to contain a full path.
        /// </summary>
        /// <param name="filename"></param>
        private void ImportUrdfObject(string filename) {
            UrdfRobot = UrdfRobotExtensionsRuntime.Create(filename, useUrdfMaterials: false);
            UrdfRobot.transform.parent = transform;
            UrdfRobot.transform.localPosition = Vector3.zero;
            UrdfRobot.transform.localEulerAngles = Vector3.zero;

            UrdfRobot.SetRigidbodiesIsKinematic(true);

            RobotModel = UrdfRobot.gameObject;

            LoadLinks();

            Debug.Log("URDF: robot created (without models yet)");

        }

        private void OnColladaModelImported(object sender, ImportedColladaEventArgs args) {
            Debug.Log("URDF: Collada model imported");
            Transform importedModel = args.Data.transform;

            RobotActionObject[] robots = importedModel.GetComponentsInParent<Base.RobotActionObject>(true);
            if (robots != null) {
                RobotActionObject robot = robots[0];

                // check if imported model corresponds to this robot
                if (ReferenceEquals(robot, this)) {

                    // get rid of the placeholder object (New Game Object)
                    Transform placeholderGameObject = importedModel.parent;
                    importedModel.SetParent(placeholderGameObject.parent, worldPositionStays: false);

                    //TODO: Temporarily, colliders are added directly to Visuals
                    AddColliders(importedModel.gameObject, setConvex: true);

                    Destroy(placeholderGameObject.gameObject);

                    SetLinkVisualLoaded(importedModel.parent.parent.parent.name, importedModel.parent.gameObject.GetComponent<UrdfVisual>());

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
                OnClickCollider click = child.AddComponent<OnClickCollider>();
                click.Target = this.gameObject;
            }
        }

        /// <summary>
        /// Initializes RobotLinks and sets a boolean to its Visuals dictionary,
        /// telling whether the model of individual visual was already imported (is type of box, cylinder, capsule)
        /// or not yet (is mesh - is going to be continually imported from ColladaImporter).
        /// </summary>
        private void LoadLinks() {
            // Get all UrdfLink components in builded Robot
            foreach (UrdfLink link in GetComponentsInChildren<UrdfLink>()) {

                // Get all UrdfVisuals of each UrdfLink
                GameObject visualsGameObject = link.gameObject.GetComponentInChildren<UrdfVisuals>().gameObject;
                Dictionary<UrdfVisual, bool> visuals = new Dictionary<UrdfVisual, bool>();
                // Traverse each UrdfVisual and set a boolean indicating whether its visual is already loaded (is of some basic type - box, cylinder, capsule)
                // or is going to be loaded by ColladaImporter (in case its type of mesh)
                foreach (UrdfVisual visual in visualsGameObject.GetComponentsInChildren<UrdfVisual>()) {
                    visuals.Add(visual, visual.GeometryType == GeometryTypes.Mesh ? false : true);
                    // hide visual if it is mesh.. mesh will be displayed when fully loaded
                    visual.gameObject.SetActive(visual.GeometryType == GeometryTypes.Mesh ? false : true);
                }
                
                UrdfJoint urdfJoint = link.GetComponent<UrdfJoint>();
                JointStateWriter jointWriter = null;
                if(urdfJoint != null) {
                    if (urdfJoint.JointType != UrdfJoint.JointTypes.Fixed) {
                        jointWriter = urdfJoint.transform.AddComponentIfNotExists<JointStateWriter>();
                        Joints.Add(urdfJoint.JointName, link.gameObject.name);
                    }
                }
                Links.Add(link.gameObject.name, new RobotLink(link.gameObject.name, urdfJoint, jointWriter, visuals, is_base_link:link.IsBaseLink));

            }
        }

        public void SetRandomJointAngles() {
            foreach (RobotLink link in Links.Values) {
                link.SetJointAngle(Random.Range(-6.28f, 6.28f));
            }
        }

        /// <summary>
        /// Sets angle of joint in given linkName.
        /// </summary>
        /// <param name="jointName"></param>
        /// <param name="angle"></param>
        public void SetJointAngle(string jointName, float angle) {
            if (robotLoaded) {
                Joints.TryGetValue(jointName, out string linkName);
                Links.TryGetValue(linkName, out RobotLink link);
                //Debug.Log(linkName + " ..angle in deg: " + angle + " ..angle in rad: " + angle * Mathf.Deg2Rad);
                angle *= Mathf.Deg2Rad;
                link?.SetJointAngle(angle);
            }
        }

        /// <summary>
        /// Sets that visual of a given link is loaded (ColladaImporter imported mesh).
        /// </summary>
        /// <param name="linkName"></param>
        /// <param name="urdfVisual"></param>
        public void SetLinkVisualLoaded(string linkName, UrdfVisual urdfVisual) {
            Links.TryGetValue(linkName, out RobotLink link);
            link?.SetVisualLoaded(urdfVisual);

            IsRobotLoaded();

            // if robot is loaded, show its visuals, otherwise hide them
            link?.SetActiveVisuals(robotLoaded);
        }

        /// <summary>
        /// Checks that all visuals (meshes, primitive types - box, cylinder..) of the robot are imported and created.
        /// </summary>
        /// <returns></returns>
        private bool IsRobotLoaded() {
            if (!robotLoaded) {
                foreach (RobotLink link in Links.Values) {
                    if (!link.HasVisualsLoaded()) {
                        return false;
                    }
                }
            }
            robotLoaded = true;
            OnRobotLoaded();

            return true;
        }

        private void OnRobotLoaded() {
            Debug.Log("URDF: robot is fully loaded");

            SetActiveAllVisuals(true);

            // if robot is loaded, unsubscribe from ColladaImporter event, for performance efficiency
            ColladaImporter.Instance.OnModelImported -= OnColladaModelImported;

            outlineOnClick.ClearRenderers();
            RobotPlaceholder.SetActive(false);
            Destroy(RobotPlaceholder);

            robotColliders.Clear();
            robotRenderers.Clear();
            robotRenderers.AddRange(RobotModel.GetComponentsInChildren<Renderer>());
            robotColliders.AddRange(RobotModel.GetComponentsInChildren<Collider>());
            outlineOnClick.InitRenderers(robotRenderers);
            outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.TwoPassShader;
        }

        /// <summary>
        /// Displays or hides all visuals of the robot.
        /// </summary>
        /// <param name="active"></param>
        private void SetActiveAllVisuals(bool active) {
            foreach (RobotLink link in Links.Values) {
                link.SetActiveVisuals(active);
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
            foreach (Renderer renderer in robotRenderers) {
                renderer.enabled = true;
            }
        }

        public override void Hide() {
            foreach (Renderer renderer in robotRenderers) {
                renderer.enabled = false;
            }
        }

        public override void SetInteractivity(bool interactive) {
            foreach (Collider collider in robotColliders) {
                collider.enabled = interactive;
            }
        }

        public override void SetVisibility(float value) {
            base.SetVisibility(value);

            if (standardShader == null) {
                standardShader = Shader.Find("Standard");
            }

            if (transparentShader == null) {
                transparentShader = Shader.Find("Transparent/Diffuse");
            }

            // Set opaque shader
            if (value >= 1) {
                transparent = false;
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
            else {
                if (!transparent) {
                    foreach (Renderer renderer in robotRenderers) {
                        if (renderer.materials.Length == 3) {
                            renderer.materials[1].shader = transparentShader;
                        } else {
                            renderer.material.shader = transparentShader;
                        }
                    }
                    transparent = true;
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
        }

        public override void OnClick(Click type) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingActionObject ||
             GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingActionPointParent) {
                GameManager.Instance.ObjectSelected(this);
                return;
            }
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
                return;
            }
            if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor &&
                GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
                Notifications.Instance.ShowNotification("Not allowed", "Editation of action object only allowed in scene or project editor");
                return;
            }
            // HANDLE MOUSE
            if (type == Click.MOUSE_RIGHT_BUTTON) {
                ShowMenu();
            }
            // HANDLE TOUCH
            else if (type == Click.TOUCH) {
                ShowMenu();
            }
        }

        public List<string> GetEndEffectors() {
            return EndEffectors;
        }

        public async Task LoadEndEffectors() {
            List<IO.Swagger.Model.IdValue> idValues = new List<IO.Swagger.Model.IdValue>();
            EndEffectors = await WebsocketManager.Instance.GetActionParamValues(Data.Id, "end_effector_id", idValues);
        }

        public override void CreateModel(CollisionModels customCollisionModels = null) {
            RobotPlaceholder = Instantiate(RobotPlaceholderPrefab, transform);
            RobotPlaceholder.transform.parent = transform;
            RobotPlaceholder.transform.localPosition = Vector3.zero;
            RobotPlaceholder.transform.localPosition = Vector3.zero;
            //Model.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);

            RobotPlaceholder.GetComponent<OnClickCollider>().Target = gameObject;

            robotColliders.Clear();
            robotRenderers.Clear();
            robotRenderers.AddRange(RobotPlaceholder.GetComponentsInChildren<Renderer>());
            robotColliders.AddRange(RobotPlaceholder.GetComponentsInChildren<Collider>());
            outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
            outlineOnClick.InitRenderers(robotRenderers);
            outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.OnePassShader;
        }

        public override GameObject GetModelCopy() {
            throw new System.NotImplementedException();
        }


        public bool HasUrdf() {
            if (Base.ActionsManager.Instance.RobotsMeta.TryGetValue(Data.Type, out RobotMeta robotMeta)) {
                return !string.IsNullOrEmpty(robotMeta.UrdfPackageFilename);
            }
            return false;
        }

        private void OnDestroy() {
            if(SceneManager.Instance != null)
                SceneManager.Instance.OnUrdfReady -= OnUrdfDownloaded;
        }

        public override void OnHoverStart() {
            if (!enabled)
                return;
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
                GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionObject &&
                GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
                if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Closed) {
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
        }

        public override void OnHoverEnd() {
            ActionObjectName.gameObject.SetActive(false);
            outlineOnClick.UnHighlight();
        }

        public override void UpdateUserId(string newUserId) {
            base.UpdateUserId(newUserId);
            ActionObjectName.text = newUserId;
        }

        public override void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger, bool visibility, bool interactivity) {
            base.ActionObjectUpdate(actionObjectSwagger, visibility, interactivity);
            ActionObjectName.text = actionObjectSwagger.Name;
        }

    }
}
