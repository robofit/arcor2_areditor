using UnityEngine;

public class SideMenuControll : MonoBehaviour {
    public MenuManager _MenuManager;
    // Start is called before the first frame update
    void Start() {
        _MenuManager = GameObject.Find("_MenuManager").gameObject.GetComponent<MenuManager>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void Hide() {
        _MenuManager.HideMenu();
    }

    void Touch() {

    }
}
