using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using IO.Swagger.Model;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.XR.ARSubsystems;

namespace Base {

    public class ActionsManager : Singleton<ActionsManager> {

        private Dictionary<string, ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
        
        public Action CurrentlyRunningAction = null;
        
        public event EventHandler OnServiceMetadataUpdated, OnActionsLoaded;

        
        public GameObject LinkableParameterInputPrefab, LinkableParameterDropdownPrefab, LinkableParameterDropdownPosesPrefab,
            LinkableParameterDropdownPositionsPrefab, ParameterDropdownJointsPrefab, ActionPointOrientationPrefab, ParameterRelPosePrefab,
            LinkableParameterBooleanPrefab, ParameterDropdownPrefab;

        public GameObject InteractiveObjects;

        public event AREditorEventArgs.StringListEventHandler OnObjectTypesAdded, OnObjectTypesRemoved, OnObjectTypesUpdated;

        public bool ActionsReady, ActionObjectsLoaded, AbstractOnlyObjects;

        public Dictionary<string, RobotMeta> RobotsMeta = new Dictionary<string, RobotMeta>();


        public Dictionary<string, ActionObjectMetadata> ActionObjectsMetadata {
            get => actionObjectsMetadata; set => actionObjectsMetadata = value;
        }
        private void Awake() {
            ActionsReady = false;
            ActionObjectsLoaded = false;
        }

        private void Start() {
            Debug.Assert(LinkableParameterInputPrefab != null);
            Debug.Assert(LinkableParameterDropdownPrefab != null);
            Debug.Assert(LinkableParameterDropdownPosesPrefab != null);
            Debug.Assert(ParameterDropdownJointsPrefab != null);
            Debug.Assert(ParameterRelPosePrefab != null);
            Debug.Assert(InteractiveObjects != null);
            Init();
            WebsocketManager.Instance.OnDisconnectEvent += OnDisconnected;
            WebsocketManager.Instance.OnObjectTypeAdded += ObjectTypeAdded;
            WebsocketManager.Instance.OnObjectTypeRemoved += ObjectTypeRemoved;
            WebsocketManager.Instance.OnObjectTypeUpdated += ObjectTypeUpdated;
        }
        
        private void OnDisconnected(object sender, EventArgs args) {
            Init();
        }

        private void Update() {
            if (!ActionsReady && ActionObjectsLoaded) {
                foreach (ActionObjectMetadata ao in ActionObjectsMetadata.Values) {
                    if (!ao.Disabled && !ao.ActionsLoaded) {
                        return;
                    }
                }
                ActionsReady = true;
                OnActionsLoaded?.Invoke(this, EventArgs.Empty);
                enabled = false;
            }
        }

        public void Init() {
            actionObjectsMetadata.Clear();
            AbstractOnlyObjects = true;
            ActionsReady = false;
            ActionObjectsLoaded = false;
        }

        public bool HasObjectTypePose(string type) {
            if (!ActionObjectsMetadata.TryGetValue(type,
            out Base.ActionObjectMetadata actionObjectMetadata)) {
                throw new ItemNotFoundException("No object type " + type);
            }
            return actionObjectMetadata.HasPose;
        }

        // TODO - solve somehow better.. perhaps own class for robot objects and services?
        internal void UpdateRobotsMetadata(List<RobotMeta> list) {
            RobotsMeta.Clear();
            foreach (RobotMeta robotMeta in list) {
                RobotsMeta[robotMeta.Type] = robotMeta;
            }
        }



        public void ObjectTypeRemoved(object sender, StringListEventArgs type) {
            List<string> removed = new List<string>();
            foreach (string item in type.Data) {
                if (actionObjectsMetadata.ContainsKey(item)) {
                    actionObjectsMetadata.Remove(item);
                    removed.Add(item);
                }
            }
            if (type.Data.Count > 0) {
                AbstractOnlyObjects = true;
                foreach (ActionObjectMetadata obj in actionObjectsMetadata.Values) {
                    if (AbstractOnlyObjects && !obj.Abstract)
                        AbstractOnlyObjects = false;
                }
                OnObjectTypesRemoved?.Invoke(this, new StringListEventArgs(new List<string>(removed)));
            }

        }

        public async void ObjectTypeAdded(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            bool robotAdded = false;
            List<string> added = new List<string>();
            foreach (ObjectTypeMeta obj in args.ObjectTypes) {
                ActionObjectMetadata m = new ActionObjectMetadata(meta: obj);
                if (AbstractOnlyObjects && !m.Abstract)
                    AbstractOnlyObjects = false;
                if (!m.Abstract && !m.BuiltIn)
                    UpdateActionsOfActionObject(m);
                else
                    m.ActionsLoaded = true;
                m.Robot = IsDescendantOfType("Robot", m);
                m.Camera = IsDescendantOfType("Camera", m);
                m.CollisionObject = IsDescendantOfType("VirtualCollisionObject", m);
                actionObjectsMetadata.Add(obj.Type, m);
                if (m.Robot)
                    robotAdded = true;
                added.Add(obj.Type);
            }
            if (robotAdded)
                UpdateRobotsMetadata(await WebsocketManager.Instance.GetRobotMeta());
            
            OnObjectTypesAdded?.Invoke(this, new StringListEventArgs(added));
        }

