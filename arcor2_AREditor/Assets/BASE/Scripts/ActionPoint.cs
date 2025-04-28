using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using WebSocketSharp;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

namespace Base {
    public abstract class ActionPoint : InteractiveObject, IActionPointParent {

        // Key string is set to IO.Swagger.Model.ActionPoint Data.Uuid
        public Dictionary<string, Action> Actions = new();
        public GameObject ActionsSpawn;
        public GameObject ActionPoints;

        public IActionPointParent Parent;

        protected Vector3 offset;
        [NonSerialized]
        public int PuckCounter = 0;
        public LineConnection ConnectionToParent;

        [NonSerialized]
        public Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint Data =
            new (id: "",
                robotJoints: new List<ProjectRobotJoints>(),
                orientations: new List<NamedOrientation>(),
                position: new Position(),
                actions: new List<Arcor2.ClientSdk.Communication.OpenApi.Models.Action>(),
                name: "");
        
        [SerializeField]
        protected GameObject orientations;

        public bool OrientationsVisible, ActionsCollapsed;

        private bool breakPoint;

        public virtual bool BreakPoint {
            get => breakPoint;
            set {
                breakPoint = value;
                PlayerPrefsHelper.SaveBool("/AP/" + Data.Id + "/breakPoint", value);
            }
        }

        protected override void Start() {
            base.Start();
        }

        

        public virtual void ActionPointBaseUpdate(BareActionPoint apData) {
            Data.Name = apData.Name;
            Data.Position = apData.Position;
            SelectorItem.SetText(apData.Name);
            // update position and rotation based on received data from swagger
            transform.localPosition = GetScenePosition();
            if (Parent != null)
                ConnectionToParent.UpdateLine();
        }

        
        public virtual void InitAP(Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint apData, float size, IActionPointParent parent = null) {
            Debug.Assert(apData != null);
            SetParent(parent);
            Data = apData;

            SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
            OrientationsVisible = PlayerPrefsHelper.LoadBool("/AP/" + Data.Id + "/visible", true);
            ActionsCollapsed = PlayerPrefsHelper.LoadBool("/AP/" + Data.Id + "/actionsCollapsed", false);
            BreakPoint = PlayerPrefsHelper.LoadBool("/AP/" + Data.Id + "/breakPoint", false);
            transform.localPosition = GetScenePosition();
            SetSize(size);
            if (Data.Actions == null)
                Data.Actions = new List<Arcor2.ClientSdk.Communication.OpenApi.Models.Action>();
            if (Data.Orientations == null)
                Data.Orientations = new List<NamedOrientation>();
            if (Data.RobotJoints == null)
                Data.RobotJoints = new List<ProjectRobotJoints>();
        }

        public void SetParent(IActionPointParent parent) {
            Parent = parent;
            if(parent != null)
                SetConnectionToParent(parent);
        }

        private void SetConnectionToParent(IActionPointParent parent) {
            // Create new Line Connection between parent AO and child AP
            GameObject c = Instantiate(SceneManager.Instance.LineConnectionPrefab);
            c.transform.parent = transform;
            LineConnection newConnection = c.GetComponent<LineConnection>();
            newConnection.targets[0] = parent.GetTransform();
            newConnection.targets[1] = transform;

            // add the connection to connections manager
            SceneManager.Instance.AOToAPConnectionsManager.AddConnection(newConnection);

            ConnectionToParent = newConnection;
            
        }

