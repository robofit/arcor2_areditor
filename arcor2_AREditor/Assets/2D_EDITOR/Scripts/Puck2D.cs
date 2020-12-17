using Base;
using UnityEngine.UI;

public class Puck2D : Base.Action {


    public override void UpdateName(string newName) {
        base.UpdateName(newName);
        gameObject.GetComponentInChildren<Text>().text = Data.Name;
    }

    public override void OnClick(Click type) {
        ActionMenu.Instance.CurrentAction = this;
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }

    public override string GetName() {
        throw new System.NotImplementedException();
    }
}
