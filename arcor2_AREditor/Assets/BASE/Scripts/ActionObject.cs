using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Base {
    public abstract class ActionObject : Clickable, IActionProvider, IActionPointParent {

        // string == IO.Swagger.Model.SceneObject Data.Uuid
        //public Dictionary<string, ActionPoint> ActionPoints = new Dictionary<string, ActionPoint>();
        public GameObject ActionPointsSpawn;
        [System.NonSerialized]
        public int CounterAP = 0;
        protected float visibility;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject(id: "", name: "", pose: DataHelper.CreatePose(new Vector3(), new Quaternion()), type: "");
        public ActionObjectMetadata ActionObjectMetadata;
        public List<string> EndEffectors = new List<string>();
        protected ActionObjectMenu actionObjectMenu;
        protected ActionObjectMenuProjectEditor actionObjectMenuProjectEditor;

        protected virtual void Start() {
            actionObjectMenu = MenuManager.Instance.ActionObjectMenuSceneEditor.gameObject.GetComponent<ActionObjectMenu>();
            actionObjectMenuProjectEditor = MenuManager.Instance.ActionObjectMenuProjectEditor.gameObject.GetComponent<ActionObjectMenuProjectEditor>();
        }

        public virtual void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata) {
            visibility = GameManager.Instance.LoadFloat(Scene.Instance.Data.Id + "/ActionObject/" + id + "/visibility", 1);
        }
        
        public virtual void UpdateUserId(string newUserId) {
            Data.Name = newUserId;
        }

        public async virtual void RenameActionObject(string newUserId) {
            bool result = await GameManager.Instance.RenameActionObject(Data.Id, newUserId);
        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.localPosition);
                SetSceneOrientation(transform.localRotation);
                transform.hasChanged = false;
            }
        }

        public virtual void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger, bool visibility, bool interactivity) {
            if (Data != null & Data.Name != actionObjectSwagger.Name)
                UpdateUserId(actionObjectSwagger.Name);
            Data = actionObjectSwagger;

            // update position and rotation based on received data from swagger
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
            if (visibility)
                Show();
            else
                Hide();

            SetInteractivity(interactivity);

            
        }

        public virtual bool SceneInteractable() {
            return (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor);
        }

        public async void LoadEndEffectors() {
            List<IO.Swagger.Model.IdValue> idValues = new List<IO.Swagger.Model.IdValue>();
            EndEffectors = await GameManager.Instance.GetActionParamValues(Data.Id, "end_effector_id", idValues);
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

        public List<string> GetEndEffectors() {
            return EndEffectors;
        }

        public bool IsRobot() {
            return ActionObjectMetadata.Robot;
        }

        public void DeleteActionObject() {
            // Remove all actions of this action point
            RemoveActionPoints();
            
            // Remove this ActionObject reference from the scene ActionObject list
            Scene.Instance.ActionObjects.Remove(this.Data.Id);

            Destroy(gameObject);
        }
        
        public void RemoveActionPoints() {
            // Remove all action points of this action object
            List<ActionPoint> actionPoints = GetActionPoints();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }


        public virtual void SetVisibility(float value) {
            Debug.Assert(value >= 0 && value <= 1, "Action object: " + Data.Id + " SetVisibility(" + value.ToString() + ")");
            visibility = value;
            GameManager.Instance.SaveFloat(Scene.Instance.Data.Id + "/ActionObject/" + Data.Id + "/visibility", value);
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
                actionObjectMenu.UpdateMenu();
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

        public List<ActionPoint> GetActionPoints() {
            List<ActionPoint> actionPoints = new List<ActionPoint>();
            foreach (ActionPoint actionPoint in Scene.Instance.ActionPoints.Values) {
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
    }

}
