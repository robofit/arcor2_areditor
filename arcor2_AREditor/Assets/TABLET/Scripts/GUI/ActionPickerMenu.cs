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
                ActionButton btn = Instantiate(ButtonPrefab, collapsableMenu.Content.transform).GetComponent<ActionButton>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetLabel(am.Name);
                if (am.Disabled) {
                    CreateTooltip(am.Problem, btn);
                    btn.Button.interactable = false;
                } else if (!string.IsNullOrEmpty(am.Description)) {
                    CreateTooltip(am.Description, btn);
                }

                btn.Button.onClick.AddListener(() => ShowAddNewActionDialog(am.Name, keyval.Key));
            }

        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        return true;
    }

    public async void Hide() {
        foreach (RectTransform o in Content.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        if (currentActionPoint != null) {
            await currentActionPoint.WriteUnlock();
            currentActionPoint = null;
        }
    }

    private static void CreateTooltip(string text, ActionButton btn) {
        TooltipContent btnTooltip = btn.gameObject.AddComponent<TooltipContent>();
        btnTooltip.enabled = true;

        if (btnTooltip.tooltipRect == null) {
            btnTooltip.tooltipRect = Base.GameManager.Instance.Tooltip;
        }
        if (btnTooltip.descriptionText == null) {
            btnTooltip.descriptionText = Base.GameManager.Instance.Text;
        }
        btnTooltip.description = text;
    }

    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), currentActionPoint);
        AddNewActionDialog.Open();
    }
}
