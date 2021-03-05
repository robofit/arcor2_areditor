using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformWheelList : EventTrigger {
    private bool dragging;
    private float mouseYLast;

    private void Awake() {
        mouseYLast = Input.mousePosition.y;
    }

    public void Update() {
        if (dragging) {
            transform.position = new Vector2(transform.position.x, transform.position.y)
                + new Vector2(0, Input.mousePosition.y - mouseYLast);
        }
        mouseYLast = Input.mousePosition.y;
    }

    public override void OnPointerDown(PointerEventData eventData) {
        dragging = true;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        dragging = false;
    }
}
