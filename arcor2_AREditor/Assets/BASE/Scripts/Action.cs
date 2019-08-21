using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Action : MonoBehaviour {
        private ActionMetadata _metadata;
        private ActionObject _actionObject;

        private Dictionary<string, ActionParameter> _parameters = new Dictionary<string, ActionParameter>();

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action();
        public void Init(string id, ActionMetadata metadata, Base.ActionPoint ap, bool updateProject = true) {
            _metadata = metadata;
            _actionObject = ap.ActionObject;
            foreach (ActionParameterMetadata actionParameterMetadata in _metadata.Parameters.Values) {
                ActionParameter actionParameter = new ActionParameter(actionParameterMetadata);
                if (actionParameter.ActionParameterMetadata.Type == ActionParameterMetadata.Types.ActionPoint) {
                    JSONObject value = new JSONObject(JSONObject.Type.OBJECT);
                    value.AddField("value", ap.ActionObject.Data.Id + "." + ap.Data.Id);
                    actionParameter.Value = value;
                } else {
                    actionParameter.Value = actionParameter.ActionParameterMetadata.DefaultValue;
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
            get => _parameters; set => _parameters = value;
        }
        public ActionMetadata Metadata {
            get => _metadata; set => _metadata = value;
        }
        public ActionObject ActionObject {
            get => _actionObject; set => _actionObject = value;
        }
        
    }

}
