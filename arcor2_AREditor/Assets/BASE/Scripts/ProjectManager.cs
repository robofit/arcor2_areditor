using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

using IO.Swagger.Model;
using System.Linq;

namespace Base {
    
    public class ProjectManager : Base.Singleton<ProjectManager> {
        public IO.Swagger.Model.Project ProjectMeta = null;

        public Dictionary<string, ActionPoint> ActionPoints = new Dictionary<string, ActionPoint>();
        public Dictionary<string, LogicItem> LogicItems = new Dictionary<string, LogicItem>();

        public GameObject ActionPointsOrigin;
        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab, StartPrefab, EndPrefab;

        private StartAction startAction;
        private EndAction endAction;

        private bool projectActive = true;
        public bool APOrientationsVisible;

        public float APSize = 0.5f;

        public bool ProjectLoaded = false;

        public bool AllowEdit = false;
        private bool projectChanged;

        public bool ProjectChanged {
            get => projectChanged;
            set {
                bool origVal = projectChanged;
                projectChanged = value;
                OnProjectChanged?.Invoke(this, EventArgs.Empty);
                if (origVal != value) {
                    OnProjectSavedSatusChanged?.Invoke(this, EventArgs.Empty);
                }
            } 
        }

        public event EventHandler OnActionPointsChanged;
        public event AREditorEventArgs.ActionPointUpdatedEventHandler OnActionPointUpdated;
        public event EventHandler OnLoadProject;
        public event EventHandler OnProjectChanged;
        public event EventHandler OnProjectSaved;
        public event EventHandler OnProjectSavedSatusChanged;

        private void Start() {
            WebsocketManager.Instance.OnLogicItemAdded += OnLogicItemAdded;
            WebsocketManager.Instance.OnLogicItemRemoved += OnLogicItemRemoved;
            WebsocketManager.Instance.OnLogicItemUpdated += OnLogicItemUpdated;
        }

        /// <summary>
        /// Creates project from given json
        /// </summary>
        /// <param name="project"></param>
        public async Task<bool> CreateProject(IO.Swagger.Model.Project project, bool allowEdit) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (ProjectMeta != null)
                return false;

            SetProjectMeta(project);
            this.AllowEdit = allowEdit;
            LoadSettings();

            startAction = Instantiate(StartPrefab, GameManager.Instance.Scene.transform).GetComponent<StartAction>();
            endAction = Instantiate(EndPrefab, GameManager.Instance.Scene.transform).GetComponent<EndAction>();

            bool success = UpdateProject(project, true);

            if (success) {
                ProjectChanged = false;
                OnLoadProject?.Invoke(this, EventArgs.Empty);
            }
            ProjectLoaded = success;
            (bool successClose, _) = await GameManager.Instance.CloseProject(false, true);
            ProjectChanged = !successClose;
            return success;
        }

