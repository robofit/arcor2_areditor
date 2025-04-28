using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Action = Base.Action;
using ActionEventArgs = Base.ActionEventArgs;
using ActionMetadata = Base.ActionMetadata;
using ActionPoint = Base.ActionPoint;
using Parameter = Base.Parameter;

public class ActionPickerMenu : RightMenu<ActionPickerMenu> {
    public GameObject Content;
    public GameObject CollapsablePrefab, ButtonPrefab;

    private ActionPoint currentActionPoint;

    public GameObject HiddenPlace;
    public VerticalLayoutGroup HiddenPlaceLayout;

    private string addedActionName;

    private void Start() {
        ProjectManager.Instance.OnActionAddedToScene += OnActionAddedToScene;
        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;
    }

    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (!IsVisible)
            return;
        if (args.Data.State == SceneStateData.StateEnum.Started ||
            args.Data.State == SceneStateData.StateEnum.Stopped) {
            List<string> uncollapsedObjects = new();
            CollapsableMenu[] collapsableMenus = Content.GetComponentsInChildren<CollapsableMenu>();
            foreach (CollapsableMenu menu in collapsableMenus) {
                if (!menu.Collapsed)
                    uncollapsedObjects.Add(menu.GetLabel());
            }
            ClearMenu();
            UpdateMenu(uncollapsedObjects);
        }
    }

    private void OnActionAddedToScene(object sender, ActionEventArgs args) {
        if (args.Action.GetName() == addedActionName) {
            SelectorMenu.Instance.SetSelectedObject(args.Action, true);
            AREditorResources.Instance.LeftMenuProject.RenameClick(true, () => {
                AREditorResources.Instance.LeftMenuProject.SetActiveSubmenu(LeftMenuSelection.Utility);
                AREditorResources.Instance.LeftMenuProject.OpenMenuButtonClick();
            }, true);
            addedActionName = null;
        }
    }

    public override async Task<bool> Show(InteractiveObject interactiveObject, bool lockTree) {
        if (!await base.Show(interactiveObject, lockTree))
            return false;

        if (interactiveObject is ActionPoint ac)
            currentActionPoint = ac;
        else
            return false;

        UpdateMenu();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        return true;
    }

    private void UpdateMenu(List<string> uncollapsedObjects = null) {
        Dictionary<IActionProvider, List<ActionMetadata>> actionsMetadata = ActionsManager.Instance.GetAllActions();

        foreach (KeyValuePair<IActionProvider, List<ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, Content.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.SetLabel(keyval.Key.GetProviderName());
            if (uncollapsedObjects != null && uncollapsedObjects.Contains(keyval.Key.GetProviderName()))
                collapsableMenu.Collapsed = false;
            else
                collapsableMenu.Collapsed = true;

            foreach (ActionMetadata am in keyval.Value) {
                ActionButtonWithIcon btn = Instantiate(ButtonPrefab, collapsableMenu.Content.transform).GetComponent<ActionButtonWithIcon>();
                ButtonWithTooltip btnTooltip = btn.GetComponent<ButtonWithTooltip>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetLabel(am.Name);
                btn.Icon.sprite = AREditorResources.Instance.Action;

                if (am.Disabled) {
                    btn.SetInteractable(false);
                    btnTooltip.SetInteractivity(false, am.Problem);
                } else {
                    RequestResult result = CheckActionParameters(am);
                    if (!result.Success) {
                        btn.SetInteractable(false);
                        btnTooltip.SetInteractivity(false, $"Action {am.Name} could not be created\n{result.Message}");
                    } else if (!string.IsNullOrEmpty(am.Description)) {
                        btnTooltip.SetDescription(am.Description);
                    }
                }

                btn.Button.onClick.AddListener(() => CreateNewAction(am.Name, keyval.Key));
            }

        }
    }

    private void ClearMenu() {
        RectTransform[] transforms = Content.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
    }

    private RequestResult CheckActionParameters(ActionMetadata actionMetadata) {
        //TODO - otestovat
        RequestResult result = new(true);
        bool poseError = false, jointsError = false, dynamicValueError = false;
        bool anyOrientation = ProjectManager.Instance.AnyOrientationInTheProject();
        bool anyJoints = ProjectManager.Instance.AnyJointsInTheProject();
        foreach (ParameterMetadata param in actionMetadata.ParametersMetadata.Values) {

            if (!poseError && param.Type == ParameterMetadata.POSE) {
                int poseParemetersCount = 0;
                if (anyOrientation) {
                    foreach (ParameterMetadata parameter in actionMetadata.ParametersMetadata.Values) {
                        if (parameter.Type == ParameterMetadata.POSE) {
                            if (++poseParemetersCount > 1) {
                                break;
                            }

                        }
                    }
                }

                if (poseParemetersCount == 1 && !currentActionPoint.AnyOrientation()) {
                    result.Success = false;
                    result.Message += "(there is no available orientation in the selected AP and this action requires it)\n";
                    poseError = true;
                } else if (!anyOrientation) {
                    result.Success = false;
                    result.Message += "(there is no available orientation in the project and this action requires it)\n";
                    poseError = true;
                }


            }
            if (!jointsError && param.Type == ParameterMetadata.JOINTS && !anyJoints) {
                result.Success = false;
                result.Message += "(there are no available robot joints in the project and this action requires them)\n";
                jointsError = true;
            }
            if (!dynamicValueError && !SceneManager.Instance.SceneStarted && param.DynamicValue) {
                result.Success = false;
                result.Message += "(actions with dynamic parameters could only be created when online)\n";
                dynamicValueError = true;
            }
        }
        return result;
    }

    public async override Task Hide() {
        await base.Hide();
        ClearMenu();
        currentActionPoint = null;
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }



    public void SetVisibility(bool visible) {
        EditorHelper.EnableCanvasGroup(CanvasGroup, visible);
    }



    public async void DuplicateAction(Action action) {
        string newActionName = ProjectManager.Instance.GetFreeActionName(action.GetName() + "_copy");
        addedActionName = newActionName;
        Hide();
        AREditorResources.Instance.LeftMenuProject.SetActiveSubmenu(AREditorResources.Instance.LeftMenuProject.CurrentSubmenuOpened);
        await CommunicationManager.Instance.Client.AddActionAsync(
            new AddActionRequestArgs(
            action.ActionPoint.GetId(),
            newActionName,
            Action.BuildActionType(action.ActionProvider.GetProviderId(), action.Metadata.Name),
            action.Parameters.Values.Cast<ActionParameter>().ToList(),
            action.Metadata.GetFlows(newActionName)));
    }

    public async void CreateNewAction(string action_id, IActionProvider actionProvider, string newName = null) {
        try {
            ActionMetadata actionMetadata = actionProvider.GetActionMetadata(action_id);
            List<IParameter> actionParameters = await Parameter.InitActionParameters(actionProvider.GetProviderId(),
                actionMetadata.ParametersMetadata.Values.ToList(), HiddenPlace, OnChangeParameterHandler, HiddenPlaceLayout, HiddenPlace, currentActionPoint, false, CanvasGroup);
            string newActionName;

            if (string.IsNullOrEmpty(newName))
                newActionName = ProjectManager.Instance.GetFreeActionName(actionMetadata.Name);
            else
                newActionName = ProjectManager.Instance.GetFreeActionName(newName);
            if (Parameter.CheckIfAllValuesValid(actionParameters)) {
                List<ActionParameter> parameters = new();
                foreach (IParameter actionParameter in actionParameters) {
                    if (!actionMetadata.ParametersMetadata.TryGetValue(actionParameter.GetName(), out ParameterMetadata actionParameterMetadata)) {
                        Notifications.Instance.ShowNotification("Failed to create new action", "Failed to get metadata for action parameter: " + actionParameter.GetName());
                        return;
                    }
                    ActionParameter ap = new(actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameter.GetCurrentType());
                    parameters.Add(ap);
                }
                try {
                    addedActionName = newActionName;
                    Hide();
                    AREditorResources.Instance.LeftMenuProject.SetActiveSubmenu(AREditorResources.Instance.LeftMenuProject.CurrentSubmenuOpened);

                    var response = await CommunicationManager.Instance.Client.AddActionAsync(new AddActionRequestArgs(currentActionPoint.GetId(), newActionName, Action.BuildActionType(
                        actionProvider.GetProviderId(), actionMetadata.Name), parameters, actionMetadata.GetFlows(newActionName)));
                    if (!response.Result) {
                        Notifications.Instance.ShowNotification("Failed to add action", string.Join(",", response.Messages));
                        addedActionName = null;
                        return;
                    }

                    foreach (Transform t in HiddenPlace.transform) {
                        if (!t.CompareTag("Persistent")) {
                            Destroy(t.gameObject);
                        }
                    }

                } catch (Arcor2ConnectionException e) {
                    Notifications.Instance.ShowNotification("Failed to add action", e.Message);
                    addedActionName = null;
                }
            }
        } catch (Arcor2ConnectionException e) {
            Notifications.Instance.ShowNotification("Failed to add action", e.Message);
        }
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {

    }
}
