using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;

public class AddNewActionObjectDialog : Dialog {
    public GameObject DynamicContent, CanvasRoot;
    public VerticalLayoutGroup DynamicContentLayout;

    private Base.ActionObjectMetadata actionObjectMetadata;
    private Dictionary<string, Base.ParameterMetadata> parametersMetadata = new Dictionary<string, Base.ParameterMetadata>();
    private List<IParameter> actionParameters = new List<IParameter>();
    public Base.ActionPoint CurrentActionPoint;
    private IActionProvider actionProvider;
    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;


    private void Init() {

    }

    public void InitFromMetadata(Base.ActionObjectMetadata metadata) {
        InitDialog(metadata);
        actionParameters = Parameter.InitParameters(parametersMetadata.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false);
        nameInput.SetValue(Base.SceneManager.Instance.GetFreeAOName(metadata.Type));
    }


    public async void InitFromActionObject(Base.ActionObject actionObject) {
        /*InitDialog(actionObject.ActionProvider, actionObject.Metadata, actionObject.ActionPoint);
        actionParameters = await Base.Action.InitParameters(actionProvider.GetProviderId(), actionObject.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
        nameInput.SetValue(Base.ProjectManager.Instance.GetFreeActionName(actionObject.Data.Name));*/
    }

    public void InitDialog(ActionObjectMetadata metadata) {
        actionObjectMetadata = metadata;
        
        parametersMetadata = new Dictionary<string, ParameterMetadata>();
        foreach (IO.Swagger.Model.ParameterMeta meta in metadata.Settings) {
            parametersMetadata.Add(meta.Name, new ParameterMetadata(meta));
        }

        foreach (Transform t in DynamicContent.transform) {
            Destroy(t.gameObject);
        }
        nameInput.SetLabel("Name", "Name of the action object");
        nameInput.SetType("string");
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, bool isValueValid = true) {
        // TODO: add some check and set create button interactivity

    }

    public async void CreateActionObject() {
        string newActionObjectName = (string) nameInput.GetValue();

        if (Base.Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.Parameter> parameters = new List<IO.Swagger.Model.Parameter>();
            foreach (IParameter actionParameter in actionParameters) {
                if (!parametersMetadata.TryGetValue(actionParameter.GetName(), out Base.ParameterMetadata actionParameterMetadata)) {
                    Base.Notifications.Instance.ShowNotification("Failed to create new action", "Failed to get metadata for action parameter: " + actionParameter.GetName());
                    return;
                }
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameterMetadata.Type);
                parameters.Add(DataHelper.ActionParameterToParameter(ap));
            }
            try {
                IO.Swagger.Model.Pose pose = null;
                if (actionObjectMetadata.HasPose) {
                    Vector3 abovePoint = SceneManager.Instance.GetCollisionFreePointAbove(SceneManager.Instance.SceneOrigin.transform, actionObjectMetadata.GetModelBB(), SceneManager.Instance.SceneOrigin.transform.localRotation);
                    IO.Swagger.Model.Position offset = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(abovePoint));
                    pose = new IO.Swagger.Model.Pose(position: offset, orientation: new IO.Swagger.Model.Orientation(1, 0, 0, 0));
                }
                await Base.WebsocketManager.Instance.AddObjectToScene(newActionObjectName, actionObjectMetadata.Type, pose, parameters);
                Close();
            } catch (Base.RequestFailedException e) {
                Base.Notifications.Instance.ShowNotification("Failed to add action", e.Message);
            }
        }
    }

    public override void Confirm() {
        CreateActionObject();
    }
}