        public bool AnyNonAbstractObject() {
            
            return false;
        }

        public async void ObjectTypeUpdated(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            bool updatedRobot = false;
            List<string> updated = new List<string>();
            foreach (ObjectTypeMeta obj in args.ObjectTypes) {
                if (actionObjectsMetadata.TryGetValue(obj.Type, out ActionObjectMetadata actionObjectMetadata)) {
                    actionObjectMetadata.Update(obj);
                    if (actionObjectMetadata.Robot)
                        updatedRobot = true;
                    if (AbstractOnlyObjects && !actionObjectMetadata.Abstract)
                        AbstractOnlyObjects = false;
                    if (!actionObjectMetadata.Abstract && !actionObjectMetadata.BuiltIn)
                        UpdateActionsOfActionObject(actionObjectMetadata);
                    else
                        actionObjectMetadata.ActionsLoaded = true;
                    updated.Add(obj.Type);
                    foreach (ActionObject updatedObj in SceneManager.Instance.GetAllObjectsOfType(obj.Type)) {
                        updatedObj.UpdateModel();
                    }
                } else {
                    Notifications.Instance.ShowNotification("Update of object types failed", "Server trying to update non-existing object!");
                }
            }
            if (updatedRobot)
                UpdateRobotsMetadata(await WebsocketManager.Instance.GetRobotMeta());
            OnObjectTypesUpdated?.Invoke(this, new StringListEventArgs(updated));
        }
        

        private void UpdateActionsOfActionObject(ActionObjectMetadata actionObject) {
            if (!actionObject.Disabled)
                try {
                    WebsocketManager.Instance.GetActions(actionObject.Type, GetActionsCallback);                    
                } catch (RequestFailedException e) {
                    Debug.LogError("Failed to load action for object " + actionObject.Type);
                    Notifications.Instance.ShowNotification("Failed to load actions", "Failed to load action for object " + actionObject.Type);
                    Notifications.Instance.SaveLogs();
                }            
        }

        public void GetActionsCallback(string actionName, string data) {
            IO.Swagger.Model.GetActionsResponse getActionsResponse = JsonConvert.DeserializeObject<IO.Swagger.Model.GetActionsResponse>(data);
            if (actionObjectsMetadata.TryGetValue(actionName, out ActionObjectMetadata actionObject)) {
                actionObject.ActionsMetadata = ParseActions(getActionsResponse.Data);
                if (actionObject.ActionsMetadata == null) {
                    actionObject.Disabled = true;
                    actionObject.Problem = "Failed to load actions";
                }
                actionObject.ActionsLoaded = true;
            }
        }
        

        private Dictionary<string, ActionMetadata> ParseActions(List<IO.Swagger.Model.ObjectAction> actions) {
            if (actions == null) {
                return null;
            }
            Dictionary<string, ActionMetadata> metadata = new Dictionary<string, ActionMetadata>();
            foreach (IO.Swagger.Model.ObjectAction action in actions) {
                ActionMetadata a = new ActionMetadata(action);
                metadata[a.Name] = a;
            }
            return metadata;
        }
        private void UpdateActionServices(object sender, EventArgs eventArgs) {
            
        }

        public void UpdateObjects(List<IO.Swagger.Model.ObjectTypeMeta> newActionObjectsMetadata) {
            ActionsReady = false;
            actionObjectsMetadata.Clear();
            foreach (IO.Swagger.Model.ObjectTypeMeta metadata in newActionObjectsMetadata) {
                ActionObjectMetadata m = new ActionObjectMetadata(meta: metadata);
                if (AbstractOnlyObjects && !m.Abstract)
                    AbstractOnlyObjects = false;
                if (!m.Abstract && !m.BuiltIn)
                    UpdateActionsOfActionObject(m);
                else
                    m.ActionsLoaded = true;
                actionObjectsMetadata.Add(metadata.Type, m);
            }
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
                kv.Value.Camera = IsDescendantOfType("Camera", kv.Value);
                kv.Value.CollisionObject = IsDescendantOfType("VirtualCollisionObject", kv.Value);
            }
            enabled = true;

            ActionObjectsLoaded = true;
        }

        private bool IsDescendantOfType(string type, ActionObjectMetadata actionObjectMetadata) {
            if (actionObjectMetadata.Type == type)
                return true;
            if (actionObjectMetadata.Type == "Generic")
                return false;
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                if (kv.Key == actionObjectMetadata.Base) {
                    return IsDescendantOfType(type, kv.Value);
                }
            }
            return false;
        }

        public void WaitUntilActionsReady(int timeout) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!ActionsReady) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                System.Threading.Thread.Sleep(100);
            }
        }

        public Dictionary<IActionProvider, List<ActionMetadata>> GetAllActions() {
            Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = new Dictionary<IActionProvider, List<ActionMetadata>>();
            foreach (ActionObject ao in SceneManager.Instance.ActionObjects.Values) {               
                if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
                    continue;
                }
                if (aom.ActionsMetadata.Count > 0) {
                    actionsMetadata[ao] = aom.ActionsMetadata.Values.ToList();                    
                }                
            }
            return actionsMetadata;
        }


        
    }
}

