using System.Collections;
using System.Collections.Generic;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Action3D : Base.Action {

    public TextMeshPro NameText;
    public Renderer Visual;

    private Color32 colorDefault = new Color32(229, 215, 68, 255);
    private Color32 colorRunnning = new Color32(255, 0, 255, 255);

    private void Start() {
        GameManager.Instance.OnStopProject += OnProjectStop;
    }

    private void OnProjectStop(object sender, System.EventArgs e) {
        StopAction();
    }

    public override void RunAction() {
        Visual.material.color = colorRunnning;
    }

    public override void StopAction() {
        Visual.material.color = colorDefault;
    }

    public override void UpdateId(string newId, bool updateProject = true) {
        base.UpdateId(newId, updateProject);
        NameText.text = newId;
    }

    public override void ActionUpdate(IO.Swagger.Model.Action aData = null) {
        base.ActionUpdate(aData);
        NameText.text = aData.Id;
    }

    public override void OnClick(Click type) {
        MenuManager.Instance.PuckMenu.GetComponent<ActionMenu>().UpdateMenu(this);
        MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
    }
}
