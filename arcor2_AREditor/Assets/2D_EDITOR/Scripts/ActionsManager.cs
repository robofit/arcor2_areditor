using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionsManager : Base.Singleton<ActionsManager> {
    Dictionary<string, ActionObjectMetadata> _ActionObjectsMetadata = new Dictionary<string, ActionObjectMetadata>();
    public GameObject Scene, InteractiveObjects, World, PuckPrefab;

    public bool ActionsReady;

    public Dictionary<string, ActionObjectMetadata> ActionObjectMetadata {
        get => _ActionObjectsMetadata; set => _ActionObjectsMetadata = value;
    }

    private void Awake() {
        ActionsReady = false;
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (!ActionsReady && _ActionObjectsMetadata.Count > 0) {

            foreach (ActionObjectMetadata ao in _ActionObjectsMetadata.Values) {
                if (!ao.ActionsLoaded) {
                    return;
                }
            }
            ActionsReady = true;
            enabled = false;
        }
    }

    public void UpdateObjects(Dictionary<string, ActionObjectMetadata> NewActionObjectsMetadata) {
        _ActionObjectsMetadata = NewActionObjectsMetadata;
        foreach (KeyValuePair<string, ActionObjectMetadata> kv in _ActionObjectsMetadata) {
            kv.Value.Robot = IsDescendantOfType("Robot", kv.Value);
        }
        foreach (InteractiveObject o in InteractiveObjects.GetComponentsInChildren<InteractiveObject>()) {
            if (!ActionObjectMetadata.ContainsKey(o.type)) {
                Destroy(o.gameObject);
            }
        }
        World.BroadcastMessage("ActionObjectsUpdated");
        ActionsReady = false;
        enabled = true;
    }

    private bool IsDescendantOfType(string type, ActionObjectMetadata actionObjectMetadata) {
        if (actionObjectMetadata.Type == type)
            return true;
        if (actionObjectMetadata.Type == "Generic")
            return false;
        foreach (KeyValuePair<string, ActionObjectMetadata> kv in _ActionObjectsMetadata) {
            if (kv.Key == actionObjectMetadata.BaseObject) {
                return IsDescendantOfType(type, kv.Value);
            }
        }
        return false;
    }

    public void UpdateObjectActionMenu(string objectType) {
        if (_ActionObjectsMetadata.TryGetValue(objectType, out ActionObjectMetadata ao)) {
            MenuManager.Instance.UpdateActionObjectMenu(ao);
        }

    }

    public Dictionary<InteractiveObject, List<ActionMetadata>> GetAllActionsOfObject(InteractiveObject interactiveObject) {
        Dictionary<InteractiveObject, List<ActionMetadata>> actionsMetadata = new Dictionary<InteractiveObject, List<ActionMetadata>>();

        foreach (InteractiveObject io in InteractiveObjects.GetComponentsInChildren<InteractiveObject>()) {
            if (io == interactiveObject) {
                if (!_ActionObjectsMetadata.TryGetValue(io.type, out ActionObjectMetadata aom)) {
                    continue;
                }
                actionsMetadata[io] = aom.ActionsMetadata.Values.ToList();
            } else {
                List<ActionMetadata> freeActions = new List<ActionMetadata>();
                if (!_ActionObjectsMetadata.TryGetValue(io.type, out ActionObjectMetadata aom)) {
                    continue;
                }
                foreach (ActionMetadata am in aom.ActionsMetadata.Values) {
                    if (am.Free)
                        freeActions.Add(am);
                }
                if (freeActions.Count > 0) {
                    actionsMetadata[io] = freeActions;
                }
            }
        }
        /*
        foreach (ActionObjectMetadata aom in _ActionObjectsMetadata.Values)
        {
            if (aom.Type == interactiveObject.type)
            {
                actionsMetadata[interactiveObject] = aom.ActionsMetadata.Values.ToList(); ;
                
                    
            } else
            {
                Dictionary<InteractiveObject, ActionMetadata> freeActions = new Dictionary<InteractiveObject, ActionMetadata>();
                List<ActionMetadata> actions = new List<ActionMetadata>();
                foreach (ActionMetadata action in aom.ActionsMetadata.Values)
                {
                    if (action.Free)
                    {
                        foreach (InteractiveObject io in InteractiveObjects.GetComponentsInChildren<InteractiveObject>())
                        {
                            if (io.type == aom.Type)
                            {
                                
                            }
                        }
                        actions.Add(action);
                        
                    }
                }
                if (actions.Count > 0)
                {
                    actionsMetadata[io] = actions;
                }
            }
        }*/
        return actionsMetadata;
    }




}
