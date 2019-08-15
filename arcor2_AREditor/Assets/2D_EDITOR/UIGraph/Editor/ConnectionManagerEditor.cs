using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ConnectionManager))]
public class ConnectionManagerEditor : Editor {
	SerializedProperty prefab;
	ReorderableList connections;

	readonly GUIContent bGUI = new GUIContent(">", "Go to connection");

	void OnEnable() {
		prefab = serializedObject.FindProperty("connectionPrefab");

		connections = new ReorderableList(
			serializedObject,
			serializedObject.FindProperty("connections"),
			true, true, true, true
		);
		connections.drawElementCallback += (Rect position, int index, bool show, bool active) => {
			SerializedProperty element = connections.serializedProperty.GetArrayElementAtIndex(index);
			Rect lRect = new Rect(position.x, position.y+2f, position.width - 20f, EditorGUIUtility.singleLineHeight);
			Rect bRect = new Rect(position.x + lRect.width, position.y+2f, 18f, EditorGUIUtility.singleLineHeight);

			if (element.objectReferenceValue != null) {
				EditorGUI.LabelField(lRect, element.objectReferenceValue.name);
				if (GUI.Button(bRect, bGUI)) {
					Selection.activeObject = element.objectReferenceValue;
				}
			} else {
				EditorGUI.LabelField(lRect, "Missing Connection");
			}
		};
		connections.drawHeaderCallback += (Rect position) => {
			Rect lRect = new Rect(position.x, position.y, position.width - 80f, position.height);
			Rect b1Rect = new Rect(position.x + lRect.width, position.y + 1f, 40f, position.height - 2f);
			Rect b2Rect = new Rect(position.x + lRect.width + b1Rect.width, position.y + 1f, 40f, position.height - 2f);

			EditorGUI.LabelField(lRect, "Connections: "+connections.count, EditorStyles.miniLabel);
			if (GUI.Button(b1Rect, "Sort", EditorStyles.miniButton)) {
				ConnectionManager.SortConnections();
				EditorUtility.SetDirty(target);
			}
			if (GUI.Button(b2Rect, "Clean", EditorStyles.miniButton)) {
				ConnectionManager.CleanConnections();
				EditorUtility.SetDirty(target);
			}
		};

		connections.onRemoveCallback += (ReorderableList l) => {
			Connection c = (Connection)l.serializedProperty.GetArrayElementAtIndex(l.index).objectReferenceValue;
			if (c) DestroyImmediate(c.gameObject);
			ReorderableList.defaultBehaviours.DoRemoveButton(l);
			ReorderableList.defaultBehaviours.DoRemoveButton(l);
			EditorUtility.SetDirty(target);
		};
		connections.onAddCallback += (ReorderableList l) => {
			ConnectionManager.CreateConnection(null, null);
			EditorUtility.SetDirty(target);
		};

		connections.onSelectCallback += (ReorderableList l) => {
			Connection c = (Connection)l.serializedProperty.GetArrayElementAtIndex(l.index).objectReferenceValue;
			if (c) {
				EditorGUIUtility.PingObject(c);
			}
		};
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.PropertyField(prefab);
		EditorGUILayout.Separator();
		connections.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
	}
}
