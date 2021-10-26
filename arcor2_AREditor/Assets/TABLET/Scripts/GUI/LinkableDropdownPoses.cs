using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using UnityEngine;
using UnityEngine.UI;

public class LinkableDropdownPoses : LinkableDropdown
{
    private ActionPoint selectedAP = null;
    private APOrientation selectedOrientation = null;
    public ActionButton Button;
    private CanvasGroup canvasGroupToHide;
    private InteractiveObject selectedObject;
    private bool selectedObjectManually;


    public void Init(ParameterMetadata parameterMetadata, string type, object value, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot,
        Parameter.OnChangeParameterHandlerDelegate onChangeParameterHandler, CanvasGroup windowToHideWhenRequestingObj, bool linkable = true) {
        base.Init(parameterMetadata, type, value, layoutGroupToBeDisabled, canvasRoot, onChangeParameterHandler, linkable);
        canvasGroupToHide = windowToHideWhenRequestingObj;
        List<string> options = new List<string>();

        /*foreach (Base.ActionPoint ap in Base.ProjectManager.Instance.GetAllActionPoints()) {
            foreach (IO.Swagger.Model.NamedOrientation orientation in ap.GetNamedOrientations()) {
                options.Add(ap.Data.Name + "." + orientation.Name);
            }
        }*/
        if (value != null) {
            try {
                selectedAP = ProjectManager.Instance.GetActionPointWithOrientation((string) value);
                selectedOrientation = selectedAP.GetOrientationVisual((string) value);
                
            } catch (KeyNotFoundException ex) {
                Debug.LogError(ex);
            }

        }
        if (type == "link")
            ActionsDropdown.SetValue($"{selectedAP.GetName()}.{selectedOrientation.GetName()}");

        UpdateButtonLabel();
    }

    private void UpdateButtonLabel() {
        if (selectedAP != null && selectedOrientation != null)
            Button.SetLabel($"AP: {selectedAP.GetName()}\nPose: {selectedOrientation.GetName()}");
        else
            Button.SetLabel($"There is no available Pose");
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
            return new RequestResult(false, "Selected object is not end effector");
        }
    }


    public override object GetValue() {
        object v = base.GetValue();
        if (type == "link")
            return v;
        else {
            string value = (string) v;
            if (value == null)
                return null;
            if (selectedOrientation == null)
                return null;
            return selectedOrientation.GetId();
        }    
    }
}
