using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Base {
    public abstract class Action : Clickable {
        private ActionMetadata metadata;
        private Dictionary<string, ActionParameter> parameters = new Dictionary<string, ActionParameter>();
        private ActionObject actionObject;
        private IO.Swagger.Model.SceneService service;
        public PuckInput Input;
        public PuckOutput Output;
        private IActionProvider actionProvider;

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action("", new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionParameter>(), "");
        public void Init(string id, ActionMetadata metadata, ActionPoint ap, bool generateData, IActionProvider actionProvider, bool updateProject = true) {
           
            this.metadata = metadata;
            this.actionProvider = actionProvider;
            
            if (generateData) {
                foreach (IO.Swagger.Model.ObjectActionArg actionParameterMetadata in this.metadata.ActionArgs) {
                    ActionParameter actionParameter = new ActionParameter(actionParameterMetadata);
                    if (actionParameter.ActionParameterMetadata.Type == IO.Swagger.Model.ObjectActionArg.TypeEnum.Pose) {
                        actionParameter.Value = ap.ActionObject.Data.Id + "." + ap.Data.Id;
                    } else {
                        switch (actionParameter.Type) {
                            case IO.Swagger.Model.ActionParameter.TypeEnum.Relativepose:
                                actionParameter.Value = (string) Regex.Replace(new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(), position: new IO.Swagger.Model.Position()).ToJson(), @"\t|\n|\r", "");
                                break;
                        }
                    }
                    Parameters[actionParameter.ActionParameterMetadata.Name] = actionParameter;
                }
                foreach (InputOutput io in GetComponentsInChildren<InputOutput>()) {
                    io.InitData();
                }
            }
            

            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }

            UpdateId(id, updateProject);            
            UpdateType();
        }

        

        public void UpdateType() {
            Data.Type = GetActionType();
        }

        public virtual void UpdateId(string newId, bool updateProject = true) {
            Data.Id = newId;
            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public string GetActionType() {
            return actionProvider.GetProviderName() + "/" + metadata.Name; //TODO: AO|Service/Id
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
        
    }

}
