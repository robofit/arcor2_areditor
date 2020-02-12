using Base;
using UnityEngine.UI;

public class Puck2D : Base.Action {


    public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        gameObject.GetComponentInChildren<Text>().text = Data.Id;
    }

    public override void OnClick(Click type) {
        MenuManager.Instance.PuckMenu.GetComponent<ActionMenu>().UpdateMenu(this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }
}
