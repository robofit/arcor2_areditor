using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphNode))]
public class GraphNodeEditor : Editor {
    GraphNode t;
    List<Connection> connections;
    Editor editor;
    ConnectionEditor editorC;

    RectTransform t1;
    int index;

    readonly GUIContent delGUI = new GUIContent("Delete", "Remove Connection");
    readonly GUIContent selGUI = new GUIContent("Select", "Select Connection");
    readonly GUILayoutOption[] delLayout = new GUILayoutOption[]{
        GUILayout.Width(40f)
    };
    readonly GUILayoutOption[] selLayout = new GUILayoutOption[]{
        GUILayout.Width(40f)
    };
    readonly GUIStyle arrowStyle = new GUIStyle();
    readonly Color boxColor = new Color(.625f, .625f, .625f);

    void OnEnable() {
        t = target as GraphNode;
        t1 = t.transform as RectTransform;
        GetConnections();

        arrowStyle.alignment = TextAnchor.MiddleCenter;
    }

    public override void OnInspectorGUI() {
        if (connections != null) {
            foreach (Connection c in connections) {
                if (c == null || c.Equals(null))
                    continue;

                Editor.CreateCachedEditor(c, typeof(ConnectionEditor), ref editor);
                editorC = editor as ConnectionEditor;
                index = editorC.GetIndex(t.transform as RectTransform);

                editorC.serializedObject.Update();

                EditorGUILayout.Separator();
                Rect box = EditorGUILayout.BeginVertical();
                box.y -= 4f;
                box.height += 8f;
                box.x -= 4f;
                box.width += 5f;
                EditorGUI.DrawRect(box, boxColor);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(delGUI, EditorStyles.miniButton, delLayout)) {
                    DestroyImmediate(c.gameObject);
                    EditorUtility.SetDirty(ConnectionManager.Instance);
                    continue;
                }
                if (GUILayout.Button(selGUI, EditorStyles.miniButton, selLayout)) {
                    Selection.activeObject = c;
                }
                EditorGUILayout.EndHorizontal();
                editorC.DrawConnectionPointInspector(index);
                EditorGUILayout.LabelField("↓ ↓", arrowStyle);
                editorC.DrawTargetInspector(index == 0 ? 1 : 0);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Separator();

                editorC.serializedObject.ApplyModifiedProperties();
            }
        }

        if (GUILayout.Button("Add New Connection", EditorStyles.miniButton)) {
            ConnectionManager.CreateConnection(t1, null);
            EditorUtility.SetDirty(ConnectionManager.Instance);
            GetConnections();
        }
    }

    void GetConnections() {
        connections = ConnectionManager.FindConnections(t1);
    }
}
