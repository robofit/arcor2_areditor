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
            Base.Action action = await Base.Scene.Instance.SpawnPuck(null, actionMetadata.Name, CurrentActionPoint.ActionObject, CurrentActionPoint, actionProvider, false, newActionName);
            if (action == null) {
                return;
            }
            foreach (IActionParameter actionParameter in actionParameters) {
                object value = actionParameter.GetValue();
                IO.Swagger.Model.ActionParameterMeta actionParameterMetadata = action.Metadata.GetParamMetadata(actionParameter.GetName());                
                Base.ActionParameter ap = new Base.ActionParameter(actionParameterMetadata, action, value);
                action.Parameters.Add(actionParameter.GetName(), ap);
            }
            Base.GameManager.Instance.UpdateProject();
            WindowManager.CloseWindow();
        }
    }

    

}
