using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Base;
using Michsky.UI.ModernUIPack;
using IO.Swagger.Model;

public class ActionMenu : Base.Singleton<ActionMenu>, IMenu {

    public Base.Action CurrentAction;
    public GameObject DynamicContent;
    public TMPro.TMP_Text ActionName;
    public TMPro.TMP_Text ActionType;
    List<IParameter> actionParameters = new List<IParameter>();
    public AddNewActionDialog AddNewActionDialog;
    public ConfirmationDialog ConfirmationDialog;
    [SerializeField]
    private InputDialog inputDialog;
    [SerializeField]
    private ButtonWithTooltip ExecuteActionBtn, StopActionBtn, RemoveActionBtn, SaveParametersBtn;
    [SerializeField]
    private TMPro.TMP_Text ActionPointName;


    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;

    private bool parametersChanged;

    private void Start() {
        Debug.Assert(DynamicContent != null);
        Debug.Assert(ActionName != null);
        Debug.Assert(ActionType != null);
        Debug.Assert(ExecuteActionBtn != null);
        Debug.Assert(StopActionBtn != null);
        Debug.Assert(AddNewActionDialog != null);
        Debug.Assert(ConfirmationDialog != null);
        Debug.Assert(inputDialog != null);
        Debug.Assert(DynamicContentLayout != null);
        Debug.Assert(CanvasRoot != null);
        Debug.Assert(SaveParametersBtn != null);

        GameManager.Instance.OnActionExecution += OnActionExecution;
        GameManager.Instance.OnActionExecutionFinished += OnActionExecutionFinished;
        GameManager.Instance.OnActionExecutionCanceled += OnActionExecutionFinished;

    }


    private void OnActionExecutionFinished(object sender, EventArgs e) {
        _ = UpdateExecuteAndStopBtns();
    }

    private void OnActionExecution(object sender, StringEventArgs args) {
        _ = UpdateExecuteAndStopBtns();
    }

    public async void UpdateMenu() {
        DynamicContent.GetComponent<VerticalLayoutGroup>().enabled = true;
        ActionPointName.text = CurrentAction.ActionPoint.Data.Name;
        foreach (RectTransform o in DynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(CurrentAction.Data.Name);
        ActionType.text = CurrentAction.ActionProvider.GetProviderName() + "/" + Base.Action.ParseActionType(CurrentAction.Data.Type).Item2;

        actionParameters = await Base.Parameter.InitActionParameters(CurrentAction.ActionProvider.GetProviderId(), CurrentAction.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot, true);
        parametersChanged = false;
        SaveParametersBtn.SetInteractivity(false, "Parameters unchaged");
        _ = UpdateExecuteAndStopBtns();
        try {
            await WebsocketManager.Instance.RemoveAction(CurrentAction.Data.Id, true);
            RemoveActionBtn.SetInteractivity(true);
        } catch (RequestFailedException e) {
            RemoveActionBtn.SetInteractivity(false, e.Message);
        }

    }

    private async Task UpdateExecuteAndStopBtns() {
        try {
            await WebsocketManager.Instance.ExecuteAction(CurrentAction.Data.Id, true);
        } catch (RequestFailedException ex) {
            ExecuteActionBtn.SetInteractivity(false, ex.Message);
            return;
        }
        if (!string.IsNullOrEmpty(GameManager.Instance.ExecutingAction) && CurrentAction.Data.Id == GameManager.Instance.ExecutingAction) {
            StopActionBtn.gameObject.SetActive(true);
            ExecuteActionBtn.gameObject.SetActive(false);
            ExecuteActionBtn.HideTooltip();
            if (CurrentAction.Metadata.Meta.Cancellable) {
                StopActionBtn.SetInteractivity(true);
            } else {
                StopActionBtn.SetInteractivity(false);
            }
        } else {
            StopActionBtn.gameObject.SetActive(false);
            ExecuteActionBtn.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(GameManager.Instance.ExecutingAction)) {
                ExecuteActionBtn.SetInteractivity(false, "Another action runs already");
            } else {
                ExecuteActionBtn.SetInteractivity(true);
            }
        }
    }


    public async void DeleteAction() {
        ConfirmationDialog.Close();
        if (CurrentAction == null)
            return;
        try {
            await WebsocketManager.Instance.RemoveAction(CurrentAction.Data.Id, false);
            MenuManager.Instance.PuckMenu.Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove action point", e.Message);
        }
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename action",
                         "",
                         "New name",
                         CurrentAction.Data.Name,
                         () => RenameAction(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameAction(string newName) {
        try {
            await WebsocketManager.Instance.RenameAction(CurrentAction.Data.Id, newName);
            inputDialog.Close();
            ActionName.text = newName;
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename action", e.Message);
        }
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action",
                                "Do you want to delete action " + CurrentAction.Data.Name + "?",
                                () => DeleteAction(),
                                () => ConfirmationDialog.Close());
    }

    internal async void HideMenu() {
        if (CurrentAction == null)
            return;
        try {
            await WebsocketManager.Instance.WriteUnlock(CurrentAction.GetId());
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to unlock AP menu", ex.Message);
            return;
        }
    }

    public void OnChangeParameterHandler(string parameterId, object newValue, bool isValueValid = true) {
        if (!isValueValid) {
            SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
            ExecuteActionBtn.SetInteractivity(false, "Save parameters first");
        } else  if (CurrentAction.Parameters.TryGetValue(parameterId, out Base.Parameter actionParameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != actionParameter.Value) {
                    parametersChanged = true;
                    SaveParametersBtn.SetInteractivity(true);
                    ExecuteActionBtn.SetInteractivity(false, "Save parameters first");
                }
            } catch (JsonReaderException) {
                SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
                ExecuteActionBtn.SetInteractivity(false, "Save parameters first");
            }
            
        }

    }

    public async void ExecuteAction() {
        ExecuteActionBtn.SetInteractivity(false, "Action already runs");
        try {
            await WebsocketManager.Instance.ExecuteAction(CurrentAction.Data.Id, false);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to execute action", ex.Message);
            return;
        }
        ExecuteActionBtn.SetInteractivity(true);
    }

    public void StopExecution() {
        _ = GameManager.Instance.CancelExecution();
    }


    public void SetHeader(string header) {
        ActionName.text = header;
    }

    public void DuplicateAction() {        
        AddNewActionDialog.InitFromAction(CurrentAction);
        AddNewActionDialog.Open();

    }

    public async void SaveParameters() {
        if (Base.Parameter.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IParameter actionParameter in actionParameters) {
                IO.Swagger.Model.ParameterMeta metadata = CurrentAction.Metadata.GetParamMetadata(actionParameter.GetName());
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(name: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: metadata.Type);
                parameters.Add(ap);
            }
            Debug.Assert(ProjectManager.Instance.AllowEdit);
            try {
                await WebsocketManager.Instance.UpdateAction(CurrentAction.Data.Id, parameters, CurrentAction.GetFlows());
                Base.Notifications.Instance.ShowToastMessage("Parameters saved");
                SaveParametersBtn.SetInteractivity(false, "Parameters unchanged");
                parametersChanged = false;
                if (string.IsNullOrEmpty(GameManager.Instance.ExecutingAction))
                    ExecuteActionBtn.SetInteractivity(true);
            } catch (RequestFailedException e) {
                Notifications.Instance.ShowNotification("Failed to update action ", e.Message);
            }            
        }
    }

}
