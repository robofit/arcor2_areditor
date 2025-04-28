using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public static class DataHelper {
    // Start is called before the first frame update

    public static Position Vector3ToPosition(Vector3 vector) {
        return new Position((decimal) vector.x, (decimal) vector.y, (decimal) vector.z);
    }

    public static Vector3 PositionToVector3(Position position) => new((float) position.X, (float) position.Y, (float) position.Z);

    public static Orientation QuaternionToOrientation(Quaternion quaternion) {
        return new Orientation(w: (decimal) quaternion.w, x: (decimal) quaternion.x, y: (decimal) quaternion.y, z: (decimal) quaternion.z);
    }

    public static Quaternion OrientationToQuaternion(Orientation orientation) => new((float) orientation.X, (float) orientation.Y, (float) orientation.Z, (float) orientation.W);

    public static Pose CreatePose(Vector3 position, Quaternion orientation) {
        return new Pose(orientation: QuaternionToOrientation(orientation), position: Vector3ToPosition(position));
    }

    public static void GetPose(Pose pose, out Vector3 position, out Quaternion orientation) {
        position = PositionToVector3(pose.Position);
        orientation = OrientationToQuaternion(pose.Orientation);
    }

    public static ActionPoint ActionPointToProjectActionPoint(ActionPoint actionPoint) {
        return new ActionPoint(id: actionPoint.Id, robotJoints: actionPoint.RobotJoints, orientations: actionPoint.Orientations,
            position: actionPoint.Position, actions: new List<Action>());
    }

    public static ActionPoint ProjectActionPointToActionPoint(ActionPoint projectActionPoint) {
        return new ActionPoint(id: projectActionPoint.Id, robotJoints: projectActionPoint.RobotJoints,
            orientations: projectActionPoint.Orientations, position: projectActionPoint.Position);
    }

    public static ActionParameter ParameterToActionParameter(Parameter parameter) {
        return new ActionParameter(parameter.Name, parameter.Type, parameter.Value);
    }

    public static Parameter ActionParameterToParameter(ActionParameter actionParameter) {
        return new Parameter(actionParameter.Name, actionParameter.Type, actionParameter.Value);
    }

    public static BareProject ProjectToBareProject(Project project) {
        return new BareProject(description: project.Description, hasLogic: project.HasLogic, id: project.Id,
            intModified: project.IntModified, modified: project.Modified, name: project.Name, sceneId: project.SceneId);
    }

    public static BareScene SceneToBareScene(Scene scene) {
        return new BareScene(description: scene.Description, id: scene.Id, intModified: scene.IntModified,
            modified: scene.Modified, name: scene.Name);
    }

    public static BareAction ActionToBareAction(Action action) {
        return new BareAction(id: action.Id, name: action.Name, type: action.Type);
    }

    public static BareActionPoint ActionPointToBareActionPoint(ActionPoint actionPoint) {
        return new BareActionPoint(id: actionPoint.Id, name: actionPoint.Name,
            parent: actionPoint.Parent, position: actionPoint.Position);
    }

    public static ActionPoint BareActionPointToActionPoint(BareActionPoint actionPoint) {
        return new ActionPoint(id: actionPoint.Id, name: actionPoint.Name,
            parent: actionPoint.Parent, position: actionPoint.Position, displayName: actionPoint.DisplayName, description: actionPoint.Description, orientations: new List<NamedOrientation>(), robotJoints: new List<ProjectRobotJoints>(), actions: new List<Action>());
    }
}
