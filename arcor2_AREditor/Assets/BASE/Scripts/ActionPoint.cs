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


        protected virtual void Awake() {

        }

        void Update() {

        }

        public void SetActionObject(ActionObject actionObject) {
            ActionObject = actionObject;
            Data.Id = ActionObject.Data.Id + " - AP" + ActionObject.CounterAP++.ToString();
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

    }

}
