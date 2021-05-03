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

    public static Vector3 OpenCVToUnity(Vector3 position) {
        return new Vector3(position.x, -position.y, position.z);
    }

    public static Quaternion OpenCVToUnity(Quaternion rotation) {
        return new Quaternion(-rotation.x, rotation.y, -rotation.z, rotation.w);
    }

    public static Matrix4x4 OpenCVToUnity(Matrix4x4 m) {
        // Multiplies 2nd row (y axis) by -1
        // xx xy xz xw
        // yx yy yz yw
        // zx zy zz zw
        // wx wy wz ww

        // to

        //  xx  xy  xz  xw
        // -yx -yy -yz -yw
        //  zx  zy  zz  zw
        //  wx  wy  wz  ww

        m.m10 *= -1;
        m.m11 *= -1;
        m.m12 *= -1;
        m.m13 *= -1;

        return m;
    }

    //get position from transform matrix
    public static Vector3 GetPositionFromMatrix(Matrix4x4 m) {
        return m.GetColumn(3);
    }

    //get rotation quaternion from matrix
    public static Quaternion GetQuaternionFromMatrix(Matrix4x4 m) {
        // Trap the case where the matrix passed in has an invalid rotation submatrix.
        if (m.GetColumn(2) == Vector4.zero) {
            Debug.Log("QuaternionFromMatrix got zero matrix.");
            return Quaternion.identity;
        }
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
}
