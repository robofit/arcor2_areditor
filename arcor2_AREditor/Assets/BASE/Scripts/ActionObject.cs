using UnityEngine;

namespace Base {
    public class ActionObject : MonoBehaviour {
        [System.NonSerialized]
        public GameObject InteractiveObjectMenu;
        [System.NonSerialized]
        public GameObject ConnectionPrefab;

        public GameObject ActionPoints;
        [System.NonSerialized]
        public int CounterAP = 0;
        private string id;

        public IO.Swagger.Model.SceneObject Data = new IO.Swagger.Model.SceneObject();
        public ActionObjectMetadata ActionObjectMetadata;

        public string Id {
            get => id; set => id = value;
        }

        private void Start() {
            ActionPoints = transform.Find("ActionPoints").gameObject;
            InteractiveObjectMenu = MenuManager.Instance.InteractiveObjectMenu;
            ConnectionPrefab = GameManager.Instance.ConnectionPrefab;
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

    }

}
