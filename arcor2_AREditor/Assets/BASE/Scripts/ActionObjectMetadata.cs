using System.Collections.Generic;
using ARServer.Models;
using IO.Swagger.Model;
using UnityEngine;


namespace Base {
    public class ActionObjectMetadata {

        private IO.Swagger.Model.ObjectTypeMeta metaData;
        private Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();
        private bool robot, actionsLoaded;


        /*public ActionObjectMetadata(string type, string description, string baseObject, IO.Swagger.Model.ObjectModel model, List<string> needsServices, bool @abstract) {
            Type = type;
            Description = description;
            BaseObject = baseObject;
            ActionsLoaded = false;
            Model = model;
            NeedsServices = needsServices;
            Abstract = @abstract;
        }*/


        public ActionObjectMetadata(IO.Swagger.Model.ObjectTypeMeta metaData) {
            MetaData = metaData;
        }

        
        public ObjectTypeMeta MetaData {
            get => metaData;
            set => metaData = value;
        }
        public bool Robot {
            get => robot;
            set => robot = value;
        }
        public bool ActionsLoaded {
            get => actionsLoaded;
            set => actionsLoaded = value;
        }
        public Dictionary<string, ActionMetadata> ActionsMetadata {
            get => actionsMetadata;
            set => actionsMetadata = value;
        }
    }

}
