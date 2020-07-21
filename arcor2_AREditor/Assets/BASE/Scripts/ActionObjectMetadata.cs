using System.Collections.Generic;
using ARServer.Models;
using IO.Swagger.Model;
using UnityEngine;


namespace Base {
    public class ActionObjectMetadata : ObjectTypeMeta {

        private Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();
        private bool robot, actionsLoaded;

        public ActionObjectMetadata(ObjectTypeMeta meta) : base(_abstract: meta.Abstract,
                                                                _base: meta.Base,
                                                                builtIn: meta.BuiltIn,
                                                                description: meta.Description,
                                                                disabled: meta.Disabled,
                                                                hasPose: meta.HasPose,
                                                                needsParentType: meta.NeedsParentType,
                                                                objectModel: meta.ObjectModel,
                                                                problem: meta.Problem,
                                                                settings: meta.Settings,
                                                                type: meta.Type) {
           
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
