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
    public Button CloseBtn;

    public void Init(InteractiveObject objectToRename, UnityAction updateVisibilityCallback, UnityAction cancelCallback = null) {
        _updateVisibilityCallback = updateVisibilityCallback;
        selectedObject = objectToRename;
        if (objectToRename == null)
            return;

        Title.text = "Rename " + selectedObject.GetObjectTypeName();


        nameInput.SetValue(objectToRename.GetName());
        nameInput.SetLabel("Name", "New name");
        nameInput.SetType("string");
        CloseBtn.onClick.RemoveAllListeners();
        CloseBtn.onClick.AddListener(() => Close());
        if (cancelCallback != null)
            CloseBtn.onClick.AddListener(cancelCallback);
    }
   
    public override async void Confirm() {
        string name = (string) nameInput.GetValue();

        try {
            selectedObject.Rename(name);
            SelectorMenu.Instance.ForceUpdateMenus();
            Close();
        } catch (RequestFailedException e) {
            //notification already shown, nothing else to do
        }
    }

    public override void Close() {
        //LeftMenu.Instance.UpdateVisibility();
        SelectorMenu.Instance.gameObject.SetActive(true);
        if (_updateVisibilityCallback != null)
            _updateVisibilityCallback.Invoke();
        base.Close();
    }
}
