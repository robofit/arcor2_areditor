using UnityEngine;
using UnityEngine.UI;
using Base;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using RuntimeInspectorNamespace;
using System.Collections.Generic;

public class ActionObjectMenuProjectEditor : ActionObjectMenu {
    public GameObject ActionPointsList;

    [SerializeField]
    private ButtonWithTooltip createAPBtn;

    public GameObject ParameterOverridePrefab;

    private Dictionary<string, ActionObjectParameterOverride> overrides = new Dictionary<string, ActionObjectParameterOverride>();

    private void Start() {
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

    public async void CreateNewAP(string name) {
        Debug.Assert(CurrentObject != null);
        
        Vector3 abovePoint = SceneManager.Instance.GetCollisionFreePointAbove(CurrentObject.transform, Vector3.one * 0.025f, Quaternion.identity);
        IO.Swagger.Model.Position offset = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(abovePoint));
        bool result = await GameManager.Instance.AddActionPoint(name, CurrentObject.Data.Id, offset);
        if (result)
            InputDialog.Close();
        UpdateMenu();
    }

    public void ShowAddActionPointDialog() {
        InputDialog.Open("Create action point",
                         "Type action point name",
                         "Name",
                         ProjectManager.Instance.GetFreeAPName(CurrentObject.Data.Name),
                         () => CreateNewAP(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public override void UpdateMenu() {
        base.UpdateMenu();



        overrides.Clear();
        createAPBtn.SetInteractivity(CurrentObject.ActionObjectMetadata.HasPose);

        foreach (Parameter param in CurrentObject.ObjectParameters.Values.ToList()) {
            ActionObjectParameterOverride overrideParam = Instantiate(ParameterOverridePrefab, Parameters.transform).GetComponent<ActionObjectParameterOverride>();
            overrideParam.transform.SetAsLastSibling();
            overrideParam.Init(param.GetStringValue(), false, param.ParameterMetadata, CurrentObject.Data.Id);
            Debug.LogError(CurrentObject.Overrides.Count);
            if (CurrentObject.Overrides.TryGetValue(param.Name, out Parameter p)) {
                Debug.LogError(p);
                overrideParam.SetValue(p.GetStringValue(), true);
            }
            overrides[param.Name] = overrideParam;
        }

        foreach (ActionPoint actionPoint in CurrentObject.GetActionPoints()) {
            Button button = GameManager.Instance.CreateButton(ActionPointsList.transform, actionPoint.Data.Name);
            button.onClick.AddListener(() => ShowActionPoint(actionPoint));

            // Add EventTrigger OnPointerEnter and OnPointerExit - to be able to highlight corresponding AP when hovering over button
            OutlineOnClick APoutline = actionPoint.GetComponent<OutlineOnClick>();
            EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            // Create OnPointerEnter entry
            EventTrigger.Entry OnPointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            OnPointerEnter.callback.AddListener((eventData) => APoutline.Highlight());
            eventTrigger.triggers.Add(OnPointerEnter);

            // Create OnPointerExit entry
            EventTrigger.Entry OnPointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            OnPointerExit.callback.AddListener((eventData) => APoutline.UnHighlight());
            eventTrigger.triggers.Add(OnPointerExit);
        }
    }

    private static void ShowActionPoint(ActionPoint actionPoint) {
        MenuManager.Instance.ActionObjectMenuProjectEditor.Close();
        actionPoint.ShowMenu(true);
        
        SceneManager.Instance.SetSelectedObject(actionPoint.gameObject);
        // Select(force = true) to force selection and not losing AP highlight upon ActionObjectMenuProjectEditor menu closing 
        actionPoint.SendMessage("Select", true);
    }

    public void OverrideParameters() {

    }

    protected override void UpdateSaveBtn() {
        if (SceneManager.Instance.SceneStarted) {
            SaveParametersBtn.SetInteractivity(false, "Parameters could be overrided only when scene is stopped.");
            return;
        }
        if (!parametersChanged) {
            SaveParametersBtn.SetInteractivity(false, "No parameter changed");
            return;
        }
        // TODO: add dry run save
        SaveParametersBtn.SetInteractivity(true);

    }
}
