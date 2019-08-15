using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObfuscateText : MonoBehaviour {
	[ContextMenu("Obfuscate")]
	public void Obfuscate() {
		Text t = GetComponent<Text>();
		if (t) {
			t.text = t.text.GetHashCode().ToString();
		}
	}
}
