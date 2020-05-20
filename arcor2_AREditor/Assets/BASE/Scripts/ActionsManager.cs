using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using IO.Swagger.Model;
using System.Threading.Tasks;

namespace Base {

    public class ServiceEventArgs : EventArgs {
        public Service Data {
            get; set;
        }

        public ServiceEventArgs(Service data) {
            Data = data;
        }
    }
    public class ActionsManager : Singleton<ActionsManager> {
        public delegate void ServiceEventHandler(object sender, ServiceEventArgs args);

        private Dictionary<string, ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
        private Dictionary<string, ServiceMetadata> servicesMetadata = new Dictionary<string, ServiceMetadata>();
        private Dictionary<string, Service> servicesData = new Dictionary<string, Service>();

        public Action CurrentlyRunningAction = null;
        
        public event EventHandler OnServiceMetadataUpdated, OnActionsLoaded;

        public event ServiceEventHandler OnServicesUpdated;

        public GameObject ParameterInputPrefab, ParameterDropdownPrefab, ParameterDropdownPosesPrefab, ParameterDropdownJointsPrefab, ActionPointOrientationPrefab;

        public GameObject InteractiveObjects;

        public event GameManager.StringEventHandler OnActionObjectsUpdated;

        public bool ActionsReady, ServicesLoaded, ActionObjectsLoaded;

        public Dictionary<string, RobotMeta> RobotsMeta = new Dictionary<string, RobotMeta>();

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
            Debug.Assert(ParameterInputPrefab != null);
            Debug.Assert(ParameterDropdownPrefab != null);
            Debug.Assert(ParameterDropdownPosesPrefab != null);
            Debug.Assert(ParameterDropdownJointsPrefab != null);
            Debug.Assert(InteractiveObjects != null);
            GameManager.Instance.OnDisconnectedFromServer += OnOpenDisconnectedScreen;
        }
        
        private void OnOpenDisconnectedScreen(object sender, EventArgs args) {
            Init();
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

        public void Init() {
            servicesData.Clear();
            servicesMetadata.Clear();
            actionObjectsMetadata.Clear();
            ActionsReady = false;
            ServicesLoaded = false;
            ActionObjectsLoaded = false;
        }        

        public void UpdateService(IO.Swagger.Model.SceneService sceneService) {
            Debug.Assert(ServicesData.ContainsKey(sceneService.Type));
            ServicesData.TryGetValue(sceneService.Type, out Service service);
            service.Data = sceneService;
            OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
        }

        public async Task AddService(IO.Swagger.Model.SceneService sceneService, bool loadResources) {
            Debug.Assert(!ServicesData.ContainsKey(sceneService.Type));
            if (servicesMetadata.TryGetValue(sceneService.Type, out ServiceMetadata serviceMetadata)) {
                Service service = new Service(sceneService, serviceMetadata);
                if (loadResources && service.IsRobot()) {
                    await service.LoadRobots();
                }
                ServicesData.Add(sceneService.Type, service);
                OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
            }
            
        }

        public void RemoveService(string serviceType) {
            Debug.Assert(ServicesData.ContainsKey(serviceType));
            ServicesData.TryGetValue(serviceType, out Service service);
            ServicesData.Remove(serviceType);
            OnServicesUpdated?.Invoke(this, new ServiceEventArgs(service));
        }


        public void ClearServices() {
            List<string> servicesKeys = servicesData.Keys.ToList();
            foreach (string service in servicesKeys) {
                RemoveService(service);
            }
        }

        public async Task UpdateServicesMetadata(List<IO.Swagger.Model.ServiceTypeMeta> newServices) {
            foreach (IO.Swagger.Model.ServiceTypeMeta newServiceMeta in newServices) {
                ServiceMetadata serviceMetadata = new ServiceMetadata(newServiceMeta);
                ServicesMetadata[serviceMetadata.Type] = serviceMetadata;
                await UpdateActionsOfService(serviceMetadata);
            }
            ServicesLoaded = true;
            OnServiceMetadataUpdated?.Invoke(this, EventArgs.Empty);
        }

        // TODO - solve somehow better.. perhaps own class for robot objects and services?
        internal void UpdateRobotsMetadata(List<RobotMeta> list) {
            RobotsMeta.Clear();
            foreach (RobotMeta robotMeta in list) {
                RobotsMeta[robotMeta.Type] = robotMeta;
            }
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

        

        public List<string> GetRobotsNames() {
            HashSet<string> robots = new HashSet<string>();
            foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot) {
                    robots.Add(actionObject.Data.Name);
                }
            }
            foreach (Service service in servicesData.Values) {
                if (service.Metadata.Robot) {
                    foreach (string s in service.GetRobotsNames()) {
                        robots.Add(s);
                    }
                }                    
            }
            return robots.ToList<string>();
        }

