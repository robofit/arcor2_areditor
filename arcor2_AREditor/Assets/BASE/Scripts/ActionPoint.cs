using UnityEngine;
using System.Collections.Generic;


namespace Base {
    public abstract class ActionPoint : Clickable {

        // Key string is set to IO.Swagger.Model.ActionPoint Data.Id
        public Dictionary<string, Action> Actions = new Dictionary<string, Action>();
        public GameObject ActionsSpawn;

        public ActionObject ActionObject;
        protected Vector3 offset;
        [System.NonSerialized]
        public int PuckCounter = 0;
        public Connection ConnectionToIO;

        [System.NonSerialized]
        public IO.Swagger.Model.ActionPoint Data = new IO.Swagger.Model.ActionPoint(id: "", robotJoints: new List<IO.Swagger.Model.RobotJoints>(), orientations: new List<IO.Swagger.Model.NamedOrientation>(), position: new IO.Swagger.Model.Position());         

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.localPosition);
                transform.hasChanged = false;
            }
        }

        public void ActionPointUpdate(IO.Swagger.Model.ActionPoint apData = null) {
            if (apData != null)
                Data = apData;
            // update position and rotation based on received data from swagger
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
        }

        public void InitAP(ActionObject actionObject, IO.Swagger.Model.ActionPoint apData = null) {
            SetActionObject(actionObject);
            if (apData != null)
                Data = apData;
            if (Data.Orientations.Count == 0)
                Data.Orientations.Add(new IO.Swagger.Model.NamedOrientation(id: "default", orientation: new IO.Swagger.Model.Orientation()));
            if (Data.RobotJoints.Count == 0)
                Data.RobotJoints.Add(new IO.Swagger.Model.RobotJoints(isValid: false, id: "default", joints: new List<IO.Swagger.Model.Joint>(), robotId: "aubo"));
        }

        public void SetActionObject(ActionObject actionObject) {
            ActionObject = actionObject;
            Data.Id = actionObject.Data.Id +  "-AP" + ActionObject.CounterAP++.ToString();
        }

        public abstract void UpdatePositionsOfPucks();

        public Dictionary<string, IO.Swagger.Model.Pose> GetPoses() {
            Dictionary<string, IO.Swagger.Model.Pose> poses = new Dictionary<string, IO.Swagger.Model.Pose>();
            foreach (IO.Swagger.Model.NamedOrientation orientation in Data.Orientations) {
                poses.Add(orientation.Id, new IO.Swagger.Model.Pose(orientation.Orientation, Data.Position));
            }
            return poses;
        }

        public Dictionary<string, IO.Swagger.Model.RobotJoints> GetJoints(bool uniqueOnly = false) {
            Dictionary<string, IO.Swagger.Model.RobotJoints> joints = new Dictionary<string, IO.Swagger.Model.RobotJoints>();
            Dictionary<string, IO.Swagger.Model.Pose> poses = new Dictionary<string, IO.Swagger.Model.Pose>();
            if (uniqueOnly) {
                poses = GetPoses();
            }
            foreach (IO.Swagger.Model.RobotJoints robotJoint in Data.RobotJoints) {
                if (uniqueOnly && poses.ContainsKey(robotJoint.Id)) {
                    continue;
                }
                joints.Add(robotJoint.Id, robotJoint);
            }
            return joints;
        }

        public void DeleteAP(bool updateProject = true) {
            foreach (KeyValuePair<string, Action> action in Actions) {
                // Remove Action from Actions list reference
                Actions.Remove(action.Key);
                action.Value.DeleteAction(false);
            }
            if (ConnectionToIO != null && ConnectionToIO.gameObject != null) {
                Destroy(ConnectionToIO.gameObject);
            }

            // Remove this ActionPoint reference from parent ActionObject list
            ActionObject.ActionPoints.Remove(this.Data.Id);

            gameObject.SetActive(false);
            Destroy(gameObject);

            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public virtual bool ProjectInteractable() {
            return GameManager.Instance.GameState == GameManager.GameStateEnum.ProjectEditor;
        }

        public abstract Vector3 GetScenePosition();
        public abstract void SetScenePosition(Vector3 position);
        public abstract void SetScenePosition(IO.Swagger.Model.Position position);
        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);


        public void RemoveActions() {
            foreach (KeyValuePair<string, Action> action in Actions) {
                Destroy(action.Value.gameObject);
            }
            Actions.Clear();
        }

        public void RemoveAction(string id) {
            Destroy(Actions[id].gameObject);
            Actions.Remove(id);
        }

        // Called when this GameObject is destroyed
        private void OnDestroy() {
            RemoveActions();
        }

    }

}
