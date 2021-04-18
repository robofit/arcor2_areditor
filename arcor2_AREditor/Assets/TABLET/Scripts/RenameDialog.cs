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

    public async void Init(InteractiveObject objectToRename, UnityAction updateVisibilityCallback) {
        try {
            await WebsocketManager.Instance.WriteLock(objectToRename.GetId(), false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to lock object", ex.Message);
            return;
        }

        _updateVisibilityCallback = updateVisibilityCallback;
        selectedObject = objectToRename;
        if (objectToRename == null)
            return;

        Title.text = "Rename " + selectedObject.GetObjectTypeName();
        

        nameInput.SetValue(objectToRename.GetName());
        nameInput.SetLabel("Name", "New name");
        nameInput.SetType("string");
    }
    public override async void Confirm() {
        string name = (string) nameInput.GetValue();

        try {
            selectedObject.Rename(name);
            SelectorMenu.Instance.ForceUpdateMenus();
            Close();
        } catch (RequestFailedException) {
            //notification already shown, nothing else to do
        }
    }

    public override async void Close() {
        //LeftMenu.Instance.UpdateVisibility();
        

        SelectorMenu.Instance.gameObject.SetActive(true);
        if (_updateVisibilityCallback != null)
            _updateVisibilityCallback.Invoke();
        base.Close();

        if (selectedObject == null)
            return;
        try {
            await WebsocketManager.Instance.WriteUnlock(selectedObject.GetId());
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to unlock object", ex.Message);
        }
        selectedObject = null;
    }
}
