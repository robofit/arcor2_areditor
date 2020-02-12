using System.Collections;
using System.Collections.Generic;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Action3D : Base.Action {

    public TextMeshPro NameText;

    public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        NameText.text = newId;
    }

    public override void OnClick(Click type) {
        MenuManager.Instance.PuckMenu.GetComponent<ActionMenu>().UpdateMenu(this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }
}
