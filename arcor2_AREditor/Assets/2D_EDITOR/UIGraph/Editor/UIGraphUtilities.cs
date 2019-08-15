using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;

public static class UIGraphUtilities {
	static string nodeName = "New Node";
	static Type[] nodeTypes = new Type[]{typeof(Canvas), typeof(Image), typeof(GraphNode)};
	static float nodeScale = 0.005f;
	static Vector2 nodeSize = new Vector2(400f, 200f);

	static string textName = "Text";
	static Type[] textTypes = new Type[]{typeof(Text)};
	static int textSize = 48;

	[MenuItem("GameObject/Create Other/Graph Node")]
	public static void CreateNode() {
		GameObject node = new GameObject(nodeName, nodeTypes);

		RectTransform transform = node.GetComponent<RectTransform>();
		transform.localScale = Vector3.one * nodeScale;
		transform.sizeDelta = nodeSize;

		CreateText(transform);
	}

	public static void CreateText(Transform parent) {
		GameObject go = new GameObject(textName, textTypes);

		RectTransform transform = go.GetComponent<RectTransform>();
		transform.SetParent(parent, false);
		transform.anchorMin = Vector2.zero;
		transform.anchorMax = Vector2.one;
		transform.sizeDelta = Vector2.zero;

		Text text = go.GetComponent<Text>();
		text.alignment = TextAnchor.MiddleCenter;
		text.color = Color.black;
		text.fontSize = textSize;
		text.text = nodeName;
	}

	[MenuItem("GameObject/Create Other/Graph Connection")]
	public static void CreateConnection() {
		new GameObject("New Connection", typeof(Connection));
	}
}
