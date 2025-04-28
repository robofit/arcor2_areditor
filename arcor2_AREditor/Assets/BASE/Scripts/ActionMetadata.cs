using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Newtonsoft.Json;

namespace Base {
    public class ActionMetadata : ObjectAction {

        public Dictionary<string, ParameterMetadata> ParametersMetadata = new();

        public ActionMetadata(ObjectAction metadata) :
            base(parameters: metadata.Parameters, meta: metadata.Meta, name: metadata.Name, origins: metadata.Origins, returns: metadata.Returns, description: metadata.Description, problem: metadata.Problem, disabled: metadata.Disabled) {
            foreach (ParameterMeta meta in Parameters) {
                ParametersMetadata.Add(meta.Name, new ParameterMetadata(meta));
            }
        }

        

        /// <summary>
        /// Returns medatada for specific action parameter defined by name.
        /// </summary>
        /// <param name="name">Name of the action parameter.</param>
        /// <returns>Returns metadata of action parameter - ActionParameterMeta</returns>
        public ParameterMeta GetParamMetadata(string name) {
            foreach (ParameterMeta actionParameterMeta in Parameters) {
                if (actionParameterMeta.Name == name)
                    return actionParameterMeta;
            }
            throw new ItemNotFoundException("Action does not exist");
        }

        public List<Flow> GetFlows(string actionName) {
            List<string> outputs = new();
            foreach (string output in Returns) {
                outputs.Add(actionName + "_" + output);
            }
            return new List<Flow> {
                new(Flow.TypeEnum.Default, outputs)
            };
        }

        public List<ActionParameter> GetDefaultParameters() {
            List<ActionParameter> parameters = new();
            foreach (ParameterMetadata actionParameterMeta in ParametersMetadata.Values) {
                if (actionParameterMeta.DynamicValue) {

                } else {
                    parameters.Add(new ActionParameter(actionParameterMeta.Name, actionParameterMeta.Type, JsonConvert.SerializeObject(actionParameterMeta.GetDefaultValue())));
                }
            }

            return parameters;
        }



    }

}