        internal string GetFreeOrientationName() {
            int i = 1;
            bool hasFreeName;
            string freeName = "default";
            do {
                hasFreeName = true;
                if (OrientationNameExist(freeName) || JointsNameExist(freeName)) {
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
            string freeName = "default";
            do {
                hasFreeName = true;
                if (JointsNameExist(freeName) || OrientationNameExist(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = "default_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public abstract void UpdatePositionsOfPucks();

        public Dictionary<string, Pose> GetPoses() {
            Dictionary<string, Pose> poses = new();
            foreach (NamedOrientation orientation in Data.Orientations) {
                poses.Add(orientation.Id, new Pose(orientation: orientation.Orientation, position: Data.Position));
            }
            return poses;
        }

        public List<NamedOrientation> GetNamedOrientations() {
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
            throw new KeyNotFoundException("Orientation with id " + id + " not found.");
        }

        public NamedOrientation GetFirstOrientation() {
            try {
                if (Data.Orientations.Count == 0) {
                    if (!string.IsNullOrEmpty(Data.Parent)) {
                        ActionPoint parent = ProjectManager.Instance.GetActionPoint(Data.Parent);
                        return parent.GetFirstOrientation();
                    }

                } else {
                    return Data.Orientations[0];
                }
            } catch (KeyNotFoundException) {
                
            }
            throw new ItemNotFoundException("No orientation");
        }

        public NamedOrientation GetFirstOrientationFromDescendants() {
            List<ActionPoint> descendantActionPoints = new();
            foreach (Transform t in ActionPoints.transform) {
                ActionPoint ap = t.GetComponent<ActionPoint>();
                if (ap.Data.Orientations.Count > 0)
                    return ap.Data.Orientations[0];
                descendantActionPoints.Add(ap);
            }
            foreach (ActionPoint ap in descendantActionPoints) {
                try {
                    return ap.GetFirstOrientationFromDescendants();
                } catch (ItemNotFoundException) {

                }
            }
            
            throw new ItemNotFoundException("No orientation");
        }

        /// <summary>
        /// 
        /// Returns visual representation of orientation
        /// </summary>
        /// <param name="id">UUID of orientation</param>
        /// <returns></returns>
        public APOrientation GetOrientationVisual(string id) {
            foreach (Transform transform in orientations.transform) {
                APOrientation orientation = transform.GetComponent<APOrientation>();
                if (orientation != null && orientation.OrientationId == id) {
                    return orientation;
                }
            }
            throw new KeyNotFoundException("Orientation with id " + id + " not found.");
        }

        public List<APOrientation> GetOrientationsVisuals() {
            List<APOrientation> orientationsList = new();
            foreach (Transform transform in orientations.transform) {
                APOrientation o = transform.GetComponent<APOrientation>();
                if (o != null)
                    orientationsList.Add(o);
            }
            return orientationsList;
        }

        public Pose GetDefaultPose() {
            foreach (NamedOrientation orientation in Data.Orientations) {
                if (orientation.Id == "default")
                    return new Pose(Data.Position, orientation.Orientation);
            }
            throw new ItemNotFoundException();            
        }

        //TODO: check if it works
        public ProjectRobotJoints GetFirstJoints(string robot_id = null, bool valid_only = false) {
            foreach (ProjectRobotJoints robotJoint in Data.RobotJoints) {
                if ((robot_id != null && robot_id != robotJoint.RobotId) ||
                        (valid_only && !robotJoint.IsValid))
                    continue;
                return robotJoint;
            }
            if (!string.IsNullOrEmpty(Data.Parent)) {
                ActionPoint parent = ProjectManager.Instance.GetActionPoint(Data.Parent);
                return parent.GetFirstJoints();
            }

            throw new ItemNotFoundException();    
        }

        public ProjectRobotJoints GetFirstJointsFromDescendants() {
            List<ActionPoint> descendantActionPoints = new();
            foreach (Transform t in ActionPoints.transform) {
                ActionPoint ap = t.GetComponent<ActionPoint>();
                if (ap.Data.RobotJoints.Count > 0)
                    return ap.Data.RobotJoints[0];
                descendantActionPoints.Add(ap);
            }
            foreach (ActionPoint ap in descendantActionPoints) {
                try {
                    return ap.GetFirstJointsFromDescendants();
                } catch (ItemNotFoundException) {

                }
            }

            throw new ItemNotFoundException("No joints");
        }

        public Dictionary<string, ProjectRobotJoints> GetAllJoints(bool uniqueOnly = false, string robot_id = null, bool valid_only = false) {
            Dictionary<string, ProjectRobotJoints> joints = new();
            Dictionary<string, Pose> poses = new();
            if (uniqueOnly) {
                poses = GetPoses();
            }
            foreach (ProjectRobotJoints robotJoint in Data.RobotJoints) {
                if ((uniqueOnly && poses.ContainsKey(robotJoint.Id)) ||
                    (!robot_id.IsNullOrEmpty() && robot_id != robotJoint.RobotId) ||
                    (valid_only && !robotJoint.IsValid)) {
                    continue;
                }                
                joints.Add(robotJoint.Id, robotJoint);
            }
            return joints;
        }

        public Dictionary<string, ProjectRobotJoints> GetJointsOfArm(string robot_id, string arm_id, bool uniqueOnly = false) {
            Dictionary<string, ProjectRobotJoints> joints = new();
            Dictionary<string, Pose> poses = new();
            if (uniqueOnly) {
                poses = GetPoses();
            }
            foreach (ProjectRobotJoints robotJoint in Data.RobotJoints) {
                if (uniqueOnly && poses.ContainsKey(robotJoint.Id)) {
                    continue;
                }
                if (robot_id == robotJoint.RobotId && arm_id == robotJoint.ArmId)
                    joints.Add(robotJoint.Id, robotJoint);
            }
            return joints;
        }




        public void DeleteAP(bool removeFromList = true) {
            // Remove all actions of this action point
            RemoveActions();
            RemoveConnectionToParent();
            RemoveOrientations();

            // Remove this ActionPoint reference from parent ActionObject list
            if (removeFromList) // to allow remove all AP in foreach
                ProjectManager.Instance.ActionPoints.Remove(Data.Id);

            DestroyObject();
            Destroy(gameObject);
        }

        public override void DestroyObject() {
            base.DestroyObject();
        }

        private void RemoveConnectionToParent() {
            // Remove connections to parent
            if (ConnectionToParent != null && ConnectionToParent.gameObject != null) {
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
        public abstract Quaternion GetSceneOrientation();

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

        

        public virtual void ActivateForGizmo(string layer) {
            gameObject.layer = LayerMask.NameToLayer(layer);
        }

        public IActionPointParent GetParent() {
            return Parent;
        }

        public override string GetName() {
            return Data.Name;
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
        public virtual (List<string>, Dictionary<string, string>) UpdateActionPoint(Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint projectActionPoint) {
            if (Data.Parent != projectActionPoint.Parent) {
                ChangeParent(projectActionPoint.Parent);
            }
            Data = projectActionPoint;
            transform.localPosition = GetScenePosition();            
            transform.localRotation = GetSceneOrientation();
            List<string> currentA = new();

            foreach (NamedOrientation orientation in Data.Orientations) {
                UpdateOrientation(orientation);
            }
            // Connections between actions (action -> output --- input <- action2)
            Dictionary<string, string> connections = new();
            if (projectActionPoint.Actions != null) {
                //update actions
                foreach (Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction in projectActionPoint.Actions) {

                    // if action exist, just update it, otherwise create new
                    if (!Actions.TryGetValue(projectAction.Id, out Action action)) {
                        try {
                            action = ProjectManager.Instance.SpawnAction(projectAction, this);
                        } catch (Arcor2ConnectionException ex) {
                            Debug.LogError(ex);
                            continue;
                        }
                    }
                    // updates name of the action
                    action.ActionUpdateBaseData(DataHelper.ActionToBareAction(projectAction));
                    // updates parameters of the action
                    action.ActionUpdate(projectAction);

                    // Add current connection from the server, we will only map the outputs
                    

                    // local list of all actions for current action point
                    currentA.Add(projectAction.Id);
                }
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
            } else {
                try {
                    IActionPointParent actionPointParent = ProjectManager.Instance.GetActionPointParent(parentId);
                    Parent = actionPointParent;
                    Data.Parent = parentId;
                    transform.parent = actionPointParent.GetTransform();
                    RemoveConnectionToParent();
                    SetConnectionToParent(Parent);
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }
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
        public NamedOrientation GetOrientation(string id) {
            foreach (NamedOrientation orientation in Data.Orientations) {
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
        public ProjectRobotJoints GetJoints(string id) {
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
        public ProjectRobotJoints GetJointsByName(string name) {
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
        public NamedOrientation GetOrientationByName(string name) {
            foreach (NamedOrientation orientation in Data.Orientations) {
                if (orientation.Name == name)
                    return orientation;
            }
            throw new KeyNotFoundException("Orientation with name " + name + " not found");
        }


        public virtual void UpdateOrientation(NamedOrientation orientation) {
            NamedOrientation originalOrientation = GetOrientation(orientation.Id);
            originalOrientation.Orientation = orientation.Orientation;
            try {
                APOrientation orientationArrow = GetOrientationVisual(orientation.Id);
                orientationArrow.SetOrientation(orientation.Orientation);
            } catch (KeyNotFoundException) {
                AddNewOrientationVisual(orientation);
            }
            BaseUpdateOrientation(originalOrientation, orientation);
        }

        public virtual void AddOrientation(NamedOrientation orientation) {
            Data.Orientations.Add(orientation);
            AddNewOrientationVisual(orientation);
        }


        public void BaseUpdateOrientation(NamedOrientation orientation) {
            NamedOrientation originalOrientation = GetOrientation(orientation.Id);
            BaseUpdateOrientation(originalOrientation, orientation);
            GetOrientationVisual(orientation.Id).SelectorItem.SetText(orientation.Name);
        }

        public void BaseUpdateOrientation(NamedOrientation originalOrientation, NamedOrientation orientation) {
            originalOrientation.Name = orientation.Name;
        }

        public void RemoveOrientations() {
            foreach (NamedOrientation o in Data.Orientations) {
                RemoveOrientationVisual(o.Id);
            }
            Data.Orientations.Clear();
        }
        
        public void RemoveOrientation(string id) {
            int i = 0;
            foreach (NamedOrientation o in Data.Orientations) {
                if (o.Id == id) {
                    Data.Orientations.RemoveAt(i);
                    break;
                    ;
                }
                ++i;
            }
            RemoveOrientationVisual(id);
        }

        private void RemoveOrientationVisual(string id) {
            try {
                APOrientation o = GetOrientationVisual(id);
                o.DestroyObject();
            } catch (KeyNotFoundException) {
                // älready destroyed..
            }          

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

        public void RemoveJoints(string id) {
            int i = 0;
            foreach (ProjectRobotJoints joints in Data.RobotJoints) {
                if (joints.Id == id) {
                    Data.RobotJoints.RemoveAt(i);
                    return;
                }
                ++i;
            }
        }
        public GameObject GetGameObject() {
            return gameObject;
        }

        public abstract void SetSize(float size);

        public void UpdateOrientationsVisuals(bool visible) {
            foreach (Transform transform in orientations.transform) {
                transform.gameObject.SetActive(visible);
            }
        }

        private void AddNewOrientationVisual(NamedOrientation orientation) {
            APOrientation apOrientation = Instantiate(ActionsManager.Instance.ActionPointOrientationPrefab, orientations.transform).GetComponent<APOrientation>();
            apOrientation.ActionPoint = this;
            apOrientation.SetOrientation(orientation.Orientation);
            apOrientation.OrientationId = orientation.Id;
            apOrientation.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(apOrientation);
        }

        internal async Task<bool> ShowOrientationDetailMenu(string orientationId) {
            if (await ActionPointAimingMenu.Instance.Show(this, true)) {
                ActionPointAimingMenu.Instance.OpenDetailMenu(GetOrientation(orientationId));
                return true;
            }
            return false;
        }

        public abstract void HighlightAP(bool highlight);


        public void ResetPosition() {
            transform.localPosition = GetScenePosition();
            transform.localRotation = GetSceneOrientation();
        }

        public override async Task<RequestResult> Movable() {

            if (IsLocked && LockOwner != LandingScreen.Instance.GetUsername())
                return new RequestResult(false, "Action point is locked");
            else {
                try {
                    await CommunicationManager.Instance.Client.UpdateActionPointPositionAsync(new UpdateActionPointPositionRequestArgs(GetId(), new Position()), true);
                    return new RequestResult(true);
                } catch (Arcor2ConnectionException e) {
                    return new RequestResult(false, e.Message);
                }

            }
        }

        public override string GetId() {
            return Data.Id;
        }

        public bool AnyOrientation() {
            return Data.Orientations.Count > 0;
        }

        public bool AnyJoints() {
            return Data.RobotJoints.Count > 0;
        }

        public Transform GetSpawnPoint() {
            return ActionPoints.transform;
        }
    }
}