        /// <summary>
        /// Updates project from given json
        /// </summary>
        /// <param name="project"></param>
        public bool UpdateProject(IO.Swagger.Model.Project project, bool forceEdit = false) {
            if (project.Id != ProjectMeta.Id) {
                return false;
            }
            if (!AllowEdit && !forceEdit) {
                Debug.LogError("Editation of this project is not allowed!");
                Notifications.Instance.SaveLogs(SceneManager.Instance.GetScene(), project, "Editation of this project is not allowed!");
                return false;
            }

            SetProjectMeta(project);
            UpdateActionPoints(project);
            if (project.HasLogic)
                UpdateLogicItems(project.Logic);
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
      public bool DestroyProject() {
            ProjectLoaded = false;
            ProjectMeta = null;
            foreach (ActionPoint ap in ActionPoints.Values) {
                ap.DeleteAP(false);
            }
            if (startAction != null) {
                Destroy(startAction.gameObject);
                startAction = null;
            }               
            if (endAction != null) {
                Destroy(endAction.gameObject);
                endAction = null;
            }
            ActionPoints.Clear();
            return true;
        }


        private void UpdateLogicItems(List<IO.Swagger.Model.LogicItem> logic) {
            foreach (IO.Swagger.Model.LogicItem projectLogicItem in logic) {
                if (!LogicItems.TryGetValue(projectLogicItem.Id, out LogicItem logicItem)) {
                    logicItem = new LogicItem(projectLogicItem);
                    LogicItems.Add(logicItem.Data.Id, logicItem);
                }
                logicItem.UpdateConnection(projectLogicItem);

            }
        }

        private void OnLogicItemUpdated(object sender, LogicItemChangedEventArgs args) {
            if (LogicItems.TryGetValue(args.Data.Id, out LogicItem logicItem)) {
                logicItem.Data = args.Data;
                logicItem.UpdateConnection(args.Data);
            } else {
                Debug.LogError("Server tries to update logic item that does not exists (id: " + args.Data.Id + ")");
            }
        }

        private void OnLogicItemRemoved(object sender, StringEventArgs args) {
            if (LogicItems.TryGetValue(args.Data, out LogicItem logicItem)) {
                logicItem.Remove();
                LogicItems.Remove(args.Data);
            } else {
                Debug.LogError("Server tries to remove logic item that does not exists (id: " + args.Data + ")");
            }
        }

        private void OnLogicItemAdded(object sender, LogicItemChangedEventArgs args) {
            LogicItem logicItem = new LogicItem(args.Data);
            LogicItems.Add(args.Data.Id, logicItem);
        }

        public void SetProjectMeta(Project project) {
            if (ProjectMeta == null) {
                ProjectMeta = new Project(sceneId: "", id: "", name: "");
            }
            ProjectMeta.Id = project.Id;
            ProjectMeta.SceneId = project.SceneId;
            ProjectMeta.HasLogic = project.HasLogic;
            ProjectMeta.Desc = project.Desc;
            ProjectMeta.IntModified = project.IntModified;
            ProjectMeta.Modified = project.Modified;
            ProjectMeta.Name = project.Name;
            
        }
        public Project GetProject() {
            if (ProjectMeta == null)
                return null;
            Project project = ProjectMeta;
            project.ActionPoints = new List<ProjectActionPoint>();
            foreach (ActionPoint ap in ActionPoints.Values) {
                ProjectActionPoint projectActionPoint = ap.Data;
                foreach (Action action in ap.Actions.Values) {
                    IO.Swagger.Model.Action projectAction = new IO.Swagger.Model.Action(id: action.Data.Id,
                        name: action.Data.Name, type: action.Data.Type) {
                        Parameters = new List<IO.Swagger.Model.ActionParameter>(),
                        //Inputs = new List<ActionIO>(),
                        //Outputs = new List<ActionIO>()
                    };
                    foreach (ActionParameter param in action.Parameters.Values) {
                        projectAction.Parameters.Add(param);
                    }
                    //projectAction.Inputs.Add(action.Input.Data);
                   // projectAction.Outputs.Add(action.Output.Data);
                }
                project.ActionPoints.Add(projectActionPoint);
            }
            return project;
        }

        private void Update() {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor &&
                GameManager.Instance.SceneInteractable &&
                GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Normal) {
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
            APOrientationsVisible = PlayerPrefsHelper.LoadBool("project/" + ProjectMeta.Id + "/APOrientationsVisibility", true);
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
            PlayerPrefsHelper.SaveBool("scene/" + ProjectMeta.Id + "/APOrientationsVisibility", false);
        }

        internal void ShowAPOrientations() {
            APOrientationsVisible = true;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.UpdateOrientationsVisuals();
            }
            PlayerPrefsHelper.SaveBool("scene/" + ProjectMeta.Id + "/APOrientationsVisibility", true);
        }


        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public void UpdateActionPoints(Project project) {
            List<string> currentAP = new List<string>();
            List<string> currentActions = new List<string>();
            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in project.ActionPoints) {
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
                //connections = connections.Concat(updateActionsResult.Item2).GroupBy(i => i.Key).ToDictionary(i => i.Key, i => i.First().Value);

                actionPoint.UpdatePositionsOfPucks();

                currentAP.Add(actionPoint.Data.Id);
            }

