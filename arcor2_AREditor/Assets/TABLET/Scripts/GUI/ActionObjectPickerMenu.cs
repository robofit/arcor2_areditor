using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Base;
using Newtonsoft.Json;
using UnityEngine;

public class ActionObjectPickerMenu : Singleton<ActionObjectPickerMenu>
{
    public GameObject Content;
    public CanvasGroup CanvasGroup;
    public ActionButtonWithIconRemovable ButtonPrefab;

    [SerializeField]
    private ConfirmationDialog confirmationDialog;

    [SerializeField]
    private AddNewActionObjectDialog addNewActionObjectDialog;

    private void Start() {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        ActionsManager.Instance.OnObjectTypesAdded += OnObjectTypesAdded;
        ActionsManager.Instance.OnObjectTypesRemoved += OnObjectTypesRemoved;
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(object sender, System.EventArgs e) {
        if (CanvasGroup.alpha > 0)
            UpdateRemoveBtns();
    }

    private void OnObjectTypesRemoved(object sender, StringListEventArgs args) {
        foreach (ActionButtonWithIconRemovable btn in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (args.Data.Contains(btn.GetLabel()))
                Destroy(btn.gameObject);
        }
    }

    private void OnObjectTypesAdded(object sender, StringListEventArgs args) {
        foreach (string objectTypeName in args.Data) {
            if (ActionsManager.Instance.ActionObjectMetadata.TryGetValue(objectTypeName, out ActionObjectMetadata actionObjectMetadata) &&
                !actionObjectMetadata.Abstract) {

                ActionButtonWithIconRemovable btn = CreateBtn(actionObjectMetadata);
                foreach (ActionButtonWithIconRemovable t in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
                    if (t.GetLabel().CompareTo(btn.GetLabel()) > 0) {
                        btn.transform.SetSiblingIndex(t.transform.GetSiblingIndex());
                        return;
                    }
                }
            }
        }
    }

    private void OnGameStateChanged(object sender, GameStateEventArgs args) {
        // destroy old buttons
        if (args.Data == GameManager.GameStateEnum.SceneEditor) {
            foreach (Transform t in Content.transform) {
                if (!t.CompareTag("Persistent"))
                    Destroy(t.gameObject);
            }
            // create one button for each object type
            foreach (ActionObjectMetadata actionObject in ActionsManager.Instance.ActionObjectMetadata.Values.OrderBy(x => x.Type)) {
                if (actionObject.Abstract)
                    continue;
                CreateBtn(actionObject);
            }
        }
        
    }

    private ActionButtonWithIconRemovable CreateBtn(ActionObjectMetadata metadata) {
        ActionButtonWithIconRemovable btn = Instantiate(ButtonPrefab, Content.transform);
        btn.transform.localScale = Vector3.one;
        btn.SetLabel(metadata.Type);
        if (metadata.Robot)
            btn.SetIcon(AREditorResources.Instance.Robot);
        else if (metadata.HasPose) {
            btn.SetIcon(AREditorResources.Instance.ActionObject);
        } else {
            btn.SetIcon(AREditorResources.Instance.NoPose);
        }
        btn.RemoveBtn.Button.onClick.AddListener(() => ShowRemoveActionObjectDialog(metadata.Type));
        btn.Button.onClick.AddListener(() => AddObjectToScene(metadata.Type));
        ButtonWithTooltip btnTooltip = btn.Button.GetComponent<ButtonWithTooltip>();
        btnTooltip.SetDescription(metadata.Description);
        btnTooltip.SetInteractivity(!metadata.Disabled, metadata.Problem);
        return btn;
    }

    public void ShowRemoveActionObjectDialog(string type) {
        confirmationDialog.Open("Delete object",
                         "Are you sure you want to delete action object " + type + "?",
                         () => RemoveActionObject(type),
                         () => confirmationDialog.Close());
    }

    public async void RemoveActionObject(string type) {
        try {
            await WebsocketManager.Instance.DeleteObjectType(type);
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to remove object type.", ex.Message);
            Debug.LogError(ex);
        } finally {
            confirmationDialog.Close();
        }
    }


    private void AddObjectToScene(string type) {
        if (Base.ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out Base.ActionObjectMetadata actionObjectMetadata)) {            
            ShowAddObjectDialog(type);
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", "Object type " + type + " does not exist!");
        }

    }


    public void UpdateRemoveBtns() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
            return;
        }
        foreach (ActionButtonWithIconRemovable b in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (b == null || b.RemoveBtn == null)
                return;
            WebsocketManager.Instance.DeleteObjectTypeDryRun(b.GetLabel(), UpdateRemoveBtnCallback);
        }
    }

    public void UpdateRemoveBtnCallback(string id, string data) {
        IO.Swagger.Model.DeleteObjectTypeResponse deleteObjectTypeResponse =
            JsonConvert.DeserializeObject<IO.Swagger.Model.DeleteObjectTypeResponse>(data);
        foreach (ActionButtonWithIconRemovable b in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (b != null && b.RemoveBtn != null && deleteObjectTypeResponse != null && id == b.GetLabel())
                b.RemoveBtn.SetInteractivity(deleteObjectTypeResponse.Result,
                    deleteObjectTypeResponse.Messages != null && deleteObjectTypeResponse.Messages.Count > 0 ? deleteObjectTypeResponse.Messages[0] : "");
        }
    }

    public void ShowAddObjectDialog(string type) {

        if (ActionsManager.Instance.ActionObjectMetadata.TryGetValue(type, out ActionObjectMetadata actionObjectMetadata)) {
            addNewActionObjectDialog.InitFromMetadata(actionObjectMetadata, UpdateRemoveBtns);
            addNewActionObjectDialog.Open();
        } else {
            Notifications.Instance.SaveLogs("Failed to load metadata for object type" + type);
        }


    }

    public void Show() {
        UpdateRemoveBtns();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void Hide() {
        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

}
