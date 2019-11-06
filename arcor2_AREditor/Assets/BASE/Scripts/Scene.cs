using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public class Scene : Singleton<Scene> {
        // Start is called before the first frame update
        public IO.Swagger.Model.Scene Data = new IO.Swagger.Model.Scene("", "JabloPCB", new List<IO.Swagger.Model.SceneObject>(), "test_it_off_demo");            
        protected void Awake() {
           
        }

        private void Start() {

        }

        // Update is called once per frame
        private void Update() {

        }
    }
}

