using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Michsky.UI.ModernUIPack;

/// <summary>
/// A helper editor script for finding missing references to objects.
/// </summary>
public class ChangeTooltipDelay : MonoBehaviour {
    private const string MENU_ROOT = "Tools/Utils/";

    /// <summary>
    /// Finds all missing references to objects in the currently loaded scene.
    /// </summary>
    [MenuItem(MENU_ROOT + "Change tooltip delay", false, 50)]
    public static void ChangeTo1s() {
        var sceneObjects = GetSceneObjects();
        if (sceneObjects == null) {
            return;
        }
        foreach (var go in sceneObjects) {
            var components = go.GetComponents<Component>();
            foreach (var component in components) {
                if (component == null)
                    continue;
                if (component is TooltipContent tooltipContent) {
                    tooltipContent.delay = 0.5f;
                }
            }
        }
        
        
    }
       


    private static GameObject[] GetSceneObjects() {
        // Use this method since GameObject.FindObjectsOfType will not return disabled objects.
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                   && go.hideFlags == HideFlags.None).ToArray();
    }

}
