using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {
        // Start is called before the first frame update
        public IO.Swagger.Model.Scene Data = new IO.Swagger.Model.Scene("", "JabloPCB", new List<IO.Swagger.Model.SceneObject>(), new List<IO.Swagger.Model.SceneService>());            
        protected void Awake() {
           
        }

        private void Start() {

        }

        // Update is called once per frame
        private void Update() {
            // Activates scene if the AREditor is in SceneEditor mode and scene is interactable (no windows are openned).
            if (GameManager.Instance.GameState == GameManager.GameStateEnum.SceneEditor && GameManager.Instance.SceneInteractable) {
                ActivateSceneForEditing(true);
            } else {
                ActivateSceneForEditing(false);
            }    
        }

        // Deactivates or activates scene and all objects in scene to ignore raycasting (clicking)
        private void ActivateSceneForEditing(bool activate) {
            Transform[] allChildren = GetComponentsInChildren<Transform>();
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
    }
}

