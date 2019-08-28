using UnityEngine;

namespace Base {
    public abstract class ActionObject : MonoBehaviour {
        [System.NonSerialized]
        public GameObject InteractiveObjectMenu;
        [System.NonSerialized]
        public GameObject ConnectionPrefab;

        public GameObject ActionPoints;
        [System.NonSerialized]
        public int CounterAP = 0;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject();
        public ActionObjectMetadata ActionObjectMetadata;


        protected virtual void Awake() {
            Data.Pose = DataHelper.CreatePose(new Vector3(), new Quaternion());
            //GameManager.Instance.Scene.GetComponent<Scene>().Data.Objects.Add(Data);
        }

        private void Start() {
            ActionPoints = transform.Find("ActionPoints").gameObject;
            InteractiveObjectMenu = MenuManager.Instance.InteractiveObjectMenu;
            ConnectionPrefab = GameManager.Instance.ConnectionPrefab;
        }

        public void UpdateId(string newId) {
            Data.Id = newId;
            //foreach (Action action in GetComponentsInChildren<Action>()) {
            //    action.UpdateType();
            //}
        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.position);
                SetSceneOrientation(transform.rotation);
                transform.hasChanged = false;
            }
        }

        public void DeleteIO(bool updateScene = true) {
            foreach (Base.ActionPoint ap in GetComponentsInChildren<Base.ActionPoint>()) {
                ap.DeleteAP(false);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
            if (updateScene)
                GameManager.Instance.UpdateScene();
        }

        public abstract Vector3 GetScenePosition();

        public abstract void SetScenePosition(Vector3 position);

        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);

    }

}
