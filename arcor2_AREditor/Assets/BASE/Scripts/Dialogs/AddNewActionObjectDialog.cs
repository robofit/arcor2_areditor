using System.Collections.Generic;
using System.Linq;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;
using ActionPoint = Base.ActionPoint;
using Parameter = Base.Parameter;
using Pose = Arcor2.ClientSdk.Communication.OpenApi.Models.Pose;

public class AddNewActionObjectDialog : Dialog {
    public GameObject DynamicContent, CanvasRoot;
    public VerticalLayoutGroup DynamicContentLayout;

    private ActionObjectMetadata actionObjectMetadata;
    private Dictionary<string, ParameterMetadata> parametersMetadata = new();
    private List<IParameter> actionParameters = new();
    public ActionPoint CurrentActionPoint;
    private IActionProvider actionProvider;
    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;
    private Action callback = null;


    private void Init() {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="callback">Function to be called if adding action object was successful</param>
    public void InitFromMetadata(ActionObjectMetadata metadata, Action callback = null) {
        InitDialog(metadata);
        actionParameters = Parameter.InitParameters(parametersMetadata.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false, false, null, null);
        nameInput.SetValue(SceneManager.Instance.GetFreeAOName(metadata.Type));
        this.callback = callback;
    }

    public void InitDialog(ActionObjectMetadata metadata) {
        actionObjectMetadata = metadata;
        
        parametersMetadata = new Dictionary<string, ParameterMetadata>();
        foreach (ParameterMeta meta in metadata.Settings) {
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

        if (Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter> parameters = new();
            foreach (IParameter actionParameter in actionParameters) {
                if (!parametersMetadata.TryGetValue(actionParameter.GetName(), out ParameterMetadata actionParameterMetadata)) {
                    Notifications.Instance.ShowNotification("Failed to create new action object", "Failed to get metadata for action object parameter: " + actionParameter.GetName());
                    return;
                }
                ActionParameter ap = new(actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameterMetadata.Type);
                parameters.Add(DataHelper.ActionParameterToParameter(ap));
            }
            try {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
                Vector3 point = TransformConvertor.UnityToROS(GameManager.Instance.Scene.transform.InverseTransformPoint(ray.GetPoint(0.5f)));
                Pose pose = null;
                if (actionObjectMetadata.HasPose)
                    pose = new Pose(DataHelper.Vector3ToPosition(point), DataHelper.QuaternionToOrientation(Quaternion.identity));
                SceneManager.Instance.SelectCreatedActionObject = newActionObjectName;
                
                await CommunicationManager.Instance.Client.AddActionObjectToSceneAsync(new AddObjectToSceneRequestArgs(newActionObjectName, actionObjectMetadata.Type, pose, parameters));
                callback?.Invoke();
                Close();
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to add action", e.Message);
            }
        }
    }

    public override void Confirm() {
        CreateActionObject();
    }
}
