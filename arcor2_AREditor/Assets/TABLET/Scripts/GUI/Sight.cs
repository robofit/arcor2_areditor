using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RuntimeInspectorNamespace;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;

namespace Base {

   public class Sight : Singleton<Sight> {
        public GameObject CurrentObject;
        public Collider CameraCollider;

        public System.DateTime HoverStartTime;

        private bool endingHover = false;

        public AREditorEventArgs.GizmoAxisEventHandler SelectedGizmoAxis;


        private void Awake() {
            GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        }

        private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
            switch (args.Data) {
                case GameManager.EditorStateEnum.Normal:
                case GameManager.EditorStateEnum.InteractionDisabled:
                    enabled = true;
                    break;
                case GameManager.EditorStateEnum.Closed:
                    enabled = false;
                    break;
            }
        }
        private void Update() {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            if (SelectedGizmoAxis?.GetInvocationList().Length > 0) {
                RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
                foreach (RaycastHit hit in hits) {
                    if (hit.collider.gameObject.CompareTag("gizmo_x")) {
                        SelectedGizmoAxis.Invoke(this, new GizmoAxisEventArgs(Gizmo.Axis.X));
                    } else if (hit.collider.gameObject.CompareTag("gizmo_y")) {
                        SelectedGizmoAxis.Invoke(this, new GizmoAxisEventArgs(Gizmo.Axis.Y));
                    } else if(hit.collider.gameObject.CompareTag("gizmo_z")) {
                        SelectedGizmoAxis.Invoke(this, new GizmoAxisEventArgs(Gizmo.Axis.Z));
                    }
                }
            }
            if (SelectorMenu.Instance.CanvasGroup.alpha > 0 && SelectorMenu.Instance.gameObject.activeSelf) {
                

                RaycastHit hitinfo = new RaycastHit();
                bool anyHit = false, directHit = false;
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    hitinfo = hit;
                    anyHit = true;
                    directHit = true;
                    //Debug.DrawRay(ray.origin, ray.direction);
                } else {
                    RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(0.03f, 0.03f, 0.0001f), ray.direction, Camera.main.transform.rotation);
                    if (hits.Length > 0) {

                        float minDist = float.MaxValue;
                        foreach (RaycastHit h in hits) {
                            Vector3 dir = ray.direction;

                            Vector3 point = ray.origin + dir * Vector3.Distance(ray.origin, h.point);
                            float dist = Vector3.Distance(point, h.collider.ClosestPointOnBounds(point));
                            Debug.DrawLine(point, h.point);
                            Debug.DrawRay(ray.origin, ray.direction);
                            if (dist < minDist) {
                                hitinfo = h;
                                anyHit = true;
                                minDist = dist;
                            }
                        }
                    }
                }
                if (anyHit) {
                    Vector3 lhs = hitinfo.point - ray.origin;

                    float dotP = Vector3.Dot(lhs, ray.direction.normalized);
                    Vector3 point = ray.origin + ray.direction.normalized * dotP;
                    List<Tuple<float, InteractiveObject>> items = new List<Tuple<float, InteractiveObject>>();
                    bool h = false;
                    foreach (SelectorItem item in SelectorMenu.Instance.SelectorItems.Values) {
                        if (!item.InteractiveObject.Enabled)
                            continue;
                        try {
                            if (item.InteractiveObject == null) {
                                continue;

                            }
                            float dist = item.InteractiveObject.GetDistance(hitinfo.point);

                            foreach (Collider c in item.InteractiveObject.Colliders) {

                                if (c == hitinfo.collider) {
                                    dist = 0;
                                    h = true;
                                }

                            }


                            if (item.InteractiveObject is ActionObjectNoPose || dist > 0.2) { // add objects max 20cm away from point of impact 

                                Debug.DrawLine(ray.origin, hitinfo.point);
                                continue;
                            }
                            
                            items.Add(new Tuple<float, InteractiveObject>(dist, item.InteractiveObject));
                        } catch (MissingReferenceException ex) {
                            Debug.LogError(ex);
                            Debug.LogError($"{item.InteractiveObject.GetName()}: {hitinfo.collider.name}");
                        }
                    }
                    if (h) {
                        items.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                       /* if (items.Count > 10)
                            items.RemoveRange(10, items.Count - 10);*/
                        SelectorMenu.Instance.UpdateAimMenu(items);
                    } else {
                        SelectorMenu.Instance.UpdateAimMenu(new List<Tuple<float, InteractiveObject>>());
                    }
                } else {
                    SelectorMenu.Instance.UpdateAimMenu(new List<Tuple<float, InteractiveObject>>());
                }

            }

            //if (SelectorMenu.Instance.Active) {
            /* if (SelectorMenu.Instance.CanvasGroup.alpha > 0 && SelectorMenu.Instance.gameObject.activeSelf) {

             //if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Mathf.Infinity)) {
                 Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

                 RaycastHit hitinfo = new RaycastHit();
                 bool anyHit = false, directHit = false;
                 if (Physics.Raycast(ray, out RaycastHit hit)) {
                     hitinfo = hit;
                     anyHit = true;
                     directHit = true;
                     //Debug.DrawRay(ray.origin, ray.direction);
                 } else {
                     RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(0.03f, 0.03f, 0.0001f), ray.direction, Camera.main.transform.rotation);
                     if (hits.Length > 0) {

                         float minDist = float.MaxValue;
                         foreach (RaycastHit h in hits) {
                             Vector3 dir = ray.direction;

                             Vector3 point = ray.origin + dir * Vector3.Distance(ray.origin, h.point);
                             float dist = Vector3.Distance(point, h.collider.ClosestPointOnBounds(point));
                             Debug.DrawLine(point, h.point);
                             Debug.DrawRay(ray.origin, ray.direction);
                             if (dist < minDist) {
                                 hitinfo = h;
                                 anyHit = true;
                                 minDist = dist;
                             }
                         }
                     }
                 }
                 if (anyHit) {
                     Vector3 lhs = hitinfo.point - ray.origin;

                     float dotP = Vector3.Dot(lhs, ray.direction.normalized);
                     Vector3 point = ray.origin + ray.direction.normalized * dotP;
                     List<Tuple<float, InteractiveObject>> items = new List<Tuple<float, InteractiveObject>>();
                     bool h = false;
                     foreach (SelectorItem item in SelectorMenu.Instance.SelectorItems.Values) {
                         if (!item.InteractiveObject.Enabled)
                             continue;
                         try {
                             if (item.InteractiveObject == null) {
                                 continue;

                             }
                             float dist = item.InteractiveObject.GetDistance(hitinfo.point);

                             foreach (Collider c in item.InteractiveObject.Colliders) {

                                 if (c == hitinfo.collider) {
                                     dist = 0;
                                     h = true;
                                 }

                             }


                             if (item.InteractiveObject is ActionObjectNoPose || dist > 0.2) { // add objects max 20cm away from point of impact 

                                 Debug.DrawLine(ray.origin, hitinfo.point);
                                 continue;
                             }

                             items.Add(new Tuple<float, InteractiveObject>(dist, item.InteractiveObject));
                         } catch (MissingReferenceException ex) {
                             Debug.LogError(ex);
                             Debug.LogError($"{item.InteractiveObject.GetName()}: {hitinfo.collider.name}");
                         }
                     }
                     if (h) {
                         items.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                         SelectorMenu.Instance.UpdateAimMenu(items);
                     } else {
                         SelectorMenu.Instance.UpdateAimMenu(new List<Tuple<float, InteractiveObject>>());
                     }
                 } else {
                     SelectorMenu.Instance.UpdateAimMenu(new List<Tuple<float, InteractiveObject>>());
                 }

             } 
             */

            /*if (SelectorMenu.Instance.CanvasGroup.alpha == 0 || !SelectorMenu.Instance.gameObject.activeSelf)
                return;
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Mathf.Infinity)) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

            RaycastHit hitinfo = new RaycastHit();
            bool anyHit = false;
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                hitinfo = hit;
                anyHit = true;
            } else {
                RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(0.03f, 0.03f, 0.0001f), ray.direction, Camera.main.transform.rotation);
                if (hits.Length > 0) {
                    float minDist = float.MaxValue;
                    foreach (RaycastHit h in hits) {
                        Vector3 dir = ray.direction;

                        Vector3 point = ray.origin + dir * Vector3.Distance(ray.origin, h.point);
                        float dist = Vector3.Distance(point, h.collider.ClosestPointOnBounds(point));
                        Debug.DrawLine(point, h.point);
                        Debug.DrawRay(ray.origin, ray.direction);
                        if (dist < minDist) {
                            hitinfo = h;
                            anyHit = true;
                            minDist = dist;
                        }
                    }
                }
            }
            if (anyHit) {
                Vector3 lhs = hitinfo.point - ray.origin;

                float dotP = Vector3.Dot(lhs, ray.direction.normalized);
                Vector3 point = ray.origin + ray.direction.normalized * dotP;
                SelectorMenu.Instance.UpdateAimMenu(hitinfo.point);
            } else {
                SelectorMenu.Instance.UpdateAimMenu(null);
            }*/

            /*
            RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(0.03f, 0.03f, 0.0001f), ray.direction, Camera.main.transform.rotation);
            if (hits.Length > 0) {
                float minDist = float.MaxValue;
                foreach (RaycastHit h in hits) {
                    Vector3 dir = ray.direction;
                    dir.Normalize();
                    Vector3 point = ray.origin + dir * Vector3.Distance(ray.origin, h.point);
                    float dist = Vector3.Distance(point, h.collider.ClosestPointOnBounds(point));
                    Debug.DrawLine(point, h.point);
                    Debug.DrawRay(ray.origin, ray.direction);
                    if (dist < minDist) {
                        hitinfo = h;
                        minDist = dist;
                    }
                }
                Vector3 lhs = hitinfo.point - ray.origin;

                float dotP = Vector3.Dot(lhs, ray.direction.normalized);
                Vector3 point2 = ray.origin + ray.direction.normalized * dotP;
                SelectorMenu.Instance.UpdateAimMenu(point2);
            } else {
                SelectorMenu.Instance.UpdateAimMenu(null);
            }
            */
            /*ExtDebug.DrawBoxCastBox(ray.origin, new Vector3(0.05f, 0.05f, 0.00001f), Camera.main.transform.rotation, ray.direction, 20f, Color.green);
            List<Tuple<float, InteractiveObject>> orderedTransforms = new List<Tuple<float, InteractiveObject>>();
            if (hits.Length > 0) {
                RaycastHit hit = hits.First();
                GameManager.Instance.GetAllInteractiveObjects();
            }*/
            /*foreach (RaycastHit hit in hits) {

                float dist = Vector3.Cross(ray.direction, hit.point - ray.origin).magnitude;
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
                    orderedTransforms.Add(new Tuple<float, InteractiveObject>(dist, interactiveObject));
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
            /*}

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

        public IO.Swagger.Model.Pose CreatePoseInTheView(float distance = 0.3f) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            Vector3 point = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(distance)));
            return new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(point), orientation: DataHelper.QuaternionToOrientation(Quaternion.identity));

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

