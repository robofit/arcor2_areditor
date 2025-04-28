using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Base {

    public class ActionsManager : Singleton<ActionsManager> {
        public Action CurrentlyRunningAction = null;
        
        public event EventHandler OnServiceMetadataUpdated, OnActionsLoaded;

        
        public GameObject LinkableParameterInputPrefab, LinkableParameterDropdownPrefab, LinkableParameterDropdownPosesPrefab,
            LinkableParameterDropdownPositionsPrefab, ParameterDropdownJointsPrefab, ActionPointOrientationPrefab, ParameterRelPosePrefab,
            LinkableParameterBooleanPrefab, ParameterDropdownPrefab;

        public GameObject InteractiveObjects;

        public event EventHandler<ObjectTypesEventArgs> OnObjectTypesAdded, OnObjectTypesRemoved, OnObjectTypesUpdated;

        public bool ActionsReady, ActionObjectsLoaded, AbstractOnlyObjects;

        public Dictionary<string, RobotMeta> RobotsMeta = new();


        public Dictionary<string, ActionObjectMetadata> ActionObjectsMetadata {
            get;
            set;
        } = new();

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
            CommunicationManager.Instance.Client.ConnectionClosed += CommunicationManager.SafeEventHandler<WebSocketCloseEventArgs>(OnDisconnected);
            CommunicationManager.Instance.Client.ObjectTypeAdded += CommunicationManager.SafeEventHandler<ObjectTypesEventArgs>(ObjectTypeAdded);
            CommunicationManager.Instance.Client.ObjectTypeRemoved += CommunicationManager.SafeEventHandler<ObjectTypesEventArgs>(ObjectTypeRemoved);
            CommunicationManager.Instance.Client.ObjectTypeUpdated += CommunicationManager.SafeEventHandler<ObjectTypesEventArgs>(ObjectTypeUpdated);
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
            ActionObjectsMetadata.Clear();
            AbstractOnlyObjects = true;
            ActionsReady = false;
            ActionObjectsLoaded = false;
        }

        public bool HasObjectTypePose(string type) {
            if (!ActionObjectsMetadata.TryGetValue(type,
            out ActionObjectMetadata actionObjectMetadata)) {
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



        public void ObjectTypeRemoved(object sender, ObjectTypesEventArgs type) {
            List<ObjectTypeMeta> removed = new();
            foreach (var item in type.Data) {
                if (ActionObjectsMetadata.ContainsKey(item.Type)) {
                    ActionObjectsMetadata.Remove(item.Type);
                    removed.Add(item);
                }
            }
            if (type.Data.Count > 0) {
                AbstractOnlyObjects = true;
                foreach (ActionObjectMetadata obj in ActionObjectsMetadata.Values) {
                    if (AbstractOnlyObjects && !obj.Abstract)
                        AbstractOnlyObjects = false;
                }
                OnObjectTypesRemoved?.Invoke(this, new ObjectTypesEventArgs(removed));
            }

        }

        public async void ObjectTypeAdded(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            bool robotAdded = false;
            List<ObjectTypeMeta> added = new();
            foreach (ObjectTypeMeta obj in args.Data) {
                ActionObjectMetadata m = new(obj);
                if (AbstractOnlyObjects && !m.Abstract)
                    AbstractOnlyObjects = false;
                if (!m.Abstract && !m.BuiltIn)
                    UpdateActionsOfActionObject(m);
                else
                    m.ActionsLoaded = true;
                m.Robot = IsDescendantOfType("Robot", m);
                m.Camera = IsDescendantOfType("Camera", m);
                m.CollisionObject = IsDescendantOfType("VirtualCollisionObject", m);
                ActionObjectsMetadata.Add(obj.Type, m);
                if (m.Robot)
                    robotAdded = true;
                added.Add(obj);
            }
            if (robotAdded)
                UpdateRobotsMetadata((await CommunicationManager.Instance.Client.GetRobotMetaAsync()).Data);
            
            OnObjectTypesAdded?.Invoke(this, new ObjectTypesEventArgs(added));
        }

        public bool AnyNonAbstractObject() {
            
            return false;
        }

        public async void ObjectTypeUpdated(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            bool updatedRobot = false;
            List<ObjectTypeMeta> updated = new();
            foreach (ObjectTypeMeta obj in args.Data) {
                if (ActionObjectsMetadata.TryGetValue(obj.Type, out ActionObjectMetadata actionObjectMetadata)) {
                    actionObjectMetadata.Update(obj);
                    if (actionObjectMetadata.Robot)
                        updatedRobot = true;
                    if (AbstractOnlyObjects && !actionObjectMetadata.Abstract)
                        AbstractOnlyObjects = false;
                    if (!actionObjectMetadata.Abstract && !actionObjectMetadata.BuiltIn)
                        UpdateActionsOfActionObject(actionObjectMetadata);
                    else
                        actionObjectMetadata.ActionsLoaded = true;
                    updated.Add(obj);
                    foreach (ActionObject updatedObj in SceneManager.Instance.GetAllObjectsOfType(obj.Type)) {
                        updatedObj.UpdateModel();
                    }
                } else {
                    Notifications.Instance.ShowNotification("Update of object types failed", "Server trying to update non-existing object!");
                }
            }
            if (updatedRobot)
                UpdateRobotsMetadata((await CommunicationManager.Instance.Client.GetRobotMetaAsync()).Data);
            OnObjectTypesUpdated?.Invoke(this, new ObjectTypesEventArgs(updated));
        }
        

        private async void UpdateActionsOfActionObject(ActionObjectMetadata actionObject) {
            if (!actionObject.Disabled)
                try {
                    var response = await CommunicationManager.Instance.Client.GetActionsAsync(new TypeArgs(actionObject.Type));
                    if (!response.Result) {
                        Debug.LogError("Failed to load action for object " + actionObject.Type);
                        Notifications.Instance.ShowNotification("Failed to load actions", "Failed to load action for object " + actionObject.Type);
                        Notifications.Instance.SaveLogs();
                        return;
                    }
                    if (ActionObjectsMetadata.TryGetValue(actionObject.Type, out ActionObjectMetadata actionObj)) {
                        actionObj.ActionsMetadata = ParseActions(response.Data);
                        if (actionObj.ActionsMetadata == null) {
                            actionObj.Disabled = true;
                            actionObj.Problem = "Failed to load actions";
                        }
                        actionObj.ActionsLoaded = true;
                    }
                } catch (Arcor2ConnectionException e) {
                    Debug.LogError("Failed to load action for object " + actionObject.Type);
                    Notifications.Instance.ShowNotification("Failed to load actions", "Failed to load action for object " + actionObject.Type);
                    Notifications.Instance.SaveLogs();
                }            
        }

        private Dictionary<string, ActionMetadata> ParseActions(List<ObjectAction> actions) {
            if (actions == null) {
                return null;
            }
            Dictionary<string, ActionMetadata> metadata = new();
            foreach (ObjectAction action in actions) {
                ActionMetadata a = new(action);
                metadata[a.Name] = a;
            }
            return metadata;
        }
        private void UpdateActionServices(object sender, EventArgs eventArgs) {
            
        }

        public void UpdateObjects(List<ObjectTypeMeta> newActionObjectsMetadata) {
            ActionsReady = false;
            ActionObjectsMetadata.Clear();
            foreach (ObjectTypeMeta metadata in newActionObjectsMetadata) {
                ActionObjectMetadata m = new(metadata);
                if (AbstractOnlyObjects && !m.Abstract)
                    AbstractOnlyObjects = false;
                if (!m.Abstract && !m.BuiltIn)
                    UpdateActionsOfActionObject(m);
                else
                    m.ActionsLoaded = true;
                ActionObjectsMetadata.Add(metadata.Type, m);
            }
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in ActionObjectsMetadata) {
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
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in ActionObjectsMetadata) {
                if (kv.Key == actionObjectMetadata.Base) {
                    return IsDescendantOfType(type, kv.Value);
                }
            }
            return false;
        }

        public void WaitUntilActionsReady(int timeout) {
            Stopwatch sw = new();
            sw.Start();
            while (!ActionsReady) {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(100);
            }
        }

        public Dictionary<IActionProvider, List<ActionMetadata>> GetAllActions() {
            Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = new();
            foreach (ActionObject ao in SceneManager.Instance.ActionObjects.Values) {               
                if (!ActionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
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

