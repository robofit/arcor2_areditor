using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APOrientation : Base.Clickable
{
    public Base.ActionPoint ActionPoint;
    public string OrientationId;
    public override void OnClick(Click type) {
        if (type == Click.MOUSE_RIGHT_BUTTON || type == Click.TOUCH) {
            ActionPoint.ShowAimingMenu(OrientationId);
        }       
        
    }
}
