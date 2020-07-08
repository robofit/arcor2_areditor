using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using UnityEngine.EventSystems;

public class OutlineOnClick : Clickable {

    public Material ClickMaterial;
    public Material HoverMaterial;

    public List<Renderer> Renderers = new List<Renderer>();
    protected Dictionary<Renderer, List<Material>> materials = new Dictionary<Renderer, List<Material>>();
    
    private void Start() {
        materials.Clear();
        foreach (Renderer renderer in Renderers) {
            materials.Add(renderer, new List<Material>(renderer.materials));
        }
    }

    public void InitRenderers(List<Renderer> renderers) {
        Renderers = renderers;
        materials.Clear();
        foreach (Renderer renderer in Renderers) {
            materials.Add(renderer, new List<Material>(renderer.materials));
        }
    }

    public void ClearRenderers() {
        Renderers.Clear();
        materials.Clear();
    }

    public override void OnClick(Click type) {

    }

    protected void AddMaterial(Material material) {
        foreach (Renderer renderer in Renderers) {
            if (!materials[renderer].Contains(material)) {
                materials[renderer].Add(material);
            }
        }
    }

    protected void RemoveMaterial(Material material) {
        foreach (Renderer renderer in Renderers) {
            if (materials[renderer].Contains(material)) {
                materials[renderer].Remove(material);
            }
        }
    }

    protected void Deselect() {
        RemoveMaterial(ClickMaterial);
        foreach (Renderer renderer in Renderers) {
            renderer.materials = materials[renderer].ToArray();
        }
    }

    protected virtual void Select(bool force = false) {
        AddMaterial(ClickMaterial);
        foreach (Renderer renderer in Renderers) {
            renderer.materials = materials[renderer].ToArray();
        }
    }

    /// <summary>
    /// Removes mat1 (=old material) and Adds mat2 (=new material) to the materials array.
    /// </summary>
    /// <param name="mat1"></param>
    /// <param name="mat2"></param>
    public void SwapMaterials(Material mat1, Material mat2) {
        RemoveMaterial(mat1);
        AddMaterial(mat2);
    }

    public void Highlight() {
        AddMaterial(HoverMaterial);
        foreach (Renderer renderer in Renderers) {
            renderer.materials = materials[renderer].ToArray();
        }
    }

    public void UnHighlight() {
        RemoveMaterial(HoverMaterial);
        foreach (Renderer renderer in Renderers) {
            renderer.materials = materials[renderer].ToArray();
        }
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
