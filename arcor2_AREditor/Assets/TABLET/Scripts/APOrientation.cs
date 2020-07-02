using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class APOrientation : Base.Clickable
{
    public Base.ActionPoint ActionPoint;
    public string OrientationId;
    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        if (type == Click.MOUSE_RIGHT_BUTTON || (type == Click.TOUCH && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate))) {
            ActionPoint.ShowAimingMenu(OrientationId);
        }       
        
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
