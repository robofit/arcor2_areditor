using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickCollider : Clickable {
    public GameObject Target;


    public override void OnHoverStart() {
        foreach (Clickable clickable in Target?.GetComponents<Clickable>()) {
            clickable.OnHoverStart();
        }
    }

    public override void OnHoverEnd() {
        foreach (Clickable clickable in Target?.GetComponents<Clickable>()) {
            clickable.OnHoverEnd();
        }
    }

}
