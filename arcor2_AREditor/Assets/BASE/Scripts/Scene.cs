using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {
        
        public IO.Swagger.Model.Scene Data = new IO.Swagger.Model.Scene("", "JabloPCB", new List<IO.Swagger.Model.SceneObject>(), new List<IO.Swagger.Model.SceneService>());
        
        // string == IO.Swagger.Model.Scene Data.Id
        public Dictionary<string, ActionObject> ActionObjects = new Dictionary<string, ActionObject>();
        public GameObject ActionObjectsSpawn;

        public GameObject ConnectionPrefab, ActionPointPrefab, PuckPrefab;
        public GameObject RobotPrefab, TesterPrefab, BoxPrefab, WorkspacePrefab, UnknownPrefab;

        private bool sceneActive = true;
        private bool projectActive = true;


        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GameState == GameManager.GameStateEnum.SceneEditor && GameManager.Instance.SceneInteractable) {
                if (!sceneActive) {
                    ActivateSceneForEditing(true, "ActionObject");
                    sceneActive = true;
                }
            } else {
                if (sceneActive) {
                    ActivateSceneForEditing(false, "ActionObject");
                    sceneActive = false;
                }
            }

            if (GameManager.Instance.GameState == GameManager.GameStateEnum.ProjectEditor && GameManager.Instance.SceneInteractable) {
                if (!projectActive) {
                    ActivateSceneForEditing(true, "ActionPoint");
                    projectActive = true;
                }
            } else {
                if (projectActive) {
                    ActivateSceneForEditing(false, "ActionPoint");
                    projectActive = false;
                }
            }
        }

        /// <summary>
        /// Deactivates or activates scene and all objects in the scene to ignore raycasting (clicking).
        /// </summary>
        /// <param name="activate"></param>
        /// <param name="tagToActivate"></param>
        private void ActivateSceneForEditing(bool activate, string tagToActivate) {
            Transform[] allChildren = Helper.FindComponentsInChildrenWithTag<Transform>(gameObject, tagToActivate);
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (Transform child in allChildren) {
                    child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child in allChildren) {
                    child.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }

        private string GetFreeIOName(string ioType) {
            int i = 1;
            bool hasFreeName;
            string freeName = ioType;
            do {
                hasFreeName = true;
                if (ActionObjects.ContainsKey(freeName)) {
                    hasFreeName = false;
                }
                if (!hasFreeName)
                    freeName = ioType + i++.ToString();
            } while (!hasFreeName);

            return freeName;
        }

        #region ACTION_OBJECTS

        public ActionObject SpawnActionObject(string type, bool updateScene = true, string id = "") {
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

            actionObject.Data.Type = type;
            if (id == "")
                actionObject.Data.Id = GetFreeIOName(type);
            else
                actionObject.Data.Id = id;
            actionObject.SetScenePosition(obj.transform.localPosition);
            actionObject.SetSceneOrientation(obj.transform.localRotation);


            actionObject.ActionObjectMetadata = aom;
            if (aom.Robot) {
                actionObject.LoadEndEffectors();
            }

            // Add the Action Object into scene reference
            ActionObjects.Add(actionObject.Data.Id, actionObject);

            if (updateScene)
                GameManager.Instance.UpdateScene();

            return actionObject;
        }

        /// <summary>
        /// Updates action GameObjects in ActionObjects dict based on the data present in IO.Swagger.Model.Scene Data.
        /// </summary>
        public void UpdateActionObjects() {
            List<string> currentAO = new List<string>();
            foreach (IO.Swagger.Model.SceneObject aoSwagger in Data.Objects) {
                if (ActionObjects.TryGetValue(aoSwagger.Id, out ActionObject actionObject)) {
                    if (aoSwagger.Type != actionObject.Data.Type) {
                        ActionObjects.Remove(aoSwagger.Id);

                        // type has changed, what now? delete object and create a new one?
                        Destroy(actionObject.gameObject);
                        // TODO: create a new one with new type
                    }
                    // Update data received from swagger
                    actionObject.ActionObjectUpdate(aoSwagger);
                } else {
                    actionObject = SpawnActionObject(aoSwagger.Type, false, aoSwagger.Id);
                    actionObject.ActionObjectUpdate(aoSwagger);
                }

                currentAO.Add(aoSwagger.Id);
            }

            // Remove deleted action objects
            foreach (string actionObjectID in ActionObjects.Keys.ToList<string>()) {
                if (!currentAO.Contains(actionObjectID)) {
                    RemoveActionObject(actionObjectID);
                }
            }
        }

        /// <summary>
        /// Destroys and removes references to all action objects in the scene.
        /// </summary>
        public void RemoveActionObjects() {
            foreach (string actionObjectID in ActionObjects.Keys.ToList<string>()) {
                RemoveActionObject(actionObjectID);
            }
            // just to make sure that none reference left
            ActionObjects.Clear();
        }

        /// <summary>
        /// Destroys and removes references to action object of given ID.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveActionObject(string id) {
            try {
                ActionObjects[id].DeleteActionObject();
            } catch (NullReferenceException e) {
                Debug.LogError(e);
            }
        }

        #endregion

        #region ACTION_POINTS

        public ActionPoint SpawnActionPoint(ActionObject actionObject, IO.Swagger.Model.ActionPoint apData, bool updateProject = true) {
            GameObject AP = Instantiate(ActionPointPrefab, actionObject.ActionPointsSpawn.transform);
            AP.transform.localPosition = new Vector3(0, 0, 0);
            AP.transform.localScale = new Vector3(1f, 1f, 1f);

            ActionPoint actionPoint = AP.GetComponent<ActionPoint>();
            actionPoint.InitAP(actionObject, apData);
            if (apData == null) {
                actionPoint.SetScenePosition(transform.localPosition);
                actionPoint.SetSceneOrientation(transform.rotation);
            }

            ActionObjects[actionObject.Data.Id].ActionPoints.Add(actionPoint.Data.Id, actionPoint);

            if (updateProject)
                GameManager.Instance.UpdateProject();

            return actionPoint;
        }

        /// <summary>
        /// Updates action point GameObject in ActionObjects.ActionPoints dict based on the data present in IO.Swagger.Model.ActionPoint Data.
        /// </summary>
        /// <param name="project"></param>
        public async void UpdateActionPoints(IO.Swagger.Model.Project project) {
            List<string> currentAP = new List<string>();
            List<string> currentActions = new List<string>();
            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.ProjectObject projectObject in project.Objects) {
                if (ActionObjects.TryGetValue(projectObject.Id, out ActionObject actionObject)) {

                    foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in projectObject.ActionPoints) {
                        // if action point exist, just update it
                        if (actionObject.ActionPoints.TryGetValue(projectActionPoint.Id, out ActionPoint actionPoint)) {
                            actionPoint.ActionPointUpdate(DataHelper.ProjectActionPointToActionPoint(projectActionPoint));
                        }
                        // if action point doesn't exist, create new one
                        else {
                            actionPoint = SpawnActionPoint(actionObject, DataHelper.ProjectActionPointToActionPoint(projectActionPoint), false);
                            actionPoint.ActionPointUpdate();
                        }

                        // update actions in current action point
                        var updateActionsResult = await UpdateActions(projectActionPoint, actionPoint);
                        currentActions.AddRange(updateActionsResult.Item1);
                        // merge dictionaries
                        connections = connections.Concat(updateActionsResult.Item2).GroupBy(i => i.Key).ToDictionary(i => i.Key, i => i.First().Value);

                        actionPoint.UpdatePositionsOfPucks();

                        currentAP.Add(projectActionPoint.Id);
                    }
                }
            }

            UpdateActionConnections(project.Objects, connections);

            // Remove deleted actions
            foreach (string actionID in GetAllActionsDict().Keys.ToList<string>()) {
                if (!currentActions.Contains(actionID)) {
                    RemoveAction(actionID);
                }
            }

            // Remove deleted action points
            foreach (string actionPointID in GetAllActionPointsDict().Keys.ToList<string>()) {
                if (!currentAP.Contains(actionPointID)) {
                    RemoveActionPoint(actionPointID);
                }
            }
        }

        /// <summary>
        /// Destroys and removes references to action point of given ID.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveActionPoint(string id) {
            ActionPoint apToRemove = GetActionPoint(id);
            string aoIdToRemove = apToRemove.ActionObject.Data.Id;
            // Call function in corresponding action point that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionObjects[aoIdToRemove].ActionPoints[id].DeleteAP(false);
        }

        /// <summary>
        /// Returns action point of given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionPoint GetActionPoint(string id) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                if (actionObject.ActionPoints.TryGetValue(id, out ActionPoint actionPoint)) {
                    return actionPoint;
                }
            }
            throw new KeyNotFoundException("ActionPoint " + id + " not found!");
        }

        /// <summary>
        /// Returns all action points in the scene in a list [ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public List<ActionPoint> GetAllActionPoints() {
            List<ActionPoint> actionPoints = new List<ActionPoint>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    actionPoints.Add(actionPoint);
                }
            }
            return actionPoints;
        }

        /// <summary>
        /// Returns all action points in the scene in a dictionary [action_point_ID, ActionPoint_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ActionPoint> GetAllActionPointsDict() {
            Dictionary<string, ActionPoint> actionPoints = new Dictionary<string, ActionPoint>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    actionPoints.Add(actionPoint.Data.Id, actionPoint);
                }
            }
            return actionPoints;
        }

        #endregion

        #region ACTIONS

        public async Task<Action> SpawnPuck(string action_id, ActionObject ao, ActionPoint ap, bool generateData, IActionProvider actionProvider, bool updateProject = true, string puck_id = "") {
            ActionMetadata actionMetadata;

            try {
                actionMetadata = actionProvider.GetActionMetadata(action_id);
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
                return null; //TODO: throw exception
            }

            if (actionMetadata == null) {
                Debug.LogError("Actions not ready");
                return null; //TODO: throw exception
            }

            GameObject puck = Instantiate(PuckPrefab, ap.ActionsSpawn.transform);
            const string glyphs = "0123456789";
            string newId = puck_id;
            if (newId == "") {
                newId = action_id;
                for (int j = 0; j < 4; j++) {
                    newId += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
                }
            }
            await puck.GetComponent<Action>().Init(newId, actionMetadata, ap, generateData, actionProvider, false);

            puck.transform.localScale = new Vector3(1f, 1f, 1f);

            Action action = puck.GetComponent<Action>();

            // Add new action into scene reference
            ActionObjects[ao.Data.Id].ActionPoints[ap.Data.Id].Actions.Add(newId, action);

            ap.UpdatePositionsOfPucks();

            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }

            return action;
        }

        /// <summary>
        /// Updates actions of given ActionPoint and ProjectActionPoint received from server.
        /// </summary>
        /// <param name="projectActionPoint"></param>
        /// <param name="actionPoint"></param>
        /// <returns></returns>
        public async Task<(List<string>, Dictionary<string, string>)> UpdateActions(IO.Swagger.Model.ProjectActionPoint projectActionPoint, ActionPoint actionPoint) {
            List<string> currentA = new List<string>();
            // Connections between actions (action -> output --- input <- action2)
            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (IO.Swagger.Model.Action projectAction in projectActionPoint.Actions) {
                string providerName = projectAction.Type.Split('/').First();
                string action_type = projectAction.Type.Split('/').Last();
                IActionProvider actionProvider;
                if (ActionObjects.TryGetValue(providerName, out ActionObject originalActionObject)) {
                    actionProvider = originalActionObject;
                } else if (ActionsManager.Instance.ServicesData.TryGetValue(providerName, out Service originalService)) {
                    actionProvider = originalService;
                } else {
                    continue; //TODO: throw exception
                }

                // if action exist, just update it
                if (actionPoint.Actions.TryGetValue(projectAction.Id, out Action action)) {
                    action.ActionUpdate(projectAction);
                }
                // if action doesn't exist, create new one
                else {
                    action = await SpawnPuck(action_type, actionPoint.ActionObject, actionPoint, false, actionProvider, false, projectAction.Id);
                    action.ActionUpdate(projectAction);
                }

                // Updates (or creates new) parameters of current action
                foreach (IO.Swagger.Model.ActionParameter projectActionParameter in projectAction.Parameters) {
                    try {
                        // If action parameter exist in action dictionary, then just update that parameter value (it's metadata will always be unchanged)
                        if (action.Parameters.TryGetValue(projectActionParameter.Id, out ActionParameter actionParameter)) {
                            actionParameter.UpdateActionParameter(projectActionParameter);
                        }
                        // Otherwise create a new action parameter, load metadata for it and add it to the dictionary of action
                        else {
                            // Loads metadata of specified action parameter - projectActionParameter. Action.Metadata is created when creating Action.
                            IO.Swagger.Model.ActionParameterMeta actionParameterMetadata = action.Metadata.GetParamMetadata(projectActionParameter.Id);

                            actionParameter = new ActionParameter(actionParameterMetadata, action, projectActionParameter);
                            action.Parameters.Add(actionParameter.Id, actionParameter);
                        }
                    } catch (ItemNotFoundException ex) {
                        Debug.LogError(ex);
                    }
                }

                // Add current connection from the server, we will only map the outputs
                foreach (IO.Swagger.Model.ActionIO actionIO in projectAction.Outputs) {
                    //if(!connections.ContainsKey(projectAction.Id))
                    connections.Add(projectAction.Id, actionIO.Default);
                }

                // local list of all actions for current action point
                currentA.Add(projectAction.Id);
            }
            
            return (currentA, connections);
        }

        /// <summary>
        /// Updates connections between actions in the scene.
        /// </summary>
        /// <param name="projectObjects"></param>
        /// <param name="connections"></param>
        public void UpdateActionConnections(List<IO.Swagger.Model.ProjectObject> projectObjects, Dictionary<string, string> connections) {
            Dictionary<string, Action> actionsToActualize = new Dictionary<string, Action>();

            // traverse through all actions (even freshly created)
            foreach (Action action in GetAllActions()) {
                // get connection from dictionary [actionID,outputAction]
                if (connections.TryGetValue(action.Data.Id, out string actionOutput)) {
                    // Check if action's output action is NOT the same as actionOutput from newly received data from server,
                    // then connection changed and we have to delete actual connection of current action and create new one
                    if (action.Output.Data.Default != actionOutput) {
                        // Destroy old connection if there was some
                        if (action.Output.Connection != null) {
                            ConnectionManagerArcoro.Instance.Connections.Remove(action.Output.Connection);
                            Destroy(action.Output.Connection.gameObject);
                        }

                        if (actionOutput != "start" && actionOutput != "end") {
                            // Create new one
                            PuckInput input = GetAction(actionOutput).Input;
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

                        actionsToActualize.Add(action.Data.Id, action);
                    }
                }
            }

            // Set action inputs and outputs for updated connections
            foreach (IO.Swagger.Model.ProjectObject projectObject in projectObjects) {
                foreach (IO.Swagger.Model.ProjectActionPoint projectActionPoint in projectObject.ActionPoints) {
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
        }

        /// <summary>
        /// Destroys and removes references to action of given ID.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveAction(string id) {
            Action aToRemove = GetAction(id);
            string apIdToRemove = aToRemove.ActionPoint.Data.Id;
            string aoIdToRemove = aToRemove.ActionPoint.ActionObject.Data.Id;
            // Call function in corresponding action that will delete it and properly remove all references and connections.
            // We don't want to update project, because we are calling this method only upon received update from server.
            ActionObjects[aoIdToRemove].ActionPoints[apIdToRemove].Actions[id].DeleteAction(false);
        }

        /// <summary>
        /// Returns action of given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Action GetAction(string id) {
            foreach (ActionObject actionObject in ActionObjects.Values) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    if (actionPoint.Actions.TryGetValue(id, out Action action)) {
                        return action;
                    }
                }
            }
            throw new KeyNotFoundException("Action " + id + " not found!");
        }

        /// <summary>
        /// Returns all actions in the scene in a list [Action_object]
        /// </summary>
        /// <returns></returns>
        public List<Action> GetAllActions() {
            List<Action> actions = new List<Action>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    foreach (Action action in actionPoint.Actions.Values) {
                        actions.Add(action);
                    }
                }
            }
            return actions;
        }

        /// <summary>
        /// Returns all actions in the scene in a dictionary [action_ID, Action_object]
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Action> GetAllActionsDict() {
            Dictionary<string, Action> actions = new Dictionary<string, Action>();
            foreach (ActionObject actionObject in ActionObjects.Values) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints.Values) {
                    foreach (Action action in actionPoint.Actions.Values) {
                        actions.Add(action.Data.Id, action);
                    }
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

