using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using Base;
using System.Collections.Generic;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

public class ActionObjectMenu : RightMenu<ActionObjectMenu> {
    public Base.ActionObject CurrentObject;
    public GameObject Parameters;
    public Slider VisibilitySlider;
    public InputDialog InputDialog;
    public ButtonWithTooltip SaveParametersBtn;
    public GameObject ObjectHasNoParameterLabel;


    public ConfirmationDialog ConfirmationDialog;

    protected bool parametersChanged = false;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;
    public TMPro.TMP_Text VisibilityLabel;

    public SwitchComponent BlocklistSwitch;

    public GameObject ParameterOverridePrefab;
    private Dictionary<string, ActionObjectParameterOverride> overrides = new Dictionary<string, ActionObjectParameterOverride>();

    protected List<IParameter> objectParameters = new List<IParameter>();

    private void Start() {
        
        Debug.Assert(VisibilitySlider != null);
        Debug.Assert(InputDialog != null);
        Debug.Assert(ConfirmationDialog != null);

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

        WebsocketManager.Instance.OnOverrideAdded += OnOverrideAddedOrUpdated;
        WebsocketManager.Instance.OnOverrideUpdated += OnOverrideAddedOrUpdated;
        WebsocketManager.Instance.OnOverrideBaseUpdated += OnOverrideAddedOrUpdated;
        WebsocketManager.Instance.OnOverrideRemoved += OnOverrideRemoved;

    }

    private void OnOverrideRemoved(object sender, ParameterEventArgs args) {
        //Debug
        if (CurrentObject.TryGetParameter(args.Parameter.Name, out IO.Swagger.Model.Parameter parameter)) {
            if (overrides.TryGetValue(args.Parameter.Name, out ActionObjectParameterOverride parameterOverride)) {
                parameterOverride.SetValue(Parameter.GetStringValue(parameter.Value, parameter.Type), false);
            }
        }
    }

    private void OnOverrideAddedOrUpdated(object sender, ParameterEventArgs args) {
        //Debug.LogError("added");
        if (overrides.TryGetValue(args.Parameter.Name, out ActionObjectParameterOverride parameterOverride)) {
            parameterOverride.SetValue(Parameter.GetStringValue(args.Parameter.Value, args.Parameter.Type), true);
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
        IO.Swagger.Model.RemoveFromSceneResponse response =
            await WebsocketManager.Instance.RemoveFromScene(CurrentObject.Data.Id, false, false);
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
            await WebsocketManager.Instance.RenameObject(CurrentObject.Data.Id, newName);
            InputDialog.Close();
        } catch (RequestFailedException e) {
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
        //SaveParametersBtn.gameObject.SetActive(CurrentObject.ObjectParameters.Count != 0);
        
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
        if (Base.Parameter.CheckIfAllValuesValid(objectParameters)) {
            List<IO.Swagger.Model.Parameter> parameters = new List<IO.Swagger.Model.Parameter>();
            foreach (IParameter p in objectParameters) {
                if (CurrentObject.TryGetParameterMetadata(p.GetName(), out IO.Swagger.Model.ParameterMeta parameterMeta)) {
                    IO.Swagger.Model.ParameterMeta metadata = parameterMeta;
                    IO.Swagger.Model.Parameter ap = new IO.Swagger.Model.Parameter(name: p.GetName(), value: JsonConvert.SerializeObject(p.GetValue()), type: metadata.Type);
                    parameters.Add(ap);
                } else {
                    Notifications.Instance.ShowNotification("Failed to save parameters!", "");

                }

            }

            try {
                await WebsocketManager.Instance.UpdateObjectParameters(CurrentObject.Data.Id, parameters, false);
                Base.Notifications.Instance.ShowToastMessage("Parameters saved");
                parametersChanged = false;
                UpdateSaveBtn();
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update object parameters ", e.Message);
            }
        }
    }


    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (!isValueValid) {
            SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
        } else if (CurrentObject.TryGetParameter(parameterId, out IO.Swagger.Model.Parameter parameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != parameter.Value) {
                    //parametersChanged = true;
                    //SaveParametersBtn.SetInteractivity(true);
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
