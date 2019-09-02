using UnityEngine;


namespace Base {
    public abstract class ActionPoint : MonoBehaviour {
        public ActionObject ActionObject;
        protected Vector3 offset;
        [System.NonSerialized]
        public int PuckCounter = 0;
        public Connection ConnectionToIO;

        [System.NonSerialized]
        public IO.Swagger.Model.ActionPoint Data = new IO.Swagger.Model.ActionPoint {
            Pose = new IO.Swagger.Model.Pose {
                Position = new IO.Swagger.Model.Position(),
                Orientation = new IO.Swagger.Model.Orientation()
            }
        };


        protected virtual void Awake() {

        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.position);
                transform.hasChanged = false;
            }
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

        public abstract Vector3 GetScenePosition();
        public abstract void SetScenePosition(Vector3 position);
        public abstract void SetScenePosition(IO.Swagger.Model.Position position);
        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);

    }

}
