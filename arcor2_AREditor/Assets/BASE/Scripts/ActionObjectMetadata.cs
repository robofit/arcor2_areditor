using System.Collections.Generic;
using ARServer.Models;
using UnityEngine;


namespace Base {
    public class ActionObjectMetadata {

        private string type, description, baseObject;
        private bool actionsLoaded, robot;
        private Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();
        private IO.Swagger.Model.ObjectModel model;
        private List<string> needsServices = new List<string>();


        public ActionObjectMetadata(string type, string description, string baseObject, IO.Swagger.Model.ObjectModel model, List<string> needsServices) {
            Type = type;
            Description = description;
            BaseObject = baseObject;
            ActionsLoaded = false;
            Model = model;
            NeedsServices = needsServices;
        }

        public string Type {
            get => type; set => type = value;
        }
        public string Description {
            get => description; set => description = value;
        }
        public bool ActionsLoaded {
            get => actionsLoaded; set => actionsLoaded = value;
        }
        public Dictionary<string, ActionMetadata> ActionsMetadata {
            get => actionsMetadata; set => actionsMetadata = value;
        }
        public bool Robot {
            get => robot; set => robot = value;
        }
        public string BaseObject {
            get => baseObject; set => baseObject = value;
        }
        public IO.Swagger.Model.ObjectModel Model {
            get => model;
            set => model = value;
        }
        public List<string> NeedsServices {
            get => needsServices;
            set => needsServices = value;
        }
    }

}
