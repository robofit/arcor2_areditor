using System;
using Base;
using UnityEngine;
using RuntimeGizmos;
using System.Threading.Tasks;

public abstract class StartEndAction : Base.Action {
    public Renderer Visual;

    protected string playerPrefsKey;
    [SerializeField]
    protected OutlineOnClick outlineOnClick;

    public override void OnClick(Click type) {
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
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingAction) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
                if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning)
                    return;
            } else {
                return;
            }
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning) {
            return;
        }
        outlineOnClick.Highlight();
        NameText.gameObject.SetActive(true);
    }

    public override void OnHoverEnd() {
        outlineOnClick.UnHighlight();
        NameText.gameObject.SetActive(false);
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

    public override string GetName() {
        return Data.Name;
    }    

    public override void OpenMenu() {
        throw new NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(false, GetObjectTypeName() + " could not be removed");
    }

    public override void Remove() {
        throw new NotImplementedException();
    }

    public override Task Rename(string name) {
        throw new NotImplementedException();
    }
}
