using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;


public class ActionMenu : Base.Singleton<ActionMenu>, IMenu {

    public Base.Action CurrentAction;
    public GameObject DynamicContent;
    public TMPro.TMP_Text ActionName;
    public TMPro.TMP_Text ActionType;
    public Button ExecuteActionBtn, SaveParametersBtn;
    List<IActionParameter> actionParameters = new List<IActionParameter>();
    public AddNewActionDialog AddNewActionDialog;
    public ConfirmationDialog ConfirmationDialog;
    [SerializeField]
    private InputDialog inputDialog;


    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;

    private bool parametersChanged;

    private void Start() {
        Debug.Assert(DynamicContent != null);
        Debug.Assert(ActionName != null);
        Debug.Assert(ActionType != null);
        Debug.Assert(ExecuteActionBtn != null);
        Debug.Assert(AddNewActionDialog != null);
        Debug.Assert(ConfirmationDialog != null);
        Debug.Assert(inputDialog != null);
        Debug.Assert(DynamicContentLayout != null);
        Debug.Assert(CanvasRoot != null);
        Debug.Assert(SaveParametersBtn != null);
    }

    public async void UpdateMenu() {
        DynamicContent.GetComponent<VerticalLayoutGroup>().enabled = true;
        foreach (RectTransform o in DynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(CurrentAction.Data.Name);
        ActionType.text = CurrentAction.ActionProvider.GetProviderName() + "/" + Base.Action.ParseActionType(CurrentAction.Data.Type).Item2;
        List<Base.ActionParameterMetadata> actionParametersMetadata = new List<Base.ActionParameterMetadata>();
        foreach (IO.Swagger.Model.ActionParameterMeta meta in CurrentAction.Metadata.Parameters) {
            actionParametersMetadata.Add(new Base.ActionParameterMetadata(meta));
        }
        actionParameters = await Base.Action.InitParameters(CurrentAction.ActionProvider.GetProviderId(), CurrentAction.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
        parametersChanged = false;
        SaveParametersBtn.interactable = false;
    }

    public async void DeleteAction() {
        ConfirmationDialog.Close();
        if (CurrentAction == null)
            return;
        if (await Base.GameManager.Instance.RemoveAction(CurrentAction.Data.Id)) {
            MenuManager.Instance.PuckMenu.Close();
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
        bool result = await Base.GameManager.Instance.RenameAction(CurrentAction.Data.Id, newName);
        if (result) {
            inputDialog.Close();
            ActionName.text = newName;
        }
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action",
                                "Do you want to delete action " + CurrentAction.Data.Name + "?",
                                () => DeleteAction(),
                                () => ConfirmationDialog.Close());
    }

    public void OnChangeParameterHandler(string parameterId, object newValue) {
        if (CurrentAction.Parameters.TryGetValue(parameterId, out Base.ActionParameter actionParameter)) {
            if (JsonConvert.SerializeObject(newValue) != actionParameter.Value) {
                parametersChanged = true;
                SaveParametersBtn.interactable = true;
                ExecuteActionBtn.interactable = false;
            }
        }
        
    }

    public async void ExecuteAction() {
        ExecuteActionBtn.interactable = false;
        if (await Base.GameManager.Instance.ExecuteAction(CurrentAction.Data.Id)) {

        }
        ExecuteActionBtn.interactable = true;     
    }


    public void SetHeader(string header) {
        ActionName.text = header;
    }

    public void DuplicateAction() {        
        AddNewActionDialog.InitFromAction(CurrentAction);
        AddNewActionDialog.WindowManager.OpenWindow();

    }

    public async void SaveParameters() {
        if (Base.Action.CheckIfAllValuesValid(actionParameters)) {
            List<IO.Swagger.Model.ActionParameter> parameters = new List<IO.Swagger.Model.ActionParameter>();
            foreach (IActionParameter actionParameter in actionParameters) {
                IO.Swagger.Model.ActionParameterMeta metadata = CurrentAction.Metadata.GetParamMetadata(actionParameter.GetName());
                IO.Swagger.Model.ActionParameter ap = new IO.Swagger.Model.ActionParameter(id: actionParameter.GetName(), value: JsonConvert.SerializeObject(actionParameter.GetValue()), type: metadata.Type);
                parameters.Add(ap);
            }
            bool success = await Base.GameManager.Instance.UpdateAction(CurrentAction.Data.Id, parameters);
            if (success) {
                Base.Notifications.Instance.ShowNotification("Parameters saved", "");
                SaveParametersBtn.interactable = false;
                parametersChanged = false;
                ExecuteActionBtn.interactable = true;
            }                
        }
    }

}
