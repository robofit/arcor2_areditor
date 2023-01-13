using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataHelper {
    // Start is called before the first frame update

    public static IO.Swagger.Model.Position Vector3ToPosition(Vector3 vector) {
        return new IO.Swagger.Model.Position((decimal) vector.x, (decimal) vector.y, (decimal) vector.z);
    }

    public static Vector3 PositionToVector3(IO.Swagger.Model.Position position) => new Vector3((float) position.X, (float) position.Y, (float) position.Z);

    public static IO.Swagger.Model.Orientation QuaternionToOrientation(Quaternion quaternion) {
        return new IO.Swagger.Model.Orientation((decimal) quaternion.w, (decimal) quaternion.x, (decimal) quaternion.y, (decimal) quaternion.z);
    }

    public static Quaternion OrientationToQuaternion(IO.Swagger.Model.Orientation orientation) => new Quaternion((float) orientation.X, (float) orientation.Y, (float) orientation.Z, (float) orientation.W);

    public static IO.Swagger.Model.Pose CreatePose(Vector3 position, Quaternion orientation) {
        return new IO.Swagger.Model.Pose(orientation: QuaternionToOrientation(orientation), position: Vector3ToPosition(position));
    }

    public static void GetPose(IO.Swagger.Model.Pose pose, out Vector3 position, out Quaternion orientation) {
        position = PositionToVector3(pose.Position);
        orientation = OrientationToQuaternion(pose.Orientation);
    }

    public static IO.Swagger.Model.ActionPoint ActionPointToProjectActionPoint(IO.Swagger.Model.ActionPoint actionPoint) {
        return new IO.Swagger.Model.ActionPoint(id: actionPoint.Id, robotJoints: actionPoint.RobotJoints, orientations: actionPoint.Orientations,
            position: actionPoint.Position, actions: new List<IO.Swagger.Model.Action>());
    }

    public static IO.Swagger.Model.ActionPoint ProjectActionPointToActionPoint(IO.Swagger.Model.ActionPoint projectActionPoint) {
        return new IO.Swagger.Model.ActionPoint(id: projectActionPoint.Id, robotJoints: projectActionPoint.RobotJoints,
            orientations: projectActionPoint.Orientations, position: projectActionPoint.Position);
    }

    public static IO.Swagger.Model.ActionParameter ParameterToActionParameter(IO.Swagger.Model.Parameter parameter) {
        return new IO.Swagger.Model.ActionParameter(name: parameter.Name, type: parameter.Type, parameter.Value);
    }

    public static IO.Swagger.Model.Parameter ActionParameterToParameter(IO.Swagger.Model.ActionParameter actionParameter) {
        return new IO.Swagger.Model.Parameter(name: actionParameter.Name, type: actionParameter.Type, value: actionParameter.Value);
    }

    public static IO.Swagger.Model.BareProject ProjectToBareProject(IO.Swagger.Model.Project project) {
        return new IO.Swagger.Model.BareProject(description: project.Description, hasLogic: project.HasLogic, id: project.Id,
            intModified: project.IntModified, modified: project.Modified, name: project.Name, sceneId: project.SceneId);
    }

    public static IO.Swagger.Model.BareScene SceneToBareScene(IO.Swagger.Model.Scene scene) {
        return new IO.Swagger.Model.BareScene(description: scene.Description, id: scene.Id, intModified: scene.IntModified,
            modified: scene.Modified, name: scene.Name);
    }

    public static IO.Swagger.Model.BareAction ActionToBareAction(IO.Swagger.Model.Action action) {
        return new IO.Swagger.Model.BareAction(id: action.Id, name: action.Name, type: action.Type);
    }

    public static IO.Swagger.Model.BareActionPoint ActionPointToBareActionPoint(IO.Swagger.Model.ActionPoint actionPoint) {
        return new IO.Swagger.Model.BareActionPoint(id: actionPoint.Id, name: actionPoint.Name,
            parent: actionPoint.Parent, position: actionPoint.Position);
    }
}
