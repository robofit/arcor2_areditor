using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Base {
    public class ActionsManager : Base.Singleton<ActionsManager> {
        private Dictionary<string, Base.ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, Base.ActionObjectMetadata>();
        public GameObject Scene, InteractiveObjects, World, PuckPrefab;

        public event EventHandler OnActionObjectsUpdated;

        public bool ActionsReady;

        public Dictionary<string, Base.ActionObjectMetadata> ActionObjectMetadata {
            get => actionObjectsMetadata; set => actionObjectsMetadata = value;
        }

        private void Awake() {
            ActionsReady = false;
        }

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (!ActionsReady && actionObjectsMetadata.Count > 0) {

                foreach (Base.ActionObjectMetadata ao in actionObjectsMetadata.Values) {
                    if (!ao.ActionsLoaded) {
                        return;
                    }
                }
                ActionsReady = true;
                enabled = false;
            }
        }

        public void UpdateObjects(Dictionary<string, Base.ActionObjectMetadata> newActionObjectsMetadata) {
            actionObjectsMetadata = newActionObjectsMetadata;
            foreach (KeyValuePair<string, Base.ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
            }
            foreach (Base.ActionObject ao in InteractiveObjects.GetComponentsInChildren<Base.ActionObject>()) {
                if (!ActionObjectMetadata.ContainsKey(ao.Data.Type)) {
                    Destroy(ao.gameObject);
                }
            }
            ActionsReady = false;
            enabled = true;
            OnActionObjectsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool IsDescendantOfType(string type, Base.ActionObjectMetadata actionObjectMetadata) {
            if (actionObjectMetadata.Type == type)
                return true;
            if (actionObjectMetadata.Type == "Generic")
                return false;
            foreach (KeyValuePair<string, Base.ActionObjectMetadata> kv in actionObjectsMetadata) {
                if (kv.Key == actionObjectMetadata.BaseObject) {
                    return IsDescendantOfType(type, kv.Value);
                }
            }
            return false;
        }


        public Dictionary<Base.ActionObject, List<Base.ActionMetadata>> GetAllActionsOfObject(Base.ActionObject interactiveObject) {
            Dictionary<Base.ActionObject, List<Base.ActionMetadata>> actionsMetadata = new Dictionary<Base.ActionObject, List<Base.ActionMetadata>>();
            foreach (Base.ActionObject ao in InteractiveObjects.GetComponentsInChildren<Base.ActionObject>()) {
                if (ao == interactiveObject) {
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out Base.ActionObjectMetadata aom)) {
                        continue;
                    }
                    actionsMetadata[ao] = aom.ActionsMetadata.Values.ToList();
                } else {
                    List<Base.ActionMetadata> freeActions = new List<Base.ActionMetadata>();
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out Base.ActionObjectMetadata aom)) {
                        continue;
                    }
                    foreach (Base.ActionMetadata am in aom.ActionsMetadata.Values) {
                        if (am.Free)
                            freeActions.Add(am);
                    }
                    if (freeActions.Count > 0) {
                        actionsMetadata[ao] = freeActions;
                    }
                }
            }

            return actionsMetadata;
        }




    }
}

