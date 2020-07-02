using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class OnHoverCollider : MonoBehaviour
{
    public GameObject Target;
    private void OnMouseEnter() {
        foreach (Clickable clickable in Target.GetComponents<Clickable>()) {
            clickable.OnHoverStart();
        }
    }

    private void OnMouseExit() {
        foreach (Clickable clickable in Target.GetComponents<Clickable>()) {
            clickable.OnHoverEnd();
        }
    }

}
