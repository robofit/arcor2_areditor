using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using System.IO;


namespace Base {
    public class WebsocketManager : Singleton<WebsocketManager> {
        public string APIDomainWS = "";

        private ClientWebSocket clientWebSocket;

        private Queue<KeyValuePair<int, string>> sendingQueue = new Queue<KeyValuePair<int, string>>();

        private bool waitingForMessage = false;

        private string receivedData;

        private bool readyToSend, ignoreProjectChanged, connecting;

        private Dictionary<int, string> responses = new Dictionary<int, string>();

        private int requestID = 1;
        
        private bool projectArrived = false, sceneArrived = false, projectStateArrived = false;

        private void Awake() {
            waitingForMessage = false;
            readyToSend = true;
            ignoreProjectChanged = false;
            connecting = false;            
            receivedData = "";
        }


        public async Task<bool> ConnectToServer(string domain, int port) {
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Connecting;
            projectArrived = false;
            sceneArrived = false;
            projectStateArrived = false;
            connecting = true;
            APIDomainWS = GetWSURI(domain, port);
            clientWebSocket = new ClientWebSocket();
            Debug.Log("[WS]:Attempting connection.");
            try {
                Uri uri = new Uri(APIDomainWS);
                await clientWebSocket.ConnectAsync(uri, CancellationToken.None);

                Debug.Log("[WS][connect]:" + "Connected");
            } catch (Exception e) {
                Debug.Log("[WS][exception]:" + e.Message);
                if (e.InnerException != null) {
                    Debug.Log("[WS][inner exception]:" + e.InnerException.Message);
                }
            }

            connecting = false;
            
            return clientWebSocket.State == WebSocketState.Open;
        }

        async public void DisconnectFromSever() {
            Debug.Log("Disconnecting");
            GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            try {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            } catch (WebSocketException e) {
                //already closed probably..
            }
            clientWebSocket = null;
        }

