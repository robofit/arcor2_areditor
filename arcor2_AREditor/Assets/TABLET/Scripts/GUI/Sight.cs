using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Base {
    public class Sight : Singleton<Sight> {
        public GameObject CurrentObject;

        public System.DateTime HoverStartTime;

        private bool endingHover = false;

        private void Update() {
            if (SelectorMenu.Instance.CanvasGroup.alpha == 0)
                return;
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Mathf.Infinity)) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(0.1f, 0.1f, 0.0001f), ray.direction);

            List<Tuple<float, InteractiveObject>> orderedTransforms = new List<Tuple<float, InteractiveObject>>();
            foreach (RaycastHit hit in hits) {

                float distance = Vector3.Cross(ray.direction, hit.point - ray.origin).magnitude;

                InteractiveObject interactiveObject = hit.collider.transform.gameObject.GetComponent<InteractiveObject>();
                if (interactiveObject is null) {
                    OnClickCollider collider = hit.collider.transform.gameObject.GetComponent<OnClickCollider>();
                    if (collider is null) {
                        continue;
                    }
                    interactiveObject = collider.Target.gameObject.GetComponent<InteractiveObject>();
                    if (interactiveObject is null) {
                        continue;
                    }
                }
                if (!InteractiveObjectInList(orderedTransforms, interactiveObject)) {
                    orderedTransforms.Add(new Tuple<float, InteractiveObject>(distance, interactiveObject));
                }

                //hit.collider.transform.gameObject.SendMessage("OnHoverStart");
                /*try {
                    if (CurrentObject == null) {
                        hit.collider.transform.gameObject.SendMessage("OnHoverStart");
                        HoverStartTime = System.DateTime.UtcNow;
                        CurrentObject = hit.collider.transform.gameObject;
                    } else {
                        if (!GameObject.ReferenceEquals(hit.collider.transform.gameObject, CurrentObject)) {
                            CurrentObject.SendMessage("OnHoverEnd");
                            if (endingHover) {
                                StopAllCoroutines();
                                endingHover = false;
                            }
                            hit.collider.transform.gameObject.SendMessage("OnHoverStart");
                            HoverStartTime = System.DateTime.UtcNow;
                            CurrentObject = hit.collider.transform.gameObject;
                        } else {

                            if (endingHover) {
                                StopAllCoroutines();
                                endingHover = false;
                                HoverStartTime = System.DateTime.UtcNow;
                            }
                        }
                    }         
                } catch (Exception e) {
                    Debug.LogError(e);
                }*/
            }

            orderedTransforms.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            SelectorMenu.Instance.UpdateAimMenu(orderedTransforms);
            //SelectorMenu.Instance.UpdateAimMenu(orderedTransforms.Select(_ => _.Item2).Distinct().ToList());
            
             /*else {
                if (CurrentObject != null) {
                    if (!endingHover)
                        StartCoroutine(HoverEnd());
                }
            }*/
        }

        private bool InteractiveObjectInList(List<Tuple<float, InteractiveObject>> list, InteractiveObject interactiveObject) {
            foreach (Tuple<float, InteractiveObject> item in list) {
                if (interactiveObject.GetId() == item.Item2.GetId()) {
                    return true;
                }
            }
            return false;
        }        

        private IEnumerator HoverEnd() {
            endingHover = true;
            yield return new WaitForSeconds((float) (0.5d - (System.DateTime.UtcNow - HoverStartTime).TotalSeconds));
            if (CurrentObject != null) {
                CurrentObject.SendMessage("OnHoverEnd");
                CurrentObject = null;
            }
            endingHover = false;
        }

        public void Touch() {
            if (CurrentObject == null)
                return;
            Clickable clickable = CurrentObject.GetComponent<Clickable>();
            if (clickable == null)
                return;
            clickable.OnClick(Clickable.Click.TOUCH);
        }

        public void LongTouch() {
            if (CurrentObject == null)
                return;
            Clickable clickable = CurrentObject.GetComponent<Clickable>();
            if (clickable == null)
                return;
            clickable.OnClick(Clickable.Click.LONG_TOUCH);
        }
    }
}

