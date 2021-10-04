
using System.Collections.Generic;
using UnityEngine;

namespace Base
{
    public class PuckInput : InputOutput {
        /*
        public override string GetObjectTypeName() {
            return "Action input";
        }


        public override string GetId() {
            return Action.GetId() + "input";
        }

        public override void UpdateColor() {
            Renderer renderer = Action.InputArrow.GetComponent<Renderer>();
            List<Material> materials = new List<Material>(renderer.materials);

            if (Enabled && !(IsLocked && !IsLockedByMe)) {
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

        public override void CloseMenu() {
            throw new System.NotImplementedException();
        }

        public override void EnableVisual(bool enable) {
            throw new System.NotImplementedException();
        }*/
    }    

}
