using System;

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

    public class ActionPointUpdatedEventArgs : EventArgs {
        public ActionPoint Data {
            get; set;
        }

        public ActionPointUpdatedEventArgs(ActionPoint data) {
            Data = data;
        }
    }

    public class RobotUrdfArgs : EventArgs {
        public string RobotType {
            get; set;
        }

        public string Path {
            get; set;
        }

        public RobotUrdfArgs(string path, string robotType) {
            RobotType = robotType;
            Path = path;
        }
    }



    public class AREditorEventArgs {
        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void EditorStateEventHandler(object sender, EditorStateEventArgs args);
        public delegate void ProjectMetaEventHandler(object sender, ProjectMetaEventArgs args);
        public delegate void RobotEefUpdatedEventHandler(object sender, RobotEefUpdatedEventArgs args);
        public delegate void RobotJointsUpdatedEventHandler(object sender, RobotJointsUpdatedEventArgs args);
        public delegate void LogicItemChangedEventHandler(object sender, LogicItemChangedEventArgs args);
        public delegate void ShowMainScreenEventHandler(object sender, ShowMainScreenEventArgs args);
        public delegate void ActionPointUpdatedEventHandler(object sender, ActionPointUpdatedEventArgs args);
        public delegate void RobotUrdfEventHandler(object sender, RobotUrdfArgs args);
    }
}
