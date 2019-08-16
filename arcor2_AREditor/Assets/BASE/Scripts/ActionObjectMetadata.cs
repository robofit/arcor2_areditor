using System.Collections.Generic;

namespace Base {
    public class ActionObjectMetadata {

        string type, description, baseObject;
        bool actionsLoaded, robot;
        Dictionary<string, ActionMetadata> actionsMetadata = new Dictionary<string, ActionMetadata>();

        public ActionObjectMetadata(string type, string description, string baseObject) {
            Type = type;
            Description = description;
            BaseObject = baseObject;
            ActionsLoaded = false;
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
    }

}
