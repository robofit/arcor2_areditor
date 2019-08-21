using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataHelper {
    // Start is called before the first frame update

    public static IO.Swagger.Model.Position Vector3ToPosition(Vector3 vector) => new IO.Swagger.Model.Position {
        X = (decimal) vector.x,
        Y = (decimal) vector.y,
        Z = (decimal) vector.z
    };

    public static Vector3 PositionToVector3(IO.Swagger.Model.Position position) => new Vector3((float) position.X, (float) position.Y, (float) position.Z);

    public static IO.Swagger.Model.Orientation QuaternionToOrientation(Quaternion quaternion) => new IO.Swagger.Model.Orientation {
        X = (decimal) quaternion.x,
        Y = (decimal) quaternion.y,
        Z = (decimal) quaternion.z,
        W = (decimal) quaternion.w
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
}
