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

    public float OutlineSize = 1f;
    public float OutlineHoverSize = 1f;

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

    /// <summary>
    /// Local copies of outline materials, so their properties could be changed independently on other objects and materials.
    /// </summary>
    private Material localOutlineClickFirstPass;
    private Material localOutlineClickSecondPass;
    private Material localOutlineHoverFirstPass;
    private Material localOutlineHoverSecondPass;

    private Material localOutlineClickMaterial;
    private Material localOutlineHoverMaterial;


    public List<Renderer> Renderers = new List<Renderer>();

    public bool HoverOnly = false;

    //[Range(0.0f, 0.1f)]
    //public float OutlineSize = 0.05f;
    //private float CurrentOutlineSize;

    private bool selected = false;
    private bool highlighted = false;

    private Material[] gizmoMaterial;
    private bool gizmoHighlighted = false;

    private bool localMaterialsInstantiated = false;

    private void Awake() {
        if (!localMaterialsInstantiated) {
            InitMaterials();
        }

        InitGizmoMaterials();
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

    public void CompensateOutlineByModelScale(float modelScale) {
        if (OutlineShaderType == OutlineType.OnePassShader) {
            OutlineSize = (1f / modelScale) * OutlineClickMaterial.GetFloat("_OutlineWidth");
            OutlineHoverSize = (1f / modelScale) * OutlineHoverMaterial.GetFloat("_OutlineWidth");

            localOutlineClickMaterial.SetFloat("_OutlineWidth", OutlineSize);
            localOutlineHoverMaterial.SetFloat("_OutlineWidth", OutlineHoverSize);
            gizmoMaterial[0].SetFloat("_OutlineWidth", OutlineHoverSize);
        } else {
            OutlineSize = (1f / modelScale) * OutlineClickSecondPass.GetFloat("_OutlineWidth");
            OutlineHoverSize = (1f / modelScale) * OutlineHoverSecondPass.GetFloat("_OutlineWidth");

            localOutlineClickFirstPass.SetFloat("_OutlineWidth", OutlineSize);
            localOutlineClickSecondPass.SetFloat("_OutlineWidth", OutlineSize);
            localOutlineHoverFirstPass.SetFloat("_OutlineWidth", OutlineHoverSize);
            localOutlineHoverSecondPass.SetFloat("_OutlineWidth", OutlineHoverSize);
            gizmoMaterial[0].SetFloat("_OutlineWidth", OutlineHoverSize);
            gizmoMaterial[1].SetFloat("_OutlineWidth", OutlineHoverSize);
        }
    }

    public void InitMaterials() {
        if (OutlineClickFirstPass != null || OutlineClickSecondPass != null
            || OutlineHoverFirstPass != null || OutlineHoverSecondPass != null) {
            localOutlineClickFirstPass = new Material(OutlineClickFirstPass);
            localOutlineClickSecondPass = new Material(OutlineClickSecondPass);
            localOutlineHoverFirstPass = new Material(OutlineHoverFirstPass);
            localOutlineHoverSecondPass = new Material(OutlineHoverSecondPass);
        }

        if (OutlineClickMaterial != null || OutlineHoverMaterial != null) {
            localOutlineClickMaterial = new Material(OutlineClickMaterial);
            localOutlineHoverMaterial = new Material(OutlineHoverMaterial);
        }

        localMaterialsInstantiated = true;
    }

    public void InitGizmoMaterials() {
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
            SetOutline(localOutlineClickMaterial);
        } else {
            SetOutline(localOutlineClickFirstPass, localOutlineClickSecondPass);
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
            if (OutlineShaderType == OutlineType.TwoPassShader && materials.Count > 0) {
                materials.RemoveAt(0);
            }
            if (materials.Count > 0)
                materials.RemoveAt(materials.Count - 1);
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    /// <summary>
    /// Called when OnHoverStart/OnHoverEnd event is triggered from attached gameobject.
    /// </summary>
    public void Highlight() {
        if (!selected && !gizmoHighlighted && !highlighted) {
            highlighted = true;
            if (OutlineShaderType == OutlineType.OnePassShader) {
                SetOutline(localOutlineHoverMaterial);
            } else {
                SetOutline(localOutlineHoverFirstPass, localOutlineHoverSecondPass);
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

        if (!gizmoHighlighted) {
            if (OutlineShaderType == OutlineType.OnePassShader) {
                SetOutline(gizmoMaterial[0]);
            } else {
                SetOutline(gizmoMaterial[0], gizmoMaterial[1]);
            }
            gizmoHighlighted = true;
        }
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
