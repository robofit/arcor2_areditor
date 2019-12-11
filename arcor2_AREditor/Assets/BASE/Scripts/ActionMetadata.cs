using System.Collections.Generic;

namespace Base {
    public class ActionMetadata : IO.Swagger.Model.ObjectAction {

        public ActionMetadata(IO.Swagger.Model.ObjectAction metadata) :
            base(actionArgs: metadata.ActionArgs, meta: metadata.Meta, name: metadata.Name, origins: metadata.Origins, returns: metadata.Returns, description: metadata.Description) {

        }

        public IO.Swagger.Model.ObjectActionArg GetParamMetadata(string name) {
            foreach (IO.Swagger.Model.ObjectActionArg objectActionArg in ActionArgs) {
                if (objectActionArg.Name == name)
                    return objectActionArg;
            }
            throw new ItemNotFoundException("Action does not exist");
        }


    }

}