            //UpdateActionConnections(project.ActionPoints, connections);

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
                return SceneManager.Instance.GetService(id);
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
            if (ProjectMeta != null)
                PlayerPrefsHelper.SaveFloat("project/" + ProjectMeta.Id + "/APSize", size);
            APSize = size;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.SetSize(size);
            }
        }


        #region ACTIONS

        public Action SpawnAction(IO.Swagger.Model.Action projectAction, ActionPoint ap) {
            //string action_id, string action_name, string action_type, 
            Debug.Assert(!ActionsContainsName(projectAction.Name));
            ActionMetadata actionMetadata;
            string providerName = projectAction.Type.Split('/').First();
            string actionType = projectAction.Type.Split('/').Last();
            IActionProvider actionProvider;
            try {
                actionProvider = SceneManager.Instance.GetActionObject(providerName);
            } catch (KeyNotFoundException ex) {
                if (SceneManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                    actionProvider = originalService;
                } else {
                    Debug.LogError(ex);
                    throw new RequestFailedException("PROVIDER NOT FOUND EXCEPTION: " + providerName + " " + actionType);
                }
            }

            try {
                actionMetadata = actionProvider.GetActionMetadata(actionType);
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

            puck.GetComponent<Action>().Init(projectAction, actionMetadata, ap, actionProvider);

            puck.transform.localScale = new Vector3(1f, 1f, 1f);

            Action action = puck.GetComponent<Action>();

            // Add new action into scene reference
            ActionPoints[ap.Data.Id].Actions.Add(action.Data.Id, action);

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
            if (id == "START")
                return startAction;
            else if (id == "END")
                return endAction;
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Actions.TryGetValue(id, out Action action)) {
                    return action;
                }
            }

            //Debug.LogError("Action " + Id + " not found!");
            throw new ItemNotFoundException("Action with ID " + id + " not found");
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
            throw new ItemNotFoundException("Action with name " + name + " not found");
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
            ProjectChanged = true;
        }

        public void ActionBaseUpdated(IO.Swagger.Model.Action projectAction) {
            Base.Action action = GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdateBaseData(projectAction);
            ProjectChanged = true;
        }

        public void ActionAdded(IO.Swagger.Model.Action projectAction, string parentId) {
            ActionPoint actionPoint = GetActionPoint(parentId);
            try {
                Base.Action action = SpawnAction(projectAction, actionPoint);
                // updates name of the action
                action.ActionUpdateBaseData(projectAction);
                // updates parameters of the action
                action.ActionUpdate(projectAction);
                ProjectChanged = true;
            } catch (RequestFailedException ex) {
                Debug.LogError(ex);
            }            
        }


        public void ActionRemoved(IO.Swagger.Model.Action action) {
            ProjectManager.Instance.RemoveAction(action.Id);
            ProjectChanged = true;
        }


        public void ActionPointUpdated(ProjectActionPoint projectActionPoint) {
            try {
                ActionPoint actionPoint = GetActionPoint(projectActionPoint.Id);
                actionPoint.UpdateActionPoint(projectActionPoint);
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
            ProjectChanged = true;


        }


        public void ActionPointRemoved(ProjectActionPoint projectActionPoint) {
            RemoveActionPoint(projectActionPoint.Id);
            OnActionPointsChanged?.Invoke(this, EventArgs.Empty);
            ProjectChanged = true;
        }


        public void ActionPointOrientationAdded(NamedOrientation orientation, string actionPointIt) {
            try {
                ActionPoint actionPoint = GetActionPoint(actionPointIt);
                actionPoint.AddOrientation(orientation);
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
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
                OnActionPointUpdated?.Invoke(this, new ActionPointUpdatedEventArgs(actionPoint));
                ProjectChanged = true;
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        internal void ProjectSaved() {
            ProjectChanged = false;
            Base.Notifications.Instance.ShowNotification("Project saved successfully", "");
            OnProjectSaved?.Invoke(this, EventArgs.Empty);
        }

        public async void ProjectBaseUpdated(Project data) {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor) {
                SetProjectMeta(data);
                ProjectChanged = true;
            } else if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen) {
                await GameManager.Instance.LoadProjects();
            }
        }



    }


}
