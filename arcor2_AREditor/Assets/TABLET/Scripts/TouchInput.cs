using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;


public class TouchInput : Singleton<TouchInput> {

    private bool longTouch = false;
    private IEnumerator coroutine;

    private void Update() {
        RaycastHit hit = new RaycastHit();
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                if(coroutine != null)
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

                    // Construct a ray from the current touch coordinates
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out hit)) {
                        try {
                            hit.transform.gameObject.SendMessage("OnClick", Base.Clickable.Click.TOUCH);
                        } catch (Exception e) {
                            Debug.LogError(e);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator LongTouch(Touch touch) {
        yield return new WaitForSeconds(3f);

        longTouch = true;
        RaycastHit hit = new RaycastHit();

        // Construct a ray from the current touch coordinates
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out hit)) {
            try {
                hit.transform.gameObject.SendMessage("OnClick", Base.Clickable.Click.LONG_TOUCH);
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }

}
