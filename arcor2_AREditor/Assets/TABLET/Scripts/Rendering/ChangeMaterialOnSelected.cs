using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swaps all materials on specified renderers with ClickMaterial on event message OnSelected.
/// Swaps materials back to normal on event message Deselect. 
/// </summary>
public class ChangeMaterialOnSelected : MonoBehaviour {

    public Material ClickMaterial;

    public List<Renderer> Renderers = new List<Renderer>();

    private Dictionary<Renderer, List<Material>> materials = new Dictionary<Renderer, List<Material>>();

    public void InitRenderers() {
        materials.Clear();
        foreach (Renderer renderer in Renderers) {
            materials.Add(renderer, new List<Material>(renderer.materials));
        }
    }

    public void AddRenderer(Renderer renderer) {
        Renderers.Add(renderer);
        materials.Add(renderer, new List<Material>(renderer.materials));
    }

    public void RemoveRenderer(Renderer renderer) {
        Renderers.Remove(renderer);
        materials.Remove(renderer);
    }

    /// <summary>
    /// Called from Scene (SendMessage) when some object gets selected.
    /// </summary>
    private void OnSelected() {
        foreach (Renderer renderer in Renderers) {
            renderer.materials = new Material[1] { ClickMaterial };
        }
    }

    /// <summary>
    /// Called from Scene (SendMessage) when some object gets deselected.
    /// </summary>
    private void Deselect() {
        foreach (Renderer renderer in Renderers) {
            renderer.materials = materials[renderer].ToArray();
        }
    }

}
