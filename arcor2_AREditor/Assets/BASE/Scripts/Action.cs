using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Action : MonoBehaviour {
        private ActionMetadata metadata;
        private ActionObject actionObject;

        private Dictionary<string, ActionParameter> parameters = new Dictionary<string, ActionParameter>();

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action();
        public void Init(string id, ActionMetadata metadata, Base.ActionPoint ap, bool updateProject = true) {
            this.metadata = metadata;
            Data.Type = metadata.Name;
            actionObject = ap.ActionObject;
            foreach (ActionParameterMetadata actionParameterMetadata in this.metadata.Parameters.Values) {
                ActionParameter actionParameter = new ActionParameter(actionParameterMetadata);
                if (actionParameter.ActionParameterMetadata.Type == ActionParameterMetadata.Types.ActionPoint) {
                    actionParameter.Data.Value = ap.ActionObject.Data.Id + "." + ap.Data.Id;
                } else {
                    actionParameter.Data.Value = actionParameter.ActionParameterMetadata.DefaultValue;
                }
                Parameters[actionParameter.ActionParameterMetadata.Name] = actionParameter;
            }

            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }

            UpdateId(id, updateProject);
        }

        public virtual void UpdateId(string newId, bool updateProject = true) {
            Data.Id = newId;
            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public void DeleteAction(bool updateProject = true) {
            foreach (InputOutput io in GetComponentsInChildren<InputOutput>()) {
                if (io.Connection != null)
                    Destroy(io.Connection.gameObject);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public Dictionary<string, ActionParameter> Parameters {
            get => parameters; set => parameters = value;
        }
        public ActionMetadata Metadata {
            get => metadata; set => metadata = value;
        }
        public ActionObject ActionObject {
            get => actionObject; set => actionObject = value;
        }
        
    }

}
