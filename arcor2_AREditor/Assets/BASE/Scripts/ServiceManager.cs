using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Base {
    public class ServiceManager : Singleton<ServiceManager> {
        public List<IO.Swagger.Model.ServiceMeta> ServicesMetadata = new List<IO.Swagger.Model.ServiceMeta>();

        public event EventHandler OnServicesUpdated;

        public void UpdateServicesMetadata(List<IO.Swagger.Model.ServiceMeta> newServices) {
            ServicesMetadata = newServices;
            OnServicesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public bool ServiceInScene(string type) {
            foreach (IO.Swagger.Model.SceneService sceneService in Scene.Instance.Data.Services) {
                if (sceneService.Type == type)
                    return true;
            }
            return false;
        }
    }

}
