using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

namespace Base {
    public abstract class Action : Clickable {
        private ActionMetadata metadata;
        private Dictionary<string, ActionParameter> parameters = new Dictionary<string, ActionParameter>();
        public PuckInput Input;
        public PuckOutput Output;
        public IActionProvider ActionProvider;

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action("", new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionParameter>(), "");
        public async Task Init(string id, ActionMetadata metadata, ActionPoint ap, bool generateData, IActionProvider actionProvider, bool updateProject = true) {

            this.metadata = metadata;
            this.ActionProvider = actionProvider;

            if (generateData) {
                List<ActionParameter> dynamicParameters = new List<ActionParameter>();
                foreach (IO.Swagger.Model.ActionParameterMeta actionParameterMetadata in this.metadata.Parameters) {
                    
                    ActionParameter actionParameter = new ActionParameter(actionParameterMetadata, this);
                    switch (actionParameter.Type) {
                        case "relative_pose":
                            actionParameter.Value = Regex.Replace(new IO.Swagger.Model.Pose(orientation: new IO.Swagger.Model.Orientation(), position: new IO.Swagger.Model.Position()).ToJson(), @"\t|\n|\r", "");
                            break;
                        case "integer_enum":
                            actionParameter.Value = (int) actionParameterMetadata.AllowedValues[0];
                            break;
                        case "string_enum":
                            actionParameter.Value = (string) actionParameterMetadata.AllowedValues[0];
                            break;
                        case "pose":
                            List<string> poses = new List<string>(ap.GetPoses().Keys);
                            if (poses.Count == 0) {
                                actionParameter.Value = "";
                                //TODO: where to get valid ID?
                            } else {
                                actionParameter.Value = (string) ap.ActionObject.Data.Id + "." + ap.Data.Id + "." + poses[0];
                            }
                            break;
                        default:
                            actionParameter.Value = actionParameterMetadata.DefaultValue;
                            break;

                    }
                    if (actionParameterMetadata.DynamicValue) {
                        dynamicParameters.Add(actionParameter);
                    }

                    Parameters[actionParameter.ActionParameterMetadata.Name] = actionParameter;
                }
                foreach (InputOutput io in GetComponentsInChildren<InputOutput>()) {
                    io.InitData();
                }
                int parentCount = 0;
                while (dynamicParameters.Count > 0) {
                    for (int i = dynamicParameters.Count - 1; i >= 0; i--) {
                        ActionParameter actionParameter = dynamicParameters[i];
                        if (actionParameter.ActionParameterMetadata.DynamicValueParents.Count == parentCount) {
                            try {
                                List<IO.Swagger.Model.IdValue> args = new List<IO.Swagger.Model.IdValue>();
                                foreach (string parent in actionParameter.ActionParameterMetadata.DynamicValueParents) {
                                    string paramValue = "";
                                    if (parameters.TryGetValue(parent, out ActionParameter parameter)) {
                                        parameter.GetValue(out paramValue);
                                    } else {
                                        //TODO raise exception
                                    }
                                    args.Add(new IO.Swagger.Model.IdValue(parent, paramValue));
                                }
                                List<string> values = await actionParameter.LoadDynamicValues(args);
                                if (values.Count > 0) {
                                    actionParameter.Value = values[0];
                                } else {
                                    actionParameter.Value = "";
                                }
                            } catch (Exception ex) when (ex is ItemNotFoundException || ex is Base.RequestFailedException) {
                                Debug.LogError(ex);
                            } finally {
                                dynamicParameters.RemoveAt(i);
                            }
                        }
                    }
                    parentCount += 1;
                }
            }

            

            UpdateId(id, false);
            UpdateType();


            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }


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
            return ActionProvider.GetProviderName() + "/" + metadata.Name; //TODO: AO|Service/Id
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
