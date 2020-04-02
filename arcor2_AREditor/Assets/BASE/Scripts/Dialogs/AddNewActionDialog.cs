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
    private List<Base.ActionParameterMetadata> actionParametersMetadata = new List<Base.ActionParameterMetadata>();
    private List<IActionParameter> actionParameters = new List<IActionParameter>();
    public Base.ActionPoint CurrentActionPoint;
    private IActionProvider actionProvider;
    [SerializeField]
    private LabeledInput nameInput;


    private async void Init() {
        
    }

    public async void InitFromMetadata(IActionProvider actionProvider, Base.ActionMetadata actionMetadata, Base.ActionPoint actionPoint) {
        InitDialog(actionProvider, actionMetadata, actionPoint);
        actionParametersMetadata = new List<Base.ActionParameterMetadata>();
        foreach (IO.Swagger.Model.ActionParameterMeta meta in actionMetadata.Parameters) {
            actionParametersMetadata.Add(new Base.ActionParameterMetadata(meta));
        }
        actionParameters = await Base.Action.InitParameters(actionProvider.GetProviderName(), actionParametersMetadata, DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, CurrentActionPoint);
        nameInput.SetValue(actionMetadata.Name);
    }


    public async void InitFromAction(Base.Action action) {
        InitDialog(action.ActionProvider, action.Metadata, action.ActionPoint);
        actionParameters = await Base.Action.InitParameters(actionProvider.GetProviderName(), action.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
        nameInput.SetValue(action.Data.Id);
    }

    public void InitDialog(IActionProvider actionProvider, Base.ActionMetadata actionMetadata, Base.ActionPoint actionPoint) {
        this.actionMetadata = actionMetadata;
        CurrentActionPoint = actionPoint;
        this.actionProvider = actionProvider;
        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        nameInput.SetLabel("Name", "Name of the action");
        nameInput.SetType("string");
    }

    public void OnChangeParameterHandler(string parameterId, object newValue) {
        // TODO: add some check and set create button interactivity    
    }

    public async void CreateAction() {
        string newActionName = (string) nameInput.GetValue();
        
        if (Base.Action.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IActionParameter actionParameter in actionParameters) {
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(CurrentActionPoint.Data.Id, JsonConvert.SerializeObject(actionParameter.GetValue()), actionParameter.GetName());
                parameters.Add(ap);
            }
            bool success = await Base.GameManager.Instance.AddAction(CurrentActionPoint.Data.Id, parameters, actionMetadata.Name, newActionName);
            if (success)
                WindowManager.CloseWindow();
        }
    }

    

}
