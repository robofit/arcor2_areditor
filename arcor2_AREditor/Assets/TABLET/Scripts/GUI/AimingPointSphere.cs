using System.Threading.Tasks;
using Base;
using UnityEngine;
using System;

[RequireComponent(typeof(OutlineOnClick))]
public class AimingPointSphere : InteractiveObject {

    private string id, pointName;
    private int index;
    private bool aimed;
    [SerializeField]
    private OutlineOnClick outlineOnClick;
    public MeshRenderer Renderer;


    public int Index => index;
    public bool Aimed => aimed;

    private void Awake() {
        id = Guid.NewGuid().ToString();
        outlineOnClick = GetComponent<OutlineOnClick>();
    }

    public void SetAimed(bool aimed) {
        this.aimed = aimed;
        if (Renderer.materials.Length > 1)
            Renderer.materials[1].color = aimed ? Color.green : Color.red;
        else
            Renderer.material.color = aimed ? Color.green : Color.red;

    }

    public void Init(int index, string name) {
        pointName = name;
        this.index = index;
        SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
    }

    public void Highlight() {
        outlineOnClick.Highlight();
    }

    public void UnHighlight() {
        outlineOnClick.UnHighlight();
    }

    public override void CloseMenu() {
        throw new System.NotImplementedException();
    }

    public override string GetId() {
        return id;
    }

    public override string GetName() {
        return pointName;
    }

    public override string GetObjectTypeName() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Aiming points cannot be moved");
    }

    public override void OnClick(Click type) {
        throw new System.NotImplementedException();
    }

    public override void OnHoverEnd() {
        outlineOnClick.UnHighlight();
    }

    public override void OnHoverStart() {
        outlineOnClick.Highlight();
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(false, "Aiming points cannot be removed");
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override Task Rename(string name) {
        throw new System.NotImplementedException();
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public override void UpdateColor() {
        throw new System.NotImplementedException();
    }

    public override void EnableVisual(bool enable) {
        throw new NotImplementedException();
    }

    private void OnDestroy() {
        SelectorMenu.Instance.DestroySelectorItem(SelectorItem);
    }
}
