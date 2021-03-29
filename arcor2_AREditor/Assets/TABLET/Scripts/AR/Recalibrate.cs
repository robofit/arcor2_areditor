using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public class Recalibrate : InteractiveObject
    {


    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        Calibrate();
    }

    public void Calibrate() {
        CalibrationManager.Instance.Recalibrate();
    }

    private void OnEnable() {
        Enabled = true;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    private void OnDisable() {
        Enabled = false;
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    public override void Enable(bool enable) {
        base.Enable(enable);
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
        return "ReCalibration cube";
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
        return new RequestResult(false, "Calibration cube could not be moved");
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override void Rename(string name) {
        throw new System.NotImplementedException();
    }
}
