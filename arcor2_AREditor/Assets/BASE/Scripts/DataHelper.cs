using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataHelper {
    // Start is called before the first frame update

    public static IO.Swagger.Model.Position Vector3ToPosition(Vector3 vector) => new IO.Swagger.Model.Position {
        X = (float) vector.x,
        Y = (float) vector.y,
        Z = (float) vector.z
    };

    public static Vector3 PositionToVector3(IO.Swagger.Model.Position position) => new Vector3((float) position.X, (float) position.Y, (float) position.Z);

    public static IO.Swagger.Model.Orientation QuaternionToOrientation(Quaternion quaternion) => new IO.Swagger.Model.Orientation {
        X = (float) quaternion.x,
        Y = (float) quaternion.y,
        Z = (float) quaternion.z,
        W = (float) quaternion.w
    };

    public static Quaternion OrientationToQuaternion(IO.Swagger.Model.Orientation orientation) => new Quaternion((float) orientation.X, (float) orientation.Y, (float) orientation.Z, (float) orientation.W);

    public static IO.Swagger.Model.Pose CreatePose(Vector3 position, Quaternion orientation) => new IO.Swagger.Model.Pose {
        Position = Vector3ToPosition(position),
        Orientation = QuaternionToOrientation(orientation)
    };

    public static void GetPose(IO.Swagger.Model.Pose pose, out Vector3 position, out Quaternion orientation) {
        position = PositionToVector3(pose.Position);
        orientation = OrientationToQuaternion(pose.Orientation);
    }

    public static IO.Swagger.Model.ProjectObject SceneObjectToProjectObject(IO.Swagger.Model.SceneObject sceneObject) {
        IO.Swagger.Model.ProjectObject projectObject = new IO.Swagger.Model.ProjectObject {
            Id = sceneObject.Id,
            ActionPoints = new List<IO.Swagger.Model.ProjectActionPoint>()
        };
        return projectObject;
    }

    public static IO.Swagger.Model.ProjectActionPoint ActionPointToProjectActionPoint(IO.Swagger.Model.ActionPoint actionPoint) {
        return new IO.Swagger.Model.ProjectActionPoint() {
            Id = actionPoint.Id,
            Pose = actionPoint.Pose,
            Actions = new List<IO.Swagger.Model.Action>()
        };
    }

    public static IO.Swagger.Model.ActionPoint ProjectActionPointToActionPoint(IO.Swagger.Model.ProjectActionPoint projectActionPoint) {
        return new IO.Swagger.Model.ActionPoint() {
            Id = projectActionPoint.Id,
            Pose = projectActionPoint.Pose
        };
    }
}
