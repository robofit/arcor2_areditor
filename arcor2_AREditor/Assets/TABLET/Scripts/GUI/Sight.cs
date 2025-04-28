using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

namespace Base {

   public class Sight : Singleton<Sight> {
        public GameObject CurrentObject;

        public DateTime HoverStartTime;

        private bool endingHover = false;

        public EventHandler<GizmoAxisEventArgs> SelectedGizmoAxis;


        private void Awake() {
            GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
        }

        private void Start() {
            InvokeRepeating("UpdateSight", 0.1f, 0.1f);
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

        private void UpdateSight() {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            if (SelectorMenu.Instance.CanvasGroup.alpha > 0 && SelectorMenu.Instance.gameObject.activeSelf) {

                RaycastHit hitinfo = new();
                bool anyHit = false, directHit = false;
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    hitinfo = hit;
                    anyHit = true;
                    directHit = true;
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
                    List<Tuple<float, InteractiveObject>> items = new();
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
                                    // little hack - set object that was directly hit to distance < 0 in order to let the sorting method of SelectorMenu display it on top
                                    dist = -1;
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
        } 

        private void FixedUpdate() {
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
            

        }

            private bool InteractiveObjectInList(List<Tuple<float, InteractiveObject>> list, InteractiveObject interactiveObject) {
            foreach (Tuple<float, InteractiveObject> item in list) {
                if (interactiveObject.GetId() == item.Item2.GetId()) {
                    return true;
                }
            }
            return false;
        }

        public Pose CreatePoseInTheView(float distance = 0.3f) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            Vector3 point = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(distance)));
            return new Pose(DataHelper.Vector3ToPosition(point), DataHelper.QuaternionToOrientation(Quaternion.identity));

        }

        private IEnumerator HoverEnd() {
            endingHover = true;
            yield return new WaitForSeconds((float) (0.5d - (DateTime.UtcNow - HoverStartTime).TotalSeconds));
            if (CurrentObject != null) {
                CurrentObject.SendMessage("OnHoverEnd");
                CurrentObject = null;
            }
            endingHover = false;
        }        
    }
}

