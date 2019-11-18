using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Base {
    public class ActionsManager : Singleton<ActionsManager> {
        private Dictionary<string, ActionObjectMetadata> actionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
        public GameObject Scene, InteractiveObjects, World, PuckPrefab;

        public event EventHandler OnActionObjectsUpdated;

        public bool ActionsReady;

        public Dictionary<string, ActionObjectMetadata> ActionObjectMetadata {
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

                foreach (ActionObjectMetadata ao in actionObjectsMetadata.Values) {
                    if (!ao.ActionsLoaded) {
                        return;
                    }
                }
                ActionsReady = true;
                enabled = false;
            }
        }

        public void UpdateObjects(Dictionary<string, ActionObjectMetadata> newActionObjectsMetadata) {
            actionObjectsMetadata = newActionObjectsMetadata;
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
            }
            foreach (ActionObject ao in InteractiveObjects.GetComponentsInChildren<ActionObject>()) {
                if (!ActionObjectMetadata.ContainsKey(ao.Data.Type)) {
                    Destroy(ao.gameObject);
                }
            }
            ActionsReady = false;
            enabled = true;
            OnActionObjectsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private bool IsDescendantOfType(string type, ActionObjectMetadata actionObjectMetadata) {
            if (actionObjectMetadata.Type == type)
                return true;
            if (actionObjectMetadata.Type == "Generic")
                return false;
            foreach (KeyValuePair<string, ActionObjectMetadata> kv in actionObjectsMetadata) {
                if (kv.Key == actionObjectMetadata.BaseObject) {
                    return IsDescendantOfType(type, kv.Value);
                }
            }
            return false;
        }


        public Dictionary<ActionObject, List<ActionMetadata>> GetAllActionsOfObject(ActionObject interactiveObject) {
            Dictionary<ActionObject, List<ActionMetadata>> actionsMetadata = new Dictionary<ActionObject, List<ActionMetadata>>();
            foreach (ActionObject ao in InteractiveObjects.GetComponentsInChildren<ActionObject>()) {
                if (ao == interactiveObject) {
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
                        continue;
                    }
                    actionsMetadata[ao] = aom.ActionsMetadata.Values.ToList();
                } else {
                    List<ActionMetadata> freeActions = new List<ActionMetadata>();
                    if (!actionObjectsMetadata.TryGetValue(ao.Data.Type, out ActionObjectMetadata aom)) {
                        continue;
                    }
                    foreach (ActionMetadata am in aom.ActionsMetadata.Values) {
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

