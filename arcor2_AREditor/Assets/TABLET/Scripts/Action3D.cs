using System;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using Action = Base.Action;
using ActionMetadata = Base.ActionMetadata;
using ActionPoint = Base.ActionPoint;

[RequireComponent(typeof(OutlineOnClick))]
[RequireComponent(typeof(Target))]
public class Action3D : Action, ISubItem {
    public Renderer Visual;

    private Color32 colorDefault = new(229, 215, 68, 255);
    private Color32 colorRunnning = new(255, 0, 255, 255);

    private bool selected = false;
    [SerializeField]
    protected OutlineOnClick outlineOnClick;

    public override void Init(Arcor2.ClientSdk.Communication.OpenApi.Models.Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider) {
        base.Init(projectAction, metadata, ap, actionProvider);
    }

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnStopPackage += OnProjectStop;
    }

    private void LateUpdate() {
        UpdateRotation();
    }

    private void OnEnable() {
        GameManager.Instance.OnSceneInteractable += OnDeselect;
    }

    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnSceneInteractable -= OnDeselect;
        }
    }

    private void OnProjectStop(object sender, EventArgs e) {
        StopAction();
    }

    public override void RunAction() {
        Visual.material.color = colorRunnning;
        foreach (ActionParameter p in Data.Parameters) {
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
        foreach (ActionParameter p in Data.Parameters) {
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

    public override void ActionUpdateBaseData(BareAction aData = null) {
        base.ActionUpdateBaseData(aData);
        NameText.text = aData.Name;
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
        if (SelectorMenu.Instance.ManuallySelected) {
            DisplayOffscreenIndicator(true);
        }
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

    public override void OpenMenu() {
        _ = ActionParametersMenu.Instance.Show(this, false);
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
                var response = await CommunicationManager.Instance.Client.RemoveActionAsync(new IdArgs(GetId()), true);
                if (!response.Result) {
                    return new RequestResult(false, string.Join(',', response.Messages));
                }

                return new RequestResult(true);
            } catch (Arcor2ConnectionException ex) {
                return new RequestResult(false, ex.Message);
            }
        }
    }

    public async override void Remove() {
        try {
            var response = await CommunicationManager.Instance.Client.RemoveActionAsync(new IdArgs(GetId()), false);
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove action " + GetName(), string.Join(',', response.Messages));
            }
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to remove action " + GetName(), ex.Message);
        }
    }

    public async override Task Rename(string newName) {
        try {
            var response = await CommunicationManager.Instance.Client.RenameActionAsync(new RenameActionRequestArgs(GetId(), newName));
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to rename action " + GetName(), string.Join(',', response.Messages));
                return;
            }
            Notifications.Instance.ShowToastMessage("Action renamed");
        } catch (Arcor2ConnectionException e) {
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
