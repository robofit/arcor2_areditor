using System.Collections.Generic;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;
using UnityEngine.UI;
using ActionPoint = Base.ActionPoint;
using Parameter = Base.Parameter;

public class LinkableDropdownPoses : LinkableDropdown {
    private ActionPoint selectedAP = null;
    private APOrientation selectedOrientation = null;
    public ActionButton Button;
    private CanvasGroup canvasGroupToHide;
    private InteractiveObject selectedObject;
    private bool selectedObjectManually;
    private ActionPoint parentActionPoint;


    public void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot,
        Parameter.OnChangeParameterHandlerDelegate onChangeParameterHandler, CanvasGroup windowToHideWhenRequestingObj, ActionPoint actionPoint, bool linkable = true) {
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        canvasGroupToHide = windowToHideWhenRequestingObj;
        List<string> options = new();
        parentActionPoint = actionPoint;

    }

    public override void SetValue(object newValue) {
        base.SetValue(newValue);
        if (newValue != null) {
            try {
                if (type == LINK || type == PROJECT_PARAMETER) {
                    selectedAP = null;
                    selectedOrientation = null;
                    if (type == LINK) {
                        ActionsDropdown.SetValue(DecodeLinkValue((string) newValue));
                    }
                } else {
                    selectedAP = ProjectManager.Instance.GetActionPointWithOrientation((string) newValue);
                    selectedOrientation = selectedAP.GetOrientationVisual((string) newValue);
                }

            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }

        }

        UpdateButtonLabel();

    }

    private void UpdateButtonLabel() {
        if (selectedAP != null && selectedOrientation != null)
            Button.SetLabel($"AP: {selectedAP.GetName()}\nPose: {selectedOrientation.GetName()}");
        else
            Button.SetLabel($"No pose is selected");
    }



    public async void OnClick() {
        selectedObject = SelectorMenu.Instance.GetSelectedObject();
        selectedObjectManually = SelectorMenu.Instance.ManuallySelected;
        await GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingAPOrientation, SelectOrientation, "Select orientation",
            ValidateOrientation, () => {
                EditorHelper.EnableCanvasGroup(canvasGroupToHide, true);
                canvasGroupToHide.gameObject.SetActive(true);
                if (selectedObject != null) {
                    SelectorMenu.Instance.SetSelectedObject(selectedObject, selectedObjectManually, false);
                }
                AREditorResources.Instance.LeftMenuProject.UpdateBtns();
                SelectorMenu.Instance.gameObject.SetActive(false);
            });
        canvasGroupToHide.gameObject.SetActive(false);
        EditorHelper.EnableCanvasGroup(canvasGroupToHide, false);
        SelectorMenu.Instance.gameObject.SetActive(true);
    }

    private async void SelectOrientation(object selectedObject) {
        APOrientation orientation = (APOrientation) selectedObject;
        if (orientation == null)
            return;
        selectedAP = orientation.ActionPoint;
        selectedOrientation = orientation;
        UpdateButtonLabel();
        EditorHelper.EnableCanvasGroup(canvasGroupToHide, true);
        canvasGroupToHide.gameObject.SetActive(true);
        if (this.selectedObject != null) {
            SelectorMenu.Instance.SetSelectedObject(this.selectedObject, selectedObjectManually, false);
        }
        onChangeParameterHandler?.Invoke(GetName(), GetValue(), type);
        AREditorResources.Instance.LeftMenuProject.UpdateBtns();
        SelectorMenu.Instance.gameObject.SetActive(false);
    }

    private async Task<RequestResult> ValidateOrientation(object selectedInput) {
        if (selectedInput is APOrientation) {
            return new RequestResult(true);
        } else {
            return new RequestResult(false, "Selected object is not APOrientation");
        }
    }


    public override object GetValue() {
        object v = base.GetValue();
        if (type == LINK)
            return (string) v;
        else {
            string value = (string) v;
            if (value == null)
                return null;
            if (selectedOrientation == null)
                return null;
            return selectedOrientation.GetId();
        }
    }

    protected override object GetDefaultValue() {
        List<NamedOrientation> orientations = parentActionPoint.GetNamedOrientations();
        if (orientations.Count > 0)
            return orientations[0].Id;
        else
            return ProjectManager.Instance.GetAnyNamedOrientation().Id;
    }
}
