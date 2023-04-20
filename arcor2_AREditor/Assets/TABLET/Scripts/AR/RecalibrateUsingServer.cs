using System;
using System.Threading.Tasks;
using Base;
using UnityEngine;

[RequireComponent(typeof(Target))]
public class RecalibrateUsingServer : InteractiveObject {

    private string ID = Guid.NewGuid().ToString();

    private void Awake() {
        CreateSelectorItem();
    }

    protected override void Start() {
        base.Start();
    }


    public void CreateSelectorItem() {
        SelectorItem = SelectorMenu.Instance.CreateSelectorItem(this);
    }

    public void Calibrate() {
        CalibrationManager.Instance.RecalibrateUsingServer(inverse: true, showNotification: true);
    }


    public override void OnHoverStart() {
        if (SelectorMenu.Instance.ManuallySelected) {
            DisplayOffscreenIndicator(true);
        }
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

    public override string GetObjectTypeName() {
        return GetName();
    }

    public override void UpdateColor() {
        //nothing to do here
    }

    public override Task Rename(string name) {
        throw new System.NotImplementedException();
    }

    public override void CloseMenu() {
        throw new NotImplementedException();
    }

    private void OnDestroy() {
        base.DestroyObject();
    }

    public override void EnableVisual(bool enable) {
        throw new NotImplementedException();
    }
}
