using UnityEngine;

public class ActionPoint : MonoBehaviour {
    public string id;
    [System.NonSerialized]
    public string type;
    Vector2 _position;
    public GameObject IntObj;
    private Vector3 offset;
    //public GameObject ActionPointMenu;
    MenuManager _MenuManager;
    [System.NonSerialized]
    public int PuckCounter = 0;
    private GameObject _ConnectionManager;
    GameManager GameManager;
    public Connection ConnectionToIO;



    private void Awake() {
        _MenuManager = GameObject.Find("_MenuManager").gameObject.GetComponent<MenuManager>();
        GameManager = GameObject.Find("_GameManager").gameObject.GetComponent<GameManager>();

    }

    void Update() {

    }

    public void SetInteractiveObject(GameObject IntObj) {
        if (_MenuManager == null) {
            _MenuManager = GameObject.Find("_MenuManager").gameObject.GetComponent<MenuManager>();
        }
        this.IntObj = IntObj;
        id = IntObj.GetComponent<InteractiveObject>().Id + " - AP" + IntObj.GetComponent<InteractiveObject>().CounterAP++.ToString();

    }

    void OnMouseDown() {
        offset = gameObject.transform.position -
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
    }

    void OnMouseDrag() {
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
    }

    void OnMouseUp() {
        GameManager.UpdateProject();
    }

    void Touch() {
        _MenuManager.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = gameObject;
        _MenuManager.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        _MenuManager.ShowMenu(_MenuManager.ActionPointMenu, id);

    }

    public void DeleteAP(bool updateProject = true) {
        foreach (Puck puck in GetComponentsInChildren<Puck>()) {
            puck.DeletePuck(false);
        }
        Destroy(ConnectionToIO.gameObject);
        gameObject.SetActive(false);
        Destroy(gameObject);

        if (updateProject)
            GameManager.UpdateProject();

    }

    public Vector3 GetScenePosition() {

        Vector3 position = Vector3.Scale(GameManager.Scene.transform.InverseTransformPoint(transform.position) + new Vector3(GameManager.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1));
        position.z = 0.7f;
        return position;
    }

    public void SetScenePosition(Vector3 position) {
        transform.position = GameManager.Scene.transform.TransformPoint(Vector3.Scale(position, new Vector3(1000f, 1000f, 1)) - new Vector3(GameManager.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Scene.GetComponent<RectTransform>().rect.height / 2, 0));
    }

}