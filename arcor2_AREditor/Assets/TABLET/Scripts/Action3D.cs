using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OutlineOnClick))]
[RequireComponent(typeof(Target))]
public class Action3D : Base.Action, ISubItem {
    public Renderer Visual;

    private Color32 colorDefault = new Color32(229, 215, 68, 255);
    private Color32 colorRunnning = new Color32(255, 0, 255, 255);

    private bool selected = false;
    [SerializeField]
    protected OutlineOnClick outlineOnClick;

    public override void Init(IO.Swagger.Model.Action projectAction, Base.ActionMetadata metadata, Base.ActionPoint ap, IActionProvider actionProvider) {
        base.Init(projectAction, metadata, ap, actionProvider);
       // Input.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Input);
        //Output.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Output);
    }

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnStopPackage += OnProjectStop;
    }

    private void OnEnable() {
        GameManager.Instance.OnSceneInteractable += OnDeselect;
    }

    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnSceneInteractable -= OnDeselect;
        }
    }

    private void OnProjectStop(object sender, System.EventArgs e) {
        StopAction();
    }

    public override void RunAction() {
        Visual.material.color = colorRunnning;
        foreach (IO.Swagger.Model.ActionParameter p in Data.Parameters) {
            if (p.Type == "pose") {
                string orientationId = JsonConvert.DeserializeObject<string>(p.Value);
                ProjectManager.Instance.HighlightOrientation(orientationId, true);
            }
        }
    }

    public override void StopAction() {
        if (Visual != null) {
            Visual.material.color = colorDefault;
        }
        foreach (IO.Swagger.Model.ActionParameter p in Data.Parameters) {
            if (p.Type == "pose") {
                string orientationId = JsonConvert.DeserializeObject<string>(p.Value);
                ProjectManager.Instance.HighlightOrientation(orientationId, false);
            }
        }
    }

    public override void UpdateName(string newName) {
        base.UpdateName(newName);
        NameText.text = newName;
    }

    public override void ActionUpdateBaseData(IO.Swagger.Model.BareAction aData = null) {
        base.ActionUpdateBaseData(aData);
        NameText.text = aData.Name;
    }

    public bool CheckClick() {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingAction) {
            GameManager.Instance.ObjectSelected(this);
            return false;
        }
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return false;
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
            Notifications.Instance.ShowNotification("Not allowed", "Editation of action only allowed in project editor");
            return false;
        }
        return true;

    }

    public override void OnClick(Click type) {
        if (!CheckClick())
            return;
        if (type == Click.MOUSE_RIGHT_BUTTON || type == Click.TOUCH) {
            OpenMenu();
        }
    }

    private void OnDeselect(object sender, EventArgs e) {
        if (selected) {
            ActionPoint.HighlightAP(false);
            selected = false;
        }
    }

    public override void OnHoverStart() {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingAction) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
                if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning)
                    return;
            } else {
                return;
            }
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning) {
            return;
        }
        outlineOnClick.Highlight();
        NameText.gameObject.SetActive(true);
        DisplayOffscreenIndicator(true);
    }

    public override void OnHoverEnd() {
        outlineOnClick.UnHighlight();
        NameText.gameObject.SetActive(false);
        DisplayOffscreenIndicator(false);
    }

    public override void UpdateColor() {
        

        foreach (Material material in Visual.materials)
            if (Enabled && !(IsLocked && !IsLockedByMe))
                material.color = new Color(0.9f, 0.84f, 0.27f);
            else
                material.color = Color.gray;
    }

    public override string GetName() {
        return Data.Name;
    }

    public override async void OpenMenu() {
        if (!await ActionParametersMenu.Instance.Show(this))
            return;
        //ActionMenu.Instance.CurrentAction = this;
        //MenuManager.Instance.ShowMenu(MenuManager.Instance.PuckMenu);
        selected = true;
        ActionPoint.HighlightAP(true);        
    }

    public override void CloseMenu() {
        selected = false;
        ActionPoint.HighlightAP(false);
        ActionParametersMenu.Instance.Hide();
    }

    public override bool HasMenu() {
        return true;
    }

    public override void StartManipulation() {
        throw new NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor)
            return new RequestResult(false, "Action could only be removed in project editor");
        else {
            try {
                await WebsocketManager.Instance.RemoveAction(GetId(), true);
                return new RequestResult(true);
            } catch (RequestFailedException ex) {
                return new RequestResult(false, ex.Message);
            }
        }
    }

    public async override void Remove() {
        try {
            await WebsocketManager.Instance.RemoveAction(GetId(), false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to remove action " + GetName(), ex.Message);
        }
    }

    public async override Task Rename(string newName) {
        try {
            await WebsocketManager.Instance.RenameAction(GetId(), newName);
            Notifications.Instance.ShowToastMessage("Action renamed");
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename action", e.Message);
            throw;
        }
    }

    public override string GetObjectTypeName() {
        return "Action";
    }

    public override void OnObjectLocked(string owner) {
        base.OnObjectLocked(owner);
        if (owner != LandingScreen.Instance.GetUsername()) {
            NameText.text = GetLockedText();
        }
    }

    public override void OnObjectUnlocked() {
        base.OnObjectUnlocked();
        NameText.text = GetName();
    }

    public InteractiveObject GetParentObject() {
        return ActionPoint;
    }


    public override void EnableVisual(bool enable) {
        throw new NotImplementedException();
    }
}
