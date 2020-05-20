using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using IO.Swagger.Model;

namespace Base {
    public abstract class ActionPoint : Clickable, IActionPointParent {

        // Key string is set to IO.Swagger.Model.ActionPoint Data.Uuid
        public Dictionary<string, Action> Actions = new Dictionary<string, Action>();
        public GameObject ActionsSpawn;

        public IActionPointParent Parent;

        protected Vector3 offset;
        [System.NonSerialized]
        public int PuckCounter = 0;
        public LineConnection ConnectionToParent;

        [System.NonSerialized]
        public IO.Swagger.Model.ProjectActionPoint Data = new IO.Swagger.Model.ProjectActionPoint(id: "", robotJoints: new List<IO.Swagger.Model.ProjectRobotJoints>(), orientations: new List<IO.Swagger.Model.NamedOrientation>(), position: new IO.Swagger.Model.Position(), actions: new List<IO.Swagger.Model.Action>(), name: "");
        protected ActionPointMenu actionPointMenu;

        [SerializeField]
        protected GameObject orientations;

        public bool OrientationsVisible;


        public bool Locked {
            get {
                return PlayerPrefsHelper.LoadBool("project/" + ProjectManager.Instance.Project.Id + "/AP/" + Data.Id + "/locked", false);
            }

            set {
                Debug.Assert(Base.ProjectManager.Instance.Project != null);
                PlayerPrefsHelper.SaveBool("project/" + Base.ProjectManager.Instance.Project.Id + "/AP/" + Data.Id + "/locked", value);
            }
        }

        private void Awake() {
            OrientationsVisible = PlayerPrefsHelper.LoadBool("/AP/" + Data.Id + "/visible", true);
        }

        protected virtual void Start() {
            actionPointMenu = MenuManager.Instance.ActionPointMenu.gameObject.GetComponent<ActionPointMenu>();
            
        }

        protected virtual void Update() {
            if (gameObject.transform.hasChanged) {
                SetScenePosition(transform.localPosition);
                transform.hasChanged = false;
            }
        }

        public void ActionPointBaseUpdate(IO.Swagger.Model.ProjectActionPoint apData) {
            Data.Name = apData.Name;
            Data.Position = apData.Position;
            // update position and rotation based on received data from swagger
            transform.localPosition = GetScenePosition();
            if (Parent != null)
                ConnectionToParent.UpdateLine();
            //TODO: ActionPoint has multiple rotations of end-effectors, for visualization, render end-effectors individually
            //transform.localRotation = GetSceneOrientation();
        }

        
        public virtual void UpdateId(string newId) {
            Data.Id = newId;

        }

        public void InitAP(IO.Swagger.Model.ProjectActionPoint apData, float size, IActionPointParent parent = null) {
            Debug.Assert(apData != null);
            SetParent(parent);
            Data = apData;
            transform.localPosition = GetScenePosition();
            SetSize(size);
            ActivateForGizmo(((ControlBoxManager.Instance.UseGizmoMove == true) && ProjectManager.Instance.AllowEdit) ? "GizmoRuntime" : "Default");
            // TODO: is this neccessary?
            /*if (Data.Orientations.Count == 0)
                Data.Orientations.Add(new IO.Swagger.Model.NamedOrientation(id: "default", orientation: new IO.Swagger.Model.Orientation()));*/
        }

        public void SetParent(IActionPointParent parent) {
            Parent = parent;
            if(parent != null)
                SetConnectionToActionObject(parent);
        }

        private void SetConnectionToActionObject(IActionPointParent parent) {
            // Create new Line Connection between parent AO and child AP
            GameObject c = Instantiate(SceneManager.Instance.LineConnectionPrefab);
            c.transform.parent = transform;
            LineConnection newConnection = c.GetComponent<LineConnection>();
            newConnection.targets[0] = parent.GetTransform();
            newConnection.targets[1] = this.transform;

            // add the connection to connections manager
            SceneManager.Instance.AOToAPConnectionsManager.AddConnection(newConnection);

            ConnectionToParent = newConnection;

            // Add connection renderer into ChangeMaterialOnSelected script attached on parent AO 
            ChangeMaterialOnSelected changeMaterial;            
            changeMaterial = parent.GetGameObject().GetComponent<ChangeMaterialOnSelected>();

            // if the script is not attached on parent AO, then add it and initialize its click material
            if (changeMaterial == null) {
                changeMaterial = parent.GetGameObject().AddComponent<ChangeMaterialOnSelected>();
                changeMaterial.ClickMaterial = ConnectionToParent.ClickMaterial;
            }
            changeMaterial.AddRenderer(ConnectionToParent.GetComponent<LineRenderer>());
        }

