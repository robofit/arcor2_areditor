using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using UnityEngine.EventSystems;

public class OutlineOnClick : Clickable {

    public enum OutlineType {
        OnePassShader,
        TwoPassShader
    }

    [HideInInspector]
    public OutlineType OutlineShaderType;

    [HideInInspector]
    public Material OutlineClickFirstPass;
    [HideInInspector]
    public Material OutlineClickSecondPass;

    [HideInInspector]
    public Material OutlineHoverFirstPass;
    [HideInInspector]
    public Material OutlineHoverSecondPass;

    [HideInInspector]
    public Material OutlineClickMaterial;
    [HideInInspector]
    public Material OutlineHoverMaterial;

    public List<Renderer> Renderers = new List<Renderer>();

    public bool HoverOnly = false;

    private bool selected = false;
    private bool highlighted = false;


    public void InitRenderers() {
        ClearRenderers();
        Renderers.AddRange(gameObject.GetComponentsInChildren<Renderer>());
        SetOutline(OutlineHoverFirstPass, OutlineHoverSecondPass);
    }

    public void InitRenderers(List<Renderer> renderers) {
        Renderers = renderers;
    }

    public void ClearRenderers() {
        Renderers.Clear();
    }

    public override void OnClick(Click type) {

    }

    protected void Deselect() {
        if (selected) {
            selected = false;
            UnsetOutline();
        }
    }

    protected virtual void Select(bool force = false) {
        if (HoverOnly)
            return;

        if (highlighted) {
            highlighted = false;
            Debug.Log("Object highlighted.. must unhighlight first.");
            UnsetOutline();
        }
        Debug.Log("Selecting object");
        selected = true;
        if (OutlineShaderType == OutlineType.OnePassShader) {
            SetOutline(OutlineClickMaterial);
        } else {
            SetOutline(OutlineClickFirstPass, OutlineClickSecondPass);
        }
    }

    private void SetOutline(Material outlineFirstPass, Material outlineSecondPass) {
        foreach (Renderer renderer in Renderers) {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);
            if (!materials.Contains(outlineFirstPass)) {
                materials.Insert(0, outlineFirstPass);
            }
            if (!materials.Contains(outlineSecondPass)) {
                materials.Insert(materials.Count, outlineSecondPass);
            }
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    private void SetOutline(Material outline) {
        foreach (Renderer renderer in Renderers) {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);
            if (!materials.Contains(outline)) {
                materials.Insert(materials.Count, outline);
            }
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    private void UnsetOutline() {
        foreach (Renderer renderer in Renderers) {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);
            if (OutlineShaderType == OutlineType.TwoPassShader) {
                materials.RemoveAt(0);
            }
            materials.RemoveAt(materials.Count - 1);
            renderer.sharedMaterials = materials.ToArray();
        }
    }


    public void Highlight() {
        if (!selected) {
            highlighted = true;
            if (OutlineShaderType == OutlineType.OnePassShader) {
                SetOutline(OutlineHoverMaterial);
            } else {
                SetOutline(OutlineHoverFirstPass, OutlineHoverSecondPass);
            }
        }
    }

    public void UnHighlight() {
        if (highlighted && !selected) {
            highlighted = false;
            UnsetOutline();
        }
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
