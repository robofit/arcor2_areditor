using UnityEngine;

namespace Base {
    public abstract class ActionObject : Clickable {
       [System.NonSerialized]
        public GameObject ConnectionPrefab;

        public GameObject ActionPoints;
        [System.NonSerialized]
        public int CounterAP = 0;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject("", DataHelper.CreatePose(new Vector3(), new Quaternion()), "");
        public ActionObjectMetadata ActionObjectMetadata;

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

    }

}
