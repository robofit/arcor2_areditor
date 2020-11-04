using System.Collections;
using System.Collections.Generic;
using Base;
using Newtonsoft.Json;
using UnityEngine;

public class ActionObjectParameterOverride : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text Name, Value;

    [SerializeField]
    private ButtonWithTooltip ModifyBtn, RestoreBtn, SaveBtn, CancelBtn;

    private string objectId;

    private ParameterMetadata parameterMetadata;

    private GameObject Input;

    private bool overridden;

    public void SetValue(string value, bool overridden) {
        Name.text = parameterMetadata.Name + (overridden ? " (overridden)" : "");
        Value.text = value;
        RestoreBtn.gameObject.SetActive(overridden);
        this.overridden = overridden;
    }

    public void Init(string value, bool overriden, ParameterMetadata parameterMetadata, string objectId) {
        
        SaveBtn.gameObject.SetActive(false);
        this.parameterMetadata = parameterMetadata;
        this.objectId = objectId;
        SetValue(value, overriden);
    }

    public void Modify() {
        Input = Parameter.InitializeParameter(parameterMetadata, OnChangeParameterHandler, null, null, Parameter.Encode(Value.text, parameterMetadata.Type), true);
        Input.GetComponent<IParameter>().SetLabel("", "");
        Value.gameObject.SetActive(false);
        Input.transform.SetParent(Value.transform.parent);
        Input.transform.SetAsFirstSibling();
        
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
        Destroy(Input.gameObject);
        Value.gameObject.SetActive(true);
        SaveBtn.gameObject.SetActive(false);
        ModifyBtn.gameObject.SetActive(true);
        RestoreBtn.gameObject.SetActive(overridden);
        CancelBtn.gameObject.SetActive(false);
    }

    public async void Save() {
        Parameter parameter = new Parameter(parameterMetadata, Input.GetComponent<IParameter>().GetValue());
        try {
            if (overridden)
                await WebsocketManager.Instance.UpdateOverride(objectId, parameter, false);
            else
                await WebsocketManager.Instance.AddOverride(objectId, parameter, false);
            Destroy(Input.gameObject);
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

    public void OnChangeParameterHandler(string parameterId, object newValue, bool isValueValid = true) {
        if (!isValueValid) {
            SaveBtn.SetInteractivity(false, "Parameter has invalid value");
        } else if (newValue.ToString() == Value.text) {
            SaveBtn.SetInteractivity(false, "Parameter was not changed");
        } else {
            SaveBtn.SetInteractivity(true);
        }

    }

}
