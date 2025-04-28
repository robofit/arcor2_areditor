using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;

namespace Base {
    /// <summary>
    /// Takes care of currently opened project. Provides methods for manipuation with project.
    /// </summary>
    public class ProjectManager : Singleton<ProjectManager> {
        /// <summary>
        /// Opened project metadata
        /// </summary>
        public Project ProjectMeta = null;
        /// <summary>
        /// All action points in scene
        /// </summary>
        public Dictionary<string, ActionPoint> ActionPoints = new();
        /// <summary>
        /// All logic items (i.e. connections of actions) in project
        /// </summary>
        public Dictionary<string, LogicItem> LogicItems = new();
        /// <summary>
        /// All Project parameters <id, instance>
        /// </summary>
        public List<ProjectParameter> ProjectParameters = new();
        /// <summary>
        /// Spawn point for global action points
        /// </summary>
        public GameObject ActionPointsOrigin;
        /// <summary>
        /// Prefab for project elements
        /// </summary>
        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab, StartPrefab, EndPrefab;
        /// <summary>
        /// Action representing start of program
        /// </summary>
        public StartAction StartAction;
        /// <summary>
        /// Action representing end of program
        /// </summary>
        public EndAction EndAction;
        /// <summary>
        /// ??? Dan?
        /// </summary>
        private bool projectActive = true;
        /// <summary>
        /// Indicates if action point orientations should be visible for given project
        /// </summary>
        public bool APOrientationsVisible;
        /// <summary>
        /// Holds current diameter of action points
        /// </summary>
        public float APSize = 0.2f;
        /// <summary>
        /// Indicates if project is loaded
        /// </summary>
        public bool Valid = false;
        /// <summary>
        /// Indicates if editation of project is allowed.
        /// </summary>
        public bool AllowEdit = false;
        /// <summary>
        /// Indicates if project was changed since last save
        /// </summary>
        private bool projectChanged;
        /// <summary>
        /// Flag which indicates whether project changed event should be trigered during Update()
        /// </summary>
        private bool updateProject = false;
        /// <summary>
        /// Public setter for project changed property. Invokes OnProjectChanged event with each change and
        /// OnProjectSavedSatusChanged when projectChanged value differs from original value (i.e. when project
        /// was not changed and now it is and vice versa) 
        /// </summary>
        public bool ProjectChanged {
            get => projectChanged;
            set {
                bool origVal = projectChanged;
                projectChanged = value;
                if (!Valid)
                    return;
                OnProjectChanged?.Invoke(this, EventArgs.Empty);
                if (origVal != value) {
                    OnProjectSavedSatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Invoked when project loaded
        /// </summary>
        public event EventHandler OnLoadProject;
        /// <summary>
        /// Invoked when project changed
        /// </summary>
        public event EventHandler OnProjectChanged;
        /// <summary>
        /// Invoked when project saved
        /// </summary>
        public event EventHandler OnProjectSaved;
        /// <summary>
        /// Invoked when projectChanged value differs from original value (i.e. when project
        /// was not changed and now it is and vice versa) 
        /// </summary>
        public event EventHandler OnProjectSavedSatusChanged;
        /// <summary>
        /// Indicates whether there is any object with available action in the scene
        /// </summary>
        public bool AnyAvailableAction;

        public event EventHandler<ActionPointEventArgs> OnActionPointAddedToScene;
        public event EventHandler<ActionEventArgs> OnActionAddedToScene;

        public event EventHandler<OrientationEventArgs> OnActionPointOrientationAdded;
        public event EventHandler<OrientationEventArgs> OnActionPointOrientationUpdated;
        public event EventHandler<OrientationEventArgs> OnActionPointOrientationBaseUpdated;
        public event EventHandler<OrientationEventArgs> OnActionPointOrientationRemoved;

        /// <summary>
        /// Initialization of projet manager
        /// </summary>
        private void Start() {
            CommunicationManager.Instance.Client.LogicItemAdded += CommunicationManager.SafeEventHandler<LogicItemEventArgs>(OnLogicItemAdded);
            CommunicationManager.Instance.Client.LogicItemRemoved += CommunicationManager.SafeEventHandler<LogicItemEventArgs>(OnLogicItemRemoved);
            CommunicationManager.Instance.Client.LogicItemUpdated += CommunicationManager.SafeEventHandler<LogicItemEventArgs>(OnLogicItemUpdated);
            CommunicationManager.Instance.Client.ProjectBaseUpdated +=
                CommunicationManager.SafeEventHandler<BareProjectEventArgs>(OnProjectBaseUpdated);

            CommunicationManager.Instance.Client.ActionPointAdded += CommunicationManager.SafeEventHandler<BareActionPointEventArgs>(OnActionPointAdded);
            CommunicationManager.Instance.Client.ActionPointRemoved += CommunicationManager.SafeEventHandler<BareActionPointEventArgs>(OnActionPointRemoved);
            CommunicationManager.Instance.Client.ActionPointUpdated += CommunicationManager.SafeEventHandler<BareActionPointEventArgs>(OnActionPointUpdated);
            CommunicationManager.Instance.Client.ActionPointBaseUpdated += CommunicationManager.SafeEventHandler<BareActionPointEventArgs>(OnActionPointBaseUpdated);

            CommunicationManager.Instance.Client.OrientationAdded += CommunicationManager.SafeEventHandler<OrientationEventArgs>(OnActionPointOrientationAddedCallback);
            CommunicationManager.Instance.Client.OrientationUpdated += CommunicationManager.SafeEventHandler<OrientationEventArgs>(OnActionPointOrientationUpdatedCallback);
            CommunicationManager.Instance.Client.OrientationBaseUpdated += CommunicationManager.SafeEventHandler<OrientationEventArgs>(OnActionPointOrientationBaseUpdatedCallback);
            CommunicationManager.Instance.Client.OrientationRemoved += CommunicationManager.SafeEventHandler<OrientationEventArgs>(OnActionPointOrientationRemovedCallback);

            CommunicationManager.Instance.Client.JointsAdded += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsAdded);
            CommunicationManager.Instance.Client.JointsUpdated += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsUpdated);
            CommunicationManager.Instance.Client.JointsBaseUpdated += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsBaseUpdated);
            CommunicationManager.Instance.Client.JointsRemoved += CommunicationManager.SafeEventHandler<JointsEventArgs>(OnActionPointJointsRemoved);

            CommunicationManager.Instance.Client.ProjectParameterAdded += CommunicationManager.SafeEventHandler<ProjectParameterEventArgs>(OnProjectParameterAdded);
            CommunicationManager.Instance.Client.ProjectParameterUpdated += CommunicationManager.SafeEventHandler<ProjectParameterEventArgs>(OnProjectParameterUpdated);
            CommunicationManager.Instance.Client.ProjectParameterRemoved += CommunicationManager.SafeEventHandler<ProjectParameterEventArgs>(OnProjectParameterRemoved);
        }

