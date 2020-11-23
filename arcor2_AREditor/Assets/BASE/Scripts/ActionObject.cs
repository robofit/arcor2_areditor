using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO.Swagger.Model;

namespace Base {
    public abstract class ActionObject : Clickable, IActionProvider, IActionPointParent {

        public GameObject ActionPointsSpawn;
        [System.NonSerialized]
        public int CounterAP = 0;
        protected float visibility;

        public Collider Collider;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject(id: "", name: "", pose: DataHelper.CreatePose(new Vector3(), new Quaternion()), type: "");
        public ActionObjectMetadata ActionObjectMetadata;
        protected ActionObjectMenu actionObjectMenu;
        protected ActionObjectMenuProjectEditor actionObjectMenuProjectEditor;

        public Dictionary<string, Parameter> ObjectParameters = new Dictionary<string, Parameter>();
        public Dictionary<string, Parameter> Overrides = new Dictionary<string, Parameter>();

        protected virtual void Start() {
            actionObjectMenu = MenuManager.Instance.ActionObjectMenuSceneEditor.gameObject.GetComponent<ActionObjectMenu>();
            actionObjectMenuProjectEditor = MenuManager.Instance.ActionObjectMenuProjectEditor.gameObject.GetComponent<ActionObjectMenuProjectEditor>();


        }

        public virtual void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata, IO.Swagger.Model.CollisionModels customCollisionModels = null, bool loadResuources = true) {
            Data.Id = id;
            Data.Type = type;
            SetScenePosition(position);
            SetSceneOrientation(orientation);
            Data.Id = uuid;
            ActionObjectMetadata = actionObjectMetadata;
            CreateModel(customCollisionModels);
            enabled = true;
            if (VRModeManager.Instance.VRModeON) {
                SetVisibility(PlayerPrefsHelper.LoadFloat("AOVisibilityVR", 1f));
            } else {
                SetVisibility(PlayerPrefsHelper.LoadFloat("AOVisibilityAR", 0f));
            }

        }
        
