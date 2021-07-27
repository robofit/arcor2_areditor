using System.Threading.Tasks;
using Base;
using UnityEngine;
using System;

[RequireComponent(typeof(OutlineOnClick))]
public class AimingPointSphere : InteractiveObject {

    private string id, pointName;
    [SerializeField]
    private OutlineOnClick outlineOnClick;
    private void Awake() {
        id = Guid.NewGuid().ToString();
        outlineOnClick = GetComponent<OutlineOnClick>();
    }

    public void Init(string name) {
        pointName = name;
        SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
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
}
