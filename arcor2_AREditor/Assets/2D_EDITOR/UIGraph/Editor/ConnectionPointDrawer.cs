using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConnectionPoint))]
public class ConnectionPointDrawer : PropertyDrawer {
	Rect positionRect, directionRect;
	Rect weightRect, colorRect;

	readonly GUIContent posGUI = new GUIContent("Position", "Position along transform edge");
	readonly GUIContent weiGUI = new GUIContent("Weight", "Weight and color of attachment control point");

	public override float GetPropertyHeight(SerializedProperty element, GUIContent label) {
		return EditorGUIUtility.singleLineHeight * 2f + 4f;
	}

	public override void OnGUI(Rect rect, SerializedProperty element, GUIContent label) {
		positionRect = new Rect(
			rect.x, rect.y + 2f,
			rect.width - 67f,
			EditorGUIUtility.singleLineHeight
		);

		directionRect = new Rect(
			rect.x + rect.width - 65f, rect.y + 2f,
			65f,
			EditorGUIUtility.singleLineHeight
		);

		weightRect = new Rect(
			rect.x, rect.y + EditorGUIUtility.singleLineHeight + 4f,
			rect.width - 67f,
			EditorGUIUtility.singleLineHeight
		);

		colorRect = new Rect(
			rect.x + rect.width - 65f, rect.y + EditorGUIUtility.singleLineHeight + 4f,
			65f,
			EditorGUIUtility.singleLineHeight
		);

		EditorGUIUtility.labelWidth = 55f;
		EditorGUI.PropertyField(positionRect, element.FindPropertyRelative("position"), posGUI);
		EditorGUI.PropertyField(directionRect, element.FindPropertyRelative("direction"), GUIContent.none);
		EditorGUI.PropertyField(weightRect, element.FindPropertyRelative("weight"), weiGUI);
		EditorGUI.PropertyField(colorRect, element.FindPropertyRelative("color"), GUIContent.none);
		EditorGUIUtility.labelWidth = 0f;
	}
}
