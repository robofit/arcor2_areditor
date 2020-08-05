using System;
using Base;
using UnityEngine;
using RuntimeGizmos;

public abstract class StartEndAction : Action3D {

    private string playerPrefsKey;

    public override void OnClick(Click type) {
        if (!CheckClick())
            return;
        if (type == Click.MOUSE_LEFT_BUTTON || type == Click.LONG_TOUCH) {
            // We have clicked with left mouse and started manipulation with object
            TransformGizmo.Instance.AddTarget(Visual.transform);
            outlineOnClick.GizmoHighlight();
        }
    }

    public void Init(IO.Swagger.Model.Action projectAction, Base.ActionMetadata metadata, Base.ActionPoint ap, IActionProvider actionProvider, string keySuffix) {
        base.Init(projectAction, metadata, ap, actionProvider);

        if (!Base.ProjectManager.Instance.ProjectMeta.HasLogic) {
            Destroy(gameObject);
            return;
        }
        playerPrefsKey = "project/" + ProjectManager.Instance.ProjectMeta.Id + "/" + keySuffix;
        transform.localPosition = PlayerPrefsHelper.LoadVector3(playerPrefsKey, Vector3.zero);
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
