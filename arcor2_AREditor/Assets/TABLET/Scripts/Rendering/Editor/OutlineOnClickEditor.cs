using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OutlineOnClick), true)]
public class OutlineOnClickEditor : Editor {
    public override void OnInspectorGUI() {
        OutlineOnClick outlineOnClick = target as OutlineOnClick;

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((OutlineOnClick) target), typeof(OutlineOnClick), false);
        GUI.enabled = true;

        outlineOnClick.OutlineShaderType = (OutlineOnClick.OutlineType) EditorGUILayout.EnumPopup("Outline Type", outlineOnClick.OutlineShaderType);

        if (outlineOnClick.OutlineShaderType == OutlineOnClick.OutlineType.OnePassShader) {
            outlineOnClick.OutlineClickMaterial = (Material) EditorGUILayout.ObjectField("Outline Click Material", outlineOnClick.OutlineClickMaterial, typeof(Material), true);
            outlineOnClick.OutlineHoverMaterial = (Material) EditorGUILayout.ObjectField("Outline Hover Material", outlineOnClick.OutlineHoverMaterial, typeof(Material), true);
        } else if (outlineOnClick.OutlineShaderType == OutlineOnClick.OutlineType.TwoPassShader) {
            outlineOnClick.OutlineClickFirstPass = (Material) EditorGUILayout.ObjectField("Outline Click First Pass Material", outlineOnClick.OutlineClickFirstPass, typeof(Material), true);
            outlineOnClick.OutlineClickSecondPass = (Material) EditorGUILayout.ObjectField("Outline Click Second Pass Material", outlineOnClick.OutlineClickSecondPass, typeof(Material), true);
            outlineOnClick.OutlineHoverFirstPass = (Material) EditorGUILayout.ObjectField("Outline Hover First Pass Material", outlineOnClick.OutlineHoverFirstPass, typeof(Material), true);
            outlineOnClick.OutlineHoverSecondPass = (Material) EditorGUILayout.ObjectField("Outline Hover Second Pass Material", outlineOnClick.OutlineHoverSecondPass, typeof(Material), true);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(outlineOnClick);

        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
        serializedObject.ApplyModifiedProperties();
    }
}