        private void OnProjectParameterRemoved(object sender, ProjectParameterEventArgs args) {
            ProjectParameters.Remove(args.Data);
            ProjectChanged = true;
        }

        private void OnProjectParameterUpdated(object sender, ProjectParameterEventArgs args) {
            ProjectParameters.RemoveAll(c => c.Id == args.Data.Id);
            ProjectParameters.Add(args.Data);
            ProjectChanged = true;
        }

        private void OnProjectParameterAdded(object sender, ProjectParameterEventArgs args) {
            ProjectParameters.Add(args.Data);
            ProjectChanged = true;
        }

        private void OnActionPointJointsRemoved(object sender, JointsEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(args.Data.Id);
                actionPoint.RemoveJoints(args.Data.Id);
                updateProject = true;
                OnProjectChanged?.Invoke(this, EventArgs.Empty);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point joints", ex.Message);
                return;
            }
        }

        private void OnActionPointJointsBaseUpdated(object sender, JointsEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(args.Data.Id);
                actionPoint.BaseUpdateJoints(args.Data);
                updateProject = true;
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        private void OnActionPointJointsUpdated(object sender, JointsEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPointWithJoints(args.Data.Id);
                actionPoint.UpdateJoints(args.Data);
                updateProject = true;
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point joints", ex.Message);
                return;
            }
        }

