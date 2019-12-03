using UnityEngine;
using System.Collections.Generic;

namespace Base {
    public abstract class ActionObject : Clickable, IActionProvider {
       [System.NonSerialized]
        public GameObject ConnectionPrefab;

        public GameObject ActionPoints;
        [System.NonSerialized]
        public int CounterAP = 0;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject("", DataHelper.CreatePose(new Vector3(), new Quaternion()), "");
        public ActionObjectMetadata ActionObjectMetadata;
        public List<string> EndEffectors = new List<string>();

        protected virtual void Start() {
            ConnectionPrefab = GameManager.Instance.ConnectionPrefab;
        }

        public virtual void UpdateId(string newId) {
            Data.Id = newId;
        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.localPosition);
                SetSceneOrientation(transform.localRotation);
                transform.hasChanged = false;
            }
        }

        public virtual bool SceneInteractable() {
            return (GameManager.Instance.GameState == GameManager.GameStateEnum.SceneEditor &&
                GameManager.Instance.SceneInteractable);
        }

        public async void LoadEndEffectors() {
            List<IO.Swagger.Model.IdValue> idValues = new List<IO.Swagger.Model.IdValue>();
            //idValues.Add(new IO.Swagger.Model.IdValue(id: "robot_id", value: Data.Id));
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
            return Data.Id;
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
    }

}
