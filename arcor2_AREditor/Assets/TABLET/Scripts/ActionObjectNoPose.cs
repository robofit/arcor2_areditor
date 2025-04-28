using System;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;

public class ActionObjectNoPose : ActionObject {
    public override void CloseMenu() {

        ActionObjectMenu.Instance.Hide();
    }

    public override void CreateModel(CollisionModels customCollisionModels = null) {
        // no pose object has no model
    }

    public override void EnableVisual(bool enable) {
        throw new NotImplementedException();
    }

    public override GameObject GetModelCopy() {
        return null;
    }

    public override string GetObjectTypeName() {
        return "Action object";
    }

    public override Quaternion GetSceneOrientation() {
        throw new Arcor2ConnectionException("This object has no pose");
    }

    public override Vector3 GetScenePosition() {
        throw new Arcor2ConnectionException("This object has no pose");
    }

    public override bool HasMenu() {
        return true;
    }

    public override void Hide() {
        throw new NotImplementedException();
    }


    public override void OnHoverEnd() {
        // should not do anything
    }

    public override void OnHoverStart() {
        // should not do anything
    }

    public override async void OpenMenu() {
        _ = ActionObjectMenu.Instance.Show(this, false);
    }

    public override void SetInteractivity(bool interactive) {

    }

    public override void SetSceneOrientation(Quaternion orientation) {
        throw new Arcor2ConnectionException("This object has no pose");
    }

    public override void SetScenePosition(Vector3 position) {
        throw new Arcor2ConnectionException("This object has no pose");
    }

    public override void Show() {
        throw new NotImplementedException();
    }

    public override void StartManipulation() {
        throw new Arcor2ConnectionException("This object has no pose");
    }

    public override void UpdateColor() {
        //nothing to do here
    }

    public override void UpdateModel() {
        // nothing to do here
    }
}
