using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using UnityEngine;

public class ActionObjectPickerMenu : Singleton<ActionObjectPickerMenu> {
    public enum Type {
        Robots,
        ActionObjects,
        CollisionObjects
    }

    public enum CollisionObjectType {
        Cube,
        Sphere,
        Cylinder
    }

    public GameObject Content;
    public CanvasGroup CanvasGroup;
    public ActionButtonWithIconRemovable ButtonPrefab;

    public GameObject ActionObjects, CollisionObjects;

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

    private void OnSceneChanged(object sender, EventArgs e) {
        if (CanvasGroup.alpha > 0)
            UpdateRemoveBtns();
    }

    private void OnObjectTypesRemoved(object sender, ObjectTypesEventArgs args) {
        foreach (ActionButtonWithIconRemovable btn in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (args.Data.Select(s => s.Type).Contains(btn.GetLabel()))
                Destroy(btn.gameObject);
        }
    }

    private void OnObjectTypesAdded(object sender, ObjectTypesEventArgs args) {
        foreach (string objectTypeName in args.Data.Select(s => s.Type)) {
            if (ActionsManager.Instance.ActionObjectsMetadata.TryGetValue(objectTypeName, out ActionObjectMetadata actionObjectMetadata) &&
                !actionObjectMetadata.Abstract && !actionObjectMetadata.CollisionObject) {

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
            foreach (ActionObjectMetadata actionObject in ActionsManager.Instance.ActionObjectsMetadata.Values.OrderBy(x => x.Type)) {
                // abstract objects could not be created
                // collision objects are intented to be instantiated only once in the moment they are created, therefore
                // they are not listed in this menu, except for mesh-based objects (which could not be created or altered in the editor)
                if (actionObject.Abstract || (actionObject.CollisionObject && actionObject.ObjectModel.Type != ObjectModel.TypeEnum.Mesh))
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
            var response = await CommunicationManager.Instance.Client.RemoveObjectTypeAsync(type);
            if (!response.Result) {
                Notifications.Instance.ShowNotification("Failed to remove object type.", string.Join(',', response.Messages));
                Debug.LogError(string.Join(',', response.Messages));
            }
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to remove object type.", ex.Message);
            Debug.LogError(ex);
        } finally {
            confirmationDialog.Close();
        }
    }


    private void AddObjectToScene(string type) {
        if (ActionsManager.Instance.ActionObjectsMetadata.TryGetValue(type, out ActionObjectMetadata actionObjectMetadata)) {
            ShowAddObjectDialog(type);
        } else {
            NotificationsModernUI.Instance.ShowNotification("Failed to add object", "Object type " + type + " does not exist!");
        }

    }


    public void UpdateRemoveBtns() {
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor) {
            return;
        }
        List<string> types = new();
        foreach (ActionButtonWithIconRemovable b in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (b == null || b.RemoveBtn == null)
                return;
            types.Add(b.GetLabel());
        }
        CommunicationManager.Instance.Client.RemoveObjectTypesAsync(types, true)
            .ContinueWith(UpdateRemoveBtnCallback, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void UpdateRemoveBtnCallback(Task<DeleteObjectTypesResponse> response) {
        Dictionary<string, string> problems = new();
        if (response.Result.Data != null) {
            foreach (DeleteObjectTypesResponseData d in response.Result.Data) {
                problems[d.Id] = d.Error;
            }
        }
        foreach (ActionButtonWithIconRemovable b in Content.GetComponentsInChildren<ActionButtonWithIconRemovable>()) {
            if (b != null && b.RemoveBtn != null) {
                if (problems.ContainsKey(b.GetLabel())) {
                    b.RemoveBtn.SetInteractivity(false, problems[b.GetLabel()]);
                } else {
                    b.RemoveBtn.SetInteractivity(true);
                }
            }

        }
    }

    public void ShowAddObjectDialog(string type) {

        if (ActionsManager.Instance.ActionObjectsMetadata.TryGetValue(type, out ActionObjectMetadata actionObjectMetadata)) {
            addNewActionObjectDialog.InitFromMetadata(actionObjectMetadata, UpdateRemoveBtns);
            addNewActionObjectDialog.Open();
        } else {
            Notifications.Instance.SaveLogs("Failed to load metadata for object type" + type);
        }


    }

    public void Show(Type type) {
        switch (type) {
            case Type.ActionObjects:
                ActionObjects.SetActive(true);
                CollisionObjects.SetActive(false);
                break;
            case Type.CollisionObjects:
                ActionObjects.SetActive(false);
                CollisionObjects.SetActive(true);
                break;
        }
        UpdateRemoveBtns();
        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
    }

    public void Hide() {
        if (IsVisible()) {
            EditorHelper.EnableCanvasGroup(CanvasGroup, false);
            AREditorResources.Instance.LeftMenuScene.SetActiveSubmenu(LeftMenuSelection.Add, false);
        }
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

    public async void CreateCube() {
        ObjectTypeMeta newObjectType = CreateObjectTypeMeta(CollisionObjectType.Cube);
        SceneManager.Instance.SelectCreatedActionObject = newObjectType.Type;
        SceneManager.Instance.OpenTransformMenuOnCreatedObject = true;
        await CommunicationManager.Instance.Client.AddVirtualCollisionObjectToSceneAsync(
            new AddVirtualCollisionObjectToSceneRequestArgs(newObjectType.Type, Sight.Instance.CreatePoseInTheView(), newObjectType.ObjectModel))
            .ContinueWith(task => AddVirtualCollisionObjectResponseCallback(task, newObjectType.Type), TaskScheduler.FromCurrentSynchronizationContext());
    }

    public async void CreateCylinder() {
        ObjectTypeMeta newObjectType = CreateObjectTypeMeta(CollisionObjectType.Cylinder);
        SceneManager.Instance.SelectCreatedActionObject = newObjectType.Type;
        SceneManager.Instance.OpenTransformMenuOnCreatedObject = true;
        await CommunicationManager.Instance.Client.AddVirtualCollisionObjectToSceneAsync(
                new AddVirtualCollisionObjectToSceneRequestArgs(newObjectType.Type, Sight.Instance.CreatePoseInTheView(), newObjectType.ObjectModel))
            .ContinueWith(task => AddVirtualCollisionObjectResponseCallback(task, newObjectType.Type), TaskScheduler.FromCurrentSynchronizationContext());
    }

    public async void CreateSphere() {
        ObjectTypeMeta newObjectType = CreateObjectTypeMeta(CollisionObjectType.Sphere);
        SceneManager.Instance.SelectCreatedActionObject = newObjectType.Type;
        SceneManager.Instance.OpenTransformMenuOnCreatedObject = true;
        await CommunicationManager.Instance.Client.AddVirtualCollisionObjectToSceneAsync(
                new AddVirtualCollisionObjectToSceneRequestArgs(newObjectType.Type, Sight.Instance.CreatePoseInTheView(), newObjectType.ObjectModel))
            .ContinueWith(task => AddVirtualCollisionObjectResponseCallback(task, newObjectType.Type), TaskScheduler.FromCurrentSynchronizationContext());
    }



    private void AddVirtualCollisionObjectResponseCallback(Task<AddVirtualCollisionObjectToSceneResponse> task, string objectType) {
        AddVirtualCollisionObjectToSceneResponse response = task.Result;
        if (response == null || !response.Result) {
            Notifications.Instance.ShowNotification($"Failed to add {objectType}", response.Messages.FirstOrDefault());
        } else {
            Hide();
        }
    }

    public ObjectTypeMeta CreateObjectTypeMeta(CollisionObjectType collisionObjectType) {
        string name;
        ObjectModel objectModel = new();
        ObjectTypeMeta objectTypeMeta;
        ObjectModel.TypeEnum modelType = new();
        switch (collisionObjectType) {
            case CollisionObjectType.Cube:
                name = SceneManager.Instance.GetFreeObjectTypeName("Cube");
                modelType = ObjectModel.TypeEnum.Box;
                decimal sizeX = 0.1m;
                decimal sizeY = 0.1m;
                decimal sizeZ = 0.1m;
               Box box = new(name, sizeX, sizeY, sizeZ);
                objectModel.Box = box;
                break;
            case CollisionObjectType.Sphere:
                name = SceneManager.Instance.GetFreeObjectTypeName("Sphere");
                modelType = ObjectModel.TypeEnum.Sphere;
                decimal radius = 0.1m;
                Sphere sphere = new(name, radius);
                objectModel.Sphere = sphere;
                break;
            case CollisionObjectType.Cylinder:
                name = SceneManager.Instance.GetFreeObjectTypeName("Cylinder");
                modelType = ObjectModel.TypeEnum.Cylinder;
                decimal cylinderRadius = 0.1m;
                decimal cylinderHeight = 0.1m;
                Cylinder cylinder = new (name, cylinderHeight, cylinderRadius);
                objectModel.Cylinder = cylinder;
                break;
            default:
                throw new NotImplementedException();
        }
        objectModel.Type = modelType;
        objectTypeMeta = new ObjectTypeMeta(builtIn: false, description: "", type: name, objectModel: objectModel,
            varBase: "CollisionObject", hasPose: true, modified: DateTime.Now);


        return objectTypeMeta;
    }

}
