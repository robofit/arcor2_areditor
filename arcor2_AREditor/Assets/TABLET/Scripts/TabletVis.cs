using System.Collections;
using System.Collections.Generic;
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

    public override bool Movable() {
        return false;
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

    public override bool Removable() {
        return false;
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override void Rename(string name) {
        throw new System.NotImplementedException();
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }
}
