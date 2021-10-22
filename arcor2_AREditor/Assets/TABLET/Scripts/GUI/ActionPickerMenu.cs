using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using Michsky.UI.ModernUIPack;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ActionPickerMenu : RightMenu<ActionPickerMenu> {
    public GameObject Content;
    public GameObject CollapsablePrefab, ButtonPrefab;

    public AddNewActionDialog AddNewActionDialog;
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
        if (args.Event.State == IO.Swagger.Model.SceneStateData.StateEnum.Started ||
            args.Event.State == IO.Swagger.Model.SceneStateData.StateEnum.Stopped) {
            List<string> uncollapsedObjects = new List<string>();
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
        Dictionary<IActionProvider, List<Base.ActionMetadata>> actionsMetadata = Base.ActionsManager.Instance.GetAllActions();

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, Content.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.SetLabel(keyval.Key.GetProviderName());
            if (uncollapsedObjects != null && uncollapsedObjects.Contains(keyval.Key.GetProviderName())) 
                collapsableMenu.Collapsed = false;
            else
                collapsableMenu.Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
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

    private RequestResult CheckActionParameters(Base.ActionMetadata actionMetadata) {
        RequestResult result = new RequestResult(true);
        bool poseError = false, jointsError = false, dynamicValueError = false;
        bool anyOrientation = ProjectManager.Instance.AnyOrientationInTheProject();
        bool anyJoints = ProjectManager.Instance.AnyJointsInTheProject();
        foreach (ParameterMetadata param in actionMetadata.ParametersMetadata.Values) {
            if (!poseError && param.Type == ParameterMetadata.POSE && !anyOrientation) {
                result.Success = false;
                result.Message += "(there is no available orientation in the project and this action requires it)\n";
                poseError = true;
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
    }

    

    public void SetVisibility(bool visible) {
        EditorHelper.EnableCanvasGroup(CanvasGroup, visible);
    }


    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), currentActionPoint);
        AddNewActionDialog.Open();
    }

    public async void DuplicateAction(Base.Action action) {
        string newActionName = Base.ProjectManager.Instance.GetFreeActionName(action.GetName() + "_copy");
        addedActionName = newActionName;
        Hide();
        AREditorResources.Instance.LeftMenuProject.SetActiveSubmenu(AREditorResources.Instance.LeftMenuProject.CurrentSubmenuOpened);
        await Base.WebsocketManager.Instance.AddAction(action.ActionPoint.GetId(), action.Parameters.Values.Cast<IO.Swagger.Model.ActionParameter>().ToList(), Base.Action.BuildActionType(
                        action.ActionProvider.GetProviderId(), action.Metadata.Name), newActionName, action.Metadata.GetFlows(newActionName));
    }

    public async void CreateNewAction(string action_id, IActionProvider actionProvider, string newName = null) {
        try {
            ActionMetadata actionMetadata = actionProvider.GetActionMetadata(action_id);
            List<IParameter> actionParameters = await Base.Parameter.InitActionParameters(actionProvider.GetProviderId(),
                actionMetadata.ParametersMetadata.Values.ToList(), HiddenPlace, OnChangeParameterHandler, HiddenPlaceLayout, HiddenPlace, currentActionPoint, false, CanvasGroup);
            string newActionName;

            if (string.IsNullOrEmpty(newName))
                newActionName = Base.ProjectManager.Instance.GetFreeActionName(actionMetadata.Name);
            else
                newActionName = Base.ProjectManager.Instance.GetFreeActionName(newName);
            if (Base.Parameter.CheckIfAllValuesValid(actionParameters)) {
                List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
                foreach (IParameter actionParameter in actionParameters) {
                    if (!actionMetadata.ParametersMetadata.TryGetValue(actionParameter.GetName(), out Base.ParameterMetadata actionParameterMetadata)) {
                        Base.Notifications.Instance.ShowNotification("Failed to create new action", "Failed to get metadata for action parameter: " + actionParameter.GetName());
                        return;
                    }
                    IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: actionParameter.GetCurrentType());
                    parameters.Add(ap);
                }
                try {
                    addedActionName = newActionName;
                    Hide();
                    AREditorResources.Instance.LeftMenuProject.SetActiveSubmenu(AREditorResources.Instance.LeftMenuProject.CurrentSubmenuOpened);

                    await Base.WebsocketManager.Instance.AddAction(currentActionPoint.GetId(), parameters, Base.Action.BuildActionType(
                        actionProvider.GetProviderId(), actionMetadata.Name), newActionName, actionMetadata.GetFlows(newActionName));

                    foreach (Transform t in HiddenPlace.transform) {
                        if (!t.CompareTag("Persistent")) {
                            Destroy(t.gameObject);
                        }
                    }

                } catch (Base.RequestFailedException e) {
                    Base.Notifications.Instance.ShowNotification("Failed to add action", e.Message);
                    addedActionName = null;
                }
            }
        } catch (Base.RequestFailedException e) {
            Base.Notifications.Instance.ShowNotification("Failed to add action", e.Message);
        }
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {

    }
}
