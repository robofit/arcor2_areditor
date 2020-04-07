using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsHelper {

    public static void SaveFloat(string key, float value) {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static float LoadFloat(string key, float defaultValue) {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SaveBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadBool(string key, bool defaultValue) {
        int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return value == 1 ? true : false;
    }

}
