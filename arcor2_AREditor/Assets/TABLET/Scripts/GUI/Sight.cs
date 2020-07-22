using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Sight : Singleton<Sight> {
        public GameObject CurrentObject;

        private void Update() {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Mathf.Infinity)) {
                try {
                    if (CurrentObject == null) {
                        hit.collider.transform.gameObject.SendMessage("OnHoverStart");
                        CurrentObject = hit.collider.transform.gameObject;
                    } else {
                        if (!GameObject.ReferenceEquals(hit.collider.transform.gameObject, CurrentObject)) {
                            CurrentObject.SendMessage("OnHoverEnd");
                            hit.collider.transform.gameObject.SendMessage("OnHoverStart");
                            CurrentObject = hit.collider.transform.gameObject;
                        }
                    }         
                } catch (Exception e) {
                    Debug.LogError(e);
                }
            } else {
                if (CurrentObject != null) {
                    CurrentObject.SendMessage("OnHoverEnd");
                    CurrentObject = null;
                }
            }
        }

        public void Click() {
            if (CurrentObject == null)
                return;
            Clickable clickable = CurrentObject.GetComponent<Clickable>();
            if (clickable == null)
                return;
            clickable.OnClick(Clickable.Click.TOUCH);
        }
    }
}

