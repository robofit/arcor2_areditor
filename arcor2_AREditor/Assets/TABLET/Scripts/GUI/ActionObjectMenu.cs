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

public class ActionObjectMenu : RightMenu<ActionObjectMenu> {
    public ActionObject CurrentObject;
    public GameObject Parameters;
    public Slider VisibilitySlider;
    public InputDialog InputDialog;
    public ButtonWithTooltip SaveParametersBtn;
    public GameObject ObjectHasNoParameterLabel;


    public ConfirmationDialog ConfirmationDialog;

    protected bool parametersChanged = false;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;
    public TMP_Text VisibilityLabel;

    public SwitchComponent BlocklistSwitch;

    public GameObject ParameterOverridePrefab;
    private Dictionary<string, ActionObjectParameterOverride> overrides = new();

    protected List<IParameter> objectParameters = new();

    private void Start() {

        Debug.Assert(VisibilitySlider != null);
        Debug.Assert(InputDialog != null);
        Debug.Assert(ConfirmationDialog != null);

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        CommunicationManager.Instance.Client.ProjectOverrideAdded += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideAddedOrUpdated);
        CommunicationManager.Instance.Client.ProjectOverrideUpdated += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideAddedOrUpdated);
        CommunicationManager.Instance.Client.ProjectOverrideRemoved += CommunicationManager.SafeEventHandler<ParameterEventArgs>(OnOverrideRemoved);

    }

    private void OnOverrideRemoved(object sender, ParameterEventArgs args) {
        if (CurrentObject.TryGetParameter(args.Data.Name, out Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter parameter)) {
            if (overrides.TryGetValue(args.Data.Name, out ActionObjectParameterOverride parameterOverride)) {
                parameterOverride.SetValue(Parameter.GetStringValue(parameter.Value, parameter.Type), false);
            }
        }
    }

    private void OnOverrideAddedOrUpdated(object sender, ParameterEventArgs args) {
        if (overrides.TryGetValue(args.Data.Name, out ActionObjectParameterOverride parameterOverride)) {
            parameterOverride.SetValue(Parameter.GetStringValue(args.Data.Value, args.Data.Type), true);
        }
    }
    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (CurrentObject != null)
            UpdateMenu();
    }

    public void PutOnBlocklist() {
        CurrentObject.Enable(false, true, false);
    }

    public void RemoveFromBlocklist() {
        CurrentObject.Enable(SelectorMenu.Instance.ObjectsToggle.Toggled, false, true);
    }

    public async void DeleteActionObject() {
       RemoveFromSceneResponse response =
            await CommunicationManager.Instance.Client.RemoveActionObjectFromSceneAsync(new RemoveFromSceneRequestArgs(CurrentObject.Data.Id, false));
        if (!response.Result) {
            Notifications.Instance.ShowNotification("Failed to remove object " + CurrentObject.Data.Name, response.Messages[0]);
            return;
        }
        CurrentObject = null;
        ConfirmationDialog.Close();
        Hide();
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action object",
                                "Do you want to delete action object " + CurrentObject.Data.Name + "?",
                                () => DeleteActionObject(),
                                () => ConfirmationDialog.Close());
    }

    public void ShowRenameDialog() {
        InputDialog.Open("Rename action object",
                         "",
                         "New name",
                         CurrentObject.Data.Name,
                         () => RenameObject(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public async void RenameObject(string newName) {
        try {
            await CommunicationManager.Instance.Client.RenameActionObjectAsync(new RenameArgs(CurrentObject.Data.Id, newName));
            InputDialog.Close();
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to rename object", e.Message);
        }
    }

    public override async Task<bool> Show(InteractiveObject obj, bool lockTree) {
        if (!await base.Show(obj, false))
            return false;
        if (obj is ActionObject actionObject) {
            CurrentObject = actionObject;
            UpdateMenu();
            EditorHelper.EnableCanvasGroup(CanvasGroup, true);
            return true;
        } else {
            return false;
        }
    }

    public override async Task Hide() {
        await base.Hide();

        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }


    public virtual void UpdateMenu() {
        // Parameters:
        ObjectHasNoParameterLabel.SetActive(CurrentObject.ObjectParameters.Count == 0);
        BlocklistSwitch.SetValue(CurrentObject.Blocklisted);
        Parameters.GetComponent<VerticalLayoutGroup>().enabled = true;
        foreach (Transform o in Parameters.transform) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor)
            UpdateMenuScene();
        else
            UpdateMenuProject();
        VisibilitySlider.gameObject.SetActive(CurrentObject.ActionObjectMetadata.HasPose);
        if (CurrentObject.ActionObjectMetadata.HasPose) {
            VisibilityLabel.text = "Visibility:";
        } else {
            VisibilityLabel.text = "Can't set visibility for objects without pose";
        }
        UpdateSaveBtn();
        VisibilitySlider.value = CurrentObject.GetVisibility() * 100;
    }


    private void UpdateMenuScene() {
        if (CurrentObject.ObjectParameters.Count > 0) {
            objectParameters = Parameter.InitParameters(CurrentObject.ObjectParameters.Values.ToList(), Parameters, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, false, false, null, null);
        }
        foreach (IParameter parameter in objectParameters) {
            parameter.SetInteractable(!SceneManager.Instance.SceneStarted);
        }

        parametersChanged = false;
    }

    private void UpdateMenuProject() {
        overrides.Clear();

        foreach (Parameter param in CurrentObject.ObjectParameters.Values.ToList()) {
            ActionObjectParameterOverride overrideParam = Instantiate(ParameterOverridePrefab, Parameters.transform).GetComponent<ActionObjectParameterOverride>();
            overrideParam.transform.SetAsLastSibling();
            overrideParam.Init(param.GetStringValue(), false, param.ParameterMetadata, CurrentObject.Data.Id, !SceneManager.Instance.SceneStarted, DynamicContentLayout, CanvasRoot);
            if (CurrentObject.Overrides.TryGetValue(param.Name, out Parameter p)) {
                Debug.LogError(p);
                overrideParam.SetValue(p.GetStringValue(), true);
            }
            overrides[param.Name] = overrideParam;
        }


    }

    protected virtual void UpdateSaveBtn() {
        if (SceneManager.Instance.SceneStarted) {
            SaveParametersBtn.SetInteractivity(false, "Parameters could be updated only when offline.");
            return;
        }
        if (!parametersChanged) {
            SaveParametersBtn.SetInteractivity(false, "No parameter changed");
            return;
        }
        // TODO: add dry run save
        SaveParametersBtn.SetInteractivity(true);
    }

    public void SaveParameters() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor)
            SaveSceneObjectParameters();
    }


    public async void SaveSceneObjectParameters() {
        if (Parameter.CheckIfAllValuesValid(objectParameters)) {
            List<Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter> parameters = new();
            foreach (IParameter p in objectParameters) {
                if (CurrentObject.TryGetParameterMetadata(p.GetName(), out ParameterMeta parameterMeta)) {
                    ParameterMeta metadata = parameterMeta;
                    Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter ap = new(p.GetName(), value: JsonConvert.SerializeObject(p.GetValue()), type: metadata.Type);
                    parameters.Add(ap);
                } else {
                    Notifications.Instance.ShowNotification("Failed to save parameters!", "");

                }

            }

            try {
                await CommunicationManager.Instance.Client.UpdateActionObjectParametersAsync(new UpdateObjectParametersRequestArgs(CurrentObject.Data.Id, parameters));
                Notifications.Instance.ShowToastMessage("Parameters saved");
                parametersChanged = false;
                UpdateSaveBtn();
            } catch (Arcor2ConnectionException e) {
                Notifications.Instance.ShowNotification("Failed to update object parameters ", e.Message);
            }
        }
    }


    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (!isValueValid) {
            SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
        } else if (CurrentObject.TryGetParameter(parameterId, out Arcor2.ClientSdk.Communication.OpenApi.Models.Parameter parameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != parameter.Value) {
                    SaveParameters();
                }
            } catch (JsonReaderException) {
                SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
            }

        }

    }

    public void OnVisibilityChange(float value) {
        if (CurrentObject != null)
            CurrentObject.SetVisibility(value / 100f);
    }





    public async void ShowNextAO() {
        if (!await CurrentObject.WriteUnlock())
            return;

        ActionObject nextAO = SceneManager.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(nextAO);
    }

    public async void ShowPreviousAO() {
        if (!await CurrentObject.WriteUnlock())
            return;
        ActionObject previousAO = SceneManager.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(previousAO);
    }

    private static void ShowActionObject(ActionObject actionObject) {
        actionObject.OpenMenu();
    }

}