        internal string GetFreeOrientationName() {
            int i = 1;
            bool hasFreeName;
            string freeName = "defaulf";
            do {
                hasFreeName = true;
                if (OrientationNameExist(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = "default_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        internal string GetFreeJointsName() {
            int i = 1;
            bool hasFreeName;
            string freeName = "defaulf";
            do {
                hasFreeName = true;
                if (JointsNameExist(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = "default_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public abstract void UpdatePositionsOfPucks();

        public Dictionary<string, IO.Swagger.Model.Pose> GetPoses() {
            Dictionary<string, IO.Swagger.Model.Pose> poses = new Dictionary<string, IO.Swagger.Model.Pose>();
            foreach (IO.Swagger.Model.NamedOrientation orientation in Data.Orientations) {
                poses.Add(orientation.Id, new IO.Swagger.Model.Pose(orientation.Orientation, Data.Position));
            }
            return poses;
        }

        public List<IO.Swagger.Model.NamedOrientation> GetNamedOrientations() {
            return Data.Orientations;
        }

        public NamedOrientation GetNamedOrientationByName(string name) {
            foreach (NamedOrientation orientation in Data.Orientations)
                if (orientation.Name == name)
                    return orientation;
            throw new KeyNotFoundException("Orientation with name " + name + " not found.");
        }

        public NamedOrientation GetNamedOrientation(string id) {
            foreach (NamedOrientation orientation in Data.Orientations)
                if (orientation.Id == id)
                    return orientation;
            throw new KeyNotFoundException("Orientation with name " + name + " not found.");
        }

        public NamedOrientation GetFirstOrientation() {
            if (Data.Orientations.Count == 0)
                throw new ItemNotFoundException();
            return Data.Orientations[0];
        }

        public IO.Swagger.Model.Pose GetDefaultPose() {
            foreach (IO.Swagger.Model.NamedOrientation orientation in Data.Orientations) {
                if (orientation.Id == "default")
                    return new IO.Swagger.Model.Pose(position: Data.Position, orientation: orientation.Orientation);
            }
            throw new ItemNotFoundException();            
        }

        public IO.Swagger.Model.ProjectRobotJoints GetFirstJoints(string robot_id = null, bool valid_only = false) {
            foreach (IO.Swagger.Model.ProjectRobotJoints robotJoint in Data.RobotJoints) {
                if ((robot_id != null && robot_id != robotJoint.RobotId) ||
                        (valid_only && !robotJoint.IsValid))
                    continue;
                return robotJoint;
            }
            throw new ItemNotFoundException();    
        }

        public Dictionary<string, IO.Swagger.Model.ProjectRobotJoints> GetAllJoints(bool uniqueOnly = false, string robot_id = null, bool valid_only = false) {
            Dictionary<string, IO.Swagger.Model.ProjectRobotJoints> joints = new Dictionary<string, IO.Swagger.Model.ProjectRobotJoints>();
            Dictionary<string, IO.Swagger.Model.Pose> poses = new Dictionary<string, IO.Swagger.Model.Pose>();
            if (uniqueOnly) {
                poses = GetPoses();
            }
            foreach (IO.Swagger.Model.ProjectRobotJoints robotJoint in Data.RobotJoints) {
                if ((uniqueOnly && poses.ContainsKey(robotJoint.Id)) ||
                    (robot_id != null && robot_id != robotJoint.RobotId) ||
                    (valid_only && !robotJoint.IsValid)) {
                    continue;
                }                
                joints.Add(robotJoint.Id, robotJoint);
            }
            return joints;
        }
        


        public void DeleteAP(bool removeFromList = true) {
            // Remove all actions of this action point
            RemoveActions();
            RemoveConnectionToParent();


            // Remove this ActionPoint reference from parent ActionObject list
            if (removeFromList) // to allow remove all AP in foreach
                ProjectManager.Instance.ActionPoints.Remove(this.Data.Id);

            Destroy(gameObject);
        }

        private void RemoveConnectionToParent() {
            // Remove connections to parent
            if (ConnectionToParent != null && ConnectionToParent.gameObject != null) {
                // remove renderer from ChangeMaterialOnSelected script attached on the AO
                ChangeMaterialOnSelected changeMaterial = Parent.GetGameObject().GetComponent<ChangeMaterialOnSelected>();
                changeMaterial.RemoveRenderer(ConnectionToParent.GetComponent<LineRenderer>());
                // remove connection from connectinos manager
                SceneManager.Instance.AOToAPConnectionsManager.RemoveConnection(ConnectionToParent);
                // destroy connection gameobject
                Destroy(ConnectionToParent.gameObject);
            }
        }

        public virtual bool ProjectInteractable() {
            return GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor;
        }

        public abstract Vector3 GetScenePosition();
        public abstract void SetScenePosition(Vector3 position);
        public abstract Quaternion GetSceneOrientation();
        public abstract void SetSceneOrientation(Quaternion orientation);

        public void RemoveActions() {
            // Remove all actions of this action point
            foreach (string actionUUID in Actions.Keys.ToList<string>()) {
                RemoveAction(actionUUID);
            }
            Actions.Clear();
        }

        public void RemoveAction(string action_id) {
            Actions[action_id].DeleteAction();
        }

        public void ShowMenu(bool enableBackButton) {
            actionPointMenu.CurrentActionPoint = this;
            actionPointMenu.EnableBackButton(enableBackButton);
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu);            
        }

        public virtual void ActivateForGizmo(string layer) {
            gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public IActionPointParent GetParent() {
            return Parent;
        }

        public string GetName() {
            return Data.Name;
        }

        public string GetId() {
            return Data.Id;
        }

        public bool IsActionObject() {
            return false;
        }

        public ActionObject GetActionObject() {
            if (Parent != null) {
                return Parent.GetActionObject();
            } else {
                return null;
            }
        }

        public Transform GetTransform() {
            return transform;
        }

        /// <summary>
        /// Updates actions of ActionPoint and ProjectActionPoint received from server.
        /// </summary>
        /// <param name="projectActionPoint"></param>
        /// <returns></returns>
        public virtual (List<string>, Dictionary<string, string>) UpdateActionPoint(IO.Swagger.Model.ProjectActionPoint projectActionPoint) {
            if (Data.Parent != projectActionPoint.Parent) {
                ChangeParent(projectActionPoint.Parent);
            }
            Data = projectActionPoint;
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
            List<string> currentA = new List<string>();
            // Connections between actions (action -> output --- input <- action2)
            Dictionary<string, string> connections = new Dictionary<string, string>();

            //update actions
            foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                string providerName = projectAction.Type.Split('/').First();
                string actionType = projectAction.Type.Split('/').Last();
                IActionProvider actionProvider;
                try {
                    actionProvider = SceneManager.Instance.GetActionObject(providerName);
                } catch (KeyNotFoundException ex) {
                    if (ActionsManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                        actionProvider = originalService;
                    } else {
                        Debug.LogError("PROVIDER NOT FOUND EXCEPTION: " + providerName + " " + actionType);
                        continue; //TODO: throw exception
                    }
                }
                

                // if action exist, just update it, otherwise create new
                if (!Actions.TryGetValue(projectAction.Id, out Action action)) {
                    action = ProjectManager.Instance.SpawnAction(projectAction.Id, projectAction.Name, actionType, this, actionProvider);
                }
                // updates name of the action
                action.ActionUpdateBaseData(projectAction);
                // updates parameters of the action
                action.ActionUpdate(projectAction);

                // Add current connection from the server, we will only map the outputs
                foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                    //if(!connections.ContainsKey(projectAction.Id))
                    connections.Add(projectAction.Id, actionIO.Default);
                }

                // local list of all actions for current action point
                currentA.Add(projectAction.Id);
            }


            if (Parent != null) {

                if (ConnectionToParent != null)
                    ConnectionToParent.UpdateLine();
                else
                    SetConnectionToActionObject(Parent);
            }

            if (actionPointMenu != null && actionPointMenu.CurrentActionPoint == this) {
                actionPointMenu.UpdateMenu();
            }
            return (currentA, connections);
        }

        private void ChangeParent(string parentId) {
            if (parentId == null || parentId == "") {
                RemoveConnectionToParent();
                Parent = null;
                Data.Parent = "";
                transform.parent = ProjectManager.Instance.ActionPointsOrigin.transform;
                transform.localRotation = Quaternion.identity;
                return;
            }
            try {
                IActionPointParent actionPointParent = ProjectManager.Instance.GetActionPointParent(parentId);
                Parent = actionPointParent;
                Data.Parent = parentId;
                transform.parent = actionPointParent.GetTransform();
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }
            
            
        }

        public bool OrientationNameExist(string name) {
            try {
                GetOrientationByName(name);
                return true;
            } catch (KeyNotFoundException ex) {
                return false;
            } 
        } 

        public bool JointsNameExist(string name) {
            try {
                GetJointsByName(name);
                return true;
            } catch (KeyNotFoundException ex) {
                return false;
            } 
        } 

        /// <summary>
        /// Returns orientation with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IO.Swagger.Model.NamedOrientation GetOrientation(string id) {
            foreach (IO.Swagger.Model.NamedOrientation orientation in Data.Orientations) {
                if (orientation.Id == id)
                    return orientation;
            }
            throw new KeyNotFoundException("Orientation with id " + id + " not found");
        }

        /// <summary>
        /// Returns joints with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IO.Swagger.Model.ProjectRobotJoints GetJoints(string id) {
            foreach (ProjectRobotJoints joints in Data.RobotJoints) {
                if (joints.Id == id)
                    return joints;
            }
            throw new KeyNotFoundException("Joints with id " + id + " not found");
        }

        /// <summary>
        /// Returns joints with name or throws KeyNotFoundException
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IO.Swagger.Model.ProjectRobotJoints GetJointsByName(string name) {
            foreach (ProjectRobotJoints joints in Data.RobotJoints) {
                if (joints.Name == name)
                    return joints;
            }
            throw new KeyNotFoundException("Joints with name " + name + " not found");
        }


        /// <summary>
        /// Returns joints with name or throws KeyNotFoundException
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IO.Swagger.Model.NamedOrientation GetOrientationByName(string name) {
            foreach (NamedOrientation orientation in Data.Orientations) {
                if (orientation.Name == name)
                    return orientation;
            }
            throw new KeyNotFoundException("Orientation with name " + name + " not found");
        }


        public virtual void UpdateOrientation(NamedOrientation orientation) {
            NamedOrientation originalOrientation = GetOrientation(orientation.Id);
            originalOrientation.Orientation = orientation.Orientation;
            BaseUpdateOrientation(originalOrientation, orientation);
        }

        public virtual void AddOrientation(NamedOrientation orientation) {
            Data.Orientations.Add(orientation);
        }


        public void BaseUpdateOrientation(NamedOrientation orientation) {
            NamedOrientation originalOrientation = GetOrientation(orientation.Id);
            BaseUpdateOrientation(originalOrientation, orientation);
        }

        public void BaseUpdateOrientation(NamedOrientation originalOrientation, NamedOrientation orientation) {
            originalOrientation.Name = orientation.Name;
        }

        
        public void RemoveOrientation(NamedOrientation orientation) {
            Data.Orientations.Remove(orientation);
        }

        public void UpdateJoints(ProjectRobotJoints joints) {
            ProjectRobotJoints originalJoints = GetJoints(joints.Id);
            originalJoints.Joints = joints.Joints;
            BaseUpdateJoints(originalJoints, joints);
        }

        public void BaseUpdateJoints(ProjectRobotJoints joints) {
            ProjectRobotJoints originalJoints = GetJoints(joints.Id);
            BaseUpdateJoints(originalJoints, joints);
        }

        public void BaseUpdateJoints(ProjectRobotJoints originalJoints, ProjectRobotJoints joints) {
            originalJoints.Name = joints.Name;
            originalJoints.IsValid = joints.IsValid;
            originalJoints.RobotId = joints.RobotId;
        }

        public void AddJoints(ProjectRobotJoints joints) {
            Data.RobotJoints.Add(joints);
        }

        public void RemoveJoints(ProjectRobotJoints joints) {
            Data.RobotJoints.Remove(joints);
        }

        public void ShowMenu() {
            ShowMenu(false);
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public abstract void SetSize(float size);

        public void UpdateOrientationsVisuals() {
            foreach (Transform transform in orientations.transform) {
                Destroy(transform.gameObject);
            }
            if (!ProjectManager.Instance.APOrientationsVisible)
                return;
            if (!OrientationsVisible)
                return;
            foreach (IO.Swagger.Model.NamedOrientation orientation in Data.Orientations) {
                APOrientation apOrientation = Instantiate(ActionsManager.Instance.ActionPointOrientationPrefab, orientations.transform).GetComponent<APOrientation>();
                apOrientation.transform.localRotation = TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(orientation.Orientation));
                apOrientation.ActionPoint = this;
                apOrientation.OrientationId = orientation.Id;
            }
        }

        internal void ShowAimingMenu(string orientationId) {
            ShowMenu(false);
            actionPointMenu.OpenActoinPointAimingMenu(orientationId);
        }
    }

}
