using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkableInput : MonoBehaviour, IParameter {
    public Button CreateLinkBtn, RemoveLinkBtn;
    public LabeledInput Input;
    private Base.Parameter parameter;
    

    public void CreateLinkCb() {
        //TODO: switch type of input and update btns
    }

    public void RemoveLinkCb() {

    }

    public string GetName() {
        return Input.GetName();
    }

    public object GetValue() {
        return Input.GetValue();
    }

    public void SetDarkMode(bool dark) {
        Input.SetDarkMode(dark);
    }

    public void SetLabel(string label, string description) {
        Input.SetLabel(label, description);
    }

    public void SetValue(object value) {
        Input.SetValue(value);
    }

    public void SetType(string type) {
        if (type == "link") {
            RemoveLinkBtn.gameObject.SetActive(true);
            CreateLinkBtn.gameObject.SetActive(false);
        } else {
            RemoveLinkBtn.gameObject.SetActive(false);
            CreateLinkBtn.gameObject.SetActive(true);
        }
        Input.SetType(type);
    }

    public void SetParameter(Base.Parameter parameter) {
        this.parameter = parameter;
        
    }


}
