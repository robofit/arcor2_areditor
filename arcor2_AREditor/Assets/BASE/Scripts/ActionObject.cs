using UnityEngine;

namespace Base {
    public class ActionObject : MonoBehaviour {
        public string Type;
        [System.NonSerialized]
        public Vector3 Position;
        [System.NonSerialized]
        public Quaternion Orientation = new Quaternion(0, 0, 0, 1);
        [System.NonSerialized]
        public GameObject InteractiveObjectMenu;
        [System.NonSerialized]
        public GameObject ConnectionPrefab;

        public GameObject ActionPoints;
        private MenuManager menuManager;
        [System.NonSerialized]
        public int CounterAP = 0;
        private Vector3 offset;
        private GameManager gameManager;
        private string id;


        public ActionObjectMetadata ActionObjectMetadata;

        public string Id {
            get => id; set => id = value;
        }

        private void Start() {
            gameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
            ActionPoints = transform.Find("ActionPoints").gameObject;
            menuManager = GameObject.Find("_MenuManager").gameObject.GetComponent<MenuManager>();
            InteractiveObjectMenu = menuManager.InteractiveObjectMenu;
            ConnectionPrefab = gameManager.ConnectionPrefab;
        }

        private void Touch() {
            menuManager.ShowMenu(InteractiveObjectMenu, Id);
            InteractiveObjectMenu.GetComponent<InteractiveObjectMenu>().CurrentObject = gameObject;
        }

        private void OnMouseDown() => offset = gameObject.transform.position -
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));

        private void OnMouseDrag() {
            Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
            transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
        }

        private void OnMouseUp() => gameManager.UpdateScene();

        public void DeleteIO(bool updateScene = true) {
            foreach (Base.ActionPoint ap in GetComponentsInChildren<Base.ActionPoint>()) {
                ap.DeleteAP(false);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
            if (updateScene)
                gameManager.UpdateScene();
        }

    }

}
