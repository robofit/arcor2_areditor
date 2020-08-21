using System;
using IO.Swagger.Model;

namespace Base {

    public class StringEventArgs : EventArgs {
        public string Data {
            get; set;
        }

        public StringEventArgs(string data) {
            Data = data;
        }
    }

    public class GameStateEventArgs : EventArgs {
        public GameManager.GameStateEnum Data {
            get; set;
        }

        public GameStateEventArgs(GameManager.GameStateEnum data) {
            Data = data;
        }
    }

    public class EditorStateEventArgs : EventArgs {
        public GameManager.EditorStateEnum Data {
            get; set;
        }

        public EditorStateEventArgs(GameManager.EditorStateEnum data) {
            Data = data;
        }
    }

    public class ProjectMetaEventArgs : EventArgs {
        public string Name {
            get; set;
        }

        public string Id {
            get; set;
        }

        public ProjectMetaEventArgs(string id, string name) {
            Id = id;
            Name = name;
        }
    }

    public class BareProjectEventArgs : EventArgs {
        
        public IO.Swagger.Model.BareProject Project {
            get; set;
        }

        public BareProjectEventArgs(IO.Swagger.Model.BareProject project) {
            Project = project;
        }
    }

     public class BareSceneEventArgs : EventArgs {
        
        public IO.Swagger.Model.BareScene Scene {
            get; set;
        }

        public BareSceneEventArgs(IO.Swagger.Model.BareScene scene) {
            Scene = scene;
        }
    }

    public class BareActionEventArgs : EventArgs {
        
        public IO.Swagger.Model.BareAction Action {
            get; set;
        }

        public BareActionEventArgs(IO.Swagger.Model.BareAction action) {
            Action = action;
        }
    }

    public class ActionEventArgs : EventArgs {
        
        public IO.Swagger.Model.Action Action {
            get; set;
        }

        public ActionEventArgs(IO.Swagger.Model.Action action) {
            Action = action;
        }
    }

    public class BareActionPointEventArgs : EventArgs {
        
        public IO.Swagger.Model.BareActionPoint ActionPoint {
            get; set;
        }

        public BareActionPointEventArgs(IO.Swagger.Model.BareActionPoint actionPoint) {
            ActionPoint = actionPoint;
        }
    }

    public class ProjectActionPointEventArgs : EventArgs {
        
        public IO.Swagger.Model.ActionPoint ActionPoint {
            get; set;
        }

        public ProjectActionPointEventArgs(IO.Swagger.Model.ActionPoint actionPoint) {
            ActionPoint = actionPoint;
        }
    }

    public class ActionPointEventArgs : EventArgs {
        
        public ActionPoint ActionPoint {
            get; set;
        }

        public ActionPointEventArgs(ActionPoint actionPoint) {
            ActionPoint = actionPoint;
        }
    }

    public class RobotEefUpdatedEventArgs : EventArgs {
        public IO.Swagger.Model.RobotEefData Data {
            get; set;
        }

        public RobotEefUpdatedEventArgs(IO.Swagger.Model.RobotEefData data) {
            Data = data;
        }
    }

    public class RobotJointsUpdatedEventArgs {
        public IO.Swagger.Model.RobotJointsData Data {
            get; set;
        }

        public RobotJointsUpdatedEventArgs(IO.Swagger.Model.RobotJointsData data) {
            Data = data;
        }
    }

    public class LogicItemChangedEventArgs {
        public IO.Swagger.Model.LogicItem Data {
            get; set;
        }

        public LogicItemChangedEventArgs(IO.Swagger.Model.LogicItem data) {
            Data = data;
        }
    }

    public class ShowMainScreenEventArgs {
        public IO.Swagger.Model.ShowMainScreenData Data {
            get; set;
        }

        public ShowMainScreenEventArgs(IO.Swagger.Model.ShowMainScreenData data) {
            Data = data;
        }
    }


    public class ActionPointOrientationEventArgs : EventArgs {
        public IO.Swagger.Model.NamedOrientation Data {
            get; set;
        }

        public string ActionPointId {
            get; set;
        }

        public ActionPointOrientationEventArgs(IO.Swagger.Model.NamedOrientation data, string actionPointId) {
            Data = data;
            ActionPointId = actionPointId;
        }
    }

    public class RobotJointsEventArgs : EventArgs {
        public IO.Swagger.Model.ProjectRobotJoints Data {
            get; set;
        }
        public string ActionPointId {
            get; set;
        }

        public RobotJointsEventArgs(IO.Swagger.Model.ProjectRobotJoints data, string actionPointId) {
            Data = data;
            ActionPointId = actionPointId;
        }
    }

    public class ActionPointOrientationAddedEventArgs : EventArgs {
        public string ActionPointID {
            get; set;
        }

        public NamedOrientation Orientation {
            get; set;
        }

        public ActionPointOrientationAddedEventArgs(string actionPointID, NamedOrientation orientation) {
            ActionPointID = actionPointID;
            Orientation = orientation;
        }
    }

    public class ActionPointJointsAddedEventArgs : EventArgs {
        public string ActionPointID {
            get; set;
        }

        public ProjectRobotJoints Joints {
            get; set;
        }
        public ActionPointJointsAddedEventArgs(string actionPointID, ProjectRobotJoints joints) {
            ActionPointID = actionPointID;
            Joints = joints;
        }
    }

    public class RobotUrdfModelArgs : EventArgs {

        public string RobotType {
            get; set;
        }

        public RobotUrdfModelArgs(string robotType) {
            RobotType = robotType;
        }
    }



    public class AREditorEventArgs {
        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void EditorStateEventHandler(object sender, EditorStateEventArgs args);
        public delegate void ProjectMetaEventHandler(object sender, ProjectMetaEventArgs args);
        public delegate void BareProjectEventHandler(object sender, BareProjectEventArgs args);
        public delegate void BareSceneEventHandler(object sender, BareSceneEventArgs args);
        public delegate void BareActionEventHandler(object sender, BareActionEventArgs args);
        public delegate void BareActionPointEventHandler(object sender, BareActionPointEventArgs args);
        public delegate void ActionEventHandler(object sender, ActionEventArgs args);
        public delegate void ProjectActionPointEventHandler(object sender, ProjectActionPointEventArgs args);
        public delegate void ActionPointEventHandler(object sender, ActionPointEventArgs args);
        public delegate void ActionPointOrientationEventHandler(object sender, ActionPointOrientationEventArgs args);
        public delegate void RobotJointsEventHandler(object sender, RobotJointsEventArgs args);
        public delegate void RobotEefUpdatedEventHandler(object sender, RobotEefUpdatedEventArgs args);
        public delegate void RobotJointsUpdatedEventHandler(object sender, RobotJointsUpdatedEventArgs args);
        public delegate void LogicItemChangedEventHandler(object sender, LogicItemChangedEventArgs args);
        public delegate void ShowMainScreenEventHandler(object sender, ShowMainScreenEventArgs args);
        public delegate void RobotUrdfModelEventHandler(object sender, RobotUrdfModelArgs args);
    }
}
