
namespace Base {
    public class Service : IActionProvider {

        private ServiceMetadata metadata;
        public IO.Swagger.Model.SceneService Data = new IO.Swagger.Model.SceneService("", "");

        public ServiceMetadata Metadata {
            get => metadata;
            set => metadata = value;
        }

        public Service(IO.Swagger.Model.SceneService service, ServiceMetadata metadata) {
            Metadata = metadata;
            Data = service;
        }

        public string GetProviderName() {
            return Data.Type;
        }


        public ActionMetadata GetActionMetadata(string action_id) {
            if (Metadata.ActionsLoaded) {
                if (Metadata.ActionsMetadata.TryGetValue(action_id, out ActionMetadata actionMetadata)) {
                    return actionMetadata;
                } else {
                    throw new ItemNotFoundException("Metadata not found");
                }
            }
            return null; //TODO: throw exception
        }
    }

}

