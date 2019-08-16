using UnityEngine.UI;

public class Puck2D : Base.Action {

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        gameObject.GetComponentInChildren<Text>().text = Data.Id;
    }

    void Touch() {
        MenuManager.Instance.PuckMenu.GetComponent<PuckMenu>().UpdateMenu(this, this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }




}
