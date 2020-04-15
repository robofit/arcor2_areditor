using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class ActionPointMenu : MonoBehaviour, IMenu {
    [System.NonSerialized]
    public Base.ActionPoint CurrentActionPoint;
    public GameObject ActionButtonPrefab;
    [SerializeField]
    private TMPro.TMP_Text actionPointName, ActionObjectType;

    [SerializeField]
    private GameObject dynamicContent,
        CollapsablePrefab, scrollableContent;

    public ConfirmationDialog ConfirmationDialog;

    public AddNewActionDialog AddNewActionDialog;

    [SerializeField]
    private Button LockedBtn, UnlockedBtn, UntieBtn, BackBtn;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private ActionPointAimingMenu ActionPointAimingMenu;


    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), CurrentActionPoint);
        AddNewActionDialog.WindowManager.OpenWindow();
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.UpdateId(new_id);
    }

    public void OpenActoinPointAimingMenu() {
        ActionPointAimingMenu.ShowMenu(CurrentActionPoint);
    }

    public void ShowRenameDialog() {
        inputDialog.Open("Rename action point",
                         "Type new name",
                         "New name",
                         CurrentActionPoint.Data.Name,
                         () => RenameActionPoint(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameActionPoint(string newUserId) {
        bool result = await Base.GameManager.Instance.RenameActionPoint(CurrentActionPoint, newUserId);
        if (result) {
            inputDialog.Close();
            actionPointName.text = newUserId;
        }
    }


    public void UpdateMenu() {
        scrollableContent.GetComponent<VerticalLayoutGroup>().enabled = true;

        Base.ActionPoint actionPoint;
        if (CurrentActionPoint == null) {
            return;
        } else {
            actionPoint = CurrentActionPoint.GetComponent<Base.ActionPoint>();
        }

        foreach (RectTransform o in dynamicContent.GetComponentsInChildren<RectTransform>()) {
            if (o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        SetHeader(actionPoint.Data.Name);
        if (actionPoint.Parent != null)
            ActionObjectType.text = actionPoint.Parent.GetName();
        else
            ActionObjectType.text = "Global action point";

        Dictionary<IActionProvider, List<Base.ActionMetadata>> actionsMetadata;
        if (actionPoint.Parent == null) {
            actionsMetadata = Base.ActionsManager.Instance.GetAllFreeActions();
        } else {
            Base.ActionObject parentActionObject = actionPoint.Parent.GetActionObject();
            if (parentActionObject == null)
                actionsMetadata = Base.ActionsManager.Instance.GetAllFreeActions();
            else
                actionsMetadata = Base.ActionsManager.Instance.GetAllActionsOfObject(parentActionObject);
        }

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, dynamicContent.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.Name = keyval.Key.GetProviderName();
            collapsableMenu.Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
                Button btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, collapsableMenu.Content.transform).GetComponent<Button>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.GetComponentInChildren<TMPro.TMP_Text>().text = am.Name;
                TooltipContent btnTooltip = btn.gameObject.AddComponent<TooltipContent>();
                btnTooltip.enabled = am.Description != "";
                
                if (btnTooltip.tooltipRect == null) {
                    btnTooltip.tooltipRect = Base.GameManager.Instance.Tooltip;
                }
                if (btnTooltip.descriptionText == null) {
                    btnTooltip.descriptionText = Base.GameManager.Instance.Text;
                }
                btnTooltip.description = am.Description;
                btn.onClick.AddListener(() => ShowAddNewActionDialog(am.Name, keyval.Key));
            }

        }
        
        UpdateLockedBtns(CurrentActionPoint.Locked);
        if (CurrentActionPoint.Parent == null)
            UntieBtn.interactable = false;
        else
            UntieBtn.interactable = true;
        
    }

    

    

    public async void DeleteAP() {
        Debug.Assert(CurrentActionPoint != null);
        bool success = await Base.GameManager.Instance.RemoveActionPoint(CurrentActionPoint.Data.Id);
        if (success) {
            ConfirmationDialog.Close();
            MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
        }    
    }

   

    public void ShowDeleteAPDialog() {
        ConfirmationDialog.Open("Delete action point",
                                "Do you want to delete action point " + CurrentActionPoint.Data.Name + "?",
                                () => DeleteAP(),
                                () => ConfirmationDialog.Close());
    }

   

    
    public void SetHeader(string header) {
        actionPointName.text = header;
    }

    public void UpdateLockedBtns(bool locked) {
        LockedBtn.gameObject.SetActive(locked);
        UnlockedBtn.gameObject.SetActive(!locked);
    }

    public void SetLocked(bool locked) {
        CurrentActionPoint.Locked = locked;
        UpdateLockedBtns(locked);
    }

   
    public async void UntieActionPoint() {
        Debug.Assert(CurrentActionPoint != null);
        bool result = await Base.GameManager.Instance.UpdateActionPointParent(CurrentActionPoint, "");
        if (result) {
            ConfirmationDialog.Close();
        }
    }

    public void ShowUntieActionPointDialog() {
        ConfirmationDialog.Open("Untie action point",
                                "Do you want to untie action point " + CurrentActionPoint.Data.Name + "?",
                                () => UntieActionPoint(),
                                () => ConfirmationDialog.Close());
    }

    public void EnableBackButton(bool enable) {
        BackBtn.gameObject.SetActive(enable);
    }

    public void BackToParentMenu() {
        CurrentActionPoint.Parent.ShowMenu();
        Base.Scene.Instance.SetSelectedObject(CurrentActionPoint.Parent.GetGameObject());
        CurrentActionPoint.Parent.GetGameObject().SendMessage("Select");
    }
}
