using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

[RequireComponent(typeof(Target))]
public class Recalibrate : InteractiveObject {

    private string ID = Guid.NewGuid().ToString();

    private void Awake() {
        CreateSelectorItem();
    }

    protected override void Start() {
        base.Start();
    }

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        Calibrate();
    }

    public void CreateSelectorItem() {
        SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
    }

    public void Calibrate() {
        CalibrationManager.Instance.RecalibrateUsingARFoundation();
    }

    public override void Enable(bool enable) {
        base.Enable(enable);
    }

    public override void OnHoverStart() {
        DisplayOffscreenIndicator(true);
    }

    public override void OnHoverEnd() {
        DisplayOffscreenIndicator(false);
    }

    public override string GetName() {
        return "Calibration cube";
    }

    public override string GetId() {
        return ID;
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

    public override Task Rename(string name) {
        throw new System.NotImplementedException();
    }
    public override string GetObjectTypeName() {
        return GetName();
    }

    public override void UpdateColor() {
        //nothing to do here
    }

    public override void CloseMenu() {
        throw new NotImplementedException();
    }
}
