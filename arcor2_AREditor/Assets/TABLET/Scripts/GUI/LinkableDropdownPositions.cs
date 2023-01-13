using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class LinkableDropdownPositions : LinkableDropdown
{
    private ActionPoint selectedAP = null;
    public ActionButton Button;
    private CanvasGroup canvasGroupToHide;
    private InteractiveObject selectedObject;
    private bool selectedObjectManually;
    private ActionPoint parentActionPoint;


    public void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot,
        Parameter.OnChangeParameterHandlerDelegate onChangeParameterHandler, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint, bool linkable = true)
    {
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        canvasGroupToHide = windowToHideWhenRequestingObj;
        List<string> options = new List<string>();
        parentActionPoint = actionPoint;



    }
    public override void SetValue(object value) {
        base.SetValue(value);
        if (value != null) {
            try {
                if (type == LINK || type == PROJECT_PARAMETER) {
                    selectedAP = null;
                    if (type == LINK) {
                        ActionsDropdown.SetValue(DecodeLinkValue((string) value));
                    } 
                } else {
                    selectedAP = ProjectManager.Instance.GetActionPoint((string) value);
                }


            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }

        }

        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (selectedAP != null)
            Button.SetLabel($"AP: {selectedAP.GetName()}");
        else
            Button.SetLabel($"There is no available Position");
    }

    public async void OnClick()
    {
        selectedObject = SelectorMenu.Instance.GetSelectedObject();
        selectedObjectManually = SelectorMenu.Instance.ManuallySelected;
        await GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionPoint, SelectAP, "Select action point",
            ValidateAP, () => {
                EditorHelper.EnableCanvasGroup(canvasGroupToHide, true);
                canvasGroupToHide.gameObject.SetActive(true);
                if (selectedObject != null)
                {
                    SelectorMenu.Instance.SetSelectedObject(selectedObject, selectedObjectManually, false);
                }
                AREditorResources.Instance.LeftMenuProject.UpdateBtns();
                SelectorMenu.Instance.gameObject.SetActive(false);
            });
        canvasGroupToHide.gameObject.SetActive(false);
        EditorHelper.EnableCanvasGroup(canvasGroupToHide, false);
        SelectorMenu.Instance.gameObject.SetActive(true);
    }

    private async void SelectAP(object selectedObject)
    {
        selectedAP = (ActionPoint) selectedObject;
        if (selectedAP == null)
            return;

        UpdateButtonLabel();
        EditorHelper.EnableCanvasGroup(canvasGroupToHide, true);
        canvasGroupToHide.gameObject.SetActive(true);
        if (this.selectedObject != null)
        {
            SelectorMenu.Instance.SetSelectedObject(this.selectedObject, selectedObjectManually, false);
        }
        onChangeParameterHandler?.Invoke(GetName(), GetValue(), type);
        AREditorResources.Instance.LeftMenuProject.UpdateBtns();
        SelectorMenu.Instance.gameObject.SetActive(false);
    }

    private async Task<RequestResult> ValidateAP(object selectedInput)
    {
        if (selectedInput is ActionPoint3D)
        {
            return new RequestResult(true);
        }
        else
        {
            return new RequestResult(false, "Selected object is not action point");
        }
    }


    public override object GetValue()
    {
        object v = base.GetValue();
        if (type == LINK)
            return v;
        else
        {
            string value = (string)v;
            if (value == null)
                return null;
            if (selectedAP == null)
                return null;
            return selectedAP.GetId();
        }
    }

    protected override object GetDefaultValue() {
        return parentActionPoint.GetId();
    }
}