        public virtual void UpdateUserId(string newUserId) {
            Data.Name = newUserId;
        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                //SetScenePosition(transform.localPosition);
                //SetSceneOrientation(transform.localRotation);
                transform.hasChanged = false;
            }
        }

        public virtual void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger, float visibility, bool interactivity) {
            if (Data != null & Data.Name != actionObjectSwagger.Name)
                UpdateUserId(actionObjectSwagger.Name);
            Data = actionObjectSwagger;
            foreach (IO.Swagger.Model.Parameter p in Data.Parameters) {

                if (!ObjectParameters.ContainsKey(p.Name)) {
                    if (TryGetParameterMetadata(p.Name, out ParameterMeta parameterMeta)) {
                        ObjectParameters[p.Name] = new Parameter(parameterMeta, p.Value);
                    } else {
                        Debug.LogError("Failed to load metadata for parameter " + p.Name);
                        Notifications.Instance.ShowNotification("Critical error", "Failed to load parameter's metadata.");
                        return;
                    }

                } else {
                    ObjectParameters[p.Name].Value = p.Value;
                }
                
            }
            
            //TODO: update all action points and actions.. ?
            ResetPosition();
            // update position and rotation based on received data from swagger
            //if (visibility)
            //    Show();
            //else
            //    Hide();

            SetVisibility(visibility);
            SetInteractivity(interactivity);

            
        }

        public void ResetPosition() {
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
        }

        public virtual bool SceneInteractable() {
            return (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor);
        }

        public bool TryGetParameter(string id, out IO.Swagger.Model.Parameter parameter) {
            foreach (IO.Swagger.Model.Parameter p in Data.Parameters) {
                if (p.Name == id) {
                    parameter = p;
                    return true;
                }
            }
            parameter = null;
            return false;
        }
                
        public bool TryGetParameterMetadata(string id, out IO.Swagger.Model.ParameterMeta parameterMeta) {
            foreach (IO.Swagger.Model.ParameterMeta p in ActionObjectMetadata.Settings) {
                if (p.Name == id) {
                    parameterMeta = p;
                    return true;
                }
            }
            parameterMeta = null;
            return false;
        }
                
        public abstract Vector3 GetScenePosition();

        public abstract void SetScenePosition(Vector3 position);

        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);

        public void SetWorldPosition(Vector3 position) {
            Data.Pose.Position = DataHelper.Vector3ToPosition(position);
        }

        public Vector3 GetWorldPosition() {
            return DataHelper.PositionToVector3(Data.Pose.Position);
        }
        public void SetWorldOrientation(Quaternion orientation) {
            Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
        }

        public Quaternion GetWorldOrientation() {
            return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
        }

        public string GetProviderName() {
            return Data.Name;
        }


        public ActionMetadata GetActionMetadata(string action_id) {
            if (ActionObjectMetadata.ActionsLoaded) {
                if (ActionObjectMetadata.ActionsMetadata.TryGetValue(action_id, out ActionMetadata actionMetadata)) {
                    return actionMetadata;
                } else {
                    throw new ItemNotFoundException("Metadata not found");
                }
            }
            return null; //TODO: throw exception
        }


        public bool IsRobot() {
            return ActionObjectMetadata.Robot;
        }

        public virtual void DeleteActionObject() {
            // Remove all actions of this action point
            RemoveActionPoints();
            
            // Remove this ActionObject reference from the scene ActionObject list
            SceneManager.Instance.ActionObjects.Remove(this.Data.Id);

            Destroy(gameObject);
        }
        
        public void RemoveActionPoints() {
            // Remove all action points of this action object
            List<ActionPoint> actionPoints = GetActionPoints();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }


        public virtual void SetVisibility(float value, bool forceShaderChange = false) {
            //Debug.Assert(value >= 0 && value <= 1, "Action object: " + Data.Id + " SetVisibility(" + value.ToString() + ")");
            visibility = value;
            //PlayerPrefsHelper.SaveFloat(SceneManager.Instance.SceneMeta.Id + "/ActionObject/" + Data.Id + "/visibility", value);
        }

        public float GetVisibility() {
            return visibility;
        }

        public abstract void Show();

        public abstract void Hide();

        public abstract void SetInteractivity(bool interactive);


        public void ShowMenu() {
            if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
                actionObjectMenu.CurrentObject = this;
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
            } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
                actionObjectMenuProjectEditor.CurrentObject = this;
                actionObjectMenuProjectEditor.UpdateMenu();
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
            }
        }

        public virtual void ActivateForGizmo(string layer) {
            gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public string GetProviderId() {
            return Data.Id;
        }

        //TODO: is this working?
        public List<ActionPoint> GetActionPoints() {
            List<ActionPoint> actionPoints = new List<ActionPoint>();
            foreach (ActionPoint actionPoint in ProjectManager.Instance.ActionPoints.Values) {
                if (actionPoint.Data.Parent == Data.Id) {
                    actionPoints.Add(actionPoint);
                }
            }
            return actionPoints;
        }

        public string GetName() {
            return Data.Name;
        }

        public string GetId() {
            return Data.Id;
        }

        public bool IsActionObject() {
            return true;
        }

        public Base.ActionObject GetActionObject() {
            return this;
        }

        public Transform GetTransform() {
            return transform;
        }

        public string GetProviderType() {
            return Data.Type;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public abstract void CreateModel(IO.Swagger.Model.CollisionModels customCollisionModels = null);
        public abstract GameObject GetModelCopy();

    public IO.Swagger.Model.Pose GetPose() {
        if (ActionObjectMetadata.HasPose)
            return new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(transform.localPosition)),
                orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(transform.localRotation)));
        else
            return new IO.Swagger.Model.Pose(new IO.Swagger.Model.Orientation(), new IO.Swagger.Model.Position());
    }

    }

}
