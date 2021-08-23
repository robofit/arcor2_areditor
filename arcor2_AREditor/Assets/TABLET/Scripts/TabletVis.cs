using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class TabletVis : InteractiveObject {
    [SerializeField]
    private GameObject name;

    public override string GetId() {
        return "Me";
    }

    public override string GetName() {
        return "Me";
    }

    public override bool HasMenu() {
        return false;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Tablet vizualization could not be moved");
    }

    public override void OnClick(Click type) {
        
    }

    public override void OnHoverEnd() {
        name.SetActive(false);
    }

    public override void OnHoverStart() {
        name.SetActive(true);
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(false, "Tablet vizualization could not be removed");
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
    public override string GetObjectTypeName() {
        return GetName();
    }

    public override void UpdateColor() {
        //nothing to do here...
    }

    public override void CloseMenu() {
        throw new System.NotImplementedException();
    }

    public override void EnableVisual(bool enable) {
        throw new System.NotImplementedException();
    }
}
