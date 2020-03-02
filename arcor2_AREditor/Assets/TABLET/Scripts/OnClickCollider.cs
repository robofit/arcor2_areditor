using System.Collections;
using System.Collections.Generic;
using Base;
using RuntimeGizmos;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickCollider : Clickable {
    public GameObject Target;

    public override void OnClick(Click type) {
        if (type == Click.TOUCH) {
            Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.TOUCH);
        } else if (type == Click.LONG_TOUCH) {
            Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.LONG_TOUCH);
        }
    }

    public void OnMouseOver() {
        // if we are clicking on UI
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_LEFT_BUTTON);
        }
        if (Input.GetMouseButtonDown(1)) {
            Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_RIGHT_BUTTON);
        }
        if (Input.GetMouseButtonDown(2)) {
            Target.GetComponent<Base.Clickable>().OnClick(Base.Clickable.Click.MOUSE_MIDDLE_BUTTON);
        }
    }
}
