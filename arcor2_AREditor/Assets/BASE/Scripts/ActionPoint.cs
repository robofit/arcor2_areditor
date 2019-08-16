using UnityEngine;


namespace Base {
    public class ActionPoint : MonoBehaviour {
        public ActionObject ActionObject;
        protected Vector3 offset;
        [System.NonSerialized]
        public int PuckCounter = 0;
        public Connection ConnectionToIO;

        [System.NonSerialized]
        public IO.Swagger.Model.ActionPoint Data = new IO.Swagger.Model.ActionPoint();


        private void Awake() {

        }

        void Update() {

        }

        public void SetActionObject(ActionObject actionObject) {
            ActionObject = actionObject;
            Data.Id = ActionObject.Id + " - AP" + ActionObject.CounterAP++.ToString();
        }

        public void DeleteAP(bool updateProject = true) {
            foreach (Action action in GetComponentsInChildren<Action>()) {
                action.DeleteAction(false);
            }
            Destroy(ConnectionToIO.gameObject);
            gameObject.SetActive(false);
            Destroy(gameObject);

            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public Vector3 GetScenePosition() {
            Vector3 position = Vector3.Scale(GameManager.Instance.Scene.transform.InverseTransformPoint(transform.position) +
                new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1));
            position.z = 0.7f;
            return position;
        }

        public void SetScenePosition(Vector3 position) => transform.position = GameManager.Instance.Scene.transform.TransformPoint(Vector3.Scale(position, new Vector3(1000f, 1000f, 1)) -
            new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0));

    }

}
