using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Parameter = Base.Parameter;

public class ActionObjectParameterOverride : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Name, Value;

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
            var response = await CommunicationManager.Instance.Client.RemoveOverrideAsync(
                new DeleteOverrideRequestArgs(objectId, new Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter(parameterMetadata.Name, parameterMetadata.Type, Value.text)), false);
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to delete parameter",
                    string.Join(',', response.Messages));
                return;
            }

            RestoreBtn.gameObject.SetActive(false);
        } catch (Arcor2ConnectionException ex) {
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
        Parameter parameter = new(parameterMetadata, Input.GetValue());
        try {
            if (overridden) {
                var response = await CommunicationManager.Instance.Client.UpdateOverrideAsync(
                    new UpdateOverrideRequestArgs(objectId, DataHelper.ActionParameterToParameter(parameter)),
                    false);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to override parameter",
                        string.Join(',', response.Messages));
                    return;
                }
            } else {
                var response = await CommunicationManager.Instance.Client.AddOverrideAsync(
                    new AddOverrideRequestArgs(objectId, DataHelper.ActionParameterToParameter(parameter)), false);
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to override parameter",
                        string.Join(',', response.Messages));
                    return;
                }
            }

            Destroy(Input.GetTransform().gameObject);
            Value.gameObject.SetActive(true);
            SaveBtn.gameObject.SetActive(false);
            ModifyBtn.gameObject.SetActive(true);
            RestoreBtn.gameObject.SetActive(true);
            CancelBtn.gameObject.SetActive(false);
        } catch (Arcor2ConnectionException ex) {
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
