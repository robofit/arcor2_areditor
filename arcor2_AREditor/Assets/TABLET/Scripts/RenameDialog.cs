using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

public class RenameDialog : Dialog
{
    //public GameObject CanvasRoot;
    public TMPro.TMP_Text Title;

    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;

    private InteractiveObject selectedObject;

    private UnityAction _updateVisibilityCallback;
    private bool isNewObject;
    public ButtonWithTooltip CloseBtn, ConfirmButton;
    private UnityAction confirmCallback;

    public async void Init(InteractiveObject objectToRename, UnityAction updateVisibilityCallback, bool isNewObject = false, UnityAction cancelCallback = null, UnityAction confirmCallback = null) {
        if (!await objectToRename.WriteLock(false))
            return;

        this.isNewObject = isNewObject;
        _updateVisibilityCallback = updateVisibilityCallback;
        selectedObject = objectToRename;
        if (objectToRename == null)
            return;

        Title.text = "Rename " + selectedObject.GetObjectTypeName();

        nameInput.SetValue(objectToRename.GetName());
        nameInput.SetLabel("Name", "New name");
        nameInput.SetType("string");
        CloseBtn.Button.onClick.RemoveAllListeners();
        CloseBtn.Button.onClick.AddListener(Cancel);
        if (cancelCallback != null)
            CloseBtn.Button.onClick.AddListener(cancelCallback);
        this.confirmCallback = confirmCallback;
    }

    public void ValidateInput() {
        if (isNewObject) {
            ConfirmButton.SetInteractivity(true);
            return;
        }

        bool valid = ((string) nameInput.GetValue()) != selectedObject.GetName();

        ConfirmButton.SetInteractivity(valid, "Name has not been changed");
    }
   
    public override async void Confirm() {
        string name = (string) nameInput.GetValue();
        if (name == selectedObject.GetName()) { //for new objects, without changing name
            Cancel();
            confirmCallback?.Invoke();
            Debug.LogError("invoke callback");
            return;
        }

        try {
            await selectedObject.Rename(name);
            Cancel();
            confirmCallback?.Invoke();

            Debug.LogError("invoke callback");
        } catch (RequestFailedException) {
            //notification already shown, nothing else to do
        }
    }

    public override async void Close() {
        //LeftMenu.Instance.UpdateVisibility();

        SelectorMenu.Instance.gameObject.SetActive(true);
        Debug.LogError("show selectormenu");
        if (_updateVisibilityCallback != null)
            _updateVisibilityCallback.Invoke();
        base.Close();

        selectedObject = null;
    }

    public void Cancel() {
        if (selectedObject != null) {
            _ = selectedObject.WriteUnlock();
        }
        Close();

    }
}
