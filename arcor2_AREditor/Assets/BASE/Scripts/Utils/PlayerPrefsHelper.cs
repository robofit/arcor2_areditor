using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsHelper {

    public static void SaveFloat(string key, float value) {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns float value of key, if it exist. If not, returns defaultValue. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static float LoadFloat(string key, float defaultValue) {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SaveBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns boolean value of key, if it exist. If not, returns defaultValue.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool LoadBool(string key, bool defaultValue) {
        int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return value == 1 ? true : false;
    }

    public static void SaveString(string key, string value) {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns boolean value of key, if it exist. If not, returns defaultValue.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string LoadString(string key, string defaultValue) {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SaveVector3(string key, Vector3 value) {
        SaveFloat(key + "/x", value.x);
        SaveFloat(key + "/y", value.y);
        SaveFloat(key + "/z", value.z);
    }

    public static Vector3 LoadVector3(string key, Vector3 defaultValue) {
        Vector3 vector = new Vector3 {
            x = LoadFloat(key + "/x", defaultValue.x),
            y = LoadFloat(key + "/y", defaultValue.y),
            z = LoadFloat(key + "/z", defaultValue.z)
        };
        return vector;
    }

    public static void SaveQuaternion(string key, Quaternion value) {
        SaveFloat(key + "/x", value.x);
        SaveFloat(key + "/y", value.y);
        SaveFloat(key + "/z", value.z);
        SaveFloat(key + "/w", value.w);
    }

    public static Quaternion LoadQuaternion(string key, Quaternion defaultValue) {
        Quaternion quaternion = new Quaternion {
            x = LoadFloat(key + "/x", defaultValue.x),
            y = LoadFloat(key + "/y", defaultValue.y),
            z = LoadFloat(key + "/z", defaultValue.z),
            w = LoadFloat(key + "/w", defaultValue.w)
        };
        return quaternion;
    }

    public static void SavePose(string key, Vector3 position, Quaternion rotation) {
        SaveVector3(key + "/position", position);
        SaveQuaternion(key + "/rotation", rotation);
    }

    public static void LoadPose(string key, Vector3 defaultPosition, Quaternion defaultRotation) {
        LoadVector3(key + "/position", defaultPosition);
        LoadQuaternion(key + "/rotation", defaultRotation);
    }

}
