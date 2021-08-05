using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using Michsky.UI.ModernUIPack;
using UnityEngine;

public class ActionPickerMenu : Base.Singleton<ActionPickerMenu>
{
    public GameObject Content;
    public CanvasGroup CanvasGroup;
    public GameObject CollapsablePrefab, ButtonPrefab;

    public AddNewActionDialog AddNewActionDialog;
    private ActionPoint currentActionPoint;

    public async Task<bool> Show(ActionPoint actionPoint) {
        if (! await actionPoint.WriteLock(false))
            return false;
        Dictionary<IActionProvider, List<Base.ActionMetadata>> actionsMetadata = Base.ActionsManager.Instance.GetAllActions();
        currentActionPoint = actionPoint;

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, Content.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.SetLabel(keyval.Key.GetProviderName());
            collapsableMenu.Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
                ActionButtonWithIcon btn = Instantiate(ButtonPrefab, collapsableMenu.Content.transform).GetComponent<ActionButtonWithIcon>();
                ButtonWithTooltip btnTooltip = btn.GetComponent<ButtonWithTooltip>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetLabel(am.Name);
                btn.Icon.sprite = AREditorResources.Instance.Action;
                
                if (am.Disabled) {
                    btn.SetInteractable(false);
                    btnTooltip.SetInteractivity(false, am.Problem);
                } else if (!string.IsNullOrEmpty(am.Description)) {
                    btnTooltip.SetDescription(am.Description);
                }               

                btn.Button.onClick.AddListener(() => ShowAddNewActionDialog(am.Name, keyval.Key));
            }

        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        return true;
    }

    public async void Hide(bool unlock = true) {
        RectTransform[] transforms = Content.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        if (currentActionPoint != null) {
            if(unlock)
                await currentActionPoint.WriteUnlock();
            currentActionPoint = null;
        }
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

    public void SetVisibility(bool visible) {
        EditorHelper.EnableCanvasGroup(CanvasGroup, visible);
    }


    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), currentActionPoint);
        AddNewActionDialog.Open();
    }
}
