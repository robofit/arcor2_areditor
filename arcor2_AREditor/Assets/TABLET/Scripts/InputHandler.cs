using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : Singleton<InputHandler> {

    public LayerMask LayerMask;

    public delegate void EventClick(object sender, EventClickArgs args);
    public event EventClick OnBlindClick;
    public event EventClick OnGeneralClick;

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
        // Do not raycast through UI element
        if (!EventSystem.current.IsPointerOverGameObject()) {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask)) {
                try {
                    hit.collider.transform.gameObject.SendMessage("OnClick", clickType);
                } catch (Exception e) {
                    Debug.LogError(e);
                }
            } else {
                OnBlindClick?.Invoke(this, new EventClickArgs(clickType));
            }

            OnGeneralClick?.Invoke(this, new EventClickArgs(clickType));
        }
    }


    private void HandleTouch() {
        RaycastHit hit = new RaycastHit();
        foreach (Touch touch in Input.touches) {
            // Do not raycast through UI element
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {

                if (touch.phase == TouchPhase.Began) {
                    if (coroutine != null)
                        StopCoroutine(coroutine);
                    coroutine = LongTouch(touch);
                    StartCoroutine(coroutine);

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, Mathf.Infinity, LayerMask)) {
                        try {
                            hit.collider.transform.gameObject.SendMessage("OnClick", Clickable.Click.TOUCH);
                        } catch (Exception e) {
                            Debug.LogError(e);
                        }
                    } else {
                        OnBlindClick?.Invoke(this, new EventClickArgs(Clickable.Click.TOUCH));
                    }

                    OnGeneralClick?.Invoke(this, new EventClickArgs(Clickable.Click.TOUCH));
                }

                // NOTE: TouchPhase.Ended always ignores UI clicking check (IsPointerOverGameObject)
                if (touch.phase == TouchPhase.Ended) {
                    if (longTouch) {
                        longTouch = false;

                    } else {
                        if (coroutine != null)
                            StopCoroutine(coroutine);
                        longTouch = false;

                        if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, Mathf.Infinity, LayerMask)) {
                            try {
                                hit.collider.transform.gameObject.SendMessage("OnClick", Clickable.Click.TOUCH_ENDED);
                            } catch (Exception e) {
                                Debug.LogError(e);
                            }
                        } else {
                            OnBlindClick?.Invoke(this, new EventClickArgs(Clickable.Click.TOUCH_ENDED));
                        }

                        OnGeneralClick?.Invoke(this, new EventClickArgs(Clickable.Click.TOUCH_ENDED));
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
                hit.collider.transform.gameObject.SendMessage("OnClick", Clickable.Click.LONG_TOUCH);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        } else {
            OnBlindClick?.Invoke(this, new EventClickArgs(Clickable.Click.LONG_TOUCH));
        }

        OnGeneralClick?.Invoke(this, new EventClickArgs(Clickable.Click.LONG_TOUCH));
    }

}

public class EventClickArgs : EventArgs {

    public Clickable.Click ClickType;

    public EventClickArgs(Clickable.Click clickType) {
        ClickType = clickType;
    }
}
