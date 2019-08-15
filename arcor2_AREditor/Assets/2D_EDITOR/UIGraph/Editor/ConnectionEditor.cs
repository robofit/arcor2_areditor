using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Connection))]
public class ConnectionEditor : Editor {
	Connection connection;
	SerializedProperty t1, t2;
	SerializedProperty p1, p2;
	SerializedProperty resolution;

	readonly GUIContent tGUI = new GUIContent("goto", "Go to Transform");
	readonly GUIStyle arrowStyle = new GUIStyle();

	void OnEnable() {
		connection = target as Connection;
		SerializedProperty targetTransforms = serializedObject.FindProperty("target");
		SerializedProperty points = serializedObject.FindProperty("points");

		t1 = targetTransforms.GetArrayElementAtIndex(0);
		t2 = targetTransforms.GetArrayElementAtIndex(1);
		p1 = points.GetArrayElementAtIndex(0);
		p2 = points.GetArrayElementAtIndex(1);

		resolution = serializedObject.FindProperty("resolution");

		arrowStyle.alignment = TextAnchor.MiddleCenter;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(resolution);
		EditorGUILayout.Separator();

		DrawTargetInspector(0);
		DrawConnectionPointInspector(0);

		EditorGUILayout.LabelField("↓ ↑", arrowStyle);

		DrawTargetInspector(1);
		DrawConnectionPointInspector(1);

		serializedObject.ApplyModifiedProperties();
	}

	public int GetIndex(RectTransform transform) {
		if (transform) {
			if (t1.objectReferenceValue != null && t1.objectReferenceValue.Equals(transform)) return 0;
			if (t2.objectReferenceValue != null && t2.objectReferenceValue.Equals(transform)) return 1;
		}
		return -1;
	}

	public void DrawTargetInspector(int index) {
		EditorGUILayout.BeginHorizontal();
		if (index == 0) {
			EditorGUILayout.PropertyField(t1, GUIContent.none);
			if (GUILayout.Button(tGUI, EditorStyles.miniButton, GUILayout.Width(33f))) {
				if (t1.objectReferenceValue != null) Selection.activeObject = t1.objectReferenceValue;
			}
		} else {
			EditorGUILayout.PropertyField(t2, GUIContent.none);
			if (GUILayout.Button(tGUI, EditorStyles.miniButton, GUILayout.Width(33f))) {
				if (t2.objectReferenceValue != null) Selection.activeObject = t2.objectReferenceValue;
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	public void DrawConnectionPointInspector(int index) {
		if (index == 0) {
			EditorGUILayout.PropertyField(p1);
		} else {
			EditorGUILayout.PropertyField(p2);
		}
	}
}
