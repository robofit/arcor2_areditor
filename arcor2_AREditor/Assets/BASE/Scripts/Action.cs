using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Base {
    public abstract class Action : Clickable {
        // Metadata of this Action
        private ActionMetadata metadata;
        // Dictionary of all action parameters for this Action
        private Dictionary<string, ActionParameter> parameters = new Dictionary<string, ActionParameter>();
        
        public PuckInput Input;
        public PuckOutput Output;

        public GameObject InputArrow, OutputArrow;
        public IActionProvider ActionProvider;

        public ActionPoint ActionPoint;

        public IO.Swagger.Model.Action Data = new IO.Swagger.Model.Action("", new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionIO>(), new List<IO.Swagger.Model.ActionParameter>(), "", "");

        public async Task Init(string uuid, string id, ActionMetadata metadata, ActionPoint ap, bool generateData, IActionProvider actionProvider, bool updateProject = true) {

            ActionPoint = ap;
            this.metadata = metadata;
            this.ActionProvider = actionProvider;

            if (generateData) {
                List<ActionParameter> dynamicParameters = new List<ActionParameter>();
                foreach (IO.Swagger.Model.ActionParameterMeta actionParameterMetadata in this.metadata.Parameters) {
                    
                    ActionParameter actionParameter = new ActionParameter(actionParameterMetadata, this);
                    
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
                                        paramValue = parameter.GetValue<string>();
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

                Data.Uuid = uuid;
            }

            if (!GameManager.Instance.CurrentProject.HasLogic) {
                InputArrow.gameObject.SetActive(false);
                OutputArrow.gameObject.SetActive(false);
            }


            UpdateId(id, false);
            //UpdateUuid(Guid.NewGuid().ToString());
            UpdateType();

            if (updateProject) {
                GameManager.Instance.UpdateProject();
            }


        }

        public virtual void ActionUpdate(IO.Swagger.Model.Action aData = null) {
            if (aData != null)
                Data = aData;
        }

        public void UpdateType() {
            Data.Type = GetActionType();
        }        

        public virtual void UpdateId(string newId, bool updateProject = true) {
            Data.Id = newId;

            // update changed IDs in connected actions
            Action actionOnInput = Scene.Instance.GetActionById(Input.Data.Default);
            if (actionOnInput != null) {
                actionOnInput.Output.Data.Default = newId;
            }
            Action actionOnOutput = Scene.Instance.GetActionById(Output.Data.Default);
            if (actionOnOutput != null) {
                actionOnOutput.Input.Data.Default = newId;
            }

            if (updateProject)
                GameManager.Instance.UpdateProject();
        }

        public string GetActionType() {
            return ActionProvider.GetProviderName() + "/" + metadata.Name; //TODO: AO|Service/Id
        }

        public void DeleteAction(bool updateProject = true) {
            // Delete connection on input and set the gameobject that was connected through its output to the "end" value.
            if (Input.Connection != null) {
                InputOutput connectedActionIO = Input.Connection.target[0].GetComponent<InputOutput>();
                connectedActionIO.Data.Default = "end";
                // Remove the reference in connections manager.
                ConnectionManagerArcoro.Instance.Connections.Remove(Input.Connection);
                Destroy(Input.Connection.gameObject);
            }
            // Delete connection on output and set the gameobject that was connected through its input to the "start" value.
            if (Output.Connection != null) {
                InputOutput connectedActionIO = Output.Connection.target[1].GetComponent<InputOutput>();
                connectedActionIO.Data.Default = "start";
                // Remove the reference in connections manager.
                ConnectionManagerArcoro.Instance.Connections.Remove(Output.Connection);
                Destroy(Output.Connection.gameObject);
            }

            Destroy(gameObject);

            ActionPoint.Actions.Remove(Data.Uuid);

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
