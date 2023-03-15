using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Base;
using IO.Swagger.Model;

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
    private System.Action callback = null;


    private void Init() {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="callback">Function to be called if adding action object was successful</param>
    public void InitFromMetadata(Base.ActionObjectMetadata metadata, System.Action callback = null) {
        InitDialog(metadata);
        actionParameters = Base.Parameter.InitParameters(parametersMetadata.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false, false, null, null);
        nameInput.SetValue(Base.SceneManager.Instance.GetFreeAOName(metadata.Type));
        this.callback = callback;
    }

    public void InitDialog(ActionObjectMetadata metadata) {
        actionObjectMetadata = metadata;
        
        parametersMetadata = new Dictionary<string, ParameterMetadata>();
        foreach (IO.Swagger.Model.ParameterMeta meta in metadata.Settings) {
            parametersMetadata.Add(meta.Name, new ParameterMetadata(meta));
        }

        foreach (Transform t in DynamicContent.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        nameInput.SetLabel("Name", "Name of the action object");
        nameInput.SetType("string");
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        // TODO: add some check and set create button interactivity

    }

    public async void CreateActionObject() {
        string newActionObjectName = (string) nameInput.GetValue();

        if (Base.Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.Parameter> parameters = new List<IO.Swagger.Model.Parameter>();
            foreach (IParameter actionParameter in actionParameters) {
                if (!parametersMetadata.TryGetValue(actionParameter.GetName(), out Base.ParameterMetadata actionParameterMetadata)) {
                    Base.Notifications.Instance.ShowNotification("Failed to create new action object", "Failed to get metadata for action object parameter: " + actionParameter.GetName());
                    return;
                }
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameterMetadata.Type);
                parameters.Add(DataHelper.ActionParameterToParameter(ap));
            }
            try {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
                Vector3 point = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(0.5f)));
                IO.Swagger.Model.Pose pose = null;
                if (actionObjectMetadata.HasPose)
                    pose = new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(point), orientation: DataHelper.QuaternionToOrientation(Quaternion.identity));
                SceneManager.Instance.SelectCreatedActionObject = newActionObjectName;
                
                await Base.WebsocketManager.Instance.AddObjectToScene(newActionObjectName, actionObjectMetadata.Type, pose, parameters);
                callback?.Invoke();
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
