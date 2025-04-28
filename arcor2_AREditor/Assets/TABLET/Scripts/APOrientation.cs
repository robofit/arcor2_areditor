using System;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;
using ActionPoint = Base.ActionPoint;

[RequireComponent(typeof(OutlineOnClick))]
[RequireComponent(typeof(Target))]
public class APOrientation : InteractiveObject, ISubItem {
    public ActionPoint ActionPoint;
    public string OrientationId;

    [SerializeField]
    private OutlineOnClick outlineOnClick;

    [SerializeField]
    private MeshRenderer renderer;

    public override void OnHoverStart() {
        if (!enabled)
            return;


        HighlightOrientation(true);
        if (SelectorMenu.Instance.ManuallySelected) {
            DisplayOffscreenIndicator(true);
        }
    }

    public override void OnHoverEnd() {
        HighlightOrientation(false);
        DisplayOffscreenIndicator(false);
    }

    public void HighlightOrientation(bool highlight) {
        if (highlight) {
            outlineOnClick.Highlight();
        } else {
            outlineOnClick.UnHighlight();
        }
    }

    public void SetOrientation(Orientation orientation) {
        transform.localRotation = TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(orientation));
    }

    public override string GetName() {
        return ActionPoint.GetNamedOrientation(OrientationId).Name;
        //return ProjectManager.Instance.GetNamedOrientation(OrientationId).Name;
    }

    public override string GetId() {
        return OrientationId;
    }

    public override async void OpenMenu() {
        throw new NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public async Task<bool> OpenDetailMenu() {
        if (await ActionPoint.ShowOrientationDetailMenu(OrientationId)) {
            HighlightOrientation(true);
            return true;
        }
        return false;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Orientation could not be moved");
    }

    public override void StartManipulation() {
        throw new NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        try {
            await CommunicationManager.Instance.Client.RemoveActionPointOrientationAsync(new RemoveActionPointOrientationRequestArgs(OrientationId), true);
            return new RequestResult(true);
        } catch (Arcor2ConnectionException ex) {
            return new RequestResult(false, ex.Message);
        }
    }

    public async override void Remove() {
        try {
            await CommunicationManager.Instance.Client.RemoveActionPointOrientationAsync(new RemoveActionPointOrientationRequestArgs(OrientationId), false);
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to remove orientation", ex.Message);
        }
    }

    public async override Task Rename(string name) {
        try {
             await CommunicationManager.Instance.Client.RenameActionPointOrientationAsync(new RenameActionPointOrientationRequestArgs(GetId(), name));
            Notifications.Instance.ShowToastMessage("Orientation renamed");
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to rename orientation", e.Message);
            throw;
        }
    }

    public override string GetObjectTypeName() {
        return "Orientation";
    }

    public override void UpdateColor() {
        Color c;
        if (Enabled && !(IsLocked && !IsLockedByMe))
            c = new Color(0.9921f, 0.721f, 0.074f);
        else
            c = Color.gray;
        foreach (Renderer r in outlineOnClick.Renderers)
            r.material.color = c;

    }

    public InteractiveObject GetParentObject() {
        return ActionPoint;
    }

    public override void DestroyObject() {
        base.DestroyObject();
        Destroy(gameObject);
    }

    public override void CloseMenu() {
        ActionPointAimingMenu.Instance.Hide();
        HighlightOrientation(false);
    }

    public override void EnableVisual(bool enable) {
        throw new NotImplementedException();
    }
}
