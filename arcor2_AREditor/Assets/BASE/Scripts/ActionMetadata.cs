using System.Collections.Generic;
using IO.Swagger.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Base {
    public class ActionMetadata : IO.Swagger.Model.ObjectAction {

        public Dictionary<string, Base.ParameterMetadata> ParametersMetadata = new Dictionary<string, Base.ParameterMetadata>();

        public ActionMetadata(IO.Swagger.Model.ObjectAction metadata) :
            base(parameters: metadata.Parameters, meta: metadata.Meta, name: metadata.Name, origins: metadata.Origins, returns: metadata.Returns, description: metadata.Description, problem: metadata.Problem, disabled: metadata.Disabled) {
            foreach (IO.Swagger.Model.ParameterMeta meta in Parameters) {
                ParametersMetadata.Add(meta.Name, new Base.ParameterMetadata(meta));
            }
        }

        

        /// <summary>
        /// Returns medatada for specific action parameter defined by name.
        /// </summary>
        /// <param name="name">Name of the action parameter.</param>
        /// <returns>Returns metadata of action parameter - ActionParameterMeta</returns>
        public IO.Swagger.Model.ParameterMeta GetParamMetadata(string name) {
            foreach (IO.Swagger.Model.ParameterMeta actionParameterMeta in Parameters) {
                if (actionParameterMeta.Name == name)
                    return actionParameterMeta;
            }
            throw new ItemNotFoundException("Action does not exist");
        }

        public List<Flow> GetFlows(string actionName) {
            List<string> outputs = new List<string>();
            foreach (string output in Returns) {
                outputs.Add(actionName + "_" + output);
            }
            return new List<Flow> {
                new Flow(type: Flow.TypeEnum.Default, outputs: outputs)
            };
        }

        public List<IO.Swagger.Model.ActionParameter> GetDefaultParameters() {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (ParameterMetadata actionParameterMeta in ParametersMetadata.Values) {
                if (actionParameterMeta.DynamicValue) {

                } else {
                    parameters.Add(new ActionParameter(name: actionParameterMeta.Name, type: actionParameterMeta.Type, value: JsonConvert.SerializeObject(actionParameterMeta.GetDefaultValue())));
                }
            }

            return parameters;
        }



    }

}
