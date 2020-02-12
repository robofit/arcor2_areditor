using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Base {
    public abstract class ActionPoint : Clickable {

        // Key string is set to IO.Swagger.Model.ActionPoint Data.Uuid
        public Dictionary<string, Action> Actions = new Dictionary<string, Action>();
        public GameObject ActionsSpawn;

        public ActionObject ActionObject;
        protected Vector3 offset;
        [System.NonSerialized]
        public int PuckCounter = 0;
        // TODO: rename (Connection to action object)
        public Connection ConnectionToIO;

        [System.NonSerialized]
        public IO.Swagger.Model.ProjectActionPoint Data = new IO.Swagger.Model.ProjectActionPoint(id: "", robotJoints: new List<IO.Swagger.Model.RobotJoints>(), orientations: new List<IO.Swagger.Model.NamedOrientation>(), position: new IO.Swagger.Model.Position(), actions: new List<IO.Swagger.Model.Action>(), uuid: "");         

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.localPosition);
                transform.hasChanged = false;
            }
        }

        public void ActionPointUpdate(IO.Swagger.Model.ProjectActionPoint apData = null) {
            if (apData != null)
                Data = apData;
            // update position and rotation based on received data from swagger
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
        }

        public void InitAP(ActionObject actionObject, IO.Swagger.Model.ProjectActionPoint apData = null) {
            SetActionObject(actionObject);
            if (apData != null) {
                Data = apData;
            } else {
                Data.Uuid = Guid.NewGuid().ToString();
            }
               
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
            // Remove all actions of this action point
            RemoveActions(false);

            // TODO: remove connections to action objects
            if (ConnectionToIO != null && ConnectionToIO.gameObject != null) {
                Destroy(ConnectionToIO.gameObject);
            }

            // Remove this ActionPoint reference from parent ActionObject list
            ActionObject.ActionPoints.Remove(this.Data.Uuid);

            Destroy(gameObject);

            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public virtual bool ProjectInteractable() {
            return GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor;
        }

        public abstract Vector3 GetScenePosition();
        public abstract void SetScenePosition(Vector3 position);
        public abstract void SetScenePosition(IO.Swagger.Model.Position position);
        public abstract Quaternion GetSceneOrientation();

        public abstract void SetSceneOrientation(Quaternion orientation);


        public void RemoveActions(bool updateProject) {
            // Remove all actions of this action point
            foreach (string actionUUID in Actions.Keys.ToList<string>()) {
                RemoveAction(actionUUID, updateProject);
            }
            Actions.Clear();
        }

        public void RemoveAction(string uuid, bool updateProject) {
            Actions[uuid].DeleteAction(updateProject);
        }
    }

}
