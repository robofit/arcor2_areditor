using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using IO.Swagger.Model;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;

namespace Base {

    public class ActionsManager : Singleton<ActionsManager> {

        private Dictionary<string, ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
       // private Dictionary<string, ServiceMetadata> servicesMetadata = new Dictionary<string, ServiceMetadata>();
        
        public Action CurrentlyRunningAction = null;
        
        public event EventHandler OnServiceMetadataUpdated, OnActionsLoaded;

        
        public GameObject ParameterInputPrefab, ParameterDropdownPrefab, ParameterDropdownPosesPrefab,
            ParameterDropdownJointsPrefab, ActionPointOrientationPrefab, ParameterRelPosePrefab,
            ParameterBooleanPrefab;

        public GameObject InteractiveObjects;

        public event AREditorEventArgs.StringListEventHandler OnActionObjectsUpdated;

        public bool ActionsReady, ActionObjectsLoaded;

        public Dictionary<string, RobotMeta> RobotsMeta = new Dictionary<string, RobotMeta>();

        public Dictionary<string, ActionObjectMetadata> ActionObjectMetadata {
            get => actionObjectsMetadata; set => actionObjectsMetadata = value;
        }
       /* public Dictionary<string, ServiceMetadata> ServicesMetadata {
            get => servicesMetadata;
            set => servicesMetadata = value;
        }
        */
        private void Awake() {
            ActionsReady = false;
            ActionObjectsLoaded = false;
        }

        private void Start() {
            Debug.Assert(ParameterInputPrefab != null);
            Debug.Assert(ParameterDropdownPrefab != null);
            Debug.Assert(ParameterDropdownPosesPrefab != null);
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
                foreach (ActionObjectMetadata ao in ActionObjectMetadata.Values) {
                    if (!ao.Disabled && !ao.ActionsLoaded) {
                        return;
                    }
                }
              /*  foreach (ServiceMetadata sm in ServicesMetadata.Values) {
                    if (!sm.ActionsLoaded) {
                        return;
                    }
                }*/
                ActionsReady = true;
                OnActionsLoaded?.Invoke(this, EventArgs.Empty);
                enabled = false;
            }
        }

        public void Init() {
           // servicesMetadata.Clear();
            actionObjectsMetadata.Clear();
            ActionsReady = false;
            ActionObjectsLoaded = false;
        }

        public bool HasObjectTypePose(string type) {
            if (!ActionObjectMetadata.TryGetValue(type,
            out Base.ActionObjectMetadata actionObjectMetadata)) {
                throw new ItemNotFoundException("No object type " + type);
            }
            return actionObjectMetadata.HasPose;
        }


        /*  
          public async Task UpdateServicesMetadata(List<IO.Swagger.Model.ServiceTypeMeta> newServices) {
              foreach (IO.Swagger.Model.ServiceTypeMeta newServiceMeta in newServices) {
                  ServiceMetadata serviceMetadata = new ServiceMetadata(newServiceMeta);
                  ServicesMetadata[serviceMetadata.Type] = serviceMetadata;
                  await UpdateActionsOfService(serviceMetadata);
              }
              ServicesLoaded = true;
              OnServiceMetadataUpdated?.Invoke(this, EventArgs.Empty);
          }*/

        // TODO - solve somehow better.. perhaps own class for robot objects and services?
        internal void UpdateRobotsMetadata(List<RobotMeta> list) {
            RobotsMeta.Clear();
            foreach (RobotMeta robotMeta in list) {
                RobotsMeta[robotMeta.Type] = robotMeta;
            }
        }



        public void ObjectTypeRemoved(object sender, StringListEventArgs type) {
            foreach (string item in type.Data) {
                if (actionObjectsMetadata.ContainsKey(item)) {
                    actionObjectsMetadata.Remove(item);
                }
            }
            if (type.Data.Count > 0)
                OnActionObjectsUpdated?.Invoke(this, new StringListEventArgs(new List<string>()));

        }

        public void ObjectTypeAdded(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            List<string> added = new List<string>();
            foreach (ObjectTypeMeta obj in args.ObjectTypes) {
                ActionObjectMetadata m = new ActionObjectMetadata(meta: obj);
                UpdateActionsOfActionObject(m);
                m.Robot = IsDescendantOfType("Robot", m);
                m.Camera = IsDescendantOfType("Camera", m);
                actionObjectsMetadata.Add(obj.Type, m);
                added.Add(obj.Type);
            }
            
            OnActionObjectsUpdated?.Invoke(this, new StringListEventArgs(added));
        }

        public void ObjectTypeUpdated(object sender, ObjectTypesEventArgs args) {
            ActionsReady = false;
            enabled = true;
            List<string> updated = new List<string>();
            foreach (ObjectTypeMeta obj in args.ObjectTypes) {
                if (actionObjectsMetadata.TryGetValue(obj.Type, out ActionObjectMetadata actionObjectMetadata)) {
                    actionObjectMetadata.Update(obj);
                    UpdateActionsOfActionObject(actionObjectMetadata);
                    updated.Add(obj.Type);
                } else {
                    Notifications.Instance.ShowNotification("Update of object types failed", "Server trying to update non-existing object!");
                }
            }
            OnActionObjectsUpdated?.Invoke(this, new StringListEventArgs(updated));
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
                /*
                foreach (IO.Swagger.Model.ActionParameterMeta arg in action.Parameters) {
                    switch (arg.Type) {
                        case IO.Swagger.Model.ObjectActionArgs.TypeEnum.String:
                            a.Parameters[arg.Name] = new ActionParameterMetadata(arg.Name, IO.Swagger.Model.ActionParameter.TypeEnum.String, "");
                            break;
                        case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Pose:
                            a.Parameters[arg.Name] = new ActionParameterMetadata(arg.Name, IO.Swagger.Model.ActionParameter.TypeEnum.Pose, "");
                            break;
                        case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Double:
                            a.Parameters[arg.Name] = new ActionParameterMetadata(arg.Name, IO.Swagger.Model.ActionParameter.TypeEnum.Double, 0d);
                            break;
                        case IO.Swagger.Model.ObjectActionArgs.TypeEnum.Integer:
                            a.Parameters[arg.Name] = new ActionParameterMetadata(arg.Name, IO.Swagger.Model.ActionParameter.TypeEnum.Integer, (long) 0);
                            break;
                    }

            }*/
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
                UpdateActionsOfActionObject(m);
                actionObjectsMetadata.Add(metadata.Type, m);
            }
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
                kv.Value.Camera = IsDescendantOfType("Camera", kv.Value);
            }
            enabled = true;

            ActionObjectsLoaded = true;
            OnActionObjectsUpdated?.Invoke(this, new Base.StringListEventArgs(new List<string>()));
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