        /// <summary>
        /// Waits until all post-connection data arrived from server or until timeout exprires
        /// </summary>
        /// <param name="timeout">Timeout in ms</param>
        public async Task WaitForInitData(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!CheckInitData()) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(100);
            }
            return;
        }

        public bool CheckInitData() {
            return projectArrived && sceneArrived && projectStateArrived;
        }

        // Update is called once per frame
        private async void Update() {
            if (clientWebSocket == null)
                return;
            if (clientWebSocket.State != WebSocketState.Open && GameManager.Instance.ConnectionStatus == GameManager.ConnectionStatusEnum.Connected) {
                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            }

            if (!waitingForMessage && clientWebSocket.State == WebSocketState.Open) {
                WebSocketReceiveResult result = null;
                waitingForMessage = true;
                ArraySegment<byte> bytesReceived = WebSocket.CreateClientBuffer(8192, 8192);
                MemoryStream ms = new MemoryStream();
                try {
                    do {
                        result = await clientWebSocket.ReceiveAsync(
                            bytesReceived,
                            CancellationToken.None
                        );

                        if (bytesReceived.Array != null)
                            ms.Write(bytesReceived.Array, bytesReceived.Offset, result.Count);

                    } while (!result.EndOfMessage);
                } catch (WebSocketException e) {
                    DisconnectFromSever();
                    return;
                }
                
                receivedData = Encoding.Default.GetString(ms.ToArray());
                HandleReceivedData(receivedData);
                receivedData = "";
                waitingForMessage = false;

            }

            if (sendingQueue.Count > 0 && readyToSend) {
                SendDataToServer();
            }

        }

        public string GetWSURI(string domain, int port) {
            return "ws://" + domain + ":" + port.ToString();
        }

        void OnApplicationQuit() {
            DisconnectFromSever();
        }

        public void SendDataToServer(string data, int key = -1, bool storeResult = false) {
            if (key < 0) {
                key = Interlocked.Increment(ref requestID);
            }
            Debug.Log("Sending data to server: " + data);

            if (storeResult) {
                responses[key] = null;
            }
            sendingQueue.Enqueue(new KeyValuePair<int, string>(key, data));
        }

        async public void SendDataToServer() {
            if (sendingQueue.Count == 0)
                return;
            KeyValuePair<int, string> keyVal = sendingQueue.Dequeue();
            readyToSend = false;
            if (clientWebSocket.State != WebSocketState.Open)
                return;

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(
                         Encoding.UTF8.GetBytes(keyVal.Value)
                     );
            await clientWebSocket.SendAsync(
                bytesToSend,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            readyToSend = true;
        }

        public void UpdateObjectTypes() {
            SendDataToServer(new IO.Swagger.Model.GetObjectTypesRequest(request: "GetObjectTypes").ToJson());
        }

        public void UpdateObjectActions(string ObjectId) {
            SendDataToServer(new IO.Swagger.Model.GetActionsRequest(request: "GetActions", args: new IO.Swagger.Model.TypeArgs(type: ObjectId)).ToJson());
        }

        public void UpdateScene(IO.Swagger.Model.Scene scene) {
            //ARServer.Models.EventSceneChanged eventData = new ARServer.Models.EventSceneChanged();
            IO.Swagger.Model.SceneChangedEvent eventData = new IO.Swagger.Model.SceneChangedEvent {
                Event = "SceneChanged",
                
            };
            if (scene != null) {
                eventData.Data = scene;
            }
            SendDataToServer(eventData.ToJson());
        }

        // TODO: add action parameters
        public void UpdateProject(IO.Swagger.Model.Project project) {
            IO.Swagger.Model.ProjectChangedEvent eventData = new IO.Swagger.Model.ProjectChangedEvent {
                Event = "ProjectChanged"
            };
            if (project != null) {
                eventData.Data = project;
            }
            SendDataToServer(eventData.ToJson());

        }


        private void HandleReceivedData(string data) {
            var dispatchType = new {
                id = 0,
                response = "",
                @event = "",
                request = ""
            };
            
            var dispatch = JsonConvert.DeserializeAnonymousType(data, dispatchType);

            if (dispatch?.response == null && dispatch?.request == null && dispatch?.@event == null)
                return;
            //if (dispatch?.@event != null && dispatch.@event != "ActionState" && dispatch.@event != "CurrentAction")
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
                    case "CurrentAction":
                        HandleCurrentAction(data);
                        break;
                    case "ProjectState":
                        HandleProjectState(data);
                        break;
                    case "ProjectChanged":
                        if (ignoreProjectChanged)
                            ignoreProjectChanged = false;
                        else
                            HandleProjectChanged(data);
                        break;
                }
            }

        }

        private async Task<T> WaitForResult<T>(int key) {
            if (responses.TryGetValue(key, out string value)) {
                if (value == null) {
                    value = await WaitForResponseReady(key);
                }
                return JsonConvert.DeserializeObject<T>(value);
            } else {
                return default;
            }
        }

        // TODO: add timeout!
        private Task<string> WaitForResponseReady(int key) {
            return Task.Run(() => {
                while (true) {
                    if (responses.TryGetValue(key, out string value)) {
                        if (value != null) {
                            return value;
                        } else {
                            Thread.Sleep(100);
                        }
                    }
                }
            });
        }

        private void HandleProjectChanged(string obj) {

            try {

                IO.Swagger.Model.ProjectChangedEvent eventProjectChanged = JsonConvert.DeserializeObject<IO.Swagger.Model.ProjectChangedEvent>(obj);
                GameManager.Instance.ProjectUpdated(eventProjectChanged.Data);
                projectArrived = true;
            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleProjectChanged()");
                GameManager.Instance.ProjectUpdated(null);

            }

        }

        private void HandleCurrentAction(string obj) {
            string puck_id;
            try {
                
                IO.Swagger.Model.CurrentActionEvent currentActionEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.CurrentActionEvent>(obj);

                puck_id = currentActionEvent.Data.ActionId;



            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleProjectChanged()");
                return;
            }

            Action puck = Scene.Instance.GetActionByID(puck_id);
            if (puck == null)
                return;

            // Stop previously running action (change its color to default)
            if(ActionsManager.Instance.CurrentlyRunningAction != null)
                ActionsManager.Instance.CurrentlyRunningAction.StopAction();

            ActionsManager.Instance.CurrentlyRunningAction = puck;
            // Run current action (set its color to running)
            puck.RunAction();
        }

        private void HandleProjectState(string obj) {
            IO.Swagger.Model.ProjectStateEvent projectState = JsonConvert.DeserializeObject<IO.Swagger.Model.ProjectStateEvent>(obj);
            GameManager.Instance.SetProjectState(projectState.Data);
            projectStateArrived = true;
        }

        private void HandleSceneChanged(string obj) {
            IO.Swagger.Model.SceneChangedEvent sceneChangedEvent = JsonConvert.DeserializeObject<IO.Swagger.Model.SceneChangedEvent>(obj);
            GameManager.Instance.SceneUpdated(sceneChangedEvent.Data);
            sceneArrived = true;
        }

       
        private void HandleOpenProject(string data) {
            IO.Swagger.Model.OpenProjectResponse response = JsonConvert.DeserializeObject<IO.Swagger.Model.OpenProjectResponse>(data);
        }

        public async Task<List<IO.Swagger.Model.ObjectTypeMeta>> GetObjectTypes() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.GetObjectTypesRequest(id: id, request: "GetObjectTypes").ToJson(), id, true);
            IO.Swagger.Model.GetObjectTypesResponse response = await WaitForResult<IO.Swagger.Model.GetObjectTypesResponse>(id);
            if (response.Result)
                return response.Data;
            else {
                throw new RequestFailedException("Failed to load object types");
            }

        }

        public async Task<List<IO.Swagger.Model.ObjectAction>> GetActions(string name) {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.GetActionsRequest(id: id, request: "GetActions", args: new IO.Swagger.Model.TypeArgs(type: name)).ToJson(), id, true);
            IO.Swagger.Model.GetActionsResponse response = await WaitForResult<IO.Swagger.Model.GetActionsResponse>(id);
            if (response.Result)
                return response.Data;
            else
                throw new RequestFailedException("Failed to load actions for object/service " + name);
        }

        public async Task<IO.Swagger.Model.SaveSceneResponse> SaveScene() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.SaveSceneRequest(id: id, request: "SaveScene").ToJson(), id, true);
            return await WaitForResult<IO.Swagger.Model.SaveSceneResponse>(id);
        }

        public async Task<IO.Swagger.Model.SaveProjectResponse> SaveProject() {
            int id = Interlocked.Increment(ref requestID);
            SendDataToServer(new IO.Swagger.Model.SaveProjectRequest(id: id, request: "SaveProject").ToJson(), id, true);
            return await WaitForResult<IO.Swagger.Model.SaveProjectResponse>(id);
        }

        public async Task OpenProject(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: id);
            IO.Swagger.Model.OpenProjectRequest request = new IO.Swagger.Model.OpenProjectRequest(id: r_id, request: "OpenProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.OpenProjectResponse response = await WaitForResult<IO.Swagger.Model.OpenProjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task RunProject(string projectId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: projectId);
            IO.Swagger.Model.RunProjectRequest request = new IO.Swagger.Model.RunProjectRequest(id: r_id, request: "RunProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.RunProjectResponse response = await WaitForResult<IO.Swagger.Model.RunProjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task StopProject() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.StopProjectRequest request = new IO.Swagger.Model.StopProjectRequest(id: r_id, request: "StopProject");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.StopProjectResponse response = await WaitForResult<IO.Swagger.Model.StopProjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task PauseProject() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.PauseProjectRequest request = new IO.Swagger.Model.PauseProjectRequest(id: r_id, request: "PauseProject");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.PauseProjectResponse response = await WaitForResult<IO.Swagger.Model.PauseProjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task ResumeProject() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ResumeProjectRequest request = new IO.Swagger.Model.ResumeProjectRequest(id: r_id, request: "ResumeProject");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ResumeProjectResponse response = await WaitForResult<IO.Swagger.Model.ResumeProjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task UpdateActionPointPosition(string actionPointId, string robotId, string endEffectorId, string orientationId, bool updatePosition) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(robotId: robotId, endEffector: endEffectorId);
            IO.Swagger.Model.UpdateActionPointPoseRequestArgs args = new IO.Swagger.Model.UpdateActionPointPoseRequestArgs(id: actionPointId,
                orientationId: orientationId, robot: robotArg, updatePosition: updatePosition);
            IO.Swagger.Model.UpdateActionPointPoseRequest request = new IO.Swagger.Model.UpdateActionPointPoseRequest(id: r_id, request: "UpdateActionPointPose", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointPoseResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointPoseResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task UpdateActionPointJoints(string actionPointId, string robotId, string jointsId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.UpdateActionPointJointsRequestArgs args = new IO.Swagger.Model.UpdateActionPointJointsRequestArgs(id: actionPointId,
                jointsId: jointsId, robotId: robotId);
            IO.Swagger.Model.UpdateActionPointJointsRequest request = new IO.Swagger.Model.UpdateActionPointJointsRequest(id: r_id, request: "UpdateActionPointJoints", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionPointJointsResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionPointJointsResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task UpdateActionObjectPosition(string actionObjectId, string robotId, string endEffectorId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(robotId: robotId, endEffector: endEffectorId);
            IO.Swagger.Model.UpdateActionObjectPoseRequestArgs args = new IO.Swagger.Model.UpdateActionObjectPoseRequestArgs(id: actionObjectId, robot: robotArg);
            IO.Swagger.Model.UpdateActionObjectPoseRequest request = new IO.Swagger.Model.UpdateActionObjectPoseRequest(id: r_id, request: "UpdateActionObjectPose", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.UpdateActionObjectPoseResponse response = await WaitForResult<IO.Swagger.Model.UpdateActionObjectPoseResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task CreateNewObjectType(IO.Swagger.Model.ObjectTypeMeta objectType) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.NewObjectTypeRequest request = new IO.Swagger.Model.NewObjectTypeRequest(id: r_id, request: "NewObjectType", args: objectType);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.NewObjectTypeResponse response = await WaitForResult<IO.Swagger.Model.NewObjectTypeResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task StartObjectFocusing(string objectId, string robotId, string endEffector) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RobotArg robotArg = new IO.Swagger.Model.RobotArg(endEffector, robotId);
            IO.Swagger.Model.FocusObjectStartRequestArgs args = new IO.Swagger.Model.FocusObjectStartRequestArgs(objectId: objectId, robot: robotArg);
            IO.Swagger.Model.FocusObjectStartRequest request = new IO.Swagger.Model.FocusObjectStartRequest(id: r_id, request: "FocusObjectStart", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectStartResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectStartResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task SavePosition(string objectId, int pointIdx) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.FocusObjectRequestArgs args = new IO.Swagger.Model.FocusObjectRequestArgs(objectId: objectId, pointIdx: pointIdx);
            IO.Swagger.Model.FocusObjectRequest request = new IO.Swagger.Model.FocusObjectRequest(id: r_id, request: "FocusObject", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task FocusObjectDone(string objectId) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: objectId);
            IO.Swagger.Model.FocusObjectDoneRequest request = new IO.Swagger.Model.FocusObjectDoneRequest(id: r_id, request: "FocusObjectDone", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.FocusObjectDoneResponse response = await WaitForResult<IO.Swagger.Model.FocusObjectDoneResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task<List<IO.Swagger.Model.IdDesc>> LoadScenes() {
            int id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ListScenesRequest request = new IO.Swagger.Model.ListScenesRequest(id: id, request: "ListScenes");
            SendDataToServer(request.ToJson(), id, true);
            IO.Swagger.Model.ListScenesResponse response = await WaitForResult<IO.Swagger.Model.ListScenesResponse>(id);
            return response.Data;
        }

        public async Task<List<IO.Swagger.Model.ListProjectsResponseData>> LoadProjects() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ListProjectsRequest request = new IO.Swagger.Model.ListProjectsRequest(id: r_id, request: "ListProjects");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ListProjectsResponse response = await WaitForResult<IO.Swagger.Model.ListProjectsResponse>(r_id);
            return response.Data;
        }

        public async Task<IO.Swagger.Model.AddObjectToSceneResponse> AddObjectToScene(IO.Swagger.Model.SceneObject sceneObject) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddObjectToSceneRequest request = new IO.Swagger.Model.AddObjectToSceneRequest(id: r_id, request: "AddObjectToScene", args: sceneObject);
            SendDataToServer(request.ToJson(), r_id, true);
            return await WaitForResult<IO.Swagger.Model.AddObjectToSceneResponse>(r_id);
        }

        public async Task AddServiceToScene(IO.Swagger.Model.SceneService sceneService) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.AddServiceToSceneRequest request = new IO.Swagger.Model.AddServiceToSceneRequest(id: r_id, request: "AddServiceToScene", args: sceneService);
            SendDataToServer(request.ToJson(), r_id, true);
            var response = await WaitForResult<IO.Swagger.Model.AddServiceToSceneResponse>(r_id);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }
        }
        public async Task<IO.Swagger.Model.AutoAddObjectToSceneResponse> AutoAddObjectToScene(string objectType) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.TypeArgs args = new IO.Swagger.Model.TypeArgs(type: objectType);
            IO.Swagger.Model.AutoAddObjectToSceneRequest request = new IO.Swagger.Model.AutoAddObjectToSceneRequest(id: r_id, request: "AutoAddObjectToScene", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            return await WaitForResult<IO.Swagger.Model.AutoAddObjectToSceneResponse>(r_id);
            
        }

        public async Task<bool> AddServiceToScene(string configId, string serviceType) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.SceneService sceneService = new IO.Swagger.Model.SceneService(configurationId: configId, type: serviceType);
            IO.Swagger.Model.AddServiceToSceneRequest request = new IO.Swagger.Model.AddServiceToSceneRequest(id: r_id, request: "AddServiceToScene", args: sceneService);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.AddServiceToSceneResponse response = await WaitForResult<IO.Swagger.Model.AddServiceToSceneResponse>(r_id);
            return response.Result;
        }

        public async Task<IO.Swagger.Model.RemoveFromSceneResponse> RemoveFromScene(string id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.RemoveFromSceneRequest request = new IO.Swagger.Model.RemoveFromSceneRequest(id: r_id, request: "RemoveFromScene", new IO.Swagger.Model.IdArgs(id: id));
            SendDataToServer(request.ToJson(), r_id, true);
            return await WaitForResult<IO.Swagger.Model.RemoveFromSceneResponse>(r_id);            
        }

        public async Task<List<IO.Swagger.Model.ServiceTypeMeta>> GetServices() {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.GetServicesRequest request = new IO.Swagger.Model.GetServicesRequest(id: r_id, request: "GetServices");
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.GetServicesResponse response = await WaitForResult<IO.Swagger.Model.GetServicesResponse>(r_id);
            if (response.Result)
                return response.Data;
            else
                return new List<IO.Swagger.Model.ServiceTypeMeta>();
        }

        public async Task OpenScene(string scene_id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: scene_id);
            IO.Swagger.Model.OpenSceneRequest request = new IO.Swagger.Model.OpenSceneRequest(id: r_id, request: "OpenScene", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.OpenSceneResponse response = await WaitForResult<IO.Swagger.Model.OpenSceneResponse>(r_id);
            if (!response.Result) {
                throw new RequestFailedException(response.Messages);
            }
        }

        public async Task<List<string>> GetActionParamValues(string actionProviderId, string param_id, List<IO.Swagger.Model.IdValue> parent_params) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ActionParamValuesArgs args = new IO.Swagger.Model.ActionParamValuesArgs(id: actionProviderId, paramId: param_id, parentParams: parent_params);
            IO.Swagger.Model.ActionParamValuesRequest request = new IO.Swagger.Model.ActionParamValuesRequest(id: r_id, request: "ActionParamValues", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ActionParamValuesResponse response = await WaitForResult<IO.Swagger.Model.ActionParamValuesResponse>(r_id);
            if (response.Result)
                return response.Data;
            else
                return new List<string>();
        }

        public async Task ExecuteAction(string actionId) {
            Debug.Assert(actionId != null);
            Debug.Assert(actionId != "");

            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.ExecuteActionArgs args = new IO.Swagger.Model.ExecuteActionArgs(actionId: actionId);
            IO.Swagger.Model.ExecuteActionRequest request = new IO.Swagger.Model.ExecuteActionRequest(id: r_id, request: "ExecuteAction", args: args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ExecuteActionResponse response = await WaitForResult<IO.Swagger.Model.ExecuteActionResponse>(r_id);
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }

        public async Task<IO.Swagger.Model.SystemInfoData> GetSystemInfo() {
             int r_id = Interlocked.Increment(ref requestID);

             IO.Swagger.Model.SystemInfoRequest request = new IO.Swagger.Model.SystemInfoRequest(id: r_id, request: "SystemInfo");
             SendDataToServer(request.ToJson(), r_id, true);
             IO.Swagger.Model.SystemInfoResponse response = await WaitForResult<IO.Swagger.Model.SystemInfoResponse>(r_id);
             if (!response.Result)
                 throw new RequestFailedException(response.Messages);
             return response.Data;
         }

        public async Task BuildProject(string project_id) {
            int r_id = Interlocked.Increment(ref requestID);
            IO.Swagger.Model.IdArgs args = new IO.Swagger.Model.IdArgs(id: project_id);
            IO.Swagger.Model.BuildProjectRequest request = new IO.Swagger.Model.BuildProjectRequest(id: r_id, request: "BuildProject", args);
            SendDataToServer(request.ToJson(), r_id, true);
            IO.Swagger.Model.ExecuteActionResponse response = await WaitForResult<IO.Swagger.Model.ExecuteActionResponse>(r_id);
            
            if (!response.Result)
                throw new RequestFailedException(response.Messages);
        }


    }
}
