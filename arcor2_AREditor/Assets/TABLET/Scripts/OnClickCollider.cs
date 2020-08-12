using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickCollider : Clickable {
    public GameObject Target;

    public override void OnClick(Click type) {
        foreach (Clickable clickable in Target.GetComponents<Clickable>()) {
            clickable.OnClick(type);
        }
    }

    public override void OnHoverStart() {
        foreach (Clickable clickable in Target.GetComponents<Clickable>()) {
            clickable.OnHoverStart();
        }
    }

    public override void OnHoverEnd() {
        foreach (Clickable clickable in Target.GetComponents<Clickable>()) {
            clickable.OnHoverEnd();
        }
    }

    //public void OnMouseOver() {
    //    // if we are clicking on UI
    //    if (EventSystem.current.IsPointerOverGameObject()) {
    //        return;
    //    }

    //    if (Input.GetMouseButtonDown(0)) {
    //        Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_LEFT_BUTTON);
    //        Debug.Log("CLICK MOUSE_LEFT_BUTTON");
    //    }
    //    if (Input.GetMouseButtonDown(1)) {
    //        Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_RIGHT_BUTTON);
    //        Debug.Log("CLICK MOUSE_RIGHT_BUTTON");
    //    }
    //    if (Input.GetMouseButtonDown(2)) {
    //        Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_MIDDLE_BUTTON);
    //        Debug.Log("CLICK MOUSE_MIDDLE_BUTTON");
    //    }
    //}
}
