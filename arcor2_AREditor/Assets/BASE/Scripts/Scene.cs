using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Scene : MonoBehaviour {
        // Start is called before the first frame update
        public IO.Swagger.Model.Scene Data;
        protected void Awake() {
            Data = new IO.Swagger.Model.Scene {
                Objects = new List<IO.Swagger.Model.SceneObject>(),
                Desc = "",
                Id = "JabloPCB",
                RobotSystemId = "test_it_off_demo"
            };            
        }

        private void Start() {

        }

        // Update is called once per frame
        private void Update() {

        }
    }
}

