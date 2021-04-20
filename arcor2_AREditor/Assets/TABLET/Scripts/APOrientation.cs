using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using UnityEngine;


[RequireComponent(typeof(OutlineOnClick))]
public class APOrientation : InteractiveObject {
    public Base.ActionPoint ActionPoint;
    public string OrientationId;

    [SerializeField]
    private OutlineOnClick outlineOnClick;


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        if (ActionPoint.Locked) {
            Notifications.Instance.ShowNotification("Failed to open orientation detail", "AP is locked");
            return;
        }
        if (type == Click.MOUSE_RIGHT_BUTTON || (type == Click.TOUCH)) {
            OpenMenu();
        }       
        
    }

    public override void OnHoverStart() {
        if (!enabled)
            return;
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPoint &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Closed) {
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

        HighlightOrientation(true);
    }

    public override void OnHoverEnd() {
        HighlightOrientation(false);
    }

    public void HighlightOrientation(bool highlight) {
        if (highlight) {
            outlineOnClick.Highlight();
        } else {
            outlineOnClick.UnHighlight();
        }
    }

    public void SetOrientation(IO.Swagger.Model.Orientation orientation) {
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
        ActionPoint.ShowAimingMenu(OrientationId);
        HighlightOrientation(true);
    }

    public override bool HasMenu() {
        return true;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Orientation could not be moved");
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        try {
            await WebsocketManager.Instance.RemoveActionPointOrientation(OrientationId, true);
            return new RequestResult(true);
        } catch (RequestFailedException ex) {
            return new RequestResult(false, ex.Message);
        }
    }

    public async override void Remove() {
        try {
            await WebsocketManager.Instance.RemoveActionPointOrientation(OrientationId, false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to remove orientation", ex.Message);
        }
    }

    public async override Task Rename(string name) {
        try {
            await WebsocketManager.Instance.RenameActionPointOrientation(GetId(), name);
            Notifications.Instance.ShowToastMessage("Orientation renamed");
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename orientation", e.Message);
            throw;
        }
    }

    public override string GetObjectTypeName() {
        return "Orientation";
    }

    public override void Enable(bool enable) {
        if (SelectorMenu.Instance.PointsToggle.Toggled && !IsLocked)
            Enabled = true;
        else
            Enabled = false;
    }
}
