using System;
using UnityEngine;

public static class JSONHelper {

    public static JSONObject CreatePose(Vector3 position, Quaternion orientation) {
        JSONObject poseJson = new JSONObject(JSONObject.Type.OBJECT);
        JSONObject positionJson = CreateVector3JSON(position);
        JSONObject orientationJson = CreateQuaternionJSON(orientation);
        poseJson.AddField("position", positionJson);
        poseJson.AddField("orientation", orientationJson);
        return poseJson;
    }

    public static JSONObject CreateVector3JSON(Vector3 v) {
        JSONObject position = new JSONObject(JSONObject.Type.OBJECT);
        position.AddField("x", v.x);
        position.AddField("y", v.y);
        position.AddField("z", v.z);
        return position;
    }

    public static JSONObject CreateQuaternionJSON(Quaternion q) {
        JSONObject quaternion = new JSONObject(JSONObject.Type.OBJECT);
        quaternion.AddField("x", q.x);
        quaternion.AddField("y", q.y);
        quaternion.AddField("z", q.z);
        quaternion.AddField("w", q.w);
        return quaternion;
    }

    public static bool GetBoolValue(JSONObject obj, string objId, bool defaultValue) {
        try {
            return obj[objId].b;
        } catch (NullReferenceException e) {
            Debug.Log("Parse error in bool value for object: " + obj.ToString(true));
            return defaultValue;
        }
    }

    public static string GetStringValue(JSONObject obj, string objId, string defaultValue) {
        try {
            return obj[objId].str;
        } catch (NullReferenceException e) {
            return defaultValue;
        }
    }

    public static long GetIntegerValue(JSONObject obj, string objId, long defaultValue) {
        try {
            return obj[objId].i;
        } catch (NullReferenceException e) {
            return defaultValue;
        }
    }

    public static float GetFloatValue(JSONObject obj, string objId, float defaultValue) {
        try {
            return obj[objId].f;
        } catch (NullReferenceException e) {
            return defaultValue;
        }
    }

    public static bool TryGetPose(JSONObject pose, out Vector3 position, out Quaternion orientation) {
        position = new Vector3();
        orientation = new Quaternion();
        try {
            JSONObject positionJson = pose["position"];
            JSONObject orientationJson = pose["orientation"];
            position.x = positionJson["x"].f;
            position.y = positionJson["y"].f;
            position.z = positionJson["z"].f;

            orientation.x = orientationJson["x"].f;
            orientation.y = orientationJson["y"].f;
            orientation.z = orientationJson["z"].f;
            orientation.w = orientationJson["w"].f;
        } catch (NullReferenceException e) {
            return false;
        }
        return true;
    }

}
