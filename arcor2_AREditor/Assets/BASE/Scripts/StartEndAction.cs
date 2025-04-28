using System;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using Action = Base.Action;

public abstract class StartEndAction : Action {
    public Renderer Visual;
    public GameObject VisualRoot;

    protected string playerPrefsKey;
    [SerializeField]
    protected OutlineOnClick outlineOnClick;
    public GameObject ModelPrefab;


    public virtual void Init(Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider, string actionType) {
        base.Init(projectAction, metadata, ap, actionProvider);

        if (!ProjectManager.Instance.ProjectMeta.HasLogic) {
            Destroy(gameObject);
            return;
        }
        playerPrefsKey = "project/" + ProjectManager.Instance.ProjectMeta.Id + "/" + actionType;

    }

    public void SavePosition() {
        PlayerPrefsHelper.SaveVector3(playerPrefsKey, transform.localPosition);
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
        if (SelectorMenu.Instance.ManuallySelected) {
            DisplayOffscreenIndicator(true);
        }
    }

    public override void OnHoverEnd() {
        outlineOnClick.UnHighlight();
        NameText.gameObject.SetActive(false);
        DisplayOffscreenIndicator(false);
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(true);
    }

    public override bool HasMenu() {
        return false;
    }

    public override void StartManipulation() {
        throw new NotImplementedException();
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

    public GameObject GetModelCopy() {
        return Instantiate(ModelPrefab);
    }

    public override void EnableVisual(bool enable) {
        VisualRoot.SetActive(enable);
    }
}
