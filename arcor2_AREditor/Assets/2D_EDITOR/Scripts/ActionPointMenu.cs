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
    private ButtonWithTooltip RemoveBtn, CollapseBtn, ExpandBtn, AimingBtn;

    [SerializeField]
    private InputDialog inputDialog;

    [SerializeField]
    private ActionPointAimingMenu ActionPointAimingMenu;

    private ManualTooltip UntieBtnTooltip;

    private void Start() {
        UntieBtnTooltip = UntieBtn.gameObject.GetComponent<ManualTooltip>();
    }

    public void ShowAddNewActionDialog(string action_id, IActionProvider actionProvider) {
        AddNewActionDialog.InitFromMetadata(actionProvider, actionProvider.GetActionMetadata(action_id), CurrentActionPoint);
        AddNewActionDialog.Open();
    }

    public void SaveID(string new_id) {
        CurrentActionPoint.UpdateId(new_id);
    }

    public void OpenActionPointAimingMenu(string preselectedOrientation) {
        ActionPointAimingMenu.ShowMenu(CurrentActionPoint, preselectedOrientation);
    }

    public void OpenActionPointAimingMenu() {
        ActionPointAimingMenu.ShowMenu(CurrentActionPoint);
    }


    public void ShowRenameDialog() {
        inputDialog.Open("Rename action point",
                         "",
                         "New name",
                         CurrentActionPoint.Data.Name,
                         () => RenameActionPoint(inputDialog.GetValue()),
                         () => inputDialog.Close());
    }

    public async void RenameActionPoint(string newUserId) {
       try {
            await WebsocketManager.Instance.RenameActionPoint(CurrentActionPoint.Data.Id, newUserId);
            inputDialog.Close();
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename action point", e.Message);
        }
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

        Vector3 abovePoint = SceneManager.Instance.GetCollisionFreePointAbove(CurrentActionPoint.transform, Vector3.one * 0.1f, Quaternion.identity);
        IO.Swagger.Model.Position offset = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(abovePoint));
        bool result = await GameManager.Instance.AddActionPoint(name, CurrentActionPoint.Data.Id, offset);
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
            actionsMetadata = Base.ActionsManager.Instance.GetAllActions();
        } else {
            Base.ActionObject parentActionObject = actionPoint.Parent.GetActionObject();
            actionsMetadata = Base.ActionsManager.Instance.GetAllActions();
        }

        foreach (KeyValuePair<IActionProvider, List<Base.ActionMetadata>> keyval in actionsMetadata) {
            CollapsableMenu collapsableMenu = Instantiate(CollapsablePrefab, dynamicContent.transform).GetComponent<CollapsableMenu>();
            collapsableMenu.SetLabel(keyval.Key.GetProviderName());
            collapsableMenu.Collapsed = true;

            foreach (Base.ActionMetadata am in keyval.Value) {
                ActionButton btn = Instantiate(Base.GameManager.Instance.ButtonPrefab, collapsableMenu.Content.transform).GetComponent<ActionButton>();
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetLabel(am.Name);
                if (am.Disabled) {
                    CreateTooltip(am.Problem, btn);
                    btn.Button.interactable = false;
                } else if (!string.IsNullOrEmpty(am.Description)) {
                    CreateTooltip(am.Description, btn);
                }

                btn.Button.onClick.AddListener(() => ShowAddNewActionDialog(am.Name, keyval.Key));
            }

        }
        
        UpdateLockedBtns(CurrentActionPoint.Locked);
        if (CurrentActionPoint.Parent == null) {
            UntieBtn.onClick.RemoveAllListeners();
            UntieBtn.onClick.AddListener(() => AssignToParent());
            UntieBtnTooltip.ShowAlternativeDescription();
        } else {
            UntieBtn.onClick.RemoveAllListeners();
            UntieBtn.onClick.AddListener(() => ShowUntieActionPointDialog());
            UntieBtnTooltip.ShowDefaultDescription();
        }

        try {
            await WebsocketManager.Instance.RemoveActionPoint(CurrentActionPoint.Data.Id, true);
            RemoveBtn.SetInteractivity(true);
        } catch (RequestFailedException e) {
            RemoveBtn.SetInteractivity(false);
        }
        ExpandBtn.gameObject.SetActive(CurrentActionPoint.ActionsCollapsed);
        CollapseBtn.gameObject.SetActive(!CurrentActionPoint.ActionsCollapsed);
        
    }

    private static void CreateTooltip(string text, ActionButton btn) {
        TooltipContent btnTooltip = btn.gameObject.AddComponent<TooltipContent>();
        btnTooltip.enabled = true;

        if (btnTooltip.tooltipRect == null) {
            btnTooltip.tooltipRect = Base.GameManager.Instance.Tooltip;
        }
        if (btnTooltip.descriptionText == null) {
            btnTooltip.descriptionText = Base.GameManager.Instance.Text;
        }
        btnTooltip.description = text;
    }



    private void AssignToParent() {
        Action<object> action = AssignToParent;
        GameManager.Instance.RequestObject(GameManager.EditorStateEnum.SelectingActionPointParent, action, "Select new parent (action object)", ValidateParent);
    }

    private async void AssignToParent(object selectedObject) {
        IActionPointParent parent = (IActionPointParent) selectedObject;
        if (parent == null)
            return;
        bool result = await Base.GameManager.Instance.UpdateActionPointParent(CurrentActionPoint, parent.GetId());
        if (result) {
            //
        }
    }

    private async Task<RequestResult> ValidateParent(object selectedParent) {
        IActionPointParent parent = (IActionPointParent) selectedParent;
        RequestResult result = new RequestResult(true, "");
        if (parent.GetId() == CurrentActionPoint.GetId()) {
            result.Success = false;
            result.Message = "Action point cannot be its own parent!";
        }
        
        return result;
    }


    public async void DeleteAP() {
        Debug.Assert(CurrentActionPoint != null);
        try {
            await WebsocketManager.Instance.RemoveActionPoint(CurrentActionPoint.Data.Id);
            ConfirmationDialog.Close();
            MenuManager.Instance.HideMenu(MenuManager.Instance.ActionPointMenu);
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to remove action point", e.Message);
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
        AimingBtn.SetInteractivity(!locked);
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
}
