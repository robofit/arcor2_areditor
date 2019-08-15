using UnityEngine;

public class ObfuscateObject : MonoBehaviour {
    [ContextMenu("Obfuscate")]
    public void Obfuscate() {
        gameObject.name = gameObject.name.GetHashCode().ToString();
    }
}
