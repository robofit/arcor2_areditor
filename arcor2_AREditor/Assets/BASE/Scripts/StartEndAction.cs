using System;
using Base;
using UnityEngine;
using RuntimeGizmos;
using System.Threading.Tasks;

public abstract class StartEndAction : Action3D {

    protected string playerPrefsKey;

    public override void OnClick(Click type) {
        if (!CheckClick())
            return;
        if (type == Click.MOUSE_LEFT_BUTTON || type == Click.LONG_TOUCH) {
            // We have clicked with left mouse and started manipulation with object
            StartManipulation();
        }
    }

    public virtual void Init(IO.Swagger.Model.Action projectAction, Base.ActionMetadata metadata, Base.ActionPoint ap, IActionProvider actionProvider, string actionType) {
        base.Init(projectAction, metadata, ap, actionProvider);

        if (!Base.ProjectManager.Instance.ProjectMeta.HasLogic) {
            Destroy(gameObject);
            return;
        }
        playerPrefsKey = "project/" + ProjectManager.Instance.ProjectMeta.Id + "/" + actionType;

    }

    private void Update() {
        if (gameObject.transform.hasChanged) {
            PlayerPrefsHelper.SaveVector3(playerPrefsKey, transform.localPosition);
            transform.hasChanged = false;
        }
    }

    public override void OnHoverStart() {
        base.OnHoverStart();
    }

    public override void OnHoverEnd() {
        base.OnHoverEnd();
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(true);
    }

    public override bool HasMenu() {
        return false;
    }

    public override void StartManipulation() {
        TransformGizmo.Instance.AddTarget(Visual.transform);
        outlineOnClick.GizmoHighlight();
    }

    public override async Task<bool> WriteUnlock() {
        return true;
    }

    public override async Task<bool> WriteLock(bool lockTree) {
        return true;
    }

    protected override void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        return;
    }

    public override void OnObjectLocked(string owner) {
        return;
    }

    public override void OnObjectUnlocked() {
        return;
    }
}
