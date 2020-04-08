using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ActionMenu : Base.Singleton<ActionMenu>, IMenu {

    public Base.Action CurrentAction;

    public GameObject DynamicContent;
    public TMPro.TMP_Text ActionName;
    public TMPro.TMP_Text ActionType;
    public Button ExecuteActionBtn;
    List<IActionParameter> actionParameters = new List<IActionParameter>();
    public AddNewActionDialog AddNewActionDialog;
    public ConfirmationDialog ConfirmationDialog;
    [SerializeField]
    private InputDialog inputDialog;


    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;

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
    }

    public async void Deletection() {
        ConfirmationDialog.Close();
        if (CurrentAction == null)
            return;
        if (await Base.GameManager.Instance.RemoveAction(CurrentAction.Data.Id)) {
            MenuManager.Instance.PuckMenu.Close();
        }        
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename action",
                         "Type new name",
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
                                () => Deletection(),
                                () => ConfirmationDialog.Close());
    }

    public void OnChangeParameterHandler(string parameterId, object newValue) {
       

        if (!CurrentAction.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.SetValue(newValue);
        
        Base.GameManager.Instance.UpdateProject();
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
}
