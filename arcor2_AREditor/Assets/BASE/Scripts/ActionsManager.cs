using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using IO.Swagger.Model;
using System.Threading.Tasks;

namespace Base {
    public class ActionsManager : Singleton<ActionsManager> {
        private Dictionary<string, ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
        private Dictionary<string, ServiceMetadata> servicesMetadata = new Dictionary<string, ServiceMetadata>();
        private Dictionary<string, Service> servicesData = new Dictionary<string, Service>();
        
        public event EventHandler OnServiceMetadataUpdated, OnServicesUpdated, OnActionsLoaded;


        public GameObject InteractiveObjects, PuckPrefab;

        public event EventHandler OnActionObjectsUpdated;

        public bool ActionsReady, ServicesLoaded, ActionObjectsLoaded;

        public Dictionary<string, ActionObjectMetadata> ActionObjectMetadata {
            get => actionObjectsMetadata; set => actionObjectsMetadata = value;
        }
        public Dictionary<string, ServiceMetadata> ServicesMetadata {
            get => servicesMetadata;
            set => servicesMetadata = value;
        }
        public Dictionary<string, Service> ServicesData {
            get => servicesData;
            set => servicesData = value;
        }

        private void Awake() {
            ActionsReady = false;
            ServicesLoaded = false;
            ActionObjectsLoaded = false;
        }

        private void Start() {
            GameManager.Instance.OnSceneChanged += SceneChanged;
        }

        private void Update() {
            if (!ActionsReady && ActionObjectsLoaded && ServicesLoaded) {
                
                foreach (ActionObjectMetadata ao in ActionObjectMetadata.Values) {
                    
                    if (!ao.ActionsLoaded) {
                       
                        return;
                    }
                }
                foreach (ServiceMetadata sm in ServicesMetadata.Values) {
                    if (!sm.ActionsLoaded) {
                        return;
                    }
                }
                ActionsReady = true;
                OnActionsLoaded?.Invoke(this, EventArgs.Empty);
                enabled = false;
            }
        }

        public void Clear() {
            servicesData.Clear();
            servicesMetadata.Clear();
            actionObjectsMetadata.Clear();
            ActionsReady = false;
            ServicesLoaded = false;
            ActionObjectsLoaded = false;
        }

        private void SceneChanged(object sender, EventArgs e) {
            ServicesData.Clear();
            foreach (IO.Swagger.Model.SceneService sceneService in Scene.Instance.Data.Services) {
                if (servicesMetadata.TryGetValue(sceneService.Type, out ServiceMetadata serviceMetadata)) {
                    ServicesData.Add(sceneService.Type, new Service(sceneService, serviceMetadata));
                }
            }
            OnServicesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateServicesMetadata(List<IO.Swagger.Model.ServiceTypeMeta> newServices) {
            foreach (IO.Swagger.Model.ServiceTypeMeta newServiceMeta in newServices) {
                ServiceMetadata serviceMetadata = new ServiceMetadata(newServiceMeta);
                ServicesMetadata[serviceMetadata.Type] = serviceMetadata;
                UpdateActionsOfService(serviceMetadata);
            }
            ServicesLoaded = true;
            OnServiceMetadataUpdated?.Invoke(this, EventArgs.Empty);
        }

        public bool ServiceInScene(string type) {
            return ServicesData.ContainsKey(type);
        }

        public Service GetService(string type) {
            if (ServicesData.TryGetValue(type, out Service sceneService)) {
                return sceneService;
            } else {
                throw new KeyNotFoundException("Service not in scene!");
            }
        }

        public List<string> GetRobots() {
            HashSet<string> robots = new HashSet<string>();
            foreach (Base.ActionObject actionObject in Base.Scene.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    robots.Add(actionObject.Data.Id);
                }
            }
            foreach (Service service in servicesData.Values) {
                if (service.Metadata.Robot) {
                    foreach (string s in service.GetRobots()) {
                        robots.Add(s);
                    }
                }                    
            }
            return robots.ToList<string>();
        }


        private async void UpdateActionsOfActionObject(ActionObjectMetadata actionObject) {
            actionObject.ActionsMetadata = ParseActions(await GameManager.Instance.GetActions(actionObject.Type));
            actionObject.ActionsLoaded = true;
        }

        private async void UpdateActionsOfService(ServiceMetadata serviceMetadata) {
            serviceMetadata.ActionsMetadata = ParseActions(await GameManager.Instance.GetActions(serviceMetadata.Type));
            serviceMetadata.ActionsLoaded = true;
        }

        private Dictionary<string, ActionMetadata> ParseActions(List<IO.Swagger.Model.ObjectAction> actions) {
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

        public async Task UpdateObjects(List<IO.Swagger.Model.ObjectTypeMeta> newActionObjectsMetadata) {
            ActionsReady = false;
            actionObjectsMetadata.Clear();
            foreach (IO.Swagger.Model.ObjectTypeMeta metadata in newActionObjectsMetadata) {
                ActionObjectMetadata m = new ActionObjectMetadata(meta: metadata);
                actionObjectsMetadata.Add(metadata.Type, m);
                UpdateActionsOfActionObject(m);
            }
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
            }
            /*foreach (ActionObject ao in InteractiveObjects.GetComponentsInChildren<ActionObject>()) {
                if (!ActionObjectMetadata.ContainsKey(ao.Data.Type)) {
                    Destroy(ao.gameObject);
                }
            }*/
            enabled = true;
            ActionObjectsLoaded = true;
            OnActionObjectsUpdated?.Invoke(this, EventArgs.Empty);
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


        public Dictionary<IActionProvider, List<ActionMetadata>> GetAllActionsOfObject(ActionObject interactiveObject) {
            Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = new Dictionary<IActionProvider, List<ActionMetadata>>();
            foreach (ActionObject ao in InteractiveObjects.GetComponentsInChildren<ActionObject>()) {
                if (ao == interactiveObject) {
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
                        continue;
                    }
                    actionsMetadata[ao] = aom.ActionsMetadata.Values.ToList();
                } else {
                    List<ActionMetadata> freeActions = new List<ActionMetadata>();
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
                        continue;
                    }
                    foreach (ActionMetadata am in aom.ActionsMetadata.Values) {
                        if (am.Meta.Free)
                            freeActions.Add(am);
                    }
                    if (freeActions.Count > 0) {
                        actionsMetadata[ao] = freeActions;
                    }
                }
            }
            foreach (Service sceneService in servicesData.Values) {
                actionsMetadata[sceneService] = sceneService.Metadata.ActionsMetadata.Values.ToList();
            }

            return actionsMetadata;
        }




    }
}

