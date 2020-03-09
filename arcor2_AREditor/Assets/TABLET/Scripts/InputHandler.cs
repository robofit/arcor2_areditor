using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;


public class InputHandler : Singleton<InputHandler> {

    public LayerMask LayerMask;

    public delegate void EventBlindClick(object sender, EventBlindClickArgs args);
    public event EventBlindClick OnBlindClick;


    private bool longTouch = false;
    private IEnumerator coroutine;

    private void Update() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        HandleTouch();
#else
        HandleClick();
#endif
    }

    private void HandleClick() {
        // Left Button
        if (Input.GetMouseButtonDown(0)) {
            TryToRaycast(Clickable.Click.MOUSE_LEFT_BUTTON);
        }
        // Right Button
        else if (Input.GetMouseButtonDown(1)) {
            TryToRaycast(Clickable.Click.MOUSE_RIGHT_BUTTON);
        }
        // Middle Button
        else if (Input.GetMouseButtonDown(2)) {
            TryToRaycast(Clickable.Click.MOUSE_MIDDLE_BUTTON);
        }
    }

    private void TryToRaycast(Clickable.Click clickType) {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask)) {
            try {
                hit.transform.gameObject.SendMessage("OnClick", clickType);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        } else {
            OnBlindClick?.Invoke(this, new EventBlindClickArgs(clickType));
        }
    }


    private void HandleTouch() {
        RaycastHit hit = new RaycastHit();
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                coroutine = LongTouch(touch);
                StartCoroutine(coroutine);
            }

            if (touch.phase == TouchPhase.Ended) {
                if (longTouch) {
                    longTouch = false;

                } else {
                    StopCoroutine(coroutine);
                    longTouch = false;

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, Mathf.Infinity, LayerMask)) {
                        try {
                            hit.transform.gameObject.SendMessage("OnClick", Clickable.Click.TOUCH);
                        } catch (Exception e) {
                            Debug.LogError(e);
                        }
                    } else {
                        OnBlindClick?.Invoke(this, new EventBlindClickArgs(Clickable.Click.TOUCH));
                    }
                }
            }
        }
    }

    private IEnumerator LongTouch(Touch touch) {
        yield return new WaitForSeconds(3f);

        longTouch = true;
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, Mathf.Infinity, LayerMask)) {
            try {
                hit.transform.gameObject.SendMessage("OnClick", Clickable.Click.LONG_TOUCH);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        } else {
            OnBlindClick?.Invoke(this, new EventBlindClickArgs(Clickable.Click.LONG_TOUCH));
        }
    }

}

public class EventBlindClickArgs : EventArgs {

    public Clickable.Click ClickType;

    public EventBlindClickArgs(Clickable.Click clickType) {
        ClickType = clickType;
    }
}
