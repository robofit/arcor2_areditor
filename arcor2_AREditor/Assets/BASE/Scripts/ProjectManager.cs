using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

using IO.Swagger.Model;
using System.Linq;

namespace Base {
    public class ProjectManager : Base.Singleton<ProjectManager> {
        public IO.Swagger.Model.Project Project = null;

        public Dictionary<string, ActionPoint> ActionPoints = new Dictionary<string, ActionPoint>();
        public GameObject ActionPointsOrigin;
        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab;


        private bool projectActive = true;
        public bool APOrientationsVisible;

        public float APSize = 0.5f;

        public bool ProjectChanged = false;


        public event EventHandler OnActionPointsChanged;
        public event EventHandler OnLoadProject;


        /// <summary>
        /// Creates project from given json
        /// </summary>
        /// <param name="project"></param>
        public bool CreateProject(IO.Swagger.Model.Project project) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (Project != null)
                return false;

            Project = project;
            LoadSettings();
            bool success = UpdateProject(project);

            if (success) {
                ProjectChanged = false;
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            }
            return success;
        }

        /// <summary>
        /// Updates project from given json
        /// </summary>
        /// <param name="project"></param>
        public bool UpdateProject(IO.Swagger.Model.Project project) {
            if (project.Id != Project.Id) {
                return false;
            }
            Project = project;
            UpdateActionPoints();
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool DestroyProject() {
            Project = null;
            return true;
        }



        private void Update() {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor && GameManager.Instance.SceneInteractable) {
                if (!projectActive && (ControlBoxManager.Instance.UseGizmoMove)) {
                    ActivateActionPointsForGizmo(true);
                    projectActive = true;
                } else if (projectActive && !(ControlBoxManager.Instance.UseGizmoMove)) {
                    ActivateActionPointsForGizmo(false);
                    projectActive = false;
                }
            } else {
                if (projectActive) {
                    ActivateActionPointsForGizmo(false);
                    projectActive = false;
                }
            }
        }

