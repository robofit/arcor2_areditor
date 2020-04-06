using System.Collections.Generic;

using System.Threading.Tasks;
namespace Base {
    public class Service : IActionProvider {

        public Dictionary<string, List<string>> Robots = new Dictionary<string, List<string>>();
        private ServiceMetadata metadata;
        public IO.Swagger.Model.SceneService Data = new IO.Swagger.Model.SceneService("", "");

        public ServiceMetadata Metadata {
            get => metadata;
            set => metadata = value;
        }

        public Service(IO.Swagger.Model.SceneService service, ServiceMetadata metadata) {
            Metadata = metadata;
            Data = service;
            if (metadata.Robot) {
                LoadRobots();
            }
        }

        public string GetProviderName() {
            return Data.Type;
        }


        public List<string> GetRobots() {
            if (!IsRobot())
                return new List<string>();
            else {
                return new List<string>(Robots.Keys);
            }
        }

        public async Task LoadRobots() {
            Robots.Clear();
            if (IsRobot()) {
                List<string> robots = await GameManager.Instance.GetActionParamValues(metadata.Type, "robot_id", new List<IO.Swagger.Model.IdValue>());
                foreach (string robot in robots) {
                    IO.Swagger.Model.IdValue idValue = new IO.Swagger.Model.IdValue(id: "robot_id", value: robot);
                    List<IO.Swagger.Model.IdValue> args = new List<IO.Swagger.Model.IdValue> {
                        idValue
                    };
                    List<string> ee = await GameManager.Instance.GetActionParamValues(metadata.Type, "end_effector_id", args);
                    Robots[robot] = ee;
                }
            }
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

        public List<string> GetEndEffectors(string robot_id) {
            if (Robots.TryGetValue(robot_id, out List<string> endeffectors))
                return endeffectors;
            else
                return new List<string>();
        }

        public bool IsRobot() {
            return metadata.Robot;
        }

        public string GetProviderId() {
            return Data.Type;
        }
    }

}

