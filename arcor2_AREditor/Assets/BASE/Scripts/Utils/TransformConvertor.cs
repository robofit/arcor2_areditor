using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Converts transforms between ROS and Unity coordinate systems. Based on:
/// https://github.com/siemens/ros-sharp/wiki/Dev_ROSUnityCoordinateSystemConversion
/// </summary>
public static class TransformConvertor {

    public static Vector3 UnityToROS(Vector3 position) {
        return new Vector3(position.z, -position.x, position.y);
    }

    public static Quaternion UnityToROS(Quaternion rotation) {
        return new Quaternion(-rotation.z, rotation.x, -rotation.y, rotation.w);
    }

    public static Vector3 UnityToROSScale(Vector3 scale) {
        return new Vector3(scale.z, scale.x, scale.y);
    }

    public static (decimal, decimal, decimal) UnityToROSScale(decimal scale_X, decimal scale_Y, decimal scale_Z) {
        return (scale_Z, scale_X, scale_Y);
    }

    public static Vector3 ROSToUnity(Vector3 position) {
        return new Vector3(-position.y, position.z, position.x);
    }

    public static Quaternion ROSToUnity(Quaternion rotation) {
        return new Quaternion(rotation.y, -rotation.z, -rotation.x, rotation.w);
    }

    public static Vector3 ROSToUnityScale(Vector3 scale) {
        return new Vector3(scale.y, scale.z, scale.x);
    }
}
