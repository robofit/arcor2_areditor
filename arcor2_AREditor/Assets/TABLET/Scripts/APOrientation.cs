using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;


[RequireComponent(typeof(OutlineOnClick))]
public class APOrientation : Base.Clickable {
    public Base.ActionPoint ActionPoint;
    public string OrientationId;

    [SerializeField]
    private OutlineOnClick outlineOnClick;


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        if (type == Click.MOUSE_RIGHT_BUTTON || (type == Click.TOUCH && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate))) {
            ActionPoint.ShowAimingMenu(OrientationId);
            HighlightOrientation(true);
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
}
