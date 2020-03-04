using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using UnityEngine.EventSystems;

public class OutlineOnClick : Clickable {

    public Material ClickMaterial;
    public Material HoverMaterial;

    private Renderer renderer;
    private List<Material> materials = new List<Material>();

    private void Start() {
        renderer = GetComponent<Renderer>();
        materials.AddRange(renderer.materials);
    }

    public override void OnClick(Click type) {
        if (type == Click.TOUCH) {
        } else if (type == Click.LONG_TOUCH) {
        }
    }

    public void OnMouseOver() {
        // if we are clicking on UI
        //if (EventSystem.current.IsPointerOverGameObject()) {
        //    AddMaterial(HoverMaterial);
        //    renderer.materials = materials.ToArray();
        //}

        if (Input.GetMouseButtonDown(0)) {
            //Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_LEFT_BUTTON);
        }
        if (Input.GetMouseButtonDown(1)) {
            //Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_RIGHT_BUTTON);
            AddMaterial(ClickMaterial);
            renderer.materials = materials.ToArray();
        }
        if (Input.GetMouseButtonDown(2)) {
            //Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_MIDDLE_BUTTON);
        }
    }

    private void AddMaterial(Material material) {
        if (!materials.Contains(material)) {
            materials.Add(material);
        }
    }

    private void RemoveMaterial(Material material) {
        if (materials.Contains(material)) {
            materials.Remove(material);
        }
    }
}
