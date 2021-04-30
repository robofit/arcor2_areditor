using UnityEngine;
using UnityEngine.UI;
using System;
using DanielLochner.Assets.SimpleSideMenu;
using Michsky.UI.ModernUIPack;
using Base;
using System.Collections.Generic;
using static IO.Swagger.Model.UpdateObjectPoseUsingRobotRequestArgs;
using Newtonsoft.Json;
using System.Linq;

[RequireComponent(typeof(SimpleSideMenu))]
public abstract class ActionObjectMenu : MonoBehaviour, IMenu {
    public Base.ActionObject CurrentObject;
    [SerializeField]
    protected TMPro.TMP_Text objectName;
    public GameObject Parameters;
    public Slider VisibilitySlider;
    public InputDialog InputDialog;
    public ButtonWithTooltip SaveParametersBtn;


    public ConfirmationDialog ConfirmationDialog;


    protected SimpleSideMenu menu;

    protected bool parametersChanged = false;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;

    protected List<IParameter> objectParameters = new List<IParameter>();

    private void Start() {
        
        Debug.Assert(objectName != null);
        Debug.Assert(VisibilitySlider != null);
        Debug.Assert(InputDialog != null);
        Debug.Assert(ConfirmationDialog != null);

        SceneManager.Instance.OnSceneStateEvent += OnSceneStateEvent;

    }

    private void Awake() {
        menu = GetComponent<SimpleSideMenu>();
    }
    private void OnSceneStateEvent(object sender, SceneStateEventArgs args) {
        if (CurrentObject != null)
            UpdateMenu();
    }

    internal async void HideMenu() {
        if (CurrentObject == null)
            return;
        await CurrentObject.WriteUnlock();
    }


    public async void DeleteActionObject() {
        IO.Swagger.Model.RemoveFromSceneResponse response =
            await WebsocketManager.Instance.RemoveFromScene(CurrentObject.Data.Id, false, false);
        if (!response.Result) {
            Notifications.Instance.ShowNotification("Failed to remove object " + CurrentObject.Data.Name, response.Messages[0]);
            return;
        }
        CurrentObject = null;
        ConfirmationDialog.Close();
        MenuManager.Instance.ActionObjectMenuSceneEditor.Close();
    }

    public void ShowDeleteActionDialog() {
        ConfirmationDialog.Open("Delete action object",
                                "Do you want to delete action object " + CurrentObject.Data.Name + "?",
                                () => DeleteActionObject(),
                                () => ConfirmationDialog.Close());
    }

    public void ShowRenameDialog() {
        InputDialog.Open("Rename action object",
                         "",
                         "New name",
                         CurrentObject.Data.Name,
                         () => RenameObject(InputDialog.GetValue()),
                         () => InputDialog.Close());
    }

    public async void RenameObject(string newName) {
        try {
            await WebsocketManager.Instance.RenameObject(CurrentObject.Data.Id, newName);
            InputDialog.Close();
            objectName.text = newName;
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to rename object", e.Message);
        }
    }


    public virtual void UpdateMenu() {
        objectName.text = CurrentObject.Data.Name;
        // Parameters:

        Parameters.GetComponent<VerticalLayoutGroup>().enabled = true;
        foreach (Transform o in Parameters.transform) {
            if (o.name != "Layout" && o.gameObject.tag != "Persistent") {
                Destroy(o.gameObject);
            }
        }
        VisibilitySlider.value = CurrentObject.GetVisibility() * 100;
    }

    protected abstract void UpdateSaveBtn();
        

    public void OnChangeParameterHandler(string parameterId, object newValue, string type, bool isValueValid = true) {
        if (!isValueValid) {
            SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
        } else if (CurrentObject.TryGetParameter(parameterId, out IO.Swagger.Model.Parameter parameter)) {
            try {
                if (JsonConvert.SerializeObject(newValue) != parameter.Value) {
                    parametersChanged = true;
                    SaveParametersBtn.SetInteractivity(true);
                }
            } catch (JsonReaderException) {
                SaveParametersBtn.SetInteractivity(false, "Some parameter has invalid value");
            }

        }

    }

    public void OnVisibilityChange(float value) {
        if (CurrentObject != null)
            CurrentObject.SetVisibility(value / 100f);
    }

    

   

    public async void ShowNextAO() {
        if (!await CurrentObject.WriteUnlock())
            return;

        ActionObject nextAO = SceneManager.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(nextAO);
    }

    public async void ShowPreviousAO() {
        if (!await CurrentObject.WriteUnlock())
            return;
        ActionObject previousAO = SceneManager.Instance.GetNextActionObject(CurrentObject.Data.Id);
        ShowActionObject(previousAO);
    }

    private static void ShowActionObject(ActionObject actionObject) {
        actionObject.OpenMenu();
        actionObject.SendMessage("Select", false);
    }

}
