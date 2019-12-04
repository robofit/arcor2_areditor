using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class ServiceMetadata : IO.Swagger.Model.ServiceTypeMeta {
        private Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();
        private bool robot, actionsLoaded;

        public ServiceMetadata(IO.Swagger.Model.ServiceTypeMeta serviceMeta) : base(configurationIds: serviceMeta.ConfigurationIds, description: serviceMeta.Description,
            type: serviceMeta.Type) {
            if (Type.Contains("Robot")) {
                robot = true;
            }
        }

        public Dictionary<string, ActionMetadata> ActionsMetadata {
            get => actionsMetadata;
            set => actionsMetadata = value;
        }
        public bool Robot {
            get => robot;
            set => robot = value;
        }
        public bool ActionsLoaded {
            get => actionsLoaded;
            set => actionsLoaded = value;
        }
    }
}

