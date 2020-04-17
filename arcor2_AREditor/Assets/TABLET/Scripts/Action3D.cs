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
        if (Visual != null) {
            Visual.material.color = colorDefault;
        }           
        
    }

    public override void UpdateName(string newName) {
        base.UpdateName(newName);
        NameText.text = newName;
    }

    public override void ActionUpdateBaseData(IO.Swagger.Model.Action aData = null) {
        base.ActionUpdateBaseData(aData);
        NameText.text = aData.Name;
    }

    public override void OnClick(Click type) {
        if (type == Click.MOUSE_RIGHT_BUTTON || (type == Click.TOUCH && !(ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate))) {
            ActionMenu.Instance.CurrentAction = this;
            MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
        }
    }
}
