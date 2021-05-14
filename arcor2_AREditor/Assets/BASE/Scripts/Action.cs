using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using IO.Swagger.Model;
using TMPro;

namespace Base {
    public abstract class Action : InteractiveObject {
        // Metadata of this Action
        private ActionMetadata metadata;
        // Dictionary of all action parameters for this Action
        private Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();
        
        public InputOutput Input;
        public PuckOutput Output;

        public GameObject InputArrow, OutputArrow;
        public IActionProvider ActionProvider;

        public ActionPoint ActionPoint;

        public IO.Swagger.Model.Action Data = null;

        public TextMeshPro NameText;

        public bool ActionBeingExecuted = false;
        
        public virtual void Init(IO.Swagger.Model.Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider) {

            ActionPoint = ap;
            this.metadata = metadata;
            ActionProvider = actionProvider;
            Data = projectAction;

            if (!Base.ProjectManager.Instance.ProjectMeta.HasLogic) {
                InputArrow.gameObject.SetActive(false);
                OutputArrow.gameObject.SetActive(false);
            }


            UpdateName(Data.Name);
            if (actionProvider != null)
                UpdateType();

            SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
            Input.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Input);
            Output.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Output);
        }

        public virtual void ActionUpdateBaseData(IO.Swagger.Model.BareAction action) {
            Data.Name = action.Name;
        }

        public virtual void ActionUpdate(IO.Swagger.Model.Action action, bool updateConnections = false) {

            // Updates (or creates new) parameters of current action
            foreach (IO.Swagger.Model.ActionParameter projectActionParameter in action.Parameters) {
                try {
                    // If action parameter exist in action dictionary, then just update that parameter value (it's metadata will always be unchanged)
                    if (Parameters.TryGetValue(projectActionParameter.Name, out Parameter actionParameter)) {
                        actionParameter.UpdateActionParameter(DataHelper.ActionParameterToParameter(projectActionParameter));
                    }
                    // Otherwise create a new action parameter, load metadata for it and add it to the dictionary of action
                    else {
                        // Loads metadata of specified action parameter - projectActionParameter. Action.Metadata is created when creating Action.
                        IO.Swagger.Model.ParameterMeta actionParameterMetadata = Metadata.GetParamMetadata(projectActionParameter.Name);

                        actionParameter = new Parameter(actionParameterMetadata, projectActionParameter.Type, projectActionParameter.Value);
                        Parameters.Add(actionParameter.Name, actionParameter);
                    }
                } catch (ItemNotFoundException ex) {
                    Debug.LogError(ex);
                }
            }
            
        }

        public void UpdateType() {
            Data.Type = GetActionType();
        }        

        public virtual void UpdateName(string newName) {
            Data.Name = newName;

        }

        public string GetActionType() {
            return ActionProvider.GetProviderType() + "/" + metadata.Name; //TODO: AO|Service/Id
        }

        public void DeleteAction() {
            // Delete connection on input and set the gameobject that was connected through its output to the "end" value.
            /*if (Input.Connection != null) {
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
            }*/

            Destroy(gameObject);

            ActionPoint.Actions.Remove(Data.Id);

        }

        public Dictionary<string, Parameter> Parameters {
            get => parameters; set => parameters = value;
        }

        public ActionMetadata Metadata {
            get => metadata; set => metadata = value;
        }

        public virtual void RunAction() {

        }

        public virtual void StopAction() {

        }




        public static Tuple<string, string> ParseActionType(string type) {
            if (!type.Contains("/"))
                throw new FormatException("Action type has to be in format action_provider_id/action_type");
            return new Tuple<string, string>(type.Split('/')[0], type.Split('/')[1]);
        }

        public static string BuildActionType(string actionProviderId, string actionType) {
            return actionProviderId + "/" + actionType;
        }

        public List<Flow> GetFlows() {
            return Data.Flows;
        }

        public override string GetId() {
            return Data.Id;
        }

        public async override Task<RequestResult> Movable() {
            return new RequestResult(false, "Actions could not be moved");
        }

        protected override void OnDestroy() {
            SelectorMenu.Instance.DestroySelectorItem(Input);
            SelectorMenu.Instance.DestroySelectorItem(Output);
            base.OnDestroy();
        }
    }

}
