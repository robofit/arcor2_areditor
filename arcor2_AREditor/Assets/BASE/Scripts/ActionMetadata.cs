using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class ActionMetadata : IO.Swagger.Model.ObjectAction {

        public ActionMetadata(IO.Swagger.Model.ObjectAction metadata) :
            base(parameters: metadata.Parameters, meta: metadata.Meta, name: metadata.Name, origins: metadata.Origins, returns: metadata.Returns, description: metadata.Description, problem: metadata.Problem, disabled: metadata.Disabled) {
        }

        /// <summary>
        /// Returns medatada for specific action parameter defined by name.
        /// </summary>
        /// <param name="name">Name of the action parameter.</param>
        /// <returns>Returns metadata of action parameter - ActionParameterMeta</returns>
        public IO.Swagger.Model.ActionParameterMeta GetParamMetadata(string name) {
            foreach (IO.Swagger.Model.ActionParameterMeta actionParameterMeta in Parameters) {
                if (actionParameterMeta.Name == name)
                    return actionParameterMeta;
            }
            throw new ItemNotFoundException("Action does not exist");
        }


    }

}
