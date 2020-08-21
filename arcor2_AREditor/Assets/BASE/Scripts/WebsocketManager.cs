using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using NativeWebSocket;
using IO.Swagger.Model;
using UnityEditor;

namespace Base {

    public class WebsocketManager : Singleton<WebsocketManager> {
        /// <summary>
        /// ARServer URI
        /// </summary>
        public string APIDomainWS = "";
        /// <summary>
        /// Websocket context
        /// </summary>
        private WebSocket websocket;
        /// <summary>
        /// Dictionary of unprocessed responses
        /// </summary>
        private Dictionary<int, string> responses = new Dictionary<int, string>();
        /// <summary>
        /// Requset id pool
        /// </summary>
        private int requestID = 1;
        /// <summary>
        /// Invoked when new end effector pose recieved from server. Contains eef pose.
        /// </summary>
        public event AREditorEventArgs.RobotEefUpdatedEventHandler OnRobotEefUpdated;
        /// <summary>
        /// Invoked when new joints values recieved from server. Contains joints values.
        /// </summary>
        public event AREditorEventArgs.RobotJointsUpdatedEventHandler OnRobotJointsUpdated;
        /// <summary>
        /// Invoked when connected to server
        /// </summary>
        public event EventHandler OnConnectedEvent;
        /// <summary>
        /// Invoked when disconnected from server.
        /// </summary>
        public event EventHandler OnDisconnectEvent;
        /// <summary>
        /// Invoked when main screen should be opened. Contains info of which list (scenes, projects, packages)
        /// should be opened and which tile should be highlighted.
        /// </summary>
        public event AREditorEventArgs.ShowMainScreenEventHandler OnShowMainScreen;
        /// <summary>
        /// Invoked when action item added. Contains info about the logic item.
        /// </summary>
        public event AREditorEventArgs.LogicItemChangedEventHandler OnLogicItemAdded;
        /// <summary>
        /// Invoked when logic item removed. Contains UUID of removed item.
        /// </summary>
        public event AREditorEventArgs.StringEventHandler OnLogicItemRemoved;
        /// <summary>
        /// Invoked when logic item updated. Contains info of updated logic item. 
        /// </summary>
        public event AREditorEventArgs.LogicItemChangedEventHandler OnLogicItemUpdated;
        public event AREditorEventArgs.StringEventHandler OnProjectRemoved;
        public event AREditorEventArgs.BareProjectEventHandler OnProjectBaseUpdated;

        public event AREditorEventArgs.StringEventHandler OnSceneRemoved;
        public event AREditorEventArgs.BareSceneEventHandler OnSceneBaseUpdated;

        public event AREditorEventArgs.ActionEventHandler OnActionAdded;
        public event AREditorEventArgs.ActionEventHandler OnActionUpdated;
        public event AREditorEventArgs.BareActionEventHandler OnActionBaseUpdated;
        public event AREditorEventArgs.StringEventHandler OnActionRemoved;

        public event AREditorEventArgs.ProjectActionPointEventHandler OnActionPointAdded;
        public event AREditorEventArgs.ProjectActionPointEventHandler OnActionPointUpdated;
        public event AREditorEventArgs.BareActionPointEventHandler OnActionPointBaseUpdated;
        public event AREditorEventArgs.StringEventHandler OnActionPointRemoved;

        public event AREditorEventArgs.ActionPointOrientationEventHandler OnActionPointOrientationAdded;
        public event AREditorEventArgs.ActionPointOrientationEventHandler OnActionPointOrientationUpdated;
        public event AREditorEventArgs.ActionPointOrientationEventHandler OnActionPointOrientationBaseUpdated;
        public event AREditorEventArgs.StringEventHandler OnActionPointOrientationRemoved;

        public event AREditorEventArgs.RobotJointsEventHandler OnActionPointJointsAdded;
        public event AREditorEventArgs.RobotJointsEventHandler OnActionPointJointsUpdated;
        public event AREditorEventArgs.RobotJointsEventHandler OnActionPointJointsBaseUpdated;
        public event AREditorEventArgs.StringEventHandler OnActionPointJointsRemoved;

