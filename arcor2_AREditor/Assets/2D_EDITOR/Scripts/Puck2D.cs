using UnityEngine.UI;

public class Puck2D : Base.Action {
    public override void OnClick() {
        throw new System.NotImplementedException();
    }

    public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        gameObject.GetComponentInChildren<Text>().text = Data.Id;
    }

    void Touch() {
        MenuManager.Instance.PuckMenu.GetComponent<PuckMenu>().UpdateMenu(this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }




}
