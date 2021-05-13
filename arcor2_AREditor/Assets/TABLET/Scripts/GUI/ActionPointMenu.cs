using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System.Linq;
using Base;
using System;
using System.Threading.Tasks;
using IO.Swagger.Model;

public class ActionPointMenu : MonoBehaviour, IMenu {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    [SerializeField]
    private TMPro.TMP_Text actionPointName, ActionObjectType;

    [SerializeField]
    private GameObject CollapsablePrefab, scrollableContent;

    public ConfirmationDialog ConfirmationDialog;

    [SerializeField]
    private Button BackBtn;

    [SerializeField]
    private ButtonWithTooltip CollapseBtn, ExpandBtn, AimingBtn;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    public ActionPointAimingMenu ActionPointAimingMenu;


    

    public void OpenActionPointAimingMenu() {
        ActionPointAimingMenu.ShowMenu(CurrentActionPoint);
    }



    public void ShowAddActionPointDialog() {
        inputDialog.Open("Create action point",
                         "Type action point name",
                         "Name",
                         ProjectManager.Instance.GetFreeAPName(CurrentActionPoint.Data.Name),
                         () => AddAP(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void AddAP(string name) {
        Debug.Assert(CurrentActionPoint != null);
        bool result = await GameManager.Instance.AddActionPoint(name, CurrentActionPoint.Data.Id);
        if (result)
            inputDialog.Close();
        UpdateMenu();
    }


    public async void UpdateMenu() {
        scrollableContent.GetComponent<VerticalLayoutGroup>().enabled = true;


        Base.ActionPoint actionPoint;
        if (CurrentActionPoint == null) {
            return;
        } else {
            actionPoint = CurrentActionPoint.GetComponent<Base.ActionPoint>();
        }

        
        SetHeader(actionPoint.Data.Name);
        if (actionPoint.Parent != null)
            ActionObjectType.text = actionPoint.Parent.GetName();
        else
            ActionObjectType.text = "Global action point";

        
        ExpandBtn.gameObject.SetActive(CurrentActionPoint.ActionsCollapsed);
        CollapseBtn.gameObject.SetActive(!CurrentActionPoint.ActionsCollapsed);
        AimingBtn.SetInteractivity(true);
    }

    


    /*
    private async Task<RequestResult> ValidateParent(object selectedParent) {
        IActionPointParent parent = (IActionPointParent) selectedParent;
        RequestResult result = new RequestResult(true, "");
        if (parent.GetId() == CurrentActionPoint.GetId()) {
            result.Success = false;
            result.Message = "Action point cannot be its own parent!";
        }
        
        return result;
    }*/

       
    public void SetHeader(string header) {
        actionPointName.text = header;
    }

    public void EnableBackButton(bool enable) {
        BackBtn.gameObject.SetActive(enable);
    }

    public void BackToParentMenu() {
        CurrentActionPoint.Parent.OpenMenu();
        Base.SceneManager.Instance.SetSelectedObject(CurrentActionPoint.Parent.GetGameObject());
        CurrentActionPoint.Parent.GetGameObject().SendMessage("Select", true);
    }

    public void CollapseActions() {
        PlayerPrefsHelper.SaveBool("/AP/" + CurrentActionPoint.Data.Id + "/actionsCollapsed", true);
        CurrentActionPoint.ActionsCollapsed = true;
        CurrentActionPoint.UpdatePositionsOfPucks();
        CollapseBtn.gameObject.SetActive(false);
        ExpandBtn.gameObject.SetActive(true);
    }

    public void ExpandActions() {
        PlayerPrefsHelper.SaveBool("/AP/" + CurrentActionPoint.Data.Id + "/actionsCollapsed", false);
        CurrentActionPoint.ActionsCollapsed = false;
        CurrentActionPoint.UpdatePositionsOfPucks();
        CollapseBtn.gameObject.SetActive(true);
        ExpandBtn.gameObject.SetActive(false);
    }



    public async void HideMenu() {
        if (CurrentActionPoint == null)
            return;
        await CurrentActionPoint.WriteUnlock();
    }
}
