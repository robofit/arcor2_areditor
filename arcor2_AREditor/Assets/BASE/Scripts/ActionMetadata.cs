using System.Collections.Generic;

namespace Base {
    public class ActionMetadata : IO.Swagger.Model.ObjectAction {

        public ActionMetadata(IO.Swagger.Model.ObjectAction metadata) :
            base(actionArgs: metadata.ActionArgs, meta: metadata.Meta, name: metadata.Name, origins: metadata.Origins, returns: metadata.Returns) {

        }

        public IO.Swagger.Model.ObjectActionArgs GetParamMetadata(string name) {
            foreach (IO.Swagger.Model.ObjectActionArgs objectActionArgs in ActionArgs) {
                if (objectActionArgs.Name == name)
                    return objectActionArgs;
            }
            throw new ItemNotFoundException("Action does not exist");
        }
    }

}
