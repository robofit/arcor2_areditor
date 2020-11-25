using System;
using Base;
using UnityEngine;
using RuntimeGizmos;

public abstract class StartEndAction : Action3D {

    protected string playerPrefsKey;

    public override void OnClick(Click type) {
        if (!CheckClick())
            return;
        if (type == Click.MOUSE_LEFT_BUTTON || type == Click.LONG_TOUCH) {
            // We have clicked with left mouse and started manipulation with object
            TransformGizmo.Instance.AddTarget(Visual.transform);
            outlineOnClick.GizmoHighlight();
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

    
}
