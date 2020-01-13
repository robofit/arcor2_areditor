using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class Action3D : Base.Action {
    
   public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        //gameObject.GetComponentInChildren<Text>().text = Data.Id;
    }

    public override void OnClick(Click type) {
        MenuManager.Instance.PuckMenu.GetComponent<PuckMenu>().UpdateMenu(this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }
}
