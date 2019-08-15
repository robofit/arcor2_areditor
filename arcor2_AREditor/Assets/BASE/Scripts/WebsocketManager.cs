using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;


namespace Base {
    public class WebsocketManager : Singleton<WebsocketManager> {
        public string APIDomainWS = "";

        private ClientWebSocket clientWebSocket;

        private List<string> actionObjectsToBeUpdated = new List<string>();
        private Queue<string> sendingQueue = new Queue<string>();
        private string waitingForObjectActions;

        private bool waitingForMessage = false;

        private string receivedData;

        private bool readyToSend, ignoreProjectChanged, connecting;

        void Awake() {
            waitingForMessage = false;
            readyToSend = true;
            ignoreProjectChanged = false;
            connecting = false;

            receivedData = "";
            waitingForObjectActions = "";
        }

        void Start() {

        }

        async public void ConnectToServer(string domain, int port, bool updateObjects = true) {
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

            Debug.Log("End");
            connecting = false;
            if (clientWebSocket.State == WebSocketState.Open) {

                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Connected;
                if (updateObjects)
                    UpdateObjectTypes();
            } else {
                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            }
        }

        async public void DisconnectFromSever() {
            Debug.Log("Disconnecting");
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            clientWebSocket = null;
        }



        // Update is called once per frame
        async void Update() {
            if (clientWebSocket == null)
                return;
            if (clientWebSocket.State == WebSocketState.Open && GameManager.Instance.ConnectionStatus == GameManager.ConnectionStatusEnum.Disconnected) {
                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Connected;
            } else if (clientWebSocket.State != WebSocketState.Open && GameManager.Instance.ConnectionStatus == GameManager.ConnectionStatusEnum.Connected) {
                GameManager.Instance.ConnectionStatus = GameManager.ConnectionStatusEnum.Disconnected;
            }

            if (!waitingForMessage && clientWebSocket.State == WebSocketState.Open) {
                WebSocketReceiveResult result = null;
                waitingForMessage = true;
                ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[8192]);
                do {
                    result = await clientWebSocket.ReceiveAsync(
                        bytesReceived,
                        CancellationToken.None
                    );
                    receivedData += Encoding.Default.GetString(bytesReceived.Array);
                } while (!result.EndOfMessage);
                HandleReceivedData(receivedData);
                receivedData = "";
                waitingForMessage = false;

            }
            if (waitingForObjectActions == "" && actionObjectsToBeUpdated.Count > 0) {
                waitingForObjectActions = actionObjectsToBeUpdated[0];
                actionObjectsToBeUpdated.RemoveAt(0);
                UpdateObjectActions(waitingForObjectActions);
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

        public void SendDataToServer(string data) {
            sendingQueue.Enqueue(data);
        }

        async public void SendDataToServer() {
            if (sendingQueue.Count == 0)
                return;
            string data = sendingQueue.Dequeue();
            readyToSend = false;
            if (clientWebSocket.State != WebSocketState.Open)
                return;

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(
                         Encoding.UTF8.GetBytes(data)
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
            JSONObject getObjectTypes = new JSONObject(JSONObject.Type.OBJECT);
            getObjectTypes.AddField("request", "getObjectTypes");
            getObjectTypes.AddField("args", new JSONObject(JSONObject.Type.OBJECT));
            SendDataToServer(getObjectTypes.ToString());
        }

        public void UpdateObjectActions(string ObjectId) {
            JSONObject getObjectActions = new JSONObject(JSONObject.Type.OBJECT);
            getObjectActions.AddField("request", "getObjectActions");
            JSONObject args = new JSONObject(JSONObject.Type.OBJECT);
            args.AddField("type", ObjectId);
            getObjectActions.AddField("args", args);
            SendDataToServer(getObjectActions.ToString());
        }

        public void UpdateScene(List<InteractiveObject> interactiveObjects) {
            JSONObject message = new JSONObject(JSONObject.Type.OBJECT);

            message.AddField("event", "sceneChanged");
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            data.AddField("id", "jabloPCB");
            JSONObject obj = new JSONObject(JSONObject.Type.ARRAY);
            foreach (InteractiveObject io in interactiveObjects) {
                JSONObject iojson = new JSONObject(JsonUtility.ToJson(io).ToString());
                iojson.AddField("id", io.Id);
                JSONObject pose = JSONHelper.CreatePose(new Vector3(io.gameObject.transform.localPosition.x, io.gameObject.transform.localPosition.y, 0), new Quaternion(0, 0, 0, 1));
                iojson.AddField("pose", pose);
                obj.Add(iojson);
            }
            data.AddField("objects", obj);
            message.AddField("data", data);
            Debug.Log(message.ToString());
            SendDataToServer(message.ToString());
        }

        // TODO: add action parameters
        public void UpdateProject(List<InteractiveObject> interactiveObjects, GameObject scene) {
            JSONObject message = new JSONObject(JSONObject.Type.OBJECT);

            message.AddField("event", "projectChanged");
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            data.AddField("id", "demo_v0");
            data.AddField("scene_id", "jabloPCB");
            JSONObject obj = new JSONObject(JSONObject.Type.ARRAY);
            foreach (InteractiveObject io in interactiveObjects) {
                JSONObject iojson = new JSONObject(JSONObject.Type.OBJECT);
                iojson.AddField("id", io.Id);
                JSONObject apArray = new JSONObject(JSONObject.Type.ARRAY);
                foreach (ActionPoint ap in io.transform.Find("ActionPoints").gameObject.GetComponentsInChildren<ActionPoint>()) {
                    JSONObject apjson = new JSONObject(JsonUtility.ToJson(ap).ToString());


                    apjson.AddField("pose", JSONHelper.CreatePose(ap.GetScenePosition(), ap.transform.rotation));
                    JSONObject puckArray = new JSONObject(JSONObject.Type.ARRAY);
                    foreach (Puck puck in ap.GetComponentsInChildren<Puck>()) {
                        JSONObject puckjson = new JSONObject(JsonUtility.ToJson(puck).ToString());
                        puckjson.AddField("type", puck.Action.InteractiveObject.Id + "/" + puck.Action.Metadata.Name);
                        puckjson.AddField("pose", JSONHelper.CreatePose(puck.transform.localPosition, puck.transform.rotation));

                        if (puck.GetComponentInChildren<PuckInput>() != null/* && ConnectionManager.GetComponent<ConnectionManagerArcoro>().ValidateConnection(puck.GetComponentInChildren<PuckInput>().GetConneciton())*/) {
                            JSONObject input_connection = new JSONObject(JSONObject.Type.ARRAY);
                            GameObject ConnectedPuck = ConnectionManagerArcoro.Instance.GetConnectedTo(puck.GetComponentInChildren<PuckInput>().GetConneciton(), puck.gameObject.GetComponentInChildren<PuckInput>().gameObject);

                            JSONObject con = new JSONObject(JSONObject.Type.OBJECT);
                            if (ConnectedPuck != null && ConnectedPuck.name != "VirtualPointer") {
                                Debug.Log(ConnectedPuck);
                                Debug.Log(ConnectedPuck.transform.GetComponentInParent<Puck>());
                                con.AddField("default", ConnectedPuck.transform.GetComponentInParent<Puck>().id);

                            } else {
                                con.AddField("default", "start");
                            }
                            input_connection.Add(con);
                            puckjson.AddField("inputs", input_connection);

                        }
                        if (puck.GetComponentInChildren<PuckOutput>()/* != null && ConnectionManager.GetComponent<ConnectionManagerArcoro>().ValidateConnection(puck.GetComponentInChildren<PuckOutput>().GetConneciton())*/) {
                            JSONObject output_connection = new JSONObject(JSONObject.Type.OBJECT);
                            GameObject ConnectedPuck = ConnectionManagerArcoro.Instance.GetConnectedTo(puck.GetComponentInChildren<PuckOutput>().GetConneciton(), puck.gameObject.GetComponentInChildren<PuckOutput>().gameObject);
                            JSONObject con = new JSONObject(JSONObject.Type.OBJECT);
                            if (ConnectedPuck != null && ConnectedPuck.name != "VirtualPointer") {

                                con.AddField("default", ConnectedPuck.transform.parent.GetComponent<Puck>().id);

                            } else {
                                con.AddField("default", "end");
                            }
                            output_connection.Add(con);
                            puckjson.AddField("outputs", output_connection);
                        }
                        JSONObject parameters = new JSONObject(JSONObject.Type.ARRAY);
                        foreach (ActionParameter parameter in puck.Action.Parameters.Values) {
                            JSONObject param = new JSONObject(JSONObject.Type.OBJECT);
                            param.AddField("id", parameter.ActionParameterMetadata.Name);
                            param.AddField("type", parameter.ActionParameterMetadata.GetStringType());
                            //
                            switch (parameter.ActionParameterMetadata.Type) {
                                case ActionParameterMetadata.Types.ActionPoint:
                                case ActionParameterMetadata.Types.String:
                                    parameter.GetValue(out string stringValue, "");
                                    param.AddField("value", stringValue);
                                    break;
                                case ActionParameterMetadata.Types.Integer:
                                    parameter.GetValue(out long intValue, 0);
                                    param.AddField("value", intValue);
                                    break;
                                case ActionParameterMetadata.Types.Bool:
                                    parameter.GetValue(out bool boolValue, false);
                                    param.AddField("value", boolValue);
                                    break;
                                default:
                                    param.AddField("value", "");
                                    break;
                            }
                            parameters.Add(param);
                        }

                        puckjson.AddField("parameters", parameters);
                        puckArray.Add(puckjson);
                    }
                    apjson.AddField("actions", puckArray);
                    apArray.Add(apjson);
                }
                iojson.AddField("action_points", apArray);
                obj.Add(iojson);
            }
            data.AddField("objects", obj);
            message.AddField("data", data);
            Debug.Log("To send: ");
            Debug.Log(message.ToString(true));
            ignoreProjectChanged = true;

            SendDataToServer(message.ToString());
        }


        private void HandleReceivedData(string data) {
            JSONObject jsonData = new JSONObject(data);
            Debug.Log("Received new data");
            Debug.Log(data);
            Debug.Log(jsonData);
            Dictionary<string, string> jsonDict = jsonData.ToDictionary();
            if (jsonDict == null)
                return;
            if (jsonDict.ContainsKey("response")) {
                switch (jsonData["response"].str) {
                    case "getObjectTypes":
                        HandleGetObjecTypes(jsonData);
                        break;
                    case "getObjectActions":
                        HandleGetObjectActions(jsonData);
                        break;
                }
            } else if (jsonDict.ContainsKey("event")) {
                switch (jsonData["event"].str) {
                    case "sceneChanged":
                        HandleSceneChanged(jsonData);
                        break;
                    case "currentAction":
                        HandleCurrentAction(jsonData);
                        break;
                    case "projectChanged":
                        Debug.Log(ignoreProjectChanged);
                        if (ignoreProjectChanged)
                            ignoreProjectChanged = false;
                        else
                            HandleProjectChanged(jsonData);
                        break;
                }
            }

        }

        void HandleProjectChanged(JSONObject obj) {

            try {
                if (obj["event"].str != "projectChanged" || obj["data"].GetType() != typeof(JSONObject)) {
                    return;
                }



                GameManager.Instance.ProjectUpdated(obj["data"]);


            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleProjectChanged()");

            }

        }

        void HandleCurrentAction(JSONObject obj) {
            string puck_id;
            try {
                if (obj["event"].str != "currentAction" || obj["data"].GetType() != typeof(JSONObject)) {
                    return;
                }

                puck_id = obj["data"]["action_id"].str;



            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleProjectChanged()");
                return;
            }

            GameObject puck = GameManager.Instance.FindPuck(puck_id);

            //Arrow.transform.SetParent(puck.transform);
            //Arrow.transform.position = puck.transform.position + new Vector3(0f, 1.5f, 0f);
        }

        void HandleSceneChanged(JSONObject obj) {


            try {
                if (obj["event"].str != "sceneChanged") {
                    Debug.Log("Wrong headers");
                    return;
                }

                JSONObject data = obj["data"];

                GameManager.Instance.SceneUpdated(data);


            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleSceneChanged()");
                Debug.Log(obj);
            }

        }

        private void HandleGetObjectActions(JSONObject obj) {
            try {
                if (!CheckHeaders(obj, "getObjectActions")) {
                    Debug.Log("Wrong headers");
                    return;
                }

                JSONObject data = obj["data"];
                if (ActionsManager.Instance.ActionObjectMetadata.TryGetValue(waitingForObjectActions, out ActionObjectMetadata ao)) {
                    JSONObject defaultValueStr = new JSONObject(JSONObject.Type.OBJECT);
                    defaultValueStr.AddField("value", "");
                    JSONObject defaultValueInt = new JSONObject(JSONObject.Type.OBJECT);
                    defaultValueInt.AddField("value", 0);

                    //handle actions for actionobject here
                    foreach (JSONObject o in data.list) {
                        JSONObject metaData = o["meta"];
                        ActionMetadata a = new ActionMetadata(o["name"].str, JSONHelper.GetBoolValue(metaData, "blocking", false), JSONHelper.GetBoolValue(metaData, "free", false), JSONHelper.GetBoolValue(metaData, "composite", false), JSONHelper.GetBoolValue(metaData, "blackbox", false));

                        ao.ActionsMetadata[a.Name] = a;
                        foreach (JSONObject args in o["action_args"].list) {
                            switch (args["type"].str) {
                                case "int":
                                    a.Parameters[args["name"].str] = new ActionParameterMetadata(args["name"].str, args["type"].str, defaultValueInt);
                                    break;
                                case "str":
                                case "ActionPoint":
                                    a.Parameters[args["name"].str] = new ActionParameterMetadata(args["name"].str, args["type"].str, defaultValueStr);
                                    break;
                            }

                        }

                    }
                    ao.ActionsLoaded = true;
                    ActionsManager.Instance.UpdateObjectActionMenu(ao.Type);
                    waitingForObjectActions = "";
                }
            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleGetObjectActions()");
            }
        }

        private void HandleGetObjecTypes(JSONObject obj) {

            try {
                if (!CheckHeaders(obj, "getObjectTypes"))
                    return;
                JSONObject data = obj["data"];
                Dictionary<string, ActionObjectMetadata> NewActionObjects = new Dictionary<string, ActionObjectMetadata>();
                foreach (JSONObject o in data.list) {
                    ActionObjectMetadata ao = new ActionObjectMetadata(o["type"].str, o["description"].str, o["base"].str);
                    NewActionObjects[ao.Type] = ao;
                    actionObjectsToBeUpdated.Add(ao.Type);
                }

                ActionsManager.Instance.UpdateObjects(NewActionObjects);
                Debug.Log(ActionsManager.Instance.ActionObjectMetadata.Count);
            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleGetObjecTypes()");
            }
        }

        private bool CheckHeaders(JSONObject obj, string method) {
            try {
                if (obj["response"].str != method)
                    return false;
                if (!obj["result"].b)
                    return false;

            } catch (NullReferenceException e) {
                Debug.Log("Parse error in HandleGetObjecTypes()");
                return false;
            }
            return true;
        }




        public void SaveProject() {
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "saveScene");
            request.AddField("args", new JSONObject(JSONObject.Type.ARRAY));
            SendDataToServer(request.ToString());
            request.AddField("request", "saveProject");
            SendDataToServer(request.ToString());
        }

        public void LoadProject() {
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "loadProject");
            JSONObject args = new JSONObject(JSONObject.Type.ARRAY);
            args.AddField("id", "demo_v0");
            request.AddField("args", args);
            SendDataToServer(request.ToString());
        }

        public void RunProject() {
            //Arrow.SetActive(true);
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "runProject");
            SendDataToServer(request.ToString());
        }

        public void StopProject() {
            //Arrow.SetActive(false);
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "stopProject");
            SendDataToServer(request.ToString());
        }

        public void PauseProject() {
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "pauseProject");
            SendDataToServer(request.ToString());
        }

        public void ResumeProject() {
            //Arrow.SetActive(true);
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "resumeProject");
            SendDataToServer(request.ToString());
        }

        public void UpdateActionPointPosition(string actionPointId, string robotId) {
            JSONObject request = new JSONObject(JSONObject.Type.OBJECT);
            request.AddField("request", "updateActionPointPose");
            JSONObject args = new JSONObject(JSONObject.Type.ARRAY);
            args.AddField("id", actionPointId);
            args.AddField("robot", robotId);
            args.AddField("end_effector", "neumime");
            request.AddField("args", args);
            Debug.Log(request.ToString(true));
            SendDataToServer(request.ToString());
        }
    }
}