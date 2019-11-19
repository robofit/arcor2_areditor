using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Base {
    public class ServiceManager : Singleton<ServiceManager> {
        public Dictionary<string, IO.Swagger.Model.ServiceMeta> ServicesMetadata = new Dictionary<string, IO.Swagger.Model.ServiceMeta>();
        private Dictionary<string, IO.Swagger.Model.SceneService> ServicesData = new Dictionary<string, IO.Swagger.Model.SceneService>();

        public event EventHandler OnServiceMetadataUpdated, OnServicesUpdated;

        public bool ServicesReady;

        private void Awake() {
            ServicesReady = false;
        }

        private void Start() {
            GameManager.Instance.OnSceneChanged += SceneChanged;
        }

        private void SceneChanged(object sender, EventArgs e) {
            ServicesData.Clear();
            Debug.LogError(ServicesMetadata.Count);
            foreach (IO.Swagger.Model.SceneService sceneService in Scene.Instance.Data.Services) {
                ServicesData.Add(sceneService.Type, sceneService);
            }
            OnServicesUpdated?.Invoke(this, EventArgs.Empty);
         }

        public void UpdateServicesMetadata(List<IO.Swagger.Model.ServiceMeta> newServices) {
            foreach (IO.Swagger.Model.ServiceMeta serviceMeta in newServices) {
                ServicesMetadata[serviceMeta.Type] = serviceMeta;
            }
            OnServiceMetadataUpdated?.Invoke(this, EventArgs.Empty);
            ServicesReady = true;
        }

        public bool ServiceInScene(string type) {
            return ServicesData.ContainsKey(type);
        }

        public IO.Swagger.Model.SceneService GetService(string type) {
            if (ServicesData.TryGetValue(type, out IO.Swagger.Model.SceneService sceneService)) {
                return sceneService;
            } else {
                throw new KeyNotFoundException("Service not in scene!");
            }
        }
    }

}
