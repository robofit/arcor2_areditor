using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ActionMenu : Base.Singleton<ActionMenu>, IMenu {

    public Base.Action CurrentPuck;

    public GameObject DynamicContent;
    public TMPro.TMP_InputField TopText;
    public TMPro.TMP_Text ActionType;
    public Button ExectuteActionBtn;
    List<IActionParameter> actionParameters = new List<IActionParameter>();
    public AddNewActionDialog AddNewActionDialog;
    public ConfirmationDialog ConfirmationDialog;

    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;
    // Start is called before the first frame update
    
   
    public async void UpdateMenu() {
        DynamicContent.GetComponent<VerticalLayoutGroup>().enabled = true;
        foreach (RectTransform o in DynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(CurrentPuck.Data.Id);
        ActionType.text = CurrentPuck.Data.Type;
        List<Base.ActionParameterMetadata> actionParametersMetadata = new List<Base.ActionParameterMetadata>();
        foreach (IO.Swagger.Model.ActionParameterMeta meta in CurrentPuck.Metadata.Parameters) {
            actionParametersMetadata.Add(new Base.ActionParameterMetadata(meta));
        }
        actionParameters = await Base.Action.InitParameters(CurrentPuck.ActionProvider.GetProviderId(), CurrentPuck.Parameters.Values.ToList(), DynamicContent, OnChangeParameterHandler, DynamicContentLayout, CanvasRoot);
    }

    public void SaveID(string new_id) {
        CurrentPuck.UpdateId(new_id);
    }

    public void DeletePuck() {
        ConfirmationDialog.Close();
        if (CurrentPuck == null)
            return;        
        CurrentPuck.DeleteAction();
        MenuManager.Instance.PuckMenu.Close();
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action",
                                "Do you want to delete action " + CurrentPuck.Data.Id + "?",
                                () => DeletePuck(),
                                () => ConfirmationDialog.Close());
    }

    public void OnChangeParameterHandler(string parameterId, object newValue) {
       

        if (!CurrentPuck.Parameters.TryGetValue(parameterId, out Base.ActionParameter parameter))
            return;
        parameter.SetValue(newValue);
        
        Base.GameManager.Instance.UpdateProject();
    }

    public async void ExecuteAction() {
        ExectuteActionBtn.interactable = false;
        if (await Base.GameManager.Instance.ExecuteAction(CurrentPuck.Data.Id)) {

        }
        ExectuteActionBtn.interactable = true;
     
    }


    public void SetHeader(string header) {
        TopText.text = header;
    }

    public void DuplicateAction() {
        
        AddNewActionDialog.InitFromAction(CurrentPuck);
        AddNewActionDialog.WindowManager.OpenWindow();

    }
}
