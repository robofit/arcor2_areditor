using System.Threading.Tasks;
using Base;
using UnityEngine;

public class CreateAnchor : InteractiveObject {

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Normal ||
            GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
            CalibrationManager.Instance.CreateAnchor(transform);
        }
    }

    private void OnEnable() {
        Enabled = true;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    private void OnDisable() {
        Enabled = false;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }

    public override string GetName() {
        return "Calibration cube";
    }

    public override string GetId() {
        return "Calibration cube";
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Calibration cube could not be moved");
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(false, "Calibration cube could not be removed");
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override Task Rename(string name) {
        throw new System.NotImplementedException();
    }

    public override string GetObjectTypeName() {
        return GetName();
    }

    public override void UpdateColor() {
        //nothing to do here
    }
}
