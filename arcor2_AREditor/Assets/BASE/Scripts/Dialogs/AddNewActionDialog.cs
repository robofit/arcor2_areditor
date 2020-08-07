using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;

public class AddNewActionDialog : Dialog
{
    public GameObject DynamicContent, CanvasRoot;
    public VerticalLayoutGroup DynamicContentLayout;

    private Base.ActionMetadata actionMetadata;
    private Dictionary<string, Base.ParameterMetadata> actionParametersMetadata = new Dictionary<string, Base.ParameterMetadata>();
    private List<IParameter> actionParameters = new List<IParameter>();
    public Base.ActionPoint CurrentActionPoint;
    private IActionProvider actionProvider;
    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;


    private void Init() {
        
    }

    public async void InitFromMetadata(IActionProvider actionProvider, Base.ActionMetadata actionMetadata, Base.ActionPoint actionPoint) {
        InitDialog(actionProvider, actionMetadata, actionPoint);
        actionParameters = await Base.Parameter.InitActionParameters(actionProvider.GetProviderId(), actionParametersMetadata.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, CurrentActionPoint);
        nameInput.SetValue(Base.ProjectManager.Instance.GetFreeActionName(actionMetadata.Name));
    }


    public async void InitFromAction(Base.Action action) {
        InitDialog(action.ActionProvider, action.Metadata, action.ActionPoint);
        actionParameters = await Base.Parameter.InitActionParameters(actionProvider.GetProviderId(), action.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
        nameInput.SetValue(Base.ProjectManager.Instance.GetFreeActionName(action.Data.Name));
    }

    public void InitDialog(IActionProvider actionProvider, Base.ActionMetadata actionMetadata, Base.ActionPoint actionPoint) {
        this.actionMetadata = actionMetadata;
        CurrentActionPoint = actionPoint;
        this.actionProvider = actionProvider;
        actionParametersMetadata = new Dictionary<string, Base.ParameterMetadata>();
        foreach (IO.Swagger.Model.ParameterMeta meta in actionMetadata.Parameters) {
            actionParametersMetadata.Add(meta.Name, new Base.ParameterMetadata(meta));
        }

        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        nameInput.SetLabel("Name", "Name of the action");
        nameInput.SetType("string");
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, bool isValueValid = true) {
        // TODO: add some check and set create button interactivity
        
    }

    public async void CreateAction() {
        string newActionName = (string) nameInput.GetValue();
        
        if (Base.Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IParameter actionParameter in actionParameters) {
                if (!actionParametersMetadata.TryGetValue(actionParameter.GetName(), out Base.ParameterMetadata actionParameterMetadata)) {
                    Base.Notifications.Instance.ShowNotification("Failed to create new action", "Failed to get metadata for action parameter: " + actionParameter.GetName());
                    return;
                }
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameterMetadata.Type);
                parameters.Add(ap);
            }
            try {
                await Base.WebsocketManager.Instance.AddAction(CurrentActionPoint.Data.Id, parameters, Base.Action.BuildActionType(
                    actionProvider.GetProviderId(), actionMetadata.Name), newActionName, actionMetadata.GetFlows(newActionName));
                Close();
            } catch (Base.RequestFailedException e) {
                Base.Notifications.Instance.ShowNotification("Failed to add action", e.Message);
            }
        }
    }

    public override void Confirm() {
        CreateAction();
    }
}
