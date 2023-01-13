using System.Collections;
using System.Collections.Generic;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ActionObjectParameterOverride : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text Name, Value;

    [SerializeField]
    private ButtonWithTooltip ModifyBtn, RestoreBtn, SaveBtn, CancelBtn;

    private string objectId;

    private ParameterMetadata parameterMetadata;

    private IParameter Input;

    private bool overridden;

    public VerticalLayoutGroup LayoutGroupToBeDisabled;

    public GameObject CanvasRoot;

    public void SetValue(string value, bool overridden) {
        Name.text = parameterMetadata.Name + (overridden ? " (overridden)" : "");
        Value.text = value;
        RestoreBtn.gameObject.SetActive(overridden);
        this.overridden = overridden;
    }

    public void Init(string value, bool overriden, ParameterMetadata parameterMetadata, string objectId, bool updateEnabled, VerticalLayoutGroup layoutGroupToBeDisabled, GameObject canvasRoot) {
        LayoutGroupToBeDisabled = layoutGroupToBeDisabled;
        CanvasRoot = canvasRoot;
        SaveBtn.gameObject.SetActive(false);
        this.parameterMetadata = parameterMetadata;
        this.objectId = objectId;
        SetValue(value, overriden);
        SaveBtn.SetInteractivity(updateEnabled, "Modification could only be done when offline");
        ModifyBtn.SetInteractivity(updateEnabled, "Modification could only be done when offline");
        RestoreBtn.SetInteractivity(updateEnabled, "Modification could only be done when offline");
        CancelBtn.SetInteractivity(updateEnabled, "Modification could only be done when offline");
    }

    public void Modify() {
        Input = Parameter.InitializeParameter(parameterMetadata, OnChangeParameterHandler, LayoutGroupToBeDisabled, CanvasRoot, Parameter.Encode(Value.text, parameterMetadata.Type), parameterMetadata.Type, null, null, false, default, false);
        Input.SetLabel("", "");
        Value.gameObject.SetActive(false);
        Input.GetTransform().SetParent(Value.transform.parent);
        Input.GetTransform().SetAsFirstSibling();
        
        SaveBtn.gameObject.SetActive(true);
        ModifyBtn.gameObject.SetActive(false);
        RestoreBtn.gameObject.SetActive(false);
        CancelBtn.gameObject.SetActive(true);
    }

    public async void Restore() {
        try {
            await WebsocketManager.Instance.DeleteOverride(objectId, new IO.Swagger.Model.Parameter(parameterMetadata.Name, parameterMetadata.Type, Value.text), false);
            RestoreBtn.gameObject.SetActive(false);
        } catch (RequestFailedException ex) {
            Debug.LogError(ex);
        }
    }

    public void Cancel() {
        Destroy(Input.GetTransform().gameObject);
        Value.gameObject.SetActive(true);
        SaveBtn.gameObject.SetActive(false);
        ModifyBtn.gameObject.SetActive(true);
        RestoreBtn.gameObject.SetActive(overridden);
        CancelBtn.gameObject.SetActive(false);
    }

    public async void Save() {
        Parameter parameter = new Parameter(parameterMetadata, Input.GetValue());
        try {
            if (overridden)
                await WebsocketManager.Instance.UpdateOverride(objectId, DataHelper.ActionParameterToParameter(parameter), false);
            else
                await WebsocketManager.Instance.AddOverride(objectId, DataHelper.ActionParameterToParameter(parameter), false);
            Destroy(Input.GetTransform().gameObject);
            Value.gameObject.SetActive(true);
            SaveBtn.gameObject.SetActive(false);
            ModifyBtn.gameObject.SetActive(true);
            RestoreBtn.gameObject.SetActive(true);
            CancelBtn.gameObject.SetActive(false);
        } catch (RequestFailedException ex) {
            Debug.LogError(ex);
            Notifications.Instance.ShowNotification("Failed to override parameter", ex.Message);
        }
        

    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (!isValueValid) {
            SaveBtn.SetInteractivity(false, "Parameter has invalid value");
        } else if (newValue.ToString() == Value.text) {
            SaveBtn.SetInteractivity(false, "Parameter was not changed");
        } else {
            SaveBtn.SetInteractivity(true);
        }

    }

}
