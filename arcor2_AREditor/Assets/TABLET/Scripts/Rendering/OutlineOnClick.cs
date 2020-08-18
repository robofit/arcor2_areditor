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

    /// <summary>
    /// Controlled from OutlineOnClickEditor.cs script.
    /// For complex meshes and cubes with spheres it is better to use TwoPassShader, for simple objects use OnePassShader.
    /// </summary>
    [HideInInspector]
    public OutlineType OutlineShaderType;

    /// <summary>
    /// If TwoPassShader is selected, then following four variables are displayed in Inspector.
    /// </summary>
    [HideInInspector]
    public Material OutlineClickFirstPass;
    [HideInInspector]
    public Material OutlineClickSecondPass;
    [HideInInspector]
    public Material OutlineHoverFirstPass;
    [HideInInspector]
    public Material OutlineHoverSecondPass;

    /// <summary>
    /// If OnePassShader is selected, then following two variables are displayed in Inspector.
    /// </summary>
    [HideInInspector]
    public Material OutlineClickMaterial;
    [HideInInspector]
    public Material OutlineHoverMaterial;

    public List<Renderer> Renderers = new List<Renderer>();

    public bool HoverOnly = false;

    [Range(0.0f, 0.1f)]
    //public float OutlineSize = 0.05f;
    //private float CurrentOutlineSize;

    private bool selected = false;
    private bool highlighted = false;

    private Material[] gizmoMaterial;
    private bool gizmoHighlighted = false;


    private void Start() {
        InitGizmoMaterials();
        //CurrentOutlineSize = OutlineSize;
    }

    //private void Update() {
    //    if (!Mathf.Approximately(OutlineSize, CurrentOutlineSize)) {
    //        CurrentOutlineSize = OutlineSize;
    //        if (OutlineShaderType == OutlineType.OnePassShader) {
    //            OutlineClickMaterial.SetFloat("_OutlineWidth", OutlineSize + 0.02f);
    //            OutlineHoverMaterial.SetFloat("_OutlineWidth", OutlineSize);
    //        } else {
    //            OutlineClickFirstPass.SetFloat("_OutlineWidth", OutlineSize + 0.02f);
    //            OutlineClickSecondPass.SetFloat("_OutlineWidth", OutlineSize + 0.02f);
    //            OutlineHoverFirstPass.SetFloat("_OutlineWidth", OutlineSize);
    //            OutlineHoverSecondPass.SetFloat("_OutlineWidth", OutlineSize);
    //        }
    //    }
    //}

    private void InitGizmoMaterials() {
        if (OutlineShaderType == OutlineType.OnePassShader) {
            gizmoMaterial = new Material[1];
            gizmoMaterial[0] = new Material(OutlineHoverMaterial) {
                name = "OutlineGizmoMaterial"
            };
            gizmoMaterial[0].SetColor("_OutlineColor", new Color(1f, 0.7f, 0f));
        } else {
            gizmoMaterial = new Material[2];
            gizmoMaterial[0] = new Material(OutlineHoverFirstPass) {
                name = "OutlineGizmoFirstPass"
            };
            gizmoMaterial[1] = new Material(OutlineHoverSecondPass) {
                name = "OutlineGizmoSecondPass",
            };
            gizmoMaterial[1].SetColor("_OutlineColor", new Color(1f, 0.7f, 0f));
        }
    }

    /// <summary>
    /// Loads all renderers on attached gameobject.
    /// </summary>
    public void InitRenderers() {
        ClearRenderers();
        Renderers.AddRange(gameObject.GetComponentsInChildren<Renderer>());
    }

    public void InitRenderers(List<Renderer> renderers) {
        Renderers = renderers;
    }

    public void ClearRenderers() {
        Renderers.Clear();
    }

    protected void Deselect() {
        if (selected) {
            selected = false;
            UnsetOutline();
        }
    }

    /// <summary>
    /// Called when OnClick event is triggered on attached gameobject.
    /// </summary>
    /// <param name="force"></param>
    protected virtual void Select(bool force = false) {
        if (HoverOnly)
            return;

        if (highlighted) {
            highlighted = false;
            UnsetOutline();
        }
        selected = true;
        if (OutlineShaderType == OutlineType.OnePassShader) {
            SetOutline(OutlineClickMaterial);
        } else {
            SetOutline(OutlineClickFirstPass, OutlineClickSecondPass);
        }
    }

    /// <summary>
    /// Sets outline for gameobjects using TwoPassShader option. Better for meshes.
    /// </summary>
    /// <param name="outlineFirstPass"></param>
    /// <param name="outlineSecondPass"></param>
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

    /// <summary>
    /// Sets outline for gameobjects using OnePassShader option. Better for simple objects.
    /// </summary>
    /// <param name="outline"></param>
    private void SetOutline(Material outline) {
        foreach (Renderer renderer in Renderers) {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);
            if (!materials.Contains(outline)) {
                materials.Insert(materials.Count, outline);
            }
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    /// <summary>
    /// Removes outline of attached gameobject.
    /// </summary>
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

    /// <summary>
    /// Called when OnHoverStart/OnHoverEnd event is triggered from attached gameobject.
    /// </summary>
    public void Highlight() {
        if (!selected && !gizmoHighlighted) {
            highlighted = true;
            if (OutlineShaderType == OutlineType.OnePassShader) {
                SetOutline(OutlineHoverMaterial);
            } else {
                SetOutline(OutlineHoverFirstPass, OutlineHoverSecondPass);
            }
        }
    }

    public void UnHighlight() {
        if (highlighted && !selected && !gizmoHighlighted) {
            highlighted = false;
            UnsetOutline();
        }
    }

    public void GizmoHighlight() {
        if (highlighted) {
            UnHighlight();
        }

        if (OutlineShaderType == OutlineType.OnePassShader) {
            SetOutline(gizmoMaterial[0]);
        } else {
            SetOutline(gizmoMaterial[0], gizmoMaterial[1]);
        }
        gizmoHighlighted = true;
    }

    public void GizmoUnHighlight() {
        if (gizmoHighlighted) {
            UnsetOutline();
            gizmoHighlighted = false;
        }
    }

    public override void OnClick(Click type) {

    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
