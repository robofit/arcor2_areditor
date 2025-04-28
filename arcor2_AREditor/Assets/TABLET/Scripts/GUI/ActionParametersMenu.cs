using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Parameter = Base.Parameter;

public class ActionParametersMenu : RightMenu<ActionParametersMenu> {
    public GameObject Content;
    private Action3D currentAction;
    private List<IParameter> actionParameters = new();
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;


    public TMP_Text ActionName, ActionType, ActionPointName;

    public override async Task<bool> Show(InteractiveObject interactiveObject, bool lockTree) {
        if (!await base.Show(interactiveObject, lockTree))
            return false;
        if (interactiveObject is Action3D action) {
            currentAction = action;
            actionParameters = await Parameter.InitActionParameters(currentAction.ActionProvider.GetProviderId(), currentAction.Parameters.Values.ToList(), Content, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false, CanvasGroup, currentAction.ActionPoint);

            ActionName.text = $"Name: {action.GetName()}";
            ActionType.text = $"Type: {action.ActionProvider.GetProviderName()}/{action.Metadata.Name}";
            ActionPointName.text = $"AP: {action.ActionPoint.GetName()}";
            EditorHelper.EnableCanvasGroup(CanvasGroup, true);
            action.ActionPoint.HighlightAP(true);
            return true;
        } else {
            return false;
        }

    }

    public override async Task Hide() {
        await base.Hide();
        RectTransform[] transforms = Content.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
        if (!IsVisible)
            return;

        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        currentAction.ActionPoint.HighlightAP(false);
        currentAction = null;

    }


    public void SetVisibility(bool visible) {
        EditorHelper.EnableCanvasGroup(CanvasGroup, visible);
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (isValueValid && currentAction.Parameters.TryGetValue(parameterId, out Parameter actionParameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != actionParameter.Value) {
                    if (newValue == null) {
                        actionParameter.SetValue(actionParameter.ParameterMetadata.GetDefaultValue());
                    }
                    SaveParameters();
                }
            } catch (JsonReaderException) {

            }
        }

    }

    public async void SaveParameters() {
        if (Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<ActionParameter> parameters = new();
            foreach (IParameter actionParameter in actionParameters) {
                ParameterMeta metadata = currentAction.Metadata.GetParamMetadata(actionParameter.GetName());
                string value = JsonConvert.SerializeObject(actionParameter.GetValue());
                ActionParameter ap = new(actionParameter.GetName(), value: value, type: actionParameter.GetCurrentType());
                parameters.Add(ap);
            }
            Debug.Assert(ProjectManager.Instance.AllowEdit);
            try {
                var response = await CommunicationManager.Instance.Client.UpdateActionAsync(new UpdateActionRequestArgs(currentAction.Data.Id, parameters, currentAction.GetFlows()));
                if (!response.Result) {
                    Notifications.Instance.ShowNotification("Failed to save parameters", string.Join(",", response.Messages));
                    return;
                }
                Notifications.Instance.ShowToastMessage("Parameters saved");
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to save parameters", e.Message);
            }
        }
    }


}