        public string RobotNameToId(string robotName) {
            foreach (Base.ActionObject actionObject in Base.SceneManager.Instance.ActionObjects.Values) {
                if (actionObject.ActionObjectMetadata.Robot && actionObject.Data.Name == robotName) {
                    return actionObject.Data.Id;
                }
            }
            foreach (Service service in servicesData.Values) {
                if (service.Metadata.Robot) {
                    foreach (string s in service.GetRobotsNames()) {
                        if (s == robotName)
                            return s;
                    }
                }
            }
            throw new KeyNotFoundException("Robot with name " + robotName + " does not exists!");
        }

        

        private async Task UpdateActionsOfActionObject(ActionObjectMetadata actionObject) {
            if (!actionObject.Disabled)
                actionObject.ActionsMetadata = ParseActions(await GameManager.Instance.GetActions(actionObject.Type));
            if (actionObject.ActionsMetadata == null) {
                actionObject.Disabled = true;
                actionObject.Problem = "Failed to load actions";
            }
            actionObject.ActionsLoaded = true;
        }

        private async Task UpdateActionsOfService(ServiceMetadata serviceMetadata) {
            if (!serviceMetadata.Disabled) {
                serviceMetadata.ActionsMetadata = ParseActions(await GameManager.Instance.GetActions(serviceMetadata.Type));
            }
            if (serviceMetadata.ActionsMetadata == null) {
                serviceMetadata.Disabled = true;
                serviceMetadata.Problem = "Failed to load actions";
            }
            serviceMetadata.ActionsLoaded = true;
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

        public async Task UpdateObjects(List<IO.Swagger.Model.ObjectTypeMeta> newActionObjectsMetadata, string highlighteObject = null) {
            ActionsReady = false;
            actionObjectsMetadata.Clear();
            foreach (IO.Swagger.Model.ObjectTypeMeta metadata in newActionObjectsMetadata) {
                ActionObjectMetadata m = new ActionObjectMetadata(meta: metadata);
                await UpdateActionsOfActionObject(m);
                actionObjectsMetadata.Add(metadata.Type, m);
            }
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
            }
            enabled = true;
            ActionObjectsLoaded = true;
            OnActionObjectsUpdated?.Invoke(this, new Base.StringEventArgs(highlighteObject));
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

        public Dictionary<IActionProvider, List<ActionMetadata>> GetAllFreeActions() {
            Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = new Dictionary<IActionProvider, List<ActionMetadata>>();
            foreach (ActionObject ao in SceneManager.Instance.ActionObjects.Values) {               
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
            foreach (Service sceneService in servicesData.Values) {
                actionsMetadata[sceneService] = sceneService.Metadata.ActionsMetadata.Values.ToList();
            }

            return actionsMetadata;
        }

        public Dictionary<IActionProvider, List<ActionMetadata>> GetAllActionsOfObject(ActionObject actionObject) {
            Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = new Dictionary<IActionProvider, List<ActionMetadata>>();
            foreach (ActionObject ao in SceneManager.Instance.ActionObjects.Values) {
                if (ao == actionObject) {
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

