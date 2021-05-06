

using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class PuckOutput : InputOutput {
        public override string GetObjectTypeName() {
            return "Action output";
        }

        public override void UpdateColor() {
            if (Action == null) {
                Debug.LogError("Action is null");
                return;
            }

            Renderer renderer = Action.OutputArrow.GetComponent<Renderer>();
            List<Material> materials = new List<Material>(renderer.materials);

            if (Enabled && !IsLocked) {
                foreach (var material in materials) {
                    if (Action.Data.Id == "START")
                        material.color = Color.green;
                    else if (Action.Data.Id == "END")
                        material.color = Color.red;
                    else
                        material.color = new Color(0.9f, 0.84f, 0.27f);
                }
            } else {
                foreach (var material in materials)
                    material.color = Color.gray;
            }
        }
    }
}
