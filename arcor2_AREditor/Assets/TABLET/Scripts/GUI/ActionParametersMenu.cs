using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ActionParametersMenu : Singleton<ActionParametersMenu>
{
    public GameObject Content;
    public CanvasGroup CanvasGroup;
    private Action3D currentAction;
    private List<IParameter> actionParameters = new List<IParameter>();
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;


    public TMPro.TMP_Text ActionName, ActionType, ActionPointName;

    public async Task<bool> Show(Action3D action) {
        if (!await action.WriteLock(false))
            return false;
        currentAction = action;
        actionParameters = await Parameter.InitActionParameters(currentAction.ActionProvider.GetProviderId(), currentAction.Parameters.Values.ToList(), Content, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false, CanvasGroup);

        ActionName.text = $"Name: {action.GetName()}";
        ActionType.text = $"Type: {action.ActionProvider.GetProviderName()}/{action.Metadata.Name}";
        ActionPointName.text = $"AP: {action.ActionPoint.GetName()}";
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        return true;
    }

    public async void Hide(bool unlock = true) {
        RectTransform[] transforms = Content.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        if (currentAction != null) {
            if(unlock)
                await currentAction.WriteUnlock();
            currentAction = null;
        }
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

    public void SetVisibility(bool visible) {
        EditorHelper.EnableCanvasGroup(CanvasGroup, visible);
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (isValueValid && currentAction.Parameters.TryGetValue(parameterId, out Parameter actionParameter)) {           
            try {
                if (JsonConvert.SerializeObject(newValue) != actionParameter.Value) {
                    SaveParameters();
                }
            } catch (JsonReaderException) {

            }
        }

    }

    public async void SaveParameters() {
        if (Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IParameter actionParameter in actionParameters) {
                IO.Swagger.Model.ParameterMeta metadata = currentAction.Metadata.GetParamMetadata(actionParameter.GetName());
                string value = JsonConvert.SerializeObject(actionParameter.GetValue());
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: value, type: actionParameter.GetCurrentType());
                parameters.Add(ap);
            }
            Debug.Assert(ProjectManager.Instance.AllowEdit);
            try {
                await WebsocketManager.Instance.UpdateAction(currentAction.Data.Id, parameters, currentAction.GetFlows());
                Notifications.Instance.ShowToastMessage("Parameters saved");
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to save parameters", e.Message);
            }
        }
    }


}