        /// <summary>
        /// Deactivates or activates all action points in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionPointsForGizmo(bool activate) {
            if (activate) {
                //TODO: this should probably be Scene
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                    actionPoint.ActivateForGizmo("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                    actionPoint.ActivateForGizmo("Default");
                }
            }
        }

        public void LoadSettings() {
            APOrientationsVisible = PlayerPrefsHelper.LoadBool("project/" + Project.Id + "/APOrientationsVisibility", true);
        }


        public ActionPoint SpawnActionPoint(IO.Swagger.Model.ProjectActionPoint apData, IActionPointParent actionPointParent) {
            Debug.Assert(apData != null);
            GameObject AP;
            if (actionPointParent == null) {
                AP = Instantiate(ActionPointPrefab, ActionPointsOrigin.transform);
            } else {
                AP = Instantiate(ActionPointPrefab, actionPointParent.GetTransform());
            }

            AP.transform.localScale = new Vector3(1f, 1f, 1f);
            ActionPoint actionPoint = AP.GetComponent<ActionPoint>();
            actionPoint.InitAP(apData, APSize, actionPointParent);
            ActionPoints.Add(actionPoint.Data.Id, actionPoint);

            return actionPoint;
        }

        public string GetFreeAPName(string apParentName) {
            int i = 2;
            bool hasFreeName;
            string freeName = apParentName + "_ap";
            do {
                hasFreeName = true;
                if (ActionPointsContainsName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = apParentName + "_ap_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        public bool ActionPointsContainsName(string name) {
            foreach (ActionPoint ap in GetAllActionPoints()) {
                if (ap.Data.Name == name)
                    return true;
            }
            return false;
        }

        internal void HideAPOrientations() {
            APOrientationsVisible = false;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.UpdateOrientationsVisuals();
            }
            PlayerPrefsHelper.SaveBool("scene/" + Project.Id + "/APOrientationsVisibility", false);
        }

        internal void ShowAPOrientations() {
            APOrientationsVisible = true;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.UpdateOrientationsVisuals();
            }
            PlayerPrefsHelper.SaveBool("scene/" + Project.Id + "/APOrientationsVisibility", true);
        }


        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public void UpdateActionPoints() {
            List<string> currentAP = new List<string>();
            List<string> currentActions = new List<string>();
            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in Project.ActionPoints) {
                // if action point exist, just update it
                if (ActionPoints.TryGetValue(projectActionPoint.Id, out ActionPoint actionPoint)) {
                    actionPoint.ActionPointBaseUpdate(projectActionPoint);
                }
                // if action point doesn't exist, create new one
                else {
                    ActionObject actionObject = null;
                    if (projectActionPoint.Parent != null) {
                        SceneManager.Instance.ActionObjects.TryGetValue(projectActionPoint.Parent, out actionObject);
                    }
                    //TODO: update spawn action point to not need action object
                    actionPoint = SpawnActionPoint(projectActionPoint, actionObject);
                    
                }

                // update actions in current action point 
                (List<string>, Dictionary<string, string>) updateActionsResult = actionPoint.UpdateActionPoint(projectActionPoint);
                currentActions.AddRange(updateActionsResult.Item1);
                // merge dictionaries
                connections = connections.Concat(updateActionsResult.Item2).GroupBy(i => i.Key).ToDictionary(i => i.Key, i => i.First().Value);

                actionPoint.UpdatePositionsOfPucks();

                currentAP.Add(actionPoint.Data.Id);
            }



            UpdateActionConnections(Project.ActionPoints, connections);

            // Remove deleted actions
            foreach (string actionId in GetAllActionsDict().Keys.ToList<string>()) {
                if (!currentActions.Contains(actionId)) {
                    RemoveAction(actionId);
                }
            }

            // Remove deleted action points
            foreach (string actionPointId in GetAllActionPointsDict().Keys.ToList<string>()) {
                if (!currentAP.Contains(actionPointId)) {
                    RemoveActionPoint(actionPointId);
                }
            }
        }

        public void RemoveActionPoints() {
            List<ActionPoint> actionPoints = ActionPoints.Values.ToList();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }

        public IActionProvider GetActionProvider(string id) {
            try {
                return ActionsManager.Instance.GetService(id);
            } catch (KeyNotFoundException ex) {

            }

            if (SceneManager.Instance.ActionObjects.TryGetValue(id, out ActionObject actionObject)) {
                return actionObject;
            }
            throw new KeyNotFoundException("No action provider with id: " + id);
        }

        public IActionPointParent GetActionPointParent(string parentId) {
            if (parentId == null || parentId == "")
                throw new KeyNotFoundException("Action point parrent " + parentId + " not found");
            if (SceneManager.Instance.ActionObjects.TryGetValue(parentId, out ActionObject actionObject)) {
                return actionObject;
            }

            throw new KeyNotFoundException("Action point parrent " + parentId + " not found");
        }

        public ActionPoint GetactionpointByName(string name) {
            foreach (ActionPoint ap in ActionPoints.Values) {
                if (ap.Data.Name == name)
                    return ap;
            }
            throw new KeyNotFoundException("Action point " + name + " not found");
        }



        /// <summary>
        /// Destroys and removes references to action point of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveActionPoint(string Id) {
            // Call function in corresponding action point that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionPoints[Id].DeleteAP();
        }

        /// <summary>
        /// Returns action point of given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionPoint GetActionPoint(string id) {
            if (ActionPoints.TryGetValue(id, out ActionPoint actionPoint)) {
                return actionPoint;
            }

            throw new KeyNotFoundException("ActionPoint \"" + id + "\" not found!");
        }

        private void tmpPrint(string v) {
            Debug.LogError("\"" + v + "\"");
        }

        /// <summary>
        /// Returns all action points in the scene in a list [ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public List<ActionPoint> GetAllActionPoints() {
            return ActionPoints.Values.ToList();
        }

        /// <summary>
        /// Returns all action points in the scene in a list [ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public List<ActionPoint> GetAllGlobalActionPoints() {
            return (from ap in ActionPoints
                    where ap.Value.Parent == null
                    select ap.Value).ToList();
        }

        /// <summary>
        /// Returns all action points in the scene in a dictionary [action_point_Id, ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ActionPoint> GetAllActionPointsDict() {
            return ActionPoints;
        }

        /// <summary>
        /// Returns joints with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IO.Swagger.Model.ProjectRobotJoints GetJoints(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    return actionPoint.GetJoints(id);
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Joints with id " + id + " not found");
        }

        /// <summary>
        /// Returns orientation with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IO.Swagger.Model.NamedOrientation GetNamedOrientation(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    return actionPoint.GetOrientation(id);
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Joints with id " + id + " not found");
        }

        /// <summary>
        /// Returns action point containing orientation with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionPoint GetActionPointWithOrientation(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    // if GetOrientation dont throw exception, correct action point was found
                    actionPoint.GetOrientation(id);
                    return actionPoint;
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Action point with orientation id " + id + " not found");
        }

        /// <summary>
        /// Returns action point containing joints with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionPoint GetActionPointWithJoints(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    // if GetJoints dont throw exception, correct action point was found
                    actionPoint.GetJoints(id);
                    return actionPoint;
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Action point with joints id " + id + " not found");
        }

        /// <summary>
        /// Change size of all action points
        /// </summary>
        /// <param name="size"><0; 1> From barely visible to quite big</param>
        public void SetAPSize(float size) {
            if (Project != null)
                PlayerPrefsHelper.SaveFloat("project/" + Project.Id + "/APSize", size);
            APSize = size;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.SetSize(size);
            }
        }


        #region ACTIONS

        public Action SpawnAction(string action_id, string action_name, string action_type, ActionPoint ap, IActionProvider actionProvider) {
            Debug.Assert(GetActionByName(action_name) == null);
            ActionMetadata actionMetadata;

            try {
                actionMetadata = actionProvider.GetActionMetadata(action_type);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                return null; //TODO: throw exception
            }

            if (actionMetadata == null) {
                Debug.LogError("Actions not ready");
                return null; //TODO: throw exception
            }
            GameObject puck = Instantiate(PuckPrefab, ap.ActionsSpawn.transform);
            puck.SetActive(false);

            puck.GetComponent<Action>().Init(action_id, action_name, actionMetadata, ap, actionProvider);

            puck.transform.localScale = new Vector3(1f, 1f, 1f);

            Action action = puck.GetComponent<Action>();

            // Add new action into scene reference
            ActionPoints[ap.Data.Id].Actions.Add(action_id, action);

            ap.UpdatePositionsOfPucks();
            puck.SetActive(true);

            return action;
        }

        public string GetFreeActionName(string actionName) {
            int i = 2;
            bool hasFreeName;
            string freeName = actionName;
            do {
                hasFreeName = true;
                if (ActionsContainsName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = actionName + "_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }



        /// <summary>
        /// Updates connections between actions in the scene.
        /// </summary>
        /// <param name="projectObjects"></param>
        /// <param name="connections"></param>
        public void UpdateActionConnections(List<IO.Swagger.Model.ProjectActionPoint> actionPoints, Dictionary<string, string> connections) {
            Dictionary<string, Action> actionsToActualize = new Dictionary<string, Action>();

            // traverse through all actions (even freshly created)
            foreach (Action action in GetAllActions()) {
                // get connection from dictionary [actionID,outputAction]
                if (connections.TryGetValue(action.Data.Id, out string actionOutput)) {
                    // Check if action's output action is NOT the same as actionOutput from newly received data from server,
                    // then connection changed and we have to delete actual connection of current action and create new one
                    Action refAction = null;
                    // Find corresponding action defined by ID
                    if (actionOutput != "start" && actionOutput != "end") {
                        refAction = GetAction(actionOutput);
                        if (refAction != null) {
                            actionOutput = refAction.Data.Id;
                        } else {
                            actionOutput = "";
                        }
                    }
                    if (action.Output.Data.Default != actionOutput) {
                        // Destroy old connection if there was some
                        if (action.Output.Connection != null) {
                            ConnectionManagerArcoro.Instance.Connections.Remove(action.Output.Connection);
                            Destroy(action.Output.Connection.gameObject);
                        }

                        // Create new connection only if connected action exists (it is not start nor end)
                        if (refAction != null) {
                            // Create new one
                            //PuckInput input = GetAction(actionOutput).Input;
                            PuckInput input = refAction.Input;
                            PuckOutput output = action.Output;

                            GameObject c = Instantiate(ConnectionPrefab);
                            c.transform.SetParent(ConnectionManager.instance.transform);
                            Connection newConnection = c.GetComponent<Connection>();
                            // We are always connecting output to input.
                            newConnection.target[0] = output.gameObject.GetComponent<RectTransform>();
                            newConnection.target[1] = input.gameObject.GetComponent<RectTransform>();

                            input.Connection = newConnection;
                            output.Connection = newConnection;
                            ConnectionManagerArcoro.Instance.Connections.Add(newConnection);
                        }
                    }
                    actionsToActualize.Add(action.Data.Id, action);
                }
            }

            // Set action inputs and outputs for updated connections
            foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in actionPoints) {
                foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                    if (actionsToActualize.TryGetValue(projectAction.Id, out Action action)) {
                        // Sets action inputs (currently each action has only 1 input)
                        foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Inputs) {
                            action.Input.Data = actionIO;
                        }

                        // Sets action outputs (currently each action has only 1 output)
                        foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                            action.Output.Data = actionIO;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Destroys and removes references to action of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveAction(string Id) {
            Action aToRemove = GetAction(Id);
            string apIdToRemove = aToRemove.ActionPoint.Data.Id;
            // Call function in corresponding action that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionPoints[apIdToRemove].Actions[Id].DeleteAction();
        }

        public bool ActionsContainsName(string name) {
            foreach (Action action in GetAllActions()) {
                if (action.Data.Name == name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns action of given Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Action GetAction(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Actions.TryGetValue(id, out Action action)) {
                    return action;
                }
            }

            //Debug.LogError("Action " + Id + " not found!");
            return null;
        }

        /// <summary>
        /// Returns action of given ID.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Action GetActionByName(string name) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    if (action.Data.Name == name) {
                        return action;
                    }
                }
            }

            //Debug.LogError("Action " + id + " not found!");
            return null;
        }

        /// <summary>
        /// Returns all actions in the scene in a list [Action_object]
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllActions() {
            List<Action> actions = new List<Action>();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    actions.Add(action);
                }
            }

            return actions;
        }

        /// <summary>
        /// Returns all actions in the scene in a dictionary [action_Id, Action_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Action> GetAllActionsDict() {
            Dictionary<string, Action> actions = new Dictionary<string, Action>();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    actions.Add(action.Data.Id, action);
                }
            }

            return actions;
        }

        #endregion







        public void ActionUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdate(projectAction, true);
        }

        public void ActionBaseUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdateBaseData(projectAction);
        }

        public void ActionAdded(IO.Swagger.Model.Action projectAction, string parentId) {
            ActionPoint actionPoint = GetActionPoint(parentId);
            IActionProvider actionProvider = GetActionProvider(Action.ParseActionType(projectAction.Type).Item1);
            Base.Action action = SpawnAction(projectAction.Id, projectAction.Name, Action.ParseActionType(projectAction.Type).Item2, actionPoint, actionProvider);
            // updates name of the action
            action.ActionUpdateBaseData(projectAction);
            // updates parameters of the action
            action.ActionUpdate(projectAction);
        }


        public void ActionRemoved(IO.Swagger.Model.Action action) {
            ProjectManager.Instance.RemoveAction(action.Id);
        }


        public void ActionPointUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = GetActionPoint(projectActionPoint.Id);
                actionPoint.UpdateActionPoint(projectActionPoint);
                // TODO - update orientations, joints etc.
            } catch (KeyNotFoundException ex) {
                Debug.LogError("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }
        }

        public void ActionPointBaseUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = GetActionPoint(projectActionPoint.Id);
                actionPoint.ActionPointBaseUpdate(projectActionPoint);
                OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
            } catch (KeyNotFoundException ex) {
                Debug.Log("Action point " + projectActionPoint.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + projectActionPoint.Id + " not found!");
                return;
            }

        }

        public void ActionPointAdded(ProjectActionPoint projectActionPoint) {
            if (projectActionPoint.Parent == null || projectActionPoint.Parent == "") {
                SpawnActionPoint(projectActionPoint, null);
            } else {
                try {
                    IActionPointParent actionPointParent = GetActionPointParent(projectActionPoint.Parent);
                    SpawnActionPoint(projectActionPoint, actionPointParent);
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }

            }
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);


        }


        public void ActionPointRemoved(ProjectActionPoint projectActionPoint) {
            RemoveActionPoint(projectActionPoint.Id);
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }


        public void ActionPointOrientationAdded(NamedOrientation orientation, string actionPointIt) {
            try {
                ActionPoint actionPoint = GetActionPoint(actionPointIt);
                actionPoint.AddOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point orientation", ex.Message);
                return;
            }
        }
        public void ActionPointOrientationRemoved(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = GetActionPointWithOrientation(orientation.Id);
                actionPoint.RemoveOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointJointsUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(joints.Id);
                actionPoint.UpdateJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsBaseUpdated(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(joints.Id);
                actionPoint.BaseUpdateJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointJointsAdded(ProjectRobotJoints joints, string actionPointIt) {
            try {
                ActionPoint actionPoint = GetActionPoint(actionPointIt);
                actionPoint.AddJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point joints", ex.Message);
                return;
            }
        }


        public void ActionPointJointsRemoved(ProjectRobotJoints joints) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(joints.Id);
                actionPoint.RemoveJoints(joints);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point joints", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = ProjectManager.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.UpdateOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        public void ActionPointOrientationBaseUpdated(NamedOrientation orientation) {
            try {
                ActionPoint actionPoint = ProjectManager.Instance.GetActionPointWithOrientation(orientation.Id);
                actionPoint.BaseUpdateOrientation(orientation);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        internal void ProjectSaved() {
            ProjectChanged = false;
        }

        public async void ProjectBaseUpdated(Project data) {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor) {
                Project.Desc = data.Desc;
                Project.HasLogic = data.HasLogic;
                Project.Modified = data.Modified;
                Project.Name = data.Name;
            } else if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen) {
                await GameManager.Instance.LoadProjects();
            }
        }



    }


}
