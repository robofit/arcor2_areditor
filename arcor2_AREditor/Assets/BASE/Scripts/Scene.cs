using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Swagger.Model;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {

        public IO.Swagger.Model.Scene Data = null;

       // string == IO.Swagger.Model.Scene Data.Id
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        public Dictionary<string, ActionPoint> ActionPoints = new Dictionary<string, ActionPoint>();
        public GameObject ActionObjectsSpawn, ActionPointsOrigin;

        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab;
        public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;

        public GameObject CurrentlySelectedObject;

        private bool sceneActive = true;
        private bool projectActive = true;

        public bool ActionObjectsInteractive, ActionObjectsVisible;

        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor && GameManager.Instance.SceneInteractable) {
                if (!sceneActive && (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(true);
                    sceneActive = true;
                } else if (sceneActive && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            } else {
                if (sceneActive) {
                    ActivateActionObjectsForGizmo(false);
                    sceneActive = false;
                }
            }
            
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.ProjectEditor && GameManager.Instance.SceneInteractable) {
                if (!projectActive && (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                    ActivateActionPointsForGizmo(true);
                    projectActive = true;
                } else if (projectActive && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
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
        /// Deactivates or activates all action objects in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionObjectsForGizmo(bool activate) {
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (ActionObject actionObject in ActionObjects.Values) {
                    actionObject.ActivateForGizmo("Default");
                }
            }
        }

        /// <summary>
        /// Deactivates or activates all action points in scene for gizmo interaction.
        /// </summary>
        /// <param name="activate"></param>
        private void ActivateActionPointsForGizmo(bool activate) {
            if (activate) {
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

         public void SetSelectedObject(GameObject obj) {
            if (CurrentlySelectedObject != null) {
                CurrentlySelectedObject.SendMessage("Deselect");
            }
            CurrentlySelectedObject = obj;
        }

        public void SceneBaseUpdated(IO.Swagger.Model.Scene scene) {

            Data.Desc = scene.Desc;
            Data.Modified = scene.Modified;
            Data.Name = scene.Name;
        }

        

        #region ACTION_OBJECTS

        public ActionObject SpawnActionObject(string id, string type, bool updateScene = true, string name = "") {
            if (!ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata aom)) {
                return null;
            }
            GameObject obj;
            if (aom.Robot) {
                obj = Instantiate(RobotPrefab, ActionObjectsSpawn.transform);
            } else {
                switch (type) {
                    case "Box":
                        obj = Instantiate(BoxPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Box2":
                        obj = Instantiate(BoxPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Tester":
                        obj = Instantiate(TesterPrefab, ActionObjectsSpawn.transform);
                        break;
                    case "Workspace":
                        obj = Instantiate(WorkspacePrefab, ActionObjectsSpawn.transform);
                        break;
                    default:
                        obj = Instantiate(UnknownPrefab, ActionObjectsSpawn.transform);
                        break;
                }
            }

            ActionObject actionObject = obj.GetComponentInChildren<ActionObject>();

            if (name == "")
                name = GetFreeAOName(type);
            
            actionObject.InitActionObject(id, type, obj.transform.localPosition, obj.transform.localRotation, id, aom);

            // Add the Action Object into scene reference
            ActionObjects.Add(id, actionObject);
            if (aom.Robot) {
                actionObject.LoadEndEffectors();
            }

            return actionObject;
        }

        public string GetFreeAOName(string ioType) {
            int i = 1;
            bool hasFreeName;
            string freeName = ioType;
            do {
                hasFreeName = true;
                if (ActionObjectsContainName(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ioType + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }



        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        public void UpdateActionObjects() {
            List<string> currentAO = new List<string>();
            foreach (IO.Swagger.Model.SceneObject aoSwagger in Data.Objects) {
                ActionObject actionObject = SpawnActionObject(aoSwagger.Id, aoSwagger.Type, false, aoSwagger.Name);
                actionObject.ActionObjectUpdate(aoSwagger, ActionObjectsVisible, ActionObjectsInteractive);
                currentAO.Add(aoSwagger.Id);
            }

        }

        /// <summary>
        /// Updates all services from scene data.  
        /// Only called when whole scene arrived, i.e. when client is connected or scene is opened, so all service needs to be added.
        /// </summary>
        public void UpdateServices() {
            ActionsManager.Instance.ClearServices(); //just to be sure
            foreach (IO.Swagger.Model.SceneService service in Data.Services) {
                ActionsManager.Instance.AddService(service);
            }
        }

        public ActionObject GetNextActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string next;
            if (index + 1 < ActionObjects.Keys.Count)
                next = keys[index + 1];
            else
                next = keys[0];
            if (!ActionObjects.TryGetValue(next, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetPreviousActionObject(string aoId) {
            List<string> keys = ActionObjects.Keys.ToList();
            Debug.Assert(keys.Count > 0);
            int index = keys.IndexOf(aoId);
            string previous;
            if (index - 1 > -1)
                previous = keys[index - 1];
            else
                previous = keys[keys.Count - 1];
            if (!ActionObjects.TryGetValue(previous, out ActionObject actionObject)) {
                throw new ItemNotFoundException("This should never happen");
            }
            return actionObject;
        }

        public ActionObject GetFirstActionObject() {
            if (ActionObjects.Count == 0) {
                return null;
            }
            return ActionObjects.First().Value;
        }

        /// <summary>
        /// Shows action objects models
        /// </summary>
        public void ShowActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Show();
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOVisibility", true);
        }

        /// <summary>
        /// Hides action objects models
        /// </summary>
        public void HideActionObjects() {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.Hide();
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOVisibility", false);
        }

         /// <summary>
        /// Sets whether action objects should react to user inputs (i.e. enables/disables colliders)
        /// </summary>
        public void SetActionObjectsInteractivity(bool interactivity) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                actionObject.SetInteractivity(interactivity);
            }
            GameManager.Instance.SaveBool("scene/" + Data.Id + "/AOInteractivity", interactivity);
        }


        /// <summary>
        /// Destroys and removes references to all action objects in the scene.
        /// </summary>
        public void RemoveActionObjects() {
            foreach (string actionObjectId in ActionObjects.Keys.ToList<string>()) {
                RemoveActionObject(actionObjectId);
            }
            // just to make sure that none reference left
            ActionObjects.Clear();
        }

        /// <summary>
        /// Destroys and removes references to action object of given Id.
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveActionObject(string Id) {
            try {
                ActionObjects[Id].DeleteActionObject();
            } catch (NullReferenceException e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Finds action object by ID or throws KeyNotFoundException.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionObject GetActionObject(string id) {
            if (ActionObjects.TryGetValue(id, out Base.ActionObject actionObject))
                return actionObject;
            throw new KeyNotFoundException("Action object not found");
        }

        public bool TryGetActionObjectByName(string name, out ActionObject actionObjectOut) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.GetName() == name) {
                    actionObjectOut = actionObject;
                    return true;
                }   
            }
            actionObjectOut = null;
            return false;
        }

        public bool ActionObjectsContainName(string name) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.GetName() == name) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ACTION_POINTS

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
            actionPoint.InitAP(apData, actionPointParent);
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

        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public void UpdateActionPoints(IO.Swagger.Model.Project project) {
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
                        ActionObjects.TryGetValue(projectActionPoint.Parent, out actionObject);
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
               
            

            UpdateActionConnections(project.ActionPoints, connections);

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

            if (ActionObjects.TryGetValue(id, out ActionObject actionObject)) {
                return actionObject;
            }
            throw new KeyNotFoundException("No action provider with id: " + id);
        }

        public IActionPointParent GetActionPointParent(string parentId) {
            if (parentId == null || parentId == "")
                throw new KeyNotFoundException("Action point parrent " + parentId + " not found");
            if (ActionObjects.TryGetValue(parentId, out ActionObject actionObject)) {
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
            foreach(ActionPoint actionPoint in ActionPoints.Values) {
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



        #endregion

        #region ACTIONS

        public Action SpawnAction(string action_id, string action_name, string action_type, ActionPoint ap, IActionProvider actionProvider) {
            Debug.Assert(GetActionByName(action_name) == null);
            GameManager.Instance.StartLoading();
            ActionMetadata actionMetadata;

            try {
                actionMetadata = actionProvider.GetActionMetadata(action_type);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                GameManager.Instance.EndLoading();
                return null; //TODO: throw exception
            }

            if (actionMetadata == null) {
                Debug.LogError("Actions not ready");
                GameManager.Instance.EndLoading();
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
            
            GameManager.Instance.EndLoading();
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


        //// Deactivates or activates scene and all objects in scene to ignore raycasting (clicking)
        //private void ActivateSceneForEditing(bool activate, string tagToActivate) {
        //    //Transform[] allChildren = Helper.FindComponentsInChildrenWithTag<Transform>(gameObject, tagToActivate);
        //    //if (activate) {
        //    //    gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    }
        //    //} else {
        //    //    gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    }
        //    //}

        //    if (activate) {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            }
        //        }
        //    } else {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("Default");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("Default");
        //            }
        //        }
        //    }
        //}

        //private void ActivateProjectForEditing(bool activate, string tagToActivate) {
        //    if (activate) {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                }
        //            }
        //        }
        //    } else {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("Default");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("Default");
        //                }
        //            }
        //        }
        //    }
        //}
    }
}

