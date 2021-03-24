using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;
using System.Threading.Tasks;
using System;

public class RenameDialog : Dialog
{
    //public GameObject CanvasRoot;
    public TMPro.TMP_Text Title;

    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;

    private InteractiveObject selectedObject;

    public void Init(InteractiveObject objectToRename) {
        selectedObject = objectToRename;
        if (objectToRename == null)
            return;

        //TODO add to InteractiveObject a method for getting human friendly name of class?
        if (selectedObject is ActionPoint3D)
            Title.text = "Rename Action point";
        else if(selectedObject is Action3D)
            Title.text = "Rename Action";
        else
            Title.text = "Rename Dummy box";

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
        } catch (RequestFailedException e) {
            //notification already shown, nothing else to do
        }
    }

    public override void Close() {
        LeftMenu.Instance.UpdateVisibility();
        SelectorMenu.Instance.gameObject.SetActive(true);

        base.Close();
    }
}
