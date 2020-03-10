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


    public async void Init(IActionProvider actionProvider, Base.ActionMetadata actionMetadata, Base.ActionPoint actionPoint) {
        actionParametersMetadata = new List<Base.ActionParameterMetadata>();
        this.actionMetadata = actionMetadata;
        CurrentActionPoint = actionPoint;
        this.actionProvider = actionProvider;
        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        foreach (IO.Swagger.Model.ActionParameterMeta meta in actionMetadata.Parameters) {
            actionParametersMetadata.Add(new Base.ActionParameterMetadata(meta));         
        }
        actionParameters = await Base.Action.InitParameters(actionProvider.GetProviderName(), actionParametersMetadata, DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
    }
        
    public void OnChangeParameterHandler(string parameterId, object newValue) {
        // TODO: add some check and set create button interactivity    
    }

    public async void CreateAction() {
        if (Base.Action.CheckIfAllValuesValid(actionParameters)) {
            Base.Action action = await Base.Scene.Instance.SpawnPuck(null, actionMetadata.Name, CurrentActionPoint.ActionObject, CurrentActionPoint, actionProvider, false);
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
