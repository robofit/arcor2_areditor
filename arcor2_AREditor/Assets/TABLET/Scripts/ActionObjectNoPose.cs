using System;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using RuntimeGizmos;
using UnityEngine;

public class ActionObjectNoPose : ActionObject {
    public override void CreateModel(CollisionModels customCollisionModels = null) {
        // no pose object has no model
    }

    public override GameObject GetModelCopy() {
        return null;
    }

    public override string GetObjectTypeName() {
        return "Action object";
    }

    public override Quaternion GetSceneOrientation() {
        throw new RequestFailedException("This object has no pose");
    }

    public override Vector3 GetScenePosition() {
        throw new RequestFailedException("This object has no pose");
    }

    public override bool HasMenu() {
        return true;
    }

    public override void Hide() {
        throw new NotImplementedException();
    }

    public override void OnClick(Click type) {
        throw new NotImplementedException();
    }

    public override void OnHoverEnd() {
        // should not do anything
    }

    public override void OnHoverStart() {
        // should not do anything
    }

    public override void OpenMenu() {
        TransformGizmo.Instance.ClearTargets();
        if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
            actionObjectMenu.CurrentObject = this;
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
        } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
            actionObjectMenuProjectEditor.CurrentObject = this;
            actionObjectMenuProjectEditor.UpdateMenu();
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
        }
    }

    public override void SetInteractivity(bool interactive) {
        
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        throw new RequestFailedException("This object has no pose");
    }

    public override void SetScenePosition(Vector3 position) {
        throw new RequestFailedException("This object has no pose");
    }

    public override void Show() {
        throw new NotImplementedException();
    }

    public override void StartManipulation() {
        throw new RequestFailedException("This object has no pose");
    }

}
