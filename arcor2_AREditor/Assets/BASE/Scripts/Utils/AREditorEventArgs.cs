using System;
using System.Collections.Generic;
using IO.Swagger.Model;
using UnityEngine;

namespace Base {

    public class StringEventArgs : EventArgs {
        public string Data {
            get; set;
        }

        public StringEventArgs(string data) {
            Data = data;
        }
    }

    public class StringListEventArgs : EventArgs {
        public List<string> Data {
            get; set;
        }

        public StringListEventArgs(List<string> data) {
            Data = data;
        }
    }

    public class FloatEventArgs : EventArgs {
        public float Data {
            get; set;
        }

        public FloatEventArgs(float data) {
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

    public class ActionModelEventArgs : EventArgs {
        
        public IO.Swagger.Model.Action Action {
            get; set;
        }

        public ActionModelEventArgs(IO.Swagger.Model.Action action) {
            Action = action;
        }
    }

    public class ActionEventArgs : EventArgs {
        
        public Action Action {
            get; set;
        }

        public ActionEventArgs(Action action) {
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

    public class RobotMoveToPoseEventArgs : EventArgs {

        public IO.Swagger.Model.RobotMoveToPose Event {
            get; set;
        }

        public RobotMoveToPoseEventArgs(RobotMoveToPose @event) {
            Event = @event;
        }
    }

    public class RobotMoveToJointsEventArgs : EventArgs {

        public IO.Swagger.Model.RobotMoveToJoints Event {
            get; set;
        }

        public RobotMoveToJointsEventArgs(RobotMoveToJoints @event) {
            Event = @event;
        }
    }

    public class RobotMoveToActionPointJointsEventArgs : EventArgs {

        public IO.Swagger.Model.RobotMoveToActionPointJoints Event {
            get; set;
        }

        public RobotMoveToActionPointJointsEventArgs(RobotMoveToActionPointJoints @event) {
            Event = @event;
        }
    }

    public class RobotMoveToActionPointOrientationEventArgs : EventArgs {

        public IO.Swagger.Model.RobotMoveToActionPointOrientation Event {
            get; set;
        }

        public RobotMoveToActionPointOrientationEventArgs(RobotMoveToActionPointOrientation @event) {
            Event = @event;
        }
    }

    public class SceneStateEventArgs : EventArgs {

        public IO.Swagger.Model.SceneStateData Event {
            get; set;
        }

        public SceneStateEventArgs(SceneStateData @event) {
            Event = @event;
        }
    }

    public class ParameterEventArgs : EventArgs {

        public IO.Swagger.Model.Parameter Parameter {
            get; set;
        }

        public string ObjectId {
            get; set;
        }

        public ParameterEventArgs(string objectId, IO.Swagger.Model.Parameter @event) {
            Parameter = @event;
            ObjectId = objectId;
        }
    }

    public class GameObjectEventArgs : EventArgs {
        public GameObject GameObj {
            get; set;
        }

        public GameObjectEventArgs(GameObject gameObject) {
            GameObj = gameObject;
        }
    }

    public class InteractiveObjectEventArgs : EventArgs {
        public InteractiveObject InteractiveObject {
            get; set;
        }

        public InteractiveObjectEventArgs(InteractiveObject interactiveObject) {
            InteractiveObject = interactiveObject;
        }
    }

    public class ObjectTypeEventArgs : EventArgs {
        public ObjectTypeMeta ObjectType {
            get; set;
        }

        public ObjectTypeEventArgs(ObjectTypeMeta objectType) {
            ObjectType = objectType;
        }
    }

    public class ObjectTypesEventArgs : EventArgs {
        public List<ObjectTypeMeta> ObjectTypes {
            get; set;
        }

        public ObjectTypesEventArgs(List<ObjectTypeMeta> objectTypes) {
            ObjectTypes = objectTypes;
        }
    }

    public class ObjectLockingEventArgs : EventArgs {
        public List<string> ObjectIds {
            get; set;
        }

        public bool Locked {
            get; set;
        }

        public string Owner {
            get;set;
        }

        public ObjectLockingEventArgs(List<string> objectIds, bool locked, string owner) {
            ObjectIds = objectIds;
            Locked = locked;
            Owner = owner;
        }
    }

    public class ProcessStateEventArgs : EventArgs {
        public ProcessStateData Data {
            get; set;
        }

        public ProcessStateEventArgs(ProcessStateData data) {
            Data = data;
        }
    }

    public class CalibrationEventArgs : EventArgs {

        public bool Calibrated {
            get; set;
        }

        public GameObject Anchor {
            get; set;
        }

        public CalibrationEventArgs(bool calibrated, GameObject anchor) {
            Calibrated = calibrated;
            Anchor = anchor;
        }
    }

    public class ProjectParameterEventArgs : EventArgs {
        public ProjectParameter ProjectParameter {
            get; set;
        }

        public ProjectParameterEventArgs(ProjectParameter projectParameter) {
            ProjectParameter = projectParameter;
        }
    }

    public class GizmoAxisEventArgs : EventArgs {
        public Gizmo.Axis SelectedAxis {
            get; set;
        }

        public GizmoAxisEventArgs(Gizmo.Axis gizmoAxis) {
            SelectedAxis = gizmoAxis;
        }
    }

    public class AREditorEventArgs {
        public delegate void StringEventHandler(object sender, StringEventArgs args);
        public delegate void StringListEventHandler(object sender, StringListEventArgs args);
        public delegate void FloatEventHandler(object sender, FloatEventArgs args);
        public delegate void GameStateEventHandler(object sender, GameStateEventArgs args);
        public delegate void EditorStateEventHandler(object sender, EditorStateEventArgs args);
        public delegate void ProjectMetaEventHandler(object sender, ProjectMetaEventArgs args);
        public delegate void BareProjectEventHandler(object sender, BareProjectEventArgs args);
        public delegate void BareSceneEventHandler(object sender, BareSceneEventArgs args);
        public delegate void BareActionEventHandler(object sender, BareActionEventArgs args);
        public delegate void BareActionPointEventHandler(object sender, BareActionPointEventArgs args);
        public delegate void ActionModelEventHandler(object sender, ActionModelEventArgs args);
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
        public delegate void RobotMoveToPoseEventHandler(object sender, RobotMoveToPoseEventArgs args);
        public delegate void RobotMoveToJointsEventHandler(object sender, RobotMoveToJointsEventArgs args);
        public delegate void RobotMoveToActionPointJointsEventHandler(object sender, RobotMoveToActionPointJointsEventArgs args);
        public delegate void RobotMoveToActionPointOrientationHandler(object sender, RobotMoveToActionPointOrientationEventArgs args);
        public delegate void SceneStateHandler(object sender, SceneStateEventArgs args);
        public delegate void ParameterHandler(object sender, ParameterEventArgs args);
        public delegate void ObjectTypeHandler(object sender, ObjectTypeEventArgs args);
        public delegate void ObjectTypesHandler(object sender, ObjectTypesEventArgs args);
        public delegate void GameObjectEventHandler(object sender, GameObjectEventArgs args);
        public delegate void InteractiveObjectEventHandler(object sender, InteractiveObjectEventArgs args);
        public delegate void ObjectLockingEventHandler(object sender, ObjectLockingEventArgs args);
        public delegate void ProcessStateEventHandler(object sender, ProcessStateEventArgs args);
        public delegate void CalibrationEventHandler(object sender, CalibrationEventArgs args);
        public delegate void ProjectParameterEventHandler(object sender, ProjectParameterEventArgs args);
        public delegate void GizmoAxisEventHandler(object sender, GizmoAxisEventArgs args);
    }
}