        /// <summary>
        /// ARServer domain or IP address
        /// </summary>
        private string serverDomain;

       
        /// <summary>
        /// Callbeck when connection to the server is closed
        /// </summary>
        /// <param name="closeCode"></param>
        private void OnClose(WebSocketCloseCode closeCode) {
            Debug.Log("Connection closed!");
            CleanupAfterDisconnect();
            OnDisconnectEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Callback when some connection error occures
        /// </summary>
        /// <param name="errorMsg"></param>
        private void OnError(string errorMsg) {
            Debug.LogError(errorMsg);
        }

        /// <summary>
        /// Callback when connected to the server
        /// </summary>
        private void OnConnected() {
            Debug.Log("On connected");
            OnConnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Tries to connect to server
        /// </summary>
        /// <param name="domain">Domain name or IP address of server</param>
        /// <param name="port">Server port</param>
        public async void ConnectToServer(string domain, int port) {
           
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Connecting;
            try {
            APIDomainWS = GetWSURI(domain, port);
            websocket = new WebSocket(APIDomainWS);
            serverDomain = domain;

            websocket.OnOpen += OnConnected;
            websocket.OnError += OnError;
            websocket.OnClose += OnClose;
            websocket.OnMessage += HandleReceivedData;

            await websocket.Connect();
            } catch (UriFormatException ex) {
                Debug.LogError(ex);
                Notifications.Instance.ShowNotification("Failed to parse domain", ex.Message);
                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
                GameManager.Instance.HideLoadingScreen(true);
            }
        }

        /// <summary>
        /// Disconnects from server
        /// </summary>
        async public void DisconnectFromSever() {
            Debug.Log("Disconnecting");
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            try {
                await websocket.Close();
            } catch (WebSocketException e) {
                //already closed probably..
            }
            CleanupAfterDisconnect();
        }

        /// <summary>
        /// Sets default state for websocket manager and game manager
        /// </summary>
        public void CleanupAfterDisconnect() {
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            websocket = null;
            serverDomain = null;
            GameManager.Instance.HideLoadingScreen();
        }

        /// <summary>
        /// If connected to server, returns domain name or IP addr of server
        /// </summary>
        /// <returns>Domain name or IP addr of server</returns>
        public string GetServerDomain() {
            if (websocket.State != WebSocketState.Open) {
                return null;
            }
            return serverDomain;
        }

       
        /// <summary>
        /// Create websocket URI from domain name and port
        /// </summary>
        /// <param name="domain">Domain name or IP address</param>
        /// <param name="port">Server port</param>
        /// <returns></returns>
        public string GetWSURI(string domain, int port) {
            return "ws://" + domain + ":" + port.ToString();
        }

        /// <summary>
        /// Disconnects from server upon closing of app
        /// </summary>
        private void OnApplicationQuit() {
            Debug.LogError("Stopping app");
            DisconnectFromSever();
        }

        /// <summary>
        /// Universal method for sending data to server
        /// </summary>
        /// <param name="data">String to send</param>
        /// <param name="key">ID of request (used to obtain result)</param>
        /// <param name="storeResult">Flag whether or not store result</param>
        /// <param name="logInfo">Flag whether or not log sended message</param>
        public void SendDataToServer(string data, int key = -1, bool storeResult = false, bool logInfo = true) {
            if (key < 0) {
                key = Interlocked.Increment(ref requestID);
            }
            if (logInfo)
                Debug.Log("Sending data to server: " + data);

            if (storeResult) {
                responses[key] = null;
            }
            SendWebSocketMessage(data);
        }
      
        /// <summary>
        /// Sends data to server
        /// </summary>
        /// <param name="data"></param>
        private async void SendWebSocketMessage(string data) {
            if (websocket.State == WebSocketState.Open) {
                await websocket.SendText(data);
            }
        }

        /// <summary>
        /// Method for parsing recieved message and invoke proper callback
        /// </summary>
        /// <param name="message">Recieved message</param>
        private async void HandleReceivedData(byte[] message) {
            string data = Encoding.Default.GetString(message);
            var dispatchType = new {
                id = 0,
                response = "",
                @event = "",
                request = ""
            };
            
            var dispatch = JsonConvert.DeserializeAnonymousType(data, dispatchType);

            if (dispatch?.response == null && dispatch?.request == null && dispatch?.@event == null)
                return;
            if (dispatch?.@event == null || (dispatch?.@event != "RobotEef" && dispatch?.@event != "RobotJoints"))
                Debug.Log("Recieved new data: " + data);
            if (dispatch.response != null) {

                if (responses.ContainsKey(dispatch.id)) {
                    responses[dispatch.id] = data;
                } else {
                    // TODO: response to unknown request
                }
                   
            } else if (dispatch.@event != null) {
                switch (dispatch.@event) {
                    case "SceneChanged":
                        HandleSceneChanged(data);
                        break;
                    case "SceneObjectChanged":
                        HandleSceneObjectChanged(data);
                        break;
                    /*case "SceneServiceChanged":
                        HandleSceneServiceChanged(data);
                        break;*/
                    case "ActionPointChanged":
                        HandleActionPointChanged(data);
                        break;
                    case "ActionChanged":
                        HandleActionChanged(data);
                        break;
                    case "LogicItemChanged":
                        HandleLogicItemChanged(data);
                        break;
                    case "OrientationChanged":
                        HandleOrientationChanged(data);
                        break;
                    case "JointsChanged":
                        HandleJointsChanged(data);
                        break;
                    //case "ObjectTypesChanged":
                    case "ChangedObjectTypes":
                        HandleChangedObjectTypesEvent(data);
                        break;
                    case "CurrentAction":
                        HandleCurrentAction(data);
                        break;
                    case "PackageState":
                        HandlePackageState(data);
                        break;
                    case "PackageInfo":
                        HandlePackageInfo(data);
                        break;
                    case "ProjectSaved":
                        HandleProjectSaved(data);
                        break;
                    case "SceneSaved":
                        HandleSceneSaved(data);
                        break;
                    case "ProjectException":
                        HandleProjectException(data);
                        break;
                    case "ActionResult":
                        HandleActionResult(data);
                        break;
                    case "ActionCancelled":
                        HandleActionCanceled(data);
                        break;
                    case "ActionExecution":
                        HandleActionExecution(data);
                        break;
                    case "RobotEef":
                        HandleRobotEef(data);
                        break;
                    case "RobotJoints":
                        HandleRobotJoints(data);
                        break;
                    case "OpenScene":
                        await HandleOpenScene(data);
                        break;
                    case "OpenProject":
                        HandleOpenProject(data);
                        break;
                    case "SceneClosed":
                        HandleCloseScene(data);
                        break;
                    case "ProjectClosed":
                        HandleCloseProject(data);
                        break;
                    case "OpenPackage":
                        HandleOpenPackage(data);
                        break;
                    case "ProjectChanged":
                        HandleProjectChanged(data);
                        break;
                    case "ShowMainScreen":
                        HandleShowMainScreen(data);
                        break;
                }
            }

        }

        /// <summary>
        /// Waits until response with selected ID is recieved.
        /// </summary>
        /// <typeparam name="T">Type of reposnse</typeparam>
        /// <param name="key">ID of response</param>
        /// <param name="timeout">Time [ms] after which timeout exception is thrown</param>
        /// <returns>Decoded response</returns>
        private Task<T> WaitForResult<T>(int key, int timeout = 15000) {
            return Task.Run(() => {
                if (responses.TryGetValue(key, out string value)) {
                    if (value == null) {
                        Task<string> result = WaitForResponseReady(key, timeout);
                        if (!result.Wait(timeout)) {
                            Debug.LogError("The timeout interval elapsed.");
                            //TODO: throw an exception and handle it properly
                            //throw new TimeoutException();
                            return default;
                        } else {
                            value = result.Result;
                        }
                    }
                    return JsonConvert.DeserializeObject<T>(value);
                } else {
                    return default;
                }
            });
        }

        // TODO: add timeout!
        /// <summary>
        /// WWaits until response with selected ID is recieved.
        /// </summary>
        /// <param name="key">ID of reponse</param>
        /// <param name="timeout">Not used</param>
        /// <returns>Raw response</returns>
        private Task<string> WaitForResponseReady(int key, int timeout) {
            return Task.Run(() => {
                while (true) {
                    if (responses.TryGetValue(key, out string value)) {
                        if (value != null) {
                            return value;
                        } else {
                            Thread.Sleep(10);
                        }
                    }
                }
            });
            
        }

        /// <summary>
        /// Handles changes on project
        /// </summary>
        /// <param name="obj">Message from server</param>
        private void HandleProjectChanged(string obj) {            
            ProjectManager.Instance.ProjectChanged = true;
            IO.Swagger.Model.ProjectChanged eventProjectChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.ProjectChanged>(obj);
            switch (eventProjectChanged.ChangeType) {
                case IO.Swagger.Model.ProjectChanged.ChangeTypeEnum.Add:
                    throw new NotImplementedException("Project changed update should never occured!");
                case IO.Swagger.Model.ProjectChanged.ChangeTypeEnum.Remove:
                    OnProjectRemoved?.Invoke(this, new StringEventArgs(eventProjectChanged.Data.Id));
                    break;
                case IO.Swagger.Model.ProjectChanged.ChangeTypeEnum.Update:
                    throw new NotImplementedException("Project changed update should never occured!");
                case IO.Swagger.Model.ProjectChanged.ChangeTypeEnum.Updatebase:
                    OnProjectBaseUpdated?.Invoke(this, new BareProjectEventArgs(eventProjectChanged.Data));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Handles message with info about robots end effector
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleRobotEef(string data) {
            IO.Swagger.Model.RobotEefEvent robotEef = JsonConvert.DeserializeObject<IO.Swagger.Model.RobotEefEvent>(data);
            OnRobotEefUpdated?.Invoke(this, new RobotEefUpdatedEventArgs(robotEef.Data));
        }

        /// <summary>
        /// Handles message with infou about robots joints
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleRobotJoints(string data) {
            IO.Swagger.Model.RobotJointsEvent robotJoints = JsonConvert.DeserializeObject<IO.Swagger.Model.RobotJointsEvent>(data);
            OnRobotJointsUpdated?.Invoke(this, new RobotJointsUpdatedEventArgs(robotJoints.Data));
        }

        /// <summary>
        /// Opens main screen
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleShowMainScreen(string data) {
            IO.Swagger.Model.ShowMainScreenEvent showMainScreenEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.ShowMainScreenEvent>(data);
            OnShowMainScreen?.Invoke(this, new ShowMainScreenEventArgs(showMainScreenEvent.Data));
        }

        /// <summary>
        /// Handles info about currently running action
        /// </summary>
        /// <param name="obj">Message from server</param>
        private void HandleCurrentAction(string obj) {
            string puck_id;
            if (!ProjectManager.Instance.ProjectLoaded) {
                Debug.LogWarning("Project not yet loaded, ignoring current action");
                return;
            }
            try {
                
                IO.Swagger.Model.CurrentActionEvent currentActionEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.CurrentActionEvent>(obj);

                puck_id = currentActionEvent.Data.ActionId;



            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleCurrentAction()");
                return;
            }
            // Stop previously running action (change its color to default)
            if (ActionsManager.Instance.CurrentlyRunningAction != null)
                ActionsManager.Instance.CurrentlyRunningAction.StopAction();
            try {
                Action puck = ProjectManager.Instance.GetAction(puck_id);
                ActionsManager.Instance.CurrentlyRunningAction = puck;
                // Run current action (set its color to running)
                puck.RunAction();
            } catch (ItemNotFoundException ex) {
                Debug.LogError(ex);
            }
            
        }

        /// <summary>
        /// Handles result of recently executed action
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleActionResult(string data) {
            IO.Swagger.Model.ActionResultEvent actionResult = JsonConvert.DeserializeObject<IO.Swagger.Model.ActionResultEvent>(data);
            GameManager.Instance.HandleActionResult(actionResult.Data);
        }

        /// <summary>
        /// Informs that execution of action was canceled
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleActionCanceled(string data) {
            GameManager.Instance.HandleActionCanceled();
        }

        /// <summary>
        /// Informs that action is being executed
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleActionExecution(string data) {
            IO.Swagger.Model.ActionExecutionEvent actionExecution = JsonConvert.DeserializeObject<IO.Swagger.Model.ActionExecutionEvent>(data);
            GameManager.Instance.HandleActionExecution(actionExecution.Data.ActionId);
        }

        /// <summary>
        /// Decodes project exception 
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleProjectException(string data) {
            IO.Swagger.Model.ProjectExceptionEvent projectException = JsonConvert.DeserializeObject<IO.Swagger.Model.ProjectExceptionEvent>(data);
            GameManager.Instance.HandleProjectException(projectException.Data);
        }

        /// <summary>
        /// Decodes package state
        /// </summary>
        /// <param name="obj"></param>
        private void HandlePackageState(string obj) {
            IO.Swagger.Model.PackageStateEvent projectState = JsonConvert.DeserializeObject<IO.Swagger.Model.PackageStateEvent>(obj);
            GameManager.Instance.PackageStateUpdated(projectState.Data);
        }

        /// <summary>
        /// Decodes package info
        /// </summary>
        /// <param name="obj">Message from server</param>
        private void HandlePackageInfo(string obj) {
            IO.Swagger.Model.PackageInfoEvent packageInfo = JsonConvert.DeserializeObject<IO.Swagger.Model.PackageInfoEvent>(obj);
            GameManager.Instance.PackageInfo = packageInfo.Data;
        }

        /// <summary>
        /// Decodes changes on scene and invoke proper callback
        /// </summary>
        /// <param name="obj">Message from server</param>
        private void HandleSceneChanged(string obj) {
            IO.Swagger.Model.SceneChanged sceneChangedEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.SceneChanged>(obj);
            switch (sceneChangedEvent.ChangeType) {
                case IO.Swagger.Model.SceneChanged.ChangeTypeEnum.Add:
                    throw new NotImplementedException("Scene add should never occure");
                case IO.Swagger.Model.SceneChanged.ChangeTypeEnum.Remove:
                    OnSceneRemoved?.Invoke(this, new StringEventArgs(sceneChangedEvent.Data.Id));
                    break;
                case IO.Swagger.Model.SceneChanged.ChangeTypeEnum.Update:
                    throw new NotImplementedException("Scene update should never occure");
                case IO.Swagger.Model.SceneChanged.ChangeTypeEnum.Updatebase:
                    OnSceneBaseUpdated?.Invoke(this, new BareSceneEventArgs(sceneChangedEvent.Data));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes on object types and invoke proper callback
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleChangedObjectTypesEvent(string data) {
            IO.Swagger.Model.ChangedObjectTypesEvent objectTypesChangedEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.ChangedObjectTypesEvent>(data);
            switch (objectTypesChangedEvent.ChangeType) {
                case IO.Swagger.Model.ChangedObjectTypesEvent.ChangeTypeEnum.Add:
                    foreach (ObjectTypeMeta type in objectTypesChangedEvent.Data)
                        ActionsManager.Instance.ObjectTypeAdded(type);
                    break;
                case IO.Swagger.Model.ChangedObjectTypesEvent.ChangeTypeEnum.Remove:
                    foreach (ObjectTypeMeta type in objectTypesChangedEvent.Data)
                        ActionsManager.Instance.ObjectTypeRemoved(type);
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes on actions and invokes proper callback 
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleActionChanged(string data) {
            IO.Swagger.Model.ActionChanged actionChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.ActionChanged>(data);
            var actionChangedFields = new {
                data = new IO.Swagger.Model.Action(id: "", name: "", type: "", flows: new List<Flow>(), parameters: new List<ActionParameter>())
            };
            ProjectManager.Instance.ProjectChanged = true;
            switch (actionChanged.ChangeType) {
                case IO.Swagger.Model.ActionChanged.ChangeTypeEnum.Add:
                    var action = JsonConvert.DeserializeAnonymousType(data, actionChangedFields);
                    ProjectManager.Instance.ActionAdded(action.data, actionChanged.ParentId);
                    break;
                case IO.Swagger.Model.ActionChanged.ChangeTypeEnum.Remove:
                    ProjectManager.Instance.ActionRemoved(actionChanged.Data);
                    break;
                case IO.Swagger.Model.ActionChanged.ChangeTypeEnum.Update:
                    var actionUpdate = JsonConvert.DeserializeAnonymousType(data, actionChangedFields);
                    ProjectManager.Instance.ActionUpdated(actionUpdate.data);
                    break;
                case IO.Swagger.Model.ActionChanged.ChangeTypeEnum.Updatebase:
                    ProjectManager.Instance.ActionBaseUpdated(actionChanged.Data);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes in program logic
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleLogicItemChanged(string data) {
            LogicItemChanged logicItemChanged = JsonConvert.DeserializeObject<LogicItemChanged>(data);
            ProjectManager.Instance.ProjectChanged = true;
            switch (logicItemChanged.ChangeType) {
                case LogicItemChanged.ChangeTypeEnum.Add:
                    OnLogicItemAdded?.Invoke(this, new LogicItemChangedEventArgs(logicItemChanged.Data));
                    break;
                case LogicItemChanged.ChangeTypeEnum.Remove:
                    OnLogicItemRemoved?.Invoke(this, new StringEventArgs(logicItemChanged.Data.Id));
                    break;
                case LogicItemChanged.ChangeTypeEnum.Update:
                    OnLogicItemUpdated?.Invoke(this, new LogicItemChangedEventArgs(logicItemChanged.Data));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes of action points
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleActionPointChanged(string data) {
            ProjectManager.Instance.ProjectChanged = true;
            IO.Swagger.Model.ActionPointChanged actionPointChangedEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.ActionPointChanged>(data);
            var actionPointChangedFields = new {
                data = new IO.Swagger.Model.ActionPoint(id: "", name: "string", parent: "", position: new Position(),
                    actions: new List<IO.Swagger.Model.Action>(), orientations: new List<NamedOrientation>(),
                    robotJoints: new List<ProjectRobotJoints>())
            };

            switch (actionPointChangedEvent.ChangeType) {
                case IO.Swagger.Model.ActionPointChanged.ChangeTypeEnum.Add:
                    var actionPoint = JsonConvert.DeserializeAnonymousType(data, actionPointChangedFields);
                    OnActionPointAdded?.Invoke(this, new ProjectActionPointEventArgs(actionPoint.data));
                    break;
                case IO.Swagger.Model.ActionPointChanged.ChangeTypeEnum.Remove:
                    OnActionPointRemoved?.Invoke(this, new StringEventArgs(actionPointChangedEvent.Data.Id));
                    break;
                case IO.Swagger.Model.ActionPointChanged.ChangeTypeEnum.Update:
                    var actionPointUpdate = JsonConvert.DeserializeAnonymousType(data, actionPointChangedFields);
                    OnActionPointUpdated?.Invoke(this, new ProjectActionPointEventArgs(actionPointUpdate.data));
                    break;
                case IO.Swagger.Model.ActionPointChanged.ChangeTypeEnum.Updatebase:
                    OnActionPointBaseUpdated?.Invoke(this, new BareActionPointEventArgs(actionPointChangedEvent.Data));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes on orientations
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleOrientationChanged(string data) {
            ProjectManager.Instance.ProjectChanged = true;
            IO.Swagger.Model.OrientationChanged orientationChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.OrientationChanged>(data);
            switch (orientationChanged.ChangeType) {
                case IO.Swagger.Model.OrientationChanged.ChangeTypeEnum.Add:
                    OnActionPointOrientationAdded?.Invoke(this, new ActionPointOrientationEventArgs(orientationChanged.Data, orientationChanged.ParentId));
                    break;
                case IO.Swagger.Model.OrientationChanged.ChangeTypeEnum.Remove:
                    OnActionPointOrientationRemoved?.Invoke(this, new StringEventArgs(orientationChanged.Data.Id));
                    break;
                case IO.Swagger.Model.OrientationChanged.ChangeTypeEnum.Update:
                    OnActionPointOrientationUpdated?.Invoke(this, new ActionPointOrientationEventArgs(orientationChanged.Data, null));
                    break;
                case IO.Swagger.Model.OrientationChanged.ChangeTypeEnum.Updatebase:
                    OnActionPointOrientationBaseUpdated?.Invoke(this, new ActionPointOrientationEventArgs(orientationChanged.Data, null));

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes on joints
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleJointsChanged(string data) {
            ProjectManager.Instance.ProjectChanged = true;
            IO.Swagger.Model.JointsChanged jointsChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.JointsChanged>(data);
            switch (jointsChanged.ChangeType) {
                case IO.Swagger.Model.JointsChanged.ChangeTypeEnum.Add:
                    OnActionPointJointsAdded?.Invoke(this, new RobotJointsEventArgs(jointsChanged.Data, jointsChanged.ParentId));
                    break;
                case IO.Swagger.Model.JointsChanged.ChangeTypeEnum.Remove:
                    OnActionPointJointsRemoved?.Invoke(this, new StringEventArgs(jointsChanged.Data.Id));
                    break;
                case IO.Swagger.Model.JointsChanged.ChangeTypeEnum.Update:
                    OnActionPointJointsUpdated?.Invoke(this, new RobotJointsEventArgs(jointsChanged.Data, ""));
                    break;
                case IO.Swagger.Model.JointsChanged.ChangeTypeEnum.Updatebase:
                    OnActionPointJointsBaseUpdated?.Invoke(this, new RobotJointsEventArgs(jointsChanged.Data, ""));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes changes on scene objects
        /// </summary>
        /// <param name="data">Message from server</param>
        /// <returns></returns>
        private async Task HandleSceneObjectChanged(string data) {
            IO.Swagger.Model.SceneObjectChanged sceneObjectChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.SceneObjectChanged>(data);
            switch (sceneObjectChanged.ChangeType) {
                case IO.Swagger.Model.SceneObjectChanged.ChangeTypeEnum.Add:
                    await SceneManager.Instance.SceneObjectAdded(sceneObjectChanged.Data);
                    break;
                case IO.Swagger.Model.SceneObjectChanged.ChangeTypeEnum.Remove:
                    SceneManager.Instance.SceneObjectRemoved(sceneObjectChanged.Data);
                    break;
                case IO.Swagger.Model.SceneObjectChanged.ChangeTypeEnum.Update:
                    SceneManager.Instance.SceneObjectUpdated(sceneObjectChanged.Data);
                    break;
                case IO.Swagger.Model.SceneObjectChanged.ChangeTypeEnum.Updatebase:
                    SceneManager.Instance.SceneObjectBaseUpdated(sceneObjectChanged.Data);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Invoked when openning of project is requested
        /// </summary>
        /// <param name="data">Message from server</param>
        private async void HandleOpenProject(string data) {
            IO.Swagger.Model.OpenProject openProjectEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.OpenProject>(data);
            GameManager.Instance.ProjectOpened(openProjectEvent.Data.Scene, openProjectEvent.Data.Project);
        }

        /// <summary>
        /// Invoked when openning of scene is requested
        /// </summary>
        /// <param name="data"Message from server></param>
        /// <returns></returns>
        private async Task HandleOpenScene(string data) {
            IO.Swagger.Model.OpenScene openSceneEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.OpenScene>(data);
            await GameManager.Instance.SceneOpened(openSceneEvent.Data.Scene);
        }

        /// <summary>
        /// Invoked when closing of project is requested
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleCloseProject(string data) {
            GameManager.Instance.ProjectClosed();
        }

        /// <summary>
        /// Invoked when closing of scene is requested
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleCloseScene(string data) {
            GameManager.Instance.SceneClosed();
        }

        /// <summary>
        /// Invoked when openning of package is requested
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleOpenPackage(string data) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when project was saved
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleProjectSaved(string data) {
            ProjectManager.Instance.ProjectSaved();
        }

        /// <summary>
        /// Invoked when scene was saved
        /// </summary>
        /// <param name="data">Message from server</param>
        private void HandleSceneSaved(string data) {
            SceneManager.Instance.SceneSaved();
        }

        /// <summary>
        /// Loads object types from server. Throws RequestFailedException when request failed
        /// </summary>
        /// <returns>List of object types</returns>
        public async Task<List<IO.Swagger.Model.ObjectTypeMeta>> GetObjectTypes() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.GetObjectTypesRequest(id: id, request: "GetObjectTypes").ToJson(), id, true);
            IO.Swagger.Model.GetObjectTypesResponse response = await WaitForResult<IO.Swagger.Model.GetObjectTypesResponse>(id);
            if (response != null && response.Result)
                return response.Data;
            else {
                throw new RequestFailedException("Failed to load object types");
            }

        }

        /// <summary>
        /// Loads actions for selected object type. Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="name">Object type</param>
        /// <returns>List of actions</returns>
        public async Task<List<IO.Swagger.Model.ObjectAction>> GetActions(string name) {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.GetActionsRequest(id: id, request: "GetActions", args: new IO.Swagger.Model.TypeArgs(type: name)).ToJson(), id, true);
            IO.Swagger.Model.GetActionsResponse response = await WaitForResult<IO.Swagger.Model.GetActionsResponse>(id);
            if (response != null && response.Result)
                return response.Data;
            else
                throw new RequestFailedException("Failed to load actions for object/service " + name);
        }

        /// <summary>
        /// Asks server to save currently openned scene
        /// </summary>
        /// <returns>Response form server</returns>
        public async Task<IO.Swagger.Model.SaveSceneResponse> SaveScene() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.SaveSceneRequest(id: id, request: "SaveScene").ToJson(), id, true);
            return await WaitForResult<IO.Swagger.Model.SaveSceneResponse>(id);
        }

        /// <summary>
        /// Asks server to save currently openned project
        /// </summary>
        /// <returns>Response form server</returns>
        public async Task<IO.Swagger.Model.SaveProjectResponse> SaveProject() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.SaveProjectRequest(id: id, request: "SaveProject").ToJson(), id, true);
            return await WaitForResult<IO.Swagger.Model.SaveProjectResponse>(id);
        }

        /// <summary>
        /// Asks server to open project. Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">Id of project</param>
        /// <returns></returns>
        public async Task OpenProject(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: id);
            IO.Swagger.Model.OpenProjectRequest request = new IO.Swagger.Model.OpenProjectRequest(id: r_id, request: "OpenProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.OpenProjectResponse response = await WaitForResult<IO.Swagger.Model.OpenProjectResponse>(r_id, 30000);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to run package. Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="packageId">Id of package</param>
        /// <param name="cleanupAfterRun"></param>
        /// <returns></returns>
        public async Task RunPackage(string packageId, bool cleanupAfterRun=true) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RunPackageArgs args = new IO.Swagger.Model.RunPackageArgs(id: packageId, cleanupAfterRun: cleanupAfterRun);
            IO.Swagger.Model.RunPackageRequest request = new IO.Swagger.Model.RunPackageRequest(id: r_id, request: "RunPackage", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RunPackageResponse response = await WaitForResult<IO.Swagger.Model.RunPackageResponse>(r_id, 30000);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to create and run temporary package. This package is not saved on execution unit and it is
        /// removed immideately after package execution. Throws RequestFailedException when request failed
        /// </summary>
        /// <returns></returns>
        public async Task TemporaryPackage() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.TemporaryPackageRequest request = new IO.Swagger.Model.TemporaryPackageRequest(id: r_id, request: "TemporaryPackage");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.TemporaryPackageResponse response = await WaitForResult<IO.Swagger.Model.TemporaryPackageResponse>(r_id, 30000);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to stop currently executed package. Throws RequestFailedException when request failed
        /// </summary>
        /// <returns></returns>
        public async Task StopPackage() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.StopPackageRequest request = new IO.Swagger.Model.StopPackageRequest(id: r_id, request: "StopPackage");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.StopPackageResponse response = await WaitForResult<IO.Swagger.Model.StopPackageResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to pause currently executed package. Throws RequestFailedException when request failed
        /// </summary>
        /// <returns></returns>
        public async Task PausePackage() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.PausePackageRequest request = new IO.Swagger.Model.PausePackageRequest(id: r_id, request: "PausePackage");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.PausePackageResponse response = await WaitForResult<IO.Swagger.Model.PausePackageResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to resume currently executed package. Throws RequestFailedException when request failed
        /// </summary>
        /// <returns></returns>
        public async Task ResumePackage() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ResumePackageRequest request = new IO.Swagger.Model.ResumePackageRequest(id: r_id, request: "ResumePackage");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ResumePackageResponse response = await WaitForResult<IO.Swagger.Model.ResumePackageResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Updates position of action point to match with selected robots end effector.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionPointId">ID of action point</param>
        /// <param name="robotId">Id of robot</param>
        /// <param name="endEffectorId">Id of end effector</param>
        /// <returns></returns>
        public async Task UpdateActionPointUsingRobot(string actionPointId, string robotId, string endEffectorId) {
            
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(robotId: robotId, endEffector: endEffectorId);
            IO.Swagger.Model.UpdateActionPointUsingRobotRequestArgs args = new IO.Swagger.Model.UpdateActionPointUsingRobotRequestArgs(actionPointId: actionPointId,
                robot: robotArg);
            IO.Swagger.Model.UpdateActionPointUsingRobotRequest request = new IO.Swagger.Model.UpdateActionPointUsingRobotRequest(id: r_id, request: "UpdateActionPointUsingRobot", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointUsingRobotResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointUsingRobotResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Updates position and orientation of action object.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionObjectId">Id of action object</param>
        /// <param name="pose">Desired pose (position and orientation)</param>
        /// <returns></returns>
        public async Task UpdateActionObjectPose(string actionObjectId, IO.Swagger.Model.Pose pose) {

            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateObjectPoseRequestArgs args = new IO.Swagger.Model.UpdateObjectPoseRequestArgs
                (objectId: actionObjectId, pose: pose);
            IO.Swagger.Model.UpdateObjectPoseRequest request = new IO.Swagger.Model.UpdateObjectPoseRequest
                (id: r_id, request: "UpdateObjectPose", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateObjectPoseResponse response = await WaitForResult<IO.Swagger.Model.UpdateObjectPoseResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);

        }

        /// <summary>
        /// Updates action object pose to match selected robots end effector.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionObjectId">Action object ID</param>
        /// <param name="robotId">ID of robot</param>
        /// <param name="endEffectorId">ID of end effector</param>
        /// <param name="pivot">Pivot point on the object. Enables to select relative
        /// point on object model to match end effector tip.</param>
        /// <returns></returns>
        public async Task UpdateActionObjectPoseUsingRobot(string actionObjectId, string robotId, string endEffectorId,
            IO.Swagger.Model.UpdateObjectPoseUsingRobotArgs.PivotEnum pivot) {
            
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(robotId: robotId, endEffector: endEffectorId);
            IO.Swagger.Model.UpdateObjectPoseUsingRobotArgs args = new IO.Swagger.Model.UpdateObjectPoseUsingRobotArgs
                (id: actionObjectId, robot: robotArg, pivot: pivot);
            IO.Swagger.Model.UpdateObjectPoseUsingRobotRequest request = new IO.Swagger.Model.UpdateObjectPoseUsingRobotRequest
                (id: r_id, request: "UpdateObjectPoseUsingRobot", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateObjectPoseUsingRobotResponse response = await WaitForResult<IO.Swagger.Model.UpdateObjectPoseUsingRobotResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
     
        }

        /// <summary>
        /// Asks server to create new object type. Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="objectType">Information about new object type (name, parent, model)</param>
        /// <param name="dryRun">If true, validates all parameters, but will not
        /// execute the requested action itself.</param>
        /// <returns></returns>
        public async Task CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.NewObjectTypeRequest request = new IO.Swagger.Model.NewObjectTypeRequest(id: r_id, request: "NewObjectType", args: objectType, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.NewObjectTypeResponse response = await WaitForResult<IO.Swagger.Model.NewObjectTypeResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Starts procedure of object aiming. Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="objectId">Action object ID</param>
        /// <param name="robotId">ID of robot</param>
        /// <param name="endEffector">ID of end effector</param>
        /// <returns></returns>
        public async Task StartObjectFocusing(string objectId, string robotId, string endEffector) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(endEffector, robotId);
            IO.Swagger.Model.FocusObjectStartRequestArgs args = new IO.Swagger.Model.FocusObjectStartRequestArgs(objectId: objectId, robot: robotArg);
            IO.Swagger.Model.FocusObjectStartRequest request = new IO.Swagger.Model.FocusObjectStartRequest(id: r_id, request: "FocusObjectStart", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectStartResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectStartResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Saves current position of robot end effector with selected index.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="objectId">Action object ID</param>
        /// <param name="pointIdx">ID of currently selected focus point</param>
        /// <returns></returns>
        public async Task SavePosition(string objectId, int pointIdx) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.FocusObjectRequestArgs args = new IO.Swagger.Model.FocusObjectRequestArgs(objectId: objectId, pointIdx: pointIdx);
            IO.Swagger.Model.FocusObjectRequest request = new IO.Swagger.Model.FocusObjectRequest(id: r_id, request: "FocusObject", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Called when all points are selected, asking server to compute pose of object.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="objectId">Action object ID</param>
        /// <returns></returns>
        public async Task FocusObjectDone(string objectId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: objectId);
            IO.Swagger.Model.FocusObjectDoneRequest request = new IO.Swagger.Model.FocusObjectDoneRequest(id: r_id, request: "FocusObjectDone", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectDoneResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectDoneResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Loads all scenes from server.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <returns>List of scenes metadata</returns>
        public async Task<List<IO.Swagger.Model.ListScenesResponseData>> LoadScenes() {
            int id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ListScenesRequest request = new IO.Swagger.Model.ListScenesRequest(id: id, request: "ListScenes");
            SendDataToServer(request.ToJson(), id, true);
            IO.Swagger.Model.ListScenesResponse response = await WaitForResult<IO.Swagger.Model.ListScenesResponse>(id);
            if (response == null)
                throw new RequestFailedException("Failed to load scenes");
            return response.Data;
        }

        /// <summary>
        /// Loads all projects from server.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <returns>List of projects metadata</returns>
        public async Task<List<IO.Swagger.Model.ListProjectsResponseData>> LoadProjects() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ListProjectsRequest request = new IO.Swagger.Model.ListProjectsRequest(id: r_id, request: "ListProjects");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ListProjectsResponse response = await WaitForResult<IO.Swagger.Model.ListProjectsResponse>(r_id);
            if (response == null)
                throw new RequestFailedException("Failed to load projects");
            return response.Data;
        }

        /// <summary>
        /// Loads all packages from server.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <returns>List of packages metadata</returns>
        public async Task<List<IO.Swagger.Model.PackageSummary>> LoadPackages() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ListPackagesRequest request = new IO.Swagger.Model.ListPackagesRequest(id: r_id, request: "ListPackages");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ListPackagesResponse response = await WaitForResult<IO.Swagger.Model.ListPackagesResponse>(r_id);
            if (response == null)
                throw new RequestFailedException("Failed to load packages");
            return response.Data;
        }

        /// <summary>
        /// Asks server to add object to scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="name">Human readable name of action object</param>
        /// <param name="type">Action object type</param>
        /// <param name="pose">Pose of new object</param>
        /// <param name="settings">List of settings of object</param>
        /// <returns></returns>
        public async Task AddObjectToScene(string name, string type, IO.Swagger.Model.Pose pose, List<IO.Swagger.Model.Parameter> settings) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddObjectToSceneRequestArgs args = new IO.Swagger.Model.AddObjectToSceneRequestArgs(pose: pose, type: type, name: name, settings: settings);
            IO.Swagger.Model.AddObjectToSceneRequest request = new IO.Swagger.Model.AddObjectToSceneRequest(id: r_id, request: "AddObjectToScene", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddObjectToSceneResponse response = await WaitForResult<IO.Swagger.Model.AddObjectToSceneResponse>(r_id, 30000);
            if (response == null || !response.Result) {
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            }
        }
        
        /// <summary>
        /// Asks server to remove object from scene.
        /// 
        /// </summary>
        /// <param name="id">ID of action object</param>
        /// <param name="force">Indicates whether or not it should be forced</param>
        /// <returns>Response from server</returns>
        public async Task<IO.Swagger.Model.RemoveFromSceneResponse> RemoveFromScene(string id, bool force) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RemoveFromSceneArgs args = new IO.Swagger.Model.RemoveFromSceneArgs(id: id, force: force);
            IO.Swagger.Model.RemoveFromSceneRequest request = new IO.Swagger.Model.RemoveFromSceneRequest(id: r_id, request: "RemoveFromScene", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            return await WaitForResult<IO.Swagger.Model.RemoveFromSceneResponse>(r_id);            
        }

        /// <summary>
        /// Asks server to remove object type
        /// </summary>
        /// <param name="type">Action object type</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task DeleteObjectType(string type, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: type);
            IO.Swagger.Model.DeleteObjectTypeRequest request = new IO.Swagger.Model.DeleteObjectTypeRequest(id: r_id, request: "DeleteObjectType", args: args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            RemoveFromSceneResponse response = await WaitForResult<IO.Swagger.Model.RemoveFromSceneResponse>(r_id);
            if (response == null || !response.Result) {
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            }
        }

        /// <summary>
        /// Asks server to open scene
        /// </summary>
        /// <param name="scene_id">Id of scene to be openned</param>
        /// <returns></returns>
        public async Task OpenScene(string scene_id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: scene_id);
            IO.Swagger.Model.OpenSceneRequest request = new IO.Swagger.Model.OpenSceneRequest(id: r_id, request: "OpenScene", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.OpenSceneResponse response = await WaitForResult<IO.Swagger.Model.OpenSceneResponse>(r_id, 30000);
            if (response == null || !response.Result) {
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            }
        }

        /// <summary>
        /// Gets available values for selected parameter.
        /// </summary>
        /// <param name="actionProviderId">ID of action provider (only action object at the moment"</param>
        /// <param name="param_id">ID of parameter</param>
        /// <param name="parent_params">List of parent parameters (e.g. to obtain list of available end effectors, robot_id has to be provided"</param>
        /// <returns>List of available options or empty list when request failed</returns>
        public async Task<List<string>> GetActionParamValues(string actionProviderId, string param_id, List<IO.Swagger.Model.IdValue> parent_params) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ActionParamValuesArgs args = new IO.Swagger.Model.ActionParamValuesArgs(id: actionProviderId, paramId: param_id, parentParams: parent_params);
            IO.Swagger.Model.ActionParamValuesRequest request = new IO.Swagger.Model.ActionParamValuesRequest(id: r_id, request: "ActionParamValues", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ActionParamValuesResponse response = await WaitForResult<IO.Swagger.Model.ActionParamValuesResponse>(r_id);
            if (response != null && response.Result)
                return response.Data;
            else
                return new List<string>();
        }

        /// <summary>
        /// Asks server to execute selected action.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionId">ID of action</param>
        /// <returns></returns>
        public async Task ExecuteAction(string actionId) {
            Debug.Assert(actionId != null);
            Debug.Assert(actionId != "");

            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ExecuteActionArgs args = new IO.Swagger.Model.ExecuteActionArgs(actionId: actionId);
            IO.Swagger.Model.ExecuteActionRequest request = new IO.Swagger.Model.ExecuteActionRequest(id: r_id, request: "ExecuteAction", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ExecuteActionResponse response = await WaitForResult<IO.Swagger.Model.ExecuteActionResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to cancel execution of action.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <returns></returns>
        public async Task CancelExecution() {
            int r_id = Interlocked.Increment(ref requestID);

            IO.Swagger.Model.CancelActionRequest request = new IO.Swagger.Model.CancelActionRequest(id: r_id, request: "CancelAction");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.CancelActionResponse response = await WaitForResult<IO.Swagger.Model.CancelActionResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Gets information about server (server version, api version, list of supported parameters and list of available RPCs).
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <returns>Information about server</returns>
        public async Task<IO.Swagger.Model.SystemInfoData> GetSystemInfo() {
             int r_id = Interlocked.Increment(ref requestID);

             IO.Swagger.Model.SystemInfoRequest request = new IO.Swagger.Model.SystemInfoRequest(id: r_id, request: "SystemInfo");
             SendDataToServer(request.ToJson(), r_id, true);
             IO.Swagger.Model.SystemInfoResponse response = await WaitForResult<IO.Swagger.Model.SystemInfoResponse>(r_id);
             if (response == null || !response.Result)
                 throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
             return response.Data;
         }

        /// <summary>
        /// Asks server to build package from project.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="projectId">UUID of project</param>
        /// <param name="packageName">Human readable name of package</param>
        /// <returns>UUID of created package</returns>
        public async Task<string> BuildPackage(string projectId, string packageName) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.BuildProjectArgs args = new IO.Swagger.Model.BuildProjectArgs(projectId: projectId, packageName: packageName);
            IO.Swagger.Model.BuildProjectRequest request = new IO.Swagger.Model.BuildProjectRequest(id: r_id, request: "BuildProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.BuildProjectResponse response = await WaitForResult<IO.Swagger.Model.BuildProjectResponse>(r_id, 30000);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            else
                return response.Data.PackageId;
        }

        /// <summary>
        /// Asks server to create new scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="name">Human readable name of scene.</param>
        /// <param name="description">Description of scene</param>
        /// <returns></returns>
        public async Task CreateScene(string name, string description) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.NewSceneRequestArgs args = new IO.Swagger.Model.NewSceneRequestArgs(name: name, desc: description);
            IO.Swagger.Model.NewSceneRequest request = new IO.Swagger.Model.NewSceneRequest(r_id, "NewScene", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.NewSceneResponse response = await WaitForResult<IO.Swagger.Model.NewSceneResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of scene</param>
        /// <returns></returns>
        internal async Task RemoveScene(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: id);
            IO.Swagger.Model.DeleteSceneRequest request = new IO.Swagger.Model.DeleteSceneRequest(r_id, "DeleteScene", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.DeleteSceneResponse response = await WaitForResult<IO.Swagger.Model.DeleteSceneResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of scene</param>
        /// <param name="newName">New human readable name of scene</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task RenameScene(string id, string newName, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameArgs args = new IO.Swagger.Model.RenameArgs(id: id, newName: newName);
            IO.Swagger.Model.RenameSceneRequest request = new IO.Swagger.Model.RenameSceneRequest(r_id, "RenameScene", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameSceneResponse response = await WaitForResult<IO.Swagger.Model.RenameSceneResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename action object.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action object</param>
        /// <param name="newName">New human readable name of action objects</param>
        /// <returns></returns>
        public async Task RenameObject(string id, string newName) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameArgs args = new IO.Swagger.Model.RenameArgs(id: id, newName: newName);
            IO.Swagger.Model.RenameObjectRequest request = new IO.Swagger.Model.RenameObjectRequest(r_id, "RenameObject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameObjectResponse response = await WaitForResult<IO.Swagger.Model.RenameObjectResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to close opened scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="force">Indicates whether the scene should be closed even when it has unsaved changes.</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task CloseScene(bool force, bool dryRun = false) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.CloseSceneRequestArgs args = new IO.Swagger.Model.CloseSceneRequestArgs(force);
            IO.Swagger.Model.CloseSceneRequest request = new IO.Swagger.Model.CloseSceneRequest(r_id, "CloseScene", args, dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.CloseSceneResponse response = await WaitForResult<IO.Swagger.Model.CloseSceneResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Gets list of projects belongings to selected scene.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="sceneId">Id of scene</param>
        /// <returns>List of project UUIDs</returns>
        public async Task<List<string>> GetProjectsWithScene(string sceneId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(sceneId);
            IO.Swagger.Model.ProjectsWithSceneRequest request = new IO.Swagger.Model.ProjectsWithSceneRequest(r_id, "ProjectsWithScene", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ProjectsWithSceneResponse response = await WaitForResult<IO.Swagger.Model.ProjectsWithSceneResponse>(r_id);
            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            return response.Data;
        }

        /// <summary>
        /// Asks server to create project.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="name">Human readable name of project</param>
        /// <param name="sceneId">UUID of scene</param>
        /// <param name="description">Description of project</param>
        /// <param name="hasLogic">Flags indicating if project specifies logical flow of thr program.</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task CreateProject(string name, string sceneId, string description, bool hasLogic, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.NewProjectRequestArgs args = new IO.Swagger.Model.NewProjectRequestArgs(name: name, sceneId: sceneId, desc: description, hasLogic: hasLogic);
            IO.Swagger.Model.NewProjectRequest request = new IO.Swagger.Model.NewProjectRequest(r_id, "NewProject", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.NewProjectResponse response = await WaitForResult<IO.Swagger.Model.NewProjectResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove project.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of project</param>
        /// <returns></returns>
        internal async Task RemoveProject(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: id);
            IO.Swagger.Model.DeleteProjectRequest request = new IO.Swagger.Model.DeleteProjectRequest(r_id, "DeleteProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.DeleteProjectResponse response = await WaitForResult<IO.Swagger.Model.DeleteProjectResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove package.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task RemovePackage(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: id);
            IO.Swagger.Model.DeletePackageRequest request = new IO.Swagger.Model.DeletePackageRequest(r_id, "DeletePackage", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.DeletePackageResponse response = await WaitForResult<IO.Swagger.Model.DeletePackageResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to add new action point.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="name">Human readable name of action point</param>
        /// <param name="parent">UUID of action point parent. Null if action point should be global.</param>
        /// <param name="position">Offset from parent (or scene origin for global AP)</param>
        /// <returns></returns>
        public async Task AddActionPoint(string name, string parent, IO.Swagger.Model.Position position) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddActionPointArgs args = new IO.Swagger.Model.AddActionPointArgs(parent: parent, position: position, name: name);
            IO.Swagger.Model.AddActionPointRequest request = new IO.Swagger.Model.AddActionPointRequest(r_id, "AddActionPoint", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddActionPointResponse response = await WaitForResult<IO.Swagger.Model.AddActionPointResponse>(r_id);

            if (response == null || !response.Result)   
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to update action point position.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="position">New position of action point.</param>
        /// <returns></returns>
        public async Task UpdateActionPointPosition(string id, IO.Swagger.Model.Position position) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionPointPositionRequestArgs args = new IO.Swagger.Model.UpdateActionPointPositionRequestArgs(actionPointId: id, newPosition: position);
            IO.Swagger.Model.UpdateActionPointPositionRequest request = new IO.Swagger.Model.UpdateActionPointPositionRequest(r_id, "UpdateActionPointPosition", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointPositionResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointPositionResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to change parent of action point.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="parentId">UUID of parent object (null if AP should be global)</param>
        /// <returns></returns>
        public async Task UpdateActionPointParent(string id, string parentId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionPointParentRequestArgs args = new IO.Swagger.Model.UpdateActionPointParentRequestArgs(actionPointId: id, newParentId: parentId);
            IO.Swagger.Model.UpdateActionPointParentRequest request = new IO.Swagger.Model.UpdateActionPointParentRequest(r_id, "UpdateActionPointParent", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointParentResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointParentResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename aciton point.
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="name">New human readable name of action point</param>
        /// <returns></returns>
        public async Task RenameActionPoint(string id, string name) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameActionPointRequestArgs args = new IO.Swagger.Model.RenameActionPointRequestArgs(actionPointId: id, newName: name);
            IO.Swagger.Model.RenameActionPointRequest request = new IO.Swagger.Model.RenameActionPointRequest(r_id, "RenameActionPoint", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameActionPointResponse response = await WaitForResult<IO.Swagger.Model.RenameActionPointResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to create new orientation for action point.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="orientation">Orientation</param>
        /// <param name="name">Human readable name of orientation</param>
        /// <returns></returns>
        public async Task AddActionPointOrientation(string id, IO.Swagger.Model.Orientation orientation, string name) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddActionPointOrientationRequestArgs args = new IO.Swagger.Model.AddActionPointOrientationRequestArgs(actionPointId: id, orientation: orientation, name: name);
            IO.Swagger.Model.AddActionPointOrientationRequest request = new IO.Swagger.Model.AddActionPointOrientationRequest(r_id, "AddActionPointOrientation", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddActionPointOrientationResponse response = await WaitForResult<IO.Swagger.Model.AddActionPointOrientationResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove action point orientation.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionPointId">UUID of action point</param>
        /// <param name="orientationId">UUID of orientation</param>
        /// <returns></returns>
        public async Task RemoveActionPointOrientation(string orientationId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RemoveActionPointOrientationRequestArgs args = new IO.Swagger.Model.RemoveActionPointOrientationRequestArgs(orientationId: orientationId);
            IO.Swagger.Model.RemoveActionPointOrientationRequest request = new IO.Swagger.Model.RemoveActionPointOrientationRequest(r_id, "RemoveActionPointOrientation", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RemoveActionPointOrientationResponse response = await WaitForResult<IO.Swagger.Model.RemoveActionPointOrientationResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }



        /// <summary>
        /// Asks server to update action point orientation.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="orientation">New orientation</param>
        /// <param name="orientationId">UUID of orientation</param>
        /// <returns></returns>
        public async Task UpdateActionPointOrientation(IO.Swagger.Model.Orientation orientation, string orientationId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionPointOrientationRequestArgs args = new IO.Swagger.Model.UpdateActionPointOrientationRequestArgs(orientation: orientation, orientationId: orientationId);
            IO.Swagger.Model.UpdateActionPointOrientationRequest request = new IO.Swagger.Model.UpdateActionPointOrientationRequest(r_id, "UpdateActionPointOrientation", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointOrientationResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointOrientationResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to add action point orientation using robot end effector.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="robotId">ID of robot</param>
        /// <param name="endEffector">ID of end effector</param>
        /// <param name="name">Human readable name of orientation</param>
        /// <returns></returns>
        public async Task AddActionPointOrientationUsingRobot(string id, string robotId, string endEffector, string name) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(endEffector, robotId);
            IO.Swagger.Model.AddActionPointOrientationUsingRobotRequestArgs args = new IO.Swagger.Model.AddActionPointOrientationUsingRobotRequestArgs(actionPointId: id, robot: robotArg, name: name);
            IO.Swagger.Model.AddActionPointOrientationUsingRobotRequest request = new IO.Swagger.Model.AddActionPointOrientationUsingRobotRequest(r_id, "AddActionPointOrientationUsingRobot", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddActionPointOrientationUsingRobotResponse response = await WaitForResult<IO.Swagger.Model.AddActionPointOrientationUsingRobotResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to update action point orientation using robots end effector.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="robotId">ID of robot</param>
        /// <param name="endEffector">ID of end effector</param>
        /// <param name="orientationId">UUID of orientation</param>
        /// <returns></returns>
        public async Task UpdateActionPointOrientationUsingRobot(string id, string robotId, string endEffector, string orientationId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(robotId: robotId, endEffector: endEffector);
            IO.Swagger.Model.UpdateActionPointOrientationUsingRobotRequestArgs args = new IO.Swagger.Model.UpdateActionPointOrientationUsingRobotRequestArgs(actionPointId: id, robot: robotArg, orientationId: orientationId);
            IO.Swagger.Model.UpdateActionPointOrientationUsingRobotRequest request = new IO.Swagger.Model.UpdateActionPointOrientationUsingRobotRequest(r_id, "UpdateActionPointOrientationUsingRobot", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointOrientationUsingRobotResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointOrientationUsingRobotResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to add action point joints
        /// </summary>
        /// <param name="id">UUID of action point</param>
        /// <param name="robotId">ID of robot</param>
        /// <param name="name">Human readable name of joints</param>
        /// <returns></returns>
        public async Task AddActionPointJoints(string id, string robotId, string name) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddActionPointJointsRequestArgs args = new IO.Swagger.Model.AddActionPointJointsRequestArgs(actionPointId: id, robotId: robotId, name: name);
            IO.Swagger.Model.AddActionPointJointsRequest request = new IO.Swagger.Model.AddActionPointJointsRequest(r_id, "AddActionPointJoints", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.AddActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to update action point joints using robot.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="robotId">ID of robot</param>
        /// <param name="jointsId">Human readable name of action point joints</param>
        /// <returns></returns>
        public async Task UpdateActionPointJoints(string robotId, string jointsId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionPointJointsRequestArgs args = new IO.Swagger.Model.UpdateActionPointJointsRequestArgs(robotId: robotId, jointsId: jointsId);
            IO.Swagger.Model.UpdateActionPointJointsRequest request = new IO.Swagger.Model.UpdateActionPointJointsRequest(r_id, "UpdateActionPointJoints", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename action point joints
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="jointsId">Id of joints</param>
        /// <param name="newName">New human-readable name</param>
        /// <returns></returns>
        public async Task RenameActionPointJoints(string jointsId, string newName) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameActionPointJointsRequestArgs args = new IO.Swagger.Model.RenameActionPointJointsRequestArgs(newName: newName, jointsId: jointsId);
            IO.Swagger.Model.RenameActionPointJointsRequest request = new IO.Swagger.Model.RenameActionPointJointsRequest(r_id, "RenameActionPointJoints", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.RenameActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename action point orientation
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="orientationId">Id of orientation</param>
        /// <param name="newName">New human-readable name</param>
        /// <returns></returns>
        public async Task RenameActionPointOrientation(string orientationId, string newName) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameActionPointOrientationRequestArgs args = new IO.Swagger.Model.RenameActionPointOrientationRequestArgs(newName: newName, orientationId: orientationId);
            IO.Swagger.Model.RenameActionPointOrientationRequest request = new IO.Swagger.Model.RenameActionPointOrientationRequest(r_id, "RenameActionPointOrientation", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.RenameActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        
        /// <summary>
        /// Asks server to move selected robot to action point, using joints
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="robotId">Id of robot</param>
        /// <param name="speed">Speed of movement in interval 0..1</param>
        /// <param name="jointsId">ID of joints on selected action point</param>
        /// <returns></returns>
        public async Task MoveToActionPointJoints(string robotId, decimal speed, string jointsId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.MoveToActionPointArgs args = new IO.Swagger.Model.MoveToActionPointArgs(robotId: robotId, endEffectorId: null, speed: speed, orientationId: null, jointsId: jointsId);
            IO.Swagger.Model.MoveToActionPointRequest request = new IO.Swagger.Model.MoveToActionPointRequest(r_id, "MoveToActionPoint", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.RenameActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to move selected robot to action point, using orientation
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="robotId">Id of robot</param>
        /// <param name="endEffectorId">Id of end effector<param>
        /// <param name="speed">Speed of movement in interval 0..1</param>
        /// <param name="orientationId">ID of orientation on selected action point</param>
        /// <returns></returns>
        public async Task MoveToActionPointOrientation(string robotId, string endEffectorId, decimal speed, string orientationId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.MoveToActionPointArgs args = new IO.Swagger.Model.MoveToActionPointArgs(robotId: robotId, endEffectorId: endEffectorId, speed: speed, orientationId: orientationId, jointsId: null);
            IO.Swagger.Model.MoveToActionPointRequest request = new IO.Swagger.Model.MoveToActionPointRequest(r_id, "MoveToActionPoint", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.MoveToActionPointResponse response = await WaitForResult<IO.Swagger.Model.MoveToActionPointResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to move selected robot to desired pose
        /// </summary>
        /// <param name="robotId">Id of robot</param>
        /// <param name="endEffectorId">Id of end effector</param>
        /// <param name="speed">Speed of movement in interval 0..1</param>
        /// <param name="position">Position in world frame</param>
        /// <param name="orientation">Orientation in world frame</param>
        /// <returns></returns>
        public async Task MoveToPose(string robotId, string endEffectorId, decimal speed, Position position, Orientation orientation) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.MoveToPoseArgs args = new IO.Swagger.Model.MoveToPoseArgs(robotId: robotId, endEffectorId: endEffectorId, speed: speed, orientation: orientation, position: position);
            IO.Swagger.Model.MoveToPoseRequest request = new IO.Swagger.Model.MoveToPoseRequest(r_id, "MoveToPose", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.MoveToPoseResponse response = await WaitForResult<IO.Swagger.Model.MoveToPoseResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove action point joints.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="jointsId">UUID of joints</param>
        /// <returns></returns>
        public async Task RemoveActionPointJoints(string jointsId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RemoveActionPointJointsRequestArgs args = new IO.Swagger.Model.RemoveActionPointJointsRequestArgs(jointsId: jointsId);
            IO.Swagger.Model.RemoveActionPointJointsRequest request = new IO.Swagger.Model.RemoveActionPointJointsRequest(r_id, "RemoveActionPointJoints", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RemoveActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.RemoveActionPointJointsResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to add new action.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionPointId">UUID of action point to which action should be added</param>
        /// <param name="actionParameters">Parameters of action</param>
        /// <param name="type">Type of action</param>
        /// <param name="name">Human readable name of action</param>
        /// <param name="flows">List of logical flows from action</param>
        /// <returns></returns>
        public async Task AddAction(string actionPointId, List<IO.Swagger.Model.ActionParameter> actionParameters, string type, string name, List<Flow> flows) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddActionRequestArgs args = new IO.Swagger.Model.AddActionRequestArgs(actionPointId: actionPointId, parameters: actionParameters, type: type, name: name, flows: flows);
            IO.Swagger.Model.AddActionRequest request = new IO.Swagger.Model.AddActionRequest(r_id, "AddAction", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddActionResponse response = await WaitForResult<IO.Swagger.Model.AddActionResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to update action parameters and flows.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionId">UUID of action</param>
        /// <param name="actionParameters">New values of action parameters</param>
        /// <param name="flows">New values of logical flows</param>
        /// <returns></returns>
        public async Task UpdateAction(string actionId, List<IO.Swagger.Model.ActionParameter> actionParameters, List<Flow> flows) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionRequestArgs args = new IO.Swagger.Model.UpdateActionRequestArgs(actionId: actionId, parameters: actionParameters, flows: flows);
            IO.Swagger.Model.UpdateActionRequest request = new IO.Swagger.Model.UpdateActionRequest(r_id, "UpdateAction", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove action.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionId">UUID of action</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task RemoveAction(string actionId, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: actionId);
            IO.Swagger.Model.RemoveActionRequest request = new IO.Swagger.Model.RemoveActionRequest(r_id, "RemoveAction", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RemoveActionResponse response = await WaitForResult<IO.Swagger.Model.RemoveActionResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename action.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionId">UUID of action</param>
        /// <param name="newName">New human readable name of action.</param>
        /// <returns></returns>
        public async Task RenameAction(string actionId, string newName) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameActionRequestArgs args = new IO.Swagger.Model.RenameActionRequestArgs(actionId: actionId, newName: newName);
            IO.Swagger.Model.RenameActionRequest request = new IO.Swagger.Model.RenameActionRequest(r_id, "RenameAction", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameActionResponse response = await WaitForResult<IO.Swagger.Model.RenameActionResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to add new logic item (actions connection).
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="startActionId">UUID of first action (from)</param>
        /// <param name="endActionId">UUID of second action (to)</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task AddLogicItem(string startActionId, string endActionId, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddLogicItemArgs args = new IO.Swagger.Model.AddLogicItemArgs(start: startActionId, end: endActionId);
            IO.Swagger.Model.AddLogicItemRequest request = new IO.Swagger.Model.AddLogicItemRequest(r_id, "AddLogicItem", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddLogicItemResponse response = await WaitForResult<IO.Swagger.Model.AddLogicItemResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to update logic item (e.g. change connection between actions).
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="logicItemId">UUID of logic item</param>
        /// <param name="startActionId">UUID of first action (from)</param>
        /// <param name="endActionId">UUID of second action (to)</param>
        /// <returns></returns>
        public async Task UpdateLogicItem(string logicItemId, string startActionId, string endActionId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateLogicItemArgs args = new IO.Swagger.Model.UpdateLogicItemArgs(start: startActionId, end: endActionId, logicItemId: logicItemId);
            IO.Swagger.Model.UpdateLogicItemRequest request = new IO.Swagger.Model.UpdateLogicItemRequest(r_id, "UpdateLogicItem", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateLogicItemResponse response = await WaitForResult<IO.Swagger.Model.UpdateLogicItemResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove logic item (destroy connection of actions).
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="logicItemId">UUID of connection.</param>
        /// <returns></returns>
        public async Task RemoveLogicItem(string logicItemId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RemoveLogicItemArgs args = new IO.Swagger.Model.RemoveLogicItemArgs(logicItemId: logicItemId);
            IO.Swagger.Model.RemoveLogicItemRequest request = new IO.Swagger.Model.RemoveLogicItemRequest(r_id, "RemoveLogicItem", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RemoveLogicItemResponse response = await WaitForResult<IO.Swagger.Model.RemoveLogicItemResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to rename project.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="projectId">UUID of project</param>
        /// <param name="newName">New human readable name of project</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task RenameProject(string projectId, string newName, bool dryRun = false) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenameProjectRequestArgs args = new IO.Swagger.Model.RenameProjectRequestArgs(projectId: projectId, newName: newName);
            IO.Swagger.Model.RenameProjectRequest request = new IO.Swagger.Model.RenameProjectRequest(r_id, "RenameProject", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenameProjectResponse response = await WaitForResult<IO.Swagger.Model.RenameProjectResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);

        }

        /// <summary>
        /// Asks server to rename package
        /// </summary>
        /// <param name="packageId">UUID of package</param>
        /// <param name="newName">New human readable name of package</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task RenamePackage(string packageId, string newName, bool dryRun) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RenamePackageArgs args = new IO.Swagger.Model.RenamePackageArgs(packageId: packageId, newName: newName);
            IO.Swagger.Model.RenamePackageRequest request = new IO.Swagger.Model.RenamePackageRequest(r_id, "RenamePackage", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RenamePackageResponse response = await WaitForResult<IO.Swagger.Model.RenamePackageResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Asks server to remove action point.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="actionPointId">UUID of aciton point</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task RemoveActionPoint(string actionPointId, bool dryRun = false) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(actionPointId);
            IO.Swagger.Model.RemoveActionPointRequest request = new IO.Swagger.Model.RemoveActionPointRequest(r_id, "RemoveActionPoint", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RemoveActionPointResponse response = await WaitForResult<IO.Swagger.Model.RemoveActionPointResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);

        }

        /// <summary>
        /// Asks server to close currently opened project.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="force">Indicates if it should be closed even with unsaved changes.</param>
        /// <param name="dryRun">If true, validates all parameters, but will not execute requested action itself.</param>
        /// <returns></returns>
        public async Task CloseProject(bool force, bool dryRun = false) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.CloseProjectRequestArgs args = new IO.Swagger.Model.CloseProjectRequestArgs(force);
            IO.Swagger.Model.CloseProjectRequest request = new IO.Swagger.Model.CloseProjectRequest(r_id, "CloseProject", args, dryRun: dryRun);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.CloseProjectResponse response = await WaitForResult<IO.Swagger.Model.CloseProjectResponse>(r_id);

            if (response == null || !response.Result)
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
        }

        /// <summary>
        /// Gets current pose of selected end effector.
        /// Throws RequestFailedException when request failed
        /// </summary>
        /// <param name="robotId">ID of robot</param>
        /// <param name="endeffectorId">ID of end effector</param>
        /// <returns></returns>
        public async Task<IO.Swagger.Model.Pose> GetEndEffectorPose(string robotId, string endeffectorId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.GetEndEffectorPoseArgs args = new IO.Swagger.Model.GetEndEffectorPoseArgs(robotId: robotId, endEffectorId: endeffectorId);
            IO.Swagger.Model.GetEndEffectorPoseRequest request = new IO.Swagger.Model.GetEndEffectorPoseRequest(r_id, "GetEndEffectorPose", args);
            SendDataToServer(request.ToJson(), r_id, true, false);
            IO.Swagger.Model.GetEndEffectorPoseResponse response = await WaitForResult<IO.Swagger.Model.GetEndEffectorPoseResponse>(r_id);
            if (response != null && response.Result) {
                return response.Data;
            } else {
                throw new RequestFailedException(response == null ? "Request timed out" : response.Messages[0]);
            }
        }

        /// <summary>
        /// Register or unregister to/from subsription of robots joints or end effectors pose.
        /// </summary>
        /// <param name="robotId">ID of robot</param>
        /// <param name="send">To subscribe or to unsubscribe</param>
        /// <param name="what">Pose of end effectors or joints</param>
        /// <returns>True if request successfull, false otherwise</returns>
        public async Task<bool> RegisterForRobotEvent(string robotId, bool send, IO.Swagger.Model.RegisterForRobotEventArgs.WhatEnum what) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RegisterForRobotEventArgs args = new IO.Swagger.Model.RegisterForRobotEventArgs(robotId: robotId, send: send, what: what);
            IO.Swagger.Model.RegisterForRobotEventRequest request = new IO.Swagger.Model.RegisterForRobotEventRequest(r_id, "RegisterForRobotEvent", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RegisterForRobotEventResponse response = await WaitForResult<IO.Swagger.Model.RegisterForRobotEventResponse>(r_id);
            
            // TODO: is this correct?
            return response == null ? false : response.Result;
        }

        /// <summary>
        /// Gets metadata about robots
        /// </summary>
        /// <returns>List of metadatas of robots</returns>
        public async Task<List<IO.Swagger.Model.RobotMeta>> GetRobotMeta() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.GetRobotMetaRequest request = new IO.Swagger.Model.GetRobotMetaRequest(r_id, "GetRobotMeta");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.GetRobotMetaResponse response = await WaitForResult<IO.Swagger.Model.GetRobotMetaResponse>(r_id);
            if (response == null || !response.Result) {
                throw new RequestFailedException(response == null ? new List<string>() { "Failed to get robot meta" } : response.Messages);
            } else {
                return response.Data;
            }
        }
    }

    
}
