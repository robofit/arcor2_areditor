using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;


public class TouchInput : Singleton<TouchInput> {

    private void Update() {
        RaycastHit hit = new RaycastHit();
        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                // Construct a ray from the current touch coordinates
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out hit)) {
                    try {
                        hit.transform.gameObject.SendMessage("OnClick", Base.Clickable.Click.TOUCH);
                    }
                    catch (Exception e) {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }

}
