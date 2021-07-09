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
    public ButtonWithTooltip SaveParametersBtn;
    private List<IParameter> actionParameters = new List<IParameter>();
    private bool parametersChanged;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;

    public async Task<bool> Show(Action3D action) {
        if (!await action.WriteLock(false))
            return false;
        currentAction = action;
        actionParameters = await Parameter.InitActionParameters(currentAction.ActionProvider.GetProviderId(), currentAction.Parameters.Values.ToList(), Content, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false);
        parametersChanged = false;
        SaveParametersBtn.SetInteractivity(false, "Parameters unchaged");


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

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (!isValueValid) {
            SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
        } else if (currentAction.Parameters.TryGetValue(parameterId, out Parameter actionParameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != actionParameter.Value) {
                    parametersChanged = true;
                    //SaveParametersBtn.SetInteractivity(true);
                    SaveParameters();
                }
            } catch (JsonReaderException) {
                SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
            }

        }

    }

    public async void SaveParameters() {
        if (Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IParameter actionParameter in actionParameters) {
                IO.Swagger.Model.ParameterMeta metadata = currentAction.Metadata.GetParamMetadata(actionParameter.GetName());
                string value;
                /*if (actionParameter.GetCurrentType() == "link")
                    value = actionParameter.GetValue().ToString();
                else*/
                    value = JsonConvert.SerializeObject(actionParameter.GetValue());
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: value, type: actionParameter.GetCurrentType());
                parameters.Add(ap);
            }
            Debug.Assert(ProjectManager.Instance.AllowEdit);
            try {
                await WebsocketManager.Instance.UpdateAction(currentAction.Data.Id, parameters, currentAction.GetFlows());
                Notifications.Instance.ShowToastMessage("Parameters saved");
                SaveParametersBtn.SetInteractivity(false, "Parameters unchanged");
                parametersChanged = false;
                /*if (string.IsNullOrEmpty(GameManager.Instance.ExecutingAction))
                    await UpdateExecuteAndStopBtns();*/
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to save parameters", e.Message);
            }
        }
    }


}
