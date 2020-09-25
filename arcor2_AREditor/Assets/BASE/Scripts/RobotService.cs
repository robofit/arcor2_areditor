using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/*
namespace Base {
    public class RobotService : Service {

        public Dictionary<string, Robot> Robots = new Dictionary<string, Robot>();


        public RobotService(IO.Swagger.Model.SceneService service, ServiceMetadata metadata) : base(service, metadata) {

        }


        public List<Robot> GetRobots() {
            return Robots.Values.ToList();
        }


        public List<string> GetRobotsNames() {
            return Robots.Keys.ToList();
        }

        public async Task LoadRobots() {
            Robots.Clear();
            List<string> robots = await GameManager.Instance.GetActionParamValues(metadata.Type, "robot_id", new List<IO.Swagger.Model.IdValue>());
                foreach (string robot in robots) {
                    IO.Swagger.Model.IdValue idValue = new IO.Swagger.Model.IdValue(id: "robot_id", value: robot);
                    List<IO.Swagger.Model.IdValue> args = new List<IO.Swagger.Model.IdValue> {
                        idValue
                    };
                    List<string> ee = await GameManager.Instance.GetActionParamValues(metadata.Type, "end_effector_id", args);
                    Robots[robot] = new Robot(robot, robot, ee);
                }
        }

        
    }
}*/
