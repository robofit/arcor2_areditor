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

        //public Dictionary<string, RobotLink> Links = new Dictionary<string, RobotLink>();
        //public Dictionary<string, string> Joints = new Dictionary<string, string>();
        public RobotModel RobotModel {
            get; private set;
        }

        private bool robotLoaded = false;

        public List<string> EndEffectors = new List<string>();
        
        private GameObject RobotPlaceholder;
        //private GameObject RobotModel;
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
            //UrdfManager.Instance.OnUrdfReady += OnUrdfDownloaded;
            Data.Id = id;
            Data.Type = type;
            SetScenePosition(position);
            SetSceneOrientation(orientation);Application.targetFrameRate = 30;
            Data.Id = uuid;
            ActionObjectMetadata = actionObjectMetadata;
            CreateModel(customCollisionModels);
            enabled = true;
            SetVisibility(visibility);
            if (HasUrdf()) {
                RobotModel = UrdfManager.Instance.GetRobotModelInstance(type);
                if (RobotModel != null) {
                    RobotModelLoaded();
                } else {
                    UrdfManager.Instance.OnRobotUrdfModelLoaded += OnRobotModelLoaded;
                }
            }
        }

        private void OnRobotModelLoaded(object sender, RobotUrdfModelArgs args) {
            Debug.Log("URDF: robot is fully loaded");

            // check if the robot of the type we need was loaded
            if (args.RobotType == Data.Type) {
                // if so, lets ask UrdfManager for the robot model
                RobotModel = UrdfManager.Instance.GetRobotModelInstance(Data.Type);
               
                RobotModelLoaded();
                
                // if robot is loaded, unsubscribe from UrdfManager event
                UrdfManager.Instance.OnRobotUrdfModelLoaded -= OnRobotModelLoaded;
            }
        }

        private void RobotModelLoaded() {
            Debug.Log("URDF: robot is fully loaded");

            RobotModel.RobotModelGameObject.transform.parent = transform;
            RobotModel.RobotModelGameObject.transform.localPosition = Vector3.zero;
            RobotModel.RobotModelGameObject.transform.localEulerAngles = Vector3.zero;

            // retarget OnClickCollider target to receive OnClick events
            foreach (OnClickCollider onCLick in RobotModel.RobotModelGameObject.GetComponentsInChildren<OnClickCollider>(true)) {
                onCLick.Target = gameObject;
            }

            RobotModel.SetActiveAllVisuals(true);

            outlineOnClick.ClearRenderers();
            RobotPlaceholder.SetActive(false);
            Destroy(RobotPlaceholder);

            robotColliders.Clear();
            robotRenderers.Clear();
            robotRenderers.AddRange(RobotModel.RobotModelGameObject.GetComponentsInChildren<Renderer>());
            robotColliders.AddRange(RobotModel.RobotModelGameObject.GetComponentsInChildren<Collider>());
            outlineOnClick.InitRenderers(robotRenderers);
            outlineOnClick.OutlineShaderType = OutlineOnClick.OutlineType.TwoPassShader;
            GameManager.Instance.SetDefaultFramerate();
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
            // if RobotModel was present, lets return it to the UrdfManager robotModel pool
            if (RobotModel != null) {
                UrdfManager.Instance.ReturnRobotModelInstace(RobotModel);
            }
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
