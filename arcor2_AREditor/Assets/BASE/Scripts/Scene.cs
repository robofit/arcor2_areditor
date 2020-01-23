using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {
        
        public IO.Swagger.Model.Scene Data = new IO.Swagger.Model.Scene("", "JabloPCB", new List<IO.Swagger.Model.SceneObject>(), new List<IO.Swagger.Model.SceneService>());


        // TODO JAK VHODNE UDELAT LIST VSECH AKCNICH OBJEKTU / POINTU???
        //public Dictionary<GameObject, List<GameObject>> ActionObjects = new Dictionary<GameObject, List<GameObject>>();

        public List<ActionObject> ActionObjects = new List<ActionObject>();


        private bool sceneActive = true;
        private bool projectActive = true;

        private void Start() {

        }

        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GameState == GameManager.GameStateEnum.SceneEditor && GameManager.Instance.SceneInteractable) {
                if (!sceneActive) {
                    ActivateSceneForEditing(true, "ActionObject");
                    sceneActive = true;
                }
            } else {
                if (sceneActive) {
                    ActivateSceneForEditing(false, "ActionObject");
                    sceneActive = false;
                }
            }

            if (GameManager.Instance.GameState == GameManager.GameStateEnum.ProjectEditor && GameManager.Instance.SceneInteractable) {
                if (!projectActive) {
                    ActivateSceneForEditing(true, "ActionPoint");
                    projectActive = true;
                }
            } else {
                if (projectActive) {
                    ActivateSceneForEditing(false, "ActionPoint");
                    projectActive = false;
                }
            }
        }

        // Deactivates or activates scene and all objects in scene to ignore raycasting (clicking)
        private void ActivateSceneForEditing(bool activate, string tagToActivate) {
            Transform[] allChildren = Helper.FindComponentsInChildrenWithTag<Transform>(gameObject, tagToActivate);
            if (activate) {
                gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                foreach (Transform child in allChildren) {
                    child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
                }
            } else {
                gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child in allChildren) {
                    child.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }


        public Action GetAction(string id) {
            foreach (ActionObject actionObject in ActionObjects) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints) {
                    foreach (Action action in actionPoint.Actions) {
                        if (action.Data.Id == id)
                            return action;
                    }
                }
            }
            throw new KeyNotFoundException("Action " + id + " not found!");
        }


        public List<Action> GetAllActions() {
            List<Action> actions = new List<Action>();
            foreach (ActionObject actionObject in ActionObjects) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints) {
                    foreach (Action action in actionPoint.Actions) {
                        actions.Add(action);
                    }
                }
            }
            return actions;
        }

        public List<ActionPoint> GetAllActionPoints() {
            List<ActionPoint> actionPoints = new List<ActionPoint>();
            foreach (ActionObject actionObject in ActionObjects) {
                foreach (ActionPoint actionPoint in actionObject.ActionPoints) {
                    actionPoints.Add(actionPoint);
                }
            }
            return actionPoints;
        }


        //// Deactivates or activates scene and all objects in scene to ignore raycasting (clicking)
        //private void ActivateSceneForEditing(bool activate, string tagToActivate) {
        //    //Transform[] allChildren = Helper.FindComponentsInChildrenWithTag<Transform>(gameObject, tagToActivate);
        //    //if (activate) {
        //    //    gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //    //    }
        //    //} else {
        //    //    gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    foreach (Transform child in allChildren) {
        //    //        child.gameObject.layer = LayerMask.NameToLayer("Default");
        //    //    }
        //    //}

        //    if (activate) {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //            }
        //        }
        //    } else {
        //        foreach (GameObject actionObject in ActionObjects.Keys) {
        //            actionObject.gameObject.layer = LayerMask.NameToLayer("Default");
        //            foreach (Transform child in actionObject.GetComponentsInChildren<Transform>()) {
        //                child.gameObject.layer = LayerMask.NameToLayer("Default");
        //            }
        //        }
        //    }
        //}

        //private void ActivateProjectForEditing(bool activate, string tagToActivate) {
        //    if (activate) {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("GizmoRuntime");
        //                }
        //            }
        //        }
        //    } else {
        //        foreach (List<GameObject> actionPoints in ActionObjects.Values) {
        //            foreach (GameObject aP in actionPoints) {
        //                aP.gameObject.layer = LayerMask.NameToLayer("Default");
        //                foreach (Transform child in aP.GetComponentsInChildren<Transform>()) {
        //                    child.gameObject.layer = LayerMask.NameToLayer("Default");
        //                }
        //            }
        //        }
        //    }
        //}
    }
}

