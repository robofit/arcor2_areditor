using UnityEngine;

public class InteractiveObjectMenu : MonoBehaviour {
    public GameObject APPrefab, CurrentObject;
    GameManager GameManager;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void CreateNewAP() {
        if (CurrentObject == null) {
            return;
        }
        GameManager.Instance.SpawnActionPoint(CurrentObject.GetComponent<Base.ActionObject>());

    }

    public void SaveID(string new_id) {
        CurrentObject.GetComponent<Base.ActionObject>().Id = new_id;
        GameManager.Instance.UpdateScene();
    }

    public void DeleteIO() {
        CurrentObject.GetComponent<Base.ActionObject>().DeleteIO();
        CurrentObject = null;
    }
}