        private void OnActionPointJointsAdded(object sender, JointsEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPoint(args.ParentId); // ActionPoint ID
                actionPoint.AddJoints(args.Data);
                updateProject = true;
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point joints", ex.Message);
                return;
            }
        }

        private void OnActionPointBaseUpdated(object sender, BareActionPointEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPoint(args.Data.Id);
                actionPoint.ActionPointBaseUpdate(args.Data);
                updateProject = true;
            } catch (KeyNotFoundException ex) {
                Debug.Log("Action point " + args.Data.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + args.Data.Id + " not found!");
                return;
            }
        }

        private void OnActionPointOrientationBaseUpdatedCallback(object sender, OrientationEventArgs args) {
            try {
                ActionPoint actionPoint = Instance.GetActionPointWithOrientation(args.Data.Id);
                actionPoint.BaseUpdateOrientation(args.Data);

                updateProject = true;
                OnActionPointOrientationBaseUpdated?.Invoke(this, args);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        private void OnActionPointOrientationRemovedCallback(object sender, OrientationEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPointWithOrientation(args.Data.Id);
                actionPoint.RemoveOrientation(args.Data.Id);
                updateProject = true;
                OnActionPointOrientationRemoved?.Invoke(this, args);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to remove action point orientation", ex.Message);
                return;
            }
        }

        private void OnActionPointOrientationUpdatedCallback(object sender, OrientationEventArgs args) {
            try {
                ActionPoint actionPoint = Instance.GetActionPointWithOrientation(args.Data.Id);
                actionPoint.UpdateOrientation(args.Data);
                updateProject = true;
                OnActionPointOrientationUpdated?.Invoke(this, args);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to update action point orientation", ex.Message);
                return;
            }
        }

        private void OnActionPointOrientationAddedCallback(object sender, OrientationEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPoint(args.ParentId);
                actionPoint.AddOrientation(args.Data);
                updateProject = true;
                OnActionPointOrientationAdded?.Invoke(this, args);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to add action point orientation", ex.Message);
                return;
            }
        }

        private void OnActionPointRemoved(object sender, BareActionPointEventArgs args) {
            RemoveActionPoint(args.Data.Id);
            updateProject = true;
        }

        private void OnActionPointUpdated(object sender, BareActionPointEventArgs args) {
            try {
                ActionPoint actionPoint = GetActionPoint(args.Data.Id);
                actionPoint.UpdateActionPoint(DataHelper.BareActionPointToActionPoint(args.Data));
                updateProject = true;
                // TODO - update orientations, joints etc.
            } catch (KeyNotFoundException ex) {
                Debug.LogError("Action point " + args.Data.Id + " not found!");
                Notifications.Instance.ShowNotification("", "Action point " + args.Data.Id + " not found!");
                return;
            }
        }


        private void OnActionPointAdded(object sender, BareActionPointEventArgs data) {
            ActionPoint ap = null;
            if (data.Data.Parent == null || data.Data.Parent == "") {
                ap = SpawnActionPoint(DataHelper.BareActionPointToActionPoint(data.Data), null);
            } else {
                try {
                    IActionPointParent actionPointParent = GetActionPointParent(data.Data.Parent);
                    ap = SpawnActionPoint(DataHelper.BareActionPointToActionPoint(data.Data), actionPointParent);
                } catch (KeyNotFoundException ex) {
                    Debug.LogError(ex);
                }

            }

            updateProject = true;
        }

        private void OnProjectBaseUpdated(object sender, BareProjectEventArgs args) {
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor) {
                SetProjectMeta(args.Data);
                updateProject = true;
            }
        }

        /// <summary>
        /// Creates project from given json
        /// </summary>
        /// <param name="project">Project descriptoin in json</param>
        /// <param name="allowEdit">Sets if project is editable</param>
        /// <returns>True if project sucessfully created</returns>
        public async Task<bool> CreateProject(Project project, bool allowEdit) {
            Debug.Assert(ActionsManager.Instance.ActionsReady);
            if (ProjectMeta != null)
                return false;

            SetProjectMeta(DataHelper.ProjectToBareProject(project));
            AllowEdit = allowEdit;
            LoadSettings();
            AnyAvailableAction = false;
            foreach (ActionObject obj in SceneManager.Instance.ActionObjects.Values)
                if (obj.ActionObjectMetadata.ActionsMetadata.Count > 0) {
                    AnyAvailableAction = true;
                    break;
                }

            StartAction = Instantiate(StartPrefab, SceneManager.Instance.SceneOrigin.transform).GetComponent<StartAction>();
            StartAction.Init(null, null, null, null, "START");
            EndAction = Instantiate(EndPrefab, SceneManager.Instance.SceneOrigin.transform).GetComponent<EndAction>();
            EndAction.Init(null, null, null, null, "END");

            foreach (SceneObjectOverride objectOverrides in project.ObjectOverrides) {
                ActionObject actionObject = SceneManager.Instance.GetActionObject(objectOverrides.Id);
                foreach (var p in objectOverrides.Parameters) {
                    if (actionObject.TryGetParameterMetadata(p.Name, out ParameterMeta meta)) {
                        Parameter parameter = new(meta, p.Value);
                        actionObject.Overrides[p.Name] = parameter;
                    }

                }
            }

            UpdateActionPoints(project);
            UpdateProjectParameters(project.Parameters);
            if (project.HasLogic) {
                UpdateLogicItems(project.LogicItems);
            }
            if (project.Modified == DateTime.MinValue) { //new project, never saved
                projectChanged = true;
            } else if (project.IntModified == DateTime.MinValue) {
                ProjectChanged = false;
            } else {
                ProjectChanged = project.IntModified > project.Modified;
            }
            Valid = true;
            OnLoadProject?.Invoke(this, EventArgs.Empty);
            SetActionInputOutputVisibility(MainSettingsMenu.Instance.ConnectionsSwitch.IsOn());
            return true;
        }

        private void UpdateProjectParameters(List<ProjectParameter> projectParameters) {
            ProjectParameters.Clear();
            if (projectParameters == null)
                return;
            ProjectParameters.AddRange(projectParameters);
        }

        /// <summary>
        /// Destroys current project
        /// </summary>
        /// <returns>True if project successfully destroyed</returns>
        public bool DestroyProject() {
            Valid = false;
            ProjectMeta = null;
            foreach (ActionPoint ap in ActionPoints.Values) {
                ap.DeleteAP(false);
            }
            if (StartAction != null) {
                Destroy(StartAction.gameObject);
                StartAction = null;
            }
            if (EndAction != null) {
                Destroy(EndAction.gameObject);
                EndAction = null;
            }
            ActionPoints.Clear();
            ConnectionManagerArcoro.Instance.Clear();
            LogicItems.Clear();
            ProjectParameters.Clear();
            return true;
        }

        /// <summary>
        /// Updates logic items
        /// </summary>
        /// <param name="logic">List of logic items</param>
        private void UpdateLogicItems(List<Arcor2.ClientSdk.Communication.OpenApi.Models.LogicItem> logic) {
            foreach (Arcor2.ClientSdk.Communication.OpenApi.Models.LogicItem projectLogicItem in logic) {
                if (!LogicItems.TryGetValue(projectLogicItem.Id, out LogicItem logicItem)) {
                    logicItem = new LogicItem(projectLogicItem);
                    LogicItems.Add(logicItem.Data.Id, logicItem);
                } else {
                    logicItem.UpdateConnection(projectLogicItem);
                }
            }
        }

        /// <summary>
        /// Updates logic item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnLogicItemUpdated(object sender, LogicItemEventArgs args) {
            if (LogicItems.TryGetValue(args.Data.Id, out LogicItem logicItem)) {
                logicItem.Data = args.Data;
                logicItem.UpdateConnection(args.Data);
            } else {
                Debug.LogError("Server tries to update logic item that does not exists (id: " + args.Data.Id + ")");
            }
        }

        /// <summary>
        /// Removes logic item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnLogicItemRemoved(object sender, LogicItemEventArgs args) {
            if (LogicItems.TryGetValue(args.Data.Id, out LogicItem logicItem)) {
                logicItem.Remove();
                LogicItems.Remove(args.Data.Id);
            } else {
                Debug.LogError("Server tries to remove logic item that does not exists (id: " + args.Data + ")");
            }
        }

        /// <summary>
        /// Adds logic item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnLogicItemAdded(object sender, LogicItemEventArgs args) {
            LogicItem logicItem = new(args.Data);
            LogicItems.Add(args.Data.Id, logicItem);
        }

        /// <summary>
        /// Sets project metadata
        /// </summary>
        /// <param name="project"></param>
        public void SetProjectMeta(BareProject project) {
            if (ProjectMeta == null) {
                ProjectMeta = new Project(sceneId: "", id: "", name: "");
            }
            ProjectMeta.Id = project.Id;
            ProjectMeta.SceneId = project.SceneId;
            ProjectMeta.HasLogic = project.HasLogic;
            ProjectMeta.Description = project.Description;
            ProjectMeta.IntModified = project.IntModified;
            ProjectMeta.Modified = project.Modified;
            ProjectMeta.Name = project.Name;

        }

        /// <summary>
        /// Gets json describing project
        /// </summary>
        /// <returns></returns>
        public Project GetProject() {
            if (ProjectMeta == null)
                return null;
            Project project = ProjectMeta;
            project.ActionPoints = new List<Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint>();
            foreach (ActionPoint ap in ActionPoints.Values) {
                Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint projectActionPoint = ap.Data;
                foreach (Action action in ap.Actions.Values) {
                    Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction = new (id: action.Data.Id,
                        name: action.Data.Name, type: action.Data.Type) {
                        Parameters = new List<ActionParameter>()
                    };
                    foreach (Parameter param in action.Parameters.Values) {
                        projectAction.Parameters.Add(param);
                    }
                }
                project.ActionPoints.Add(projectActionPoint);
            }
            return project;
        }

        public List<Action> GetActionsWithReturnType(string type) {
            List<Action> actions = new();
            foreach (Action action in GetAllActions()) {
                if (action.Metadata.Returns.Contains(type)) {
                    actions.Add(action);
                }
            }
            return actions;
        }


        private void Update() {

            if (updateProject) {
                ProjectChanged = true;
                updateProject = false;
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

        /// <summary>
        /// Loads project settings from persistant storage
        /// </summary>
        public void LoadSettings() {
            APOrientationsVisible = PlayerPrefsHelper.LoadBool("project/" + ProjectMeta.Id + "/APOrientationsVisibility", true);
            APSize = PlayerPrefsHelper.LoadFloat("project/" + ProjectMeta.Id + "/APSize", 0.2f);
        }

        /// <summary>
        /// Spawn action point into the project
        /// </summary>
        /// <param name="apData">Json describing action point</param>
        /// <param name="actionPointParent">Parent of action point. If null, AP is spawned as global.</param>
        /// <returns></returns>
        public ActionPoint SpawnActionPoint(Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint apData, IActionPointParent actionPointParent) {
            Debug.Assert(apData != null);
            GameObject AP;
            if (actionPointParent == null) {
                AP = Instantiate(ActionPointPrefab, ActionPointsOrigin.transform);
            } else {
                AP = Instantiate(ActionPointPrefab, actionPointParent.GetSpawnPoint());
            }
            AP.transform.localScale = new Vector3(1f, 1f, 1f);
            ActionPoint actionPoint = AP.GetComponent<ActionPoint>();
            actionPoint.InitAP(apData, APSize, actionPointParent);
            ActionPoints.Add(actionPoint.Data.Id, actionPoint);
            OnActionPointAddedToScene?.Invoke(this, new ActionPointEventArgs(actionPoint));
            return actionPoint;
        }

        /// <summary>
        /// Finds free AP name, based on given name (e.g. globalAP, globalAP_1, globalAP_2 etc.)
        /// </summary>
        /// <param name="apDefaultName">Name of parent or "globalAP"</param>
        /// <returns></returns>
        public string GetFreeAPName(string apDefaultName) {
            int i = 2;
            bool hasFreeName;
            string freeName = apDefaultName + "_ap";
            do {
                hasFreeName = true;
                if (ActionPointsContainsName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = apDefaultName + "_ap_" + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }


        public string GetFreeProjectName(string projectName) {
            int i = 1;
            bool hasFreeName;
            string freeName = projectName;
            do {
                hasFreeName = true;
                try {
                    GameManager.Instance.GetProjectId(freeName);
                    hasFreeName = false;
                    freeName = projectName + "_" + i++.ToString();
                } catch (Arcor2ConnectionException) {
                    // there is no project called "freeName" -> that is our new name
                }

            } while (!hasFreeName);

            return freeName;
        }

        /// <summary>
        /// Checks if action point with given name exists
        /// </summary>
        /// <param name="name">Human readable action point name</param>
        /// <returns></returns>
        public bool ActionPointsContainsName(string name) {
            foreach (ActionPoint ap in GetAllActionPoints()) {
                if (ap.Data.Name == name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Hides all arrows representing action point orientations
        /// </summary>
        internal void HideAPOrientations() {
            APOrientationsVisible = false;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.UpdateOrientationsVisuals(false);
            }
            PlayerPrefsHelper.SaveBool("scene/" + ProjectMeta.Id + "/APOrientationsVisibility", false);
        }

        /// <summary>
        /// Shows all arrows representing action point orientations
        /// </summary>
        internal void ShowAPOrientations() {
            APOrientationsVisible = true;
            foreach (ActionPoint actionPoint in GetAllActionPoints()) {
                actionPoint.UpdateOrientationsVisuals(true);
            }
            PlayerPrefsHelper.SaveBool("scene/" + ProjectMeta.Id + "/APOrientationsVisibility", true);
        }


        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public void UpdateActionPoints(Project project) {
            List<string> currentAP = new();
            List<string> currentActions = new();
            Dictionary<string, List<Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint>> actionPointsWithParents = new();
            // ordered list of already processed parents. This ensure that global APs are processed first,
            // then APs with action objects as a parents and then APs with already processed AP parents
            List<string> processedParents = new() {
                "global"
            };
            foreach (var projectActionPoint in project.ActionPoints) {
                string parent = projectActionPoint.Parent;
                if (string.IsNullOrEmpty(parent)) {
                    parent = "global";
                }
                if (actionPointsWithParents.TryGetValue(parent, out List<Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint> projectActionPoints)) {
                    projectActionPoints.Add(projectActionPoint);
                } else {
                    List<Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint> aps = new() {
                        projectActionPoint
                    };
                    actionPointsWithParents[parent] = aps;
                }
                // if parent is action object, we dont need to process it
                if (SceneManager.Instance.ActionObjects.ContainsKey(parent)) {
                    processedParents.Add(parent);
                }
            }

            for (int i = 0; i < processedParents.Count; ++i) {
                if (actionPointsWithParents.TryGetValue(processedParents[i], out List<Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint> projectActionPoints)) {
                    foreach (Arcor2.ClientSdk.Communication.OpenApi.Models.ActionPoint projectActionPoint in projectActionPoints) {
                        // if action point exist, just update it
                        if (ActionPoints.TryGetValue(projectActionPoint.Id, out ActionPoint actionPoint)) {
                            actionPoint.ActionPointBaseUpdate(DataHelper.ActionPointToBareActionPoint(projectActionPoint));
                        }
                        // if action point doesn't exist, create new one
                        else {
                            IActionPointParent parent = null;
                            if (!string.IsNullOrEmpty(projectActionPoint.Parent)) {
                                parent = Instance.GetActionPointParent(projectActionPoint.Parent);
                            }


                            actionPoint = SpawnActionPoint(projectActionPoint, parent);

                        }

                        // update actions in current action point 
                        (List<string>, Dictionary<string, string>) updateActionsResult = actionPoint.UpdateActionPoint(projectActionPoint);
                        currentActions.AddRange(updateActionsResult.Item1);

                        actionPoint.UpdatePositionsOfPucks();

                        currentAP.Add(actionPoint.Data.Id);

                        processedParents.Add(projectActionPoint.Id);
                    }
                }

            }

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

        /// <summary>
        /// Removes all action points in project
        /// </summary>
        public void RemoveActionPoints() {
            List<ActionPoint> actionPoints = ActionPoints.Values.ToList();
            foreach (ActionPoint actionPoint in actionPoints) {
                actionPoint.DeleteAP();
            }
        }

        /// <summary>
        /// Finds parent of action point based on its ID
        /// </summary>
        /// <param name="parentId">ID of parent object</param>
        /// <returns></returns>
        public IActionPointParent GetActionPointParent(string parentId) {
            if (parentId == null || parentId == "")
                throw new KeyNotFoundException("Action point parent " + parentId + " not found");
            if (SceneManager.Instance.ActionObjects.TryGetValue(parentId, out ActionObject actionObject)) {
                return actionObject;
            }
            if (Instance.ActionPoints.TryGetValue(parentId, out ActionPoint actionPoint)) {
                return actionPoint;
            }

            throw new KeyNotFoundException("Action point parent " + parentId + " not found");
        }

        /// <summary>
        /// Gets action points based on its human readable name
        /// </summary>
        /// <param name="name">Human readable name of action point</param>
        /// <returns></returns>
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
            if (ActionPoints.TryGetValue(Id, out ActionPoint actionPoint)) {

                // If deleted AP is selected in SelectorMenu (which most of the times should be),
                // deselect it, in order to update buttons, references, etc.
                if (actionPoint == SelectorMenu.Instance.GetSelectedObject()) {
                    SelectorMenu.Instance.DeselectObject();
                }

                // Call function in corresponding action point that will delete it and properly remove all references and connections.
                // We don't want to update project, because we are calling this method only upon received update from server.
                actionPoint.DeleteAP();
            }
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
        public ProjectRobotJoints GetJoints(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    return actionPoint.GetJoints(id);
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Joints with id " + id + " not found");
        }

        public ProjectRobotJoints GetAnyJoints() {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Data.RobotJoints.Count > 0)
                    return actionPoint.Data.RobotJoints[0];
            }
            throw new KeyNotFoundException("No joints available");
        }


        /// <summary>
        /// Returns orientation with id or throws KeyNotFoundException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NamedOrientation GetNamedOrientation(string id) {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                try {
                    return actionPoint.GetOrientation(id);
                } catch (KeyNotFoundException ex) { }
            }
            throw new KeyNotFoundException("Orientations with id " + id + " not found");
        }

        public NamedOrientation GetAnyNamedOrientation() {
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Data.Orientations.Count > 0)
                    return actionPoint.Data.Orientations[0];
            }
            throw new ItemNotFoundException("No orientation available");
        }

        public ActionPoint GetAnyActionPoint() {
            if (ActionPoints.Count > 0)
                return ActionPoints.First().Value;
            throw new ItemNotFoundException("No action point available");
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
                } catch (KeyNotFoundException) { }
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

        /// <summary>
        /// Disables all action points
        /// </summary>
        public void EnableAllActionPoints(bool enable) {
            if (!Valid)
                return;
            foreach (ActionPoint ap in ActionPoints.Values) {
                ap.Enable(enable);
            }
        }

        /// <summary>
        /// Disables all orientation visuals
        /// </summary>
        public void EnableAllOrientations(bool enable) {
            if (!Valid)
                return;
            foreach (ActionPoint ap in ActionPoints.Values) {
                foreach (APOrientation orietationVisual in ap.GetOrientationsVisuals()) {
                    orietationVisual.Enable(enable);
                }
            }
        }

        /// <summary>
        /// Disables all orientation visuals
        /// </summary>
        public async Task EnableAllRobotsEE(bool enable) {
            foreach (IRobot robot in SceneManager.Instance.GetRobots()) {
                foreach (RobotEE robotEE in await robot.GetAllEE()) {
                    robotEE.Enable(enable);
                }
            }
        }


        /// <summary>
        /// Disables all actions
        /// </summary>
        public void EnableAllActions(bool enable) {
            if (!Valid)
                return;
            foreach (ActionPoint ap in ActionPoints.Values) {
                foreach (Action action in ap.Actions.Values)
                    action.Enable(enable);
            }
            if (StartAction != null)
                StartAction.Enable(enable);
            if (EndAction != null)
                EndAction.Enable(enable);
        }




        #region ACTIONS

        public Action SpawnAction(Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction, ActionPoint ap) {
            Debug.Assert(!ActionsContainsName(projectAction.Name));
            ActionMetadata actionMetadata;
            string providerName = projectAction.Type.Split('/').First();
            string actionType = projectAction.Type.Split('/').Last();
            IActionProvider actionProvider;
            try {
                actionProvider = SceneManager.Instance.GetActionObject(providerName);
            } catch (KeyNotFoundException ex) {
                throw new Arcor2ConnectionException("PROVIDER NOT FOUND EXCEPTION: " + providerName + " " + actionType);
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
                return StartAction;
            else if (id == "END")
                return EndAction;
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.Actions.TryGetValue(id, out Action action)) {
                    return action;
                }
            }

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

            throw new ItemNotFoundException("Action with name " + name + " not found");
        }

        /// <summary>
        /// Returns all actions in the scene in a list [Action_object]
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllActions() {
            List<Action> actions = new();
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
            Dictionary<string, Action> actions = new();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                foreach (Action action in actionPoint.Actions.Values) {
                    actions.Add(action.Data.Id, action);
                }
            }

            return actions;
        }

        #endregion

        /// <summary>
        /// Updates action 
        /// </summary>
        /// <param name="projectAction">Action description</param>
        public void ActionUpdated(Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction) {
            Action action = GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdate(projectAction, true);
            updateProject = true;
        }

        /// <summary>
        /// Updates metadata of action
        /// </summary>
        /// <param name="projectAction">Action description</param>
        public void ActionBaseUpdated(BareAction projectAction) {
            Action action = GetAction(projectAction.Id);
            if (action == null) {
                Debug.LogError("Trying to update non-existing action!");
                return;
            }
            action.ActionUpdateBaseData(projectAction);
            updateProject = true;
        }

        /// <summary>
        /// Adds action to the project
        /// </summary>
        /// <param name="projectAction">Action description</param>
        /// <param name="parentId">UUID of action point to which the action should be added</param>
        public void ActionAdded(Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction, string parentId) {
            ActionPoint actionPoint = GetActionPoint(parentId);
            try {
                var action = SpawnAction(projectAction, actionPoint);
                // updates name of the action
                action.ActionUpdateBaseData(DataHelper.ActionToBareAction(projectAction));
                // updates parameters of the action
                action.ActionUpdate(projectAction);
                //action.EnableInputOutput(MainSettingsMenu.Instance.ConnectionsSwitch.IsOn());
                updateProject = true;
                OnActionAddedToScene?.Invoke(this, new ActionEventArgs(action));
            } catch (Arcor2ConnectionException ex) {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Removes actino from project
        /// </summary>
        /// <param name="action"></param>
        public void ActionRemoved(BareAction action) {
            Instance.RemoveAction(action.Id);
            updateProject = true;
        }

        /// <summary>
        /// Called when project saved. Invokes OnProjectSaved event
        /// </summary>
        internal void ProjectSaved() {
            ProjectChanged = false;
            Notifications.Instance.ShowToastMessage("Project saved successfully.");
            OnProjectSaved?.Invoke(this, EventArgs.Empty);
        }

        public void HighlightOrientation(string orientationId, bool highlight) {
            if (!Valid)
                return;
            try {
                ActionPoint ap = GetActionPointWithOrientation(orientationId);
                APOrientation orientation = ap.GetOrientationVisual(orientationId);
                orientation.HighlightOrientation(highlight);
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }
        }

        public void SetActionInputOutputVisibility(bool visible) {
            if (!Valid || !ProjectMeta.HasLogic)
                return;
            if (SelectorMenu.Instance.IOToggle.Toggled != visible)
                SelectorMenu.Instance.IOToggle.SwitchToggle();
            SelectorMenu.Instance.IOToggle.SetInteractivity(visible, "Connections are hidden");
        }

        public bool AnyOrientationInTheProject() {
            foreach (ActionPoint ap in ActionPoints.Values) {
                if (ap.AnyOrientation())
                    return true;
            }
            return false;
        }

        public bool AnyJointsInTheProject() {
            foreach (ActionPoint ap in ActionPoints.Values) {
                if (ap.AnyJoints())
                    return true;
            }
            return false;
        }

        public List<string> GetAllBreakpoints() {
            List<string> breakPoints = new();
            foreach (ActionPoint actionPoint in ActionPoints.Values) {
                if (actionPoint.BreakPoint)
                    breakPoints.Add(actionPoint.GetId());
            }
            return breakPoints;
        }

    }


}
