using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;
using IO.Swagger.Model;
using TriLibCore;
using System;
using TriLibCore.General;

[RequireComponent(typeof(OutlineOnClick))]
public class ActionObject3D : ActionObject {
    public TextMeshPro ActionObjectName;
    public GameObject Visual, Model;

    public GameObject CubePrefab, CylinderPrefab, SpherePrefab;

    private bool transparent = false;

    private bool manipulationStarted = false;
    private TransformGizmo tfGizmo;

    private float interval = 0.1f;
    private float nextUpdate = 0;

    private bool updatePose = false;
    private Renderer modelRenderer;
    private Material modelMaterial;
    [SerializeField]
    private OutlineOnClick outlineOnClick;

    private Shader standardShader;
    private Shader transparentShader;

    private List<Renderer> aoRenderers = new List<Renderer>();
    private List<Collider> aoColliders = new List<Collider>();

    protected override void Start() {
        base.Start();
        transform.localScale = new Vector3(1f, 1f, 1f);
        tfGizmo = TransformGizmo.Instance;

    }


    protected override void Update() {

        if (ActionObjectMetadata != null && ActionObjectMetadata.HasPose && manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null && GameObject.ReferenceEquals(tfGizmo.mainTargetRoot.gameObject, Model)) {
                if (!tfGizmo.isTransforming && updatePose) {
                    updatePose = false;

                    UpdatePose();
                }

                if (tfGizmo.isTransforming)
                    updatePose = true;

            } else {
                manipulationStarted = false;
            }

        }

        base.Update();
    }

    private async void UpdatePose() {
        try {
            if (ActionObjectMetadata.HasPose)
                await WebsocketManager.Instance.UpdateActionObjectPose(Data.Id, GetPose());
        } catch (RequestFailedException e) {
            Notifications.Instance.ShowNotification("Failed to update action object pose", e.Message);
            ResetPosition();
        }
    }

    public override Vector3 GetScenePosition() {
        if (ActionObjectMetadata.HasPose)
            return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Pose.Position));
        else
            throw new RequestFailedException("This object has no pose");
    }

    public override void SetScenePosition(Vector3 position) {
        if (ActionObjectMetadata.HasPose)
            Data.Pose.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
    }

    public override Quaternion GetSceneOrientation() {
        if (ActionObjectMetadata.HasPose)
            return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Pose.Orientation));
        else
            throw new RequestFailedException("This object has no pose");
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        if (ActionObjectMetadata.HasPose)
            Data.Pose.Orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation));
    }

    public async override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingActionObject ||
            GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingActionPointParent) {
            GameManager.Instance.ObjectSelected(this);
            return;
        }
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor) {
            Notifications.Instance.ShowNotification("Not allowed", "Editation of action object only allowed in scene or project editor");
            return;
        }

        // HANDLE MOUSE
        if (type == Click.MOUSE_LEFT_BUTTON || type == Click.LONG_TOUCH) {
            // We have clicked with left mouse and started manipulation with object
            if (ActionObjectMetadata.HasPose && GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor) {
                StartManipulation();
            }
        } else if (type == Click.MOUSE_RIGHT_BUTTON || type == Click.TOUCH) {
            OpenMenu();

        }

    }

    public override void UpdateUserId(string newUserId) {
        base.UpdateUserId(newUserId);
        ActionObjectName.text = newUserId;
    }

    public override void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger) {
        base.ActionObjectUpdate(actionObjectSwagger);
        if (ActionObjectMetadata.HasPose) {
            ActionObjectName.text = actionObjectSwagger.Name;
        } else {
            ActionObjectName.gameObject.SetActive(false);
        }
        

    }


    public override bool SceneInteractable() {
        return base.SceneInteractable() && !MenuManager.Instance.IsAnyMenuOpened;
    }


    public override void SetVisibility(float value, bool forceShaderChange = false) {
        base.SetVisibility(value);
        if (!ActionObjectMetadata.HasPose)
            return;
        if (standardShader == null) {
            standardShader = Shader.Find("Standard");
        }

        if (transparentShader == null) {
            transparentShader = Shader.Find("Transparent/Diffuse");
        }

        if (ActionObjectMetadata.ObjectModel != null &&
            ActionObjectMetadata.ObjectModel.Type == ObjectModel.TypeEnum.Mesh) {
            // Set opaque shader
            if (value >= 1) {
                transparent = false;
                foreach (var renderer in aoRenderers) {
                    foreach (var material in renderer.materials) {
                        material.shader = standardShader;
                        Color col = material.color;
                        col.a = 1f;
                        material.color = col;
                    }
                }
            }
            // Set transparent shader
            else {
                if (!transparent) {
                    transparent = true;
                    foreach (var renderer in aoRenderers) {
                        foreach (var material in renderer.materials) {
                            material.shader = transparentShader;
                        }
                    }
                }
                foreach (var renderer in aoRenderers) {
                    foreach (var material in renderer.materials) {
                        Color col = material.color;
                        col.a = value;
                        material.color = col;
                    }
                }
            }
        } else { //not mesh
            // Set opaque shader
            if (value >= 1) {
                transparent = false;
                modelMaterial.shader = standardShader;
            }
            // Set transparent shader
            else {
                if (!transparent) {
                    modelMaterial.shader = transparentShader;
                    transparent = true;
                }
                // set alpha of the material
                Color color = modelMaterial.color;
                color.a = value;
                modelMaterial.color = color;
            }
        }
    }

    public override void Show() {
        Debug.Assert(Model != null);
        SetVisibility(1);
    }

    public override void Hide() {
        Debug.Assert(Model != null);
        SetVisibility(0);
    }

    public override void SetInteractivity(bool interactivity) {
        Debug.Assert(Model != null && ActionObjectMetadata.HasPose);
        //Model.GetComponent<Collider>().enabled = interactivity;
        if (ActionObjectMetadata.ObjectModel != null &&
            ActionObjectMetadata.ObjectModel.Type == ObjectModel.TypeEnum.Mesh) {
            foreach (var col in aoColliders) {
                col.enabled = interactivity;
            }
        } else {
            Collider.enabled = interactivity;
        }
    }

    public override void ActivateForGizmo(string layer) {
        base.ActivateForGizmo(layer);
        if (ActionObjectMetadata.HasPose)
            Model.layer = LayerMask.NameToLayer(layer);
    }

    public override void CreateModel(CollisionModels customCollisionModels = null) {
        if (!ActionObjectMetadata.HasPose)
            return;
        if (ActionObjectMetadata.ObjectModel == null || ActionObjectMetadata.ObjectModel.Type == IO.Swagger.Model.ObjectModel.TypeEnum.None) {
            Model = Instantiate(CubePrefab, Visual.transform);
            Model.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
        } else {
            switch (ActionObjectMetadata.ObjectModel.Type) {
                case IO.Swagger.Model.ObjectModel.TypeEnum.Box:
                    Model = Instantiate(CubePrefab, Visual.transform);

                    if (customCollisionModels == null) {
                        Model.transform.localScale = TransformConvertor.ROSToUnityScale(new Vector3((float) ActionObjectMetadata.ObjectModel.Box.SizeX, (float) ActionObjectMetadata.ObjectModel.Box.SizeY, (float) ActionObjectMetadata.ObjectModel.Box.SizeZ));
                    } else {
                        foreach (IO.Swagger.Model.Box box in customCollisionModels.Boxes) {
                            if (box.Id == ActionObjectMetadata.Type) {
                                Model.transform.localScale = TransformConvertor.ROSToUnityScale(new Vector3((float) box.SizeX, (float) box.SizeY, (float) box.SizeZ));
                                break;
                            }
                        }
                    }
                    break;
                case IO.Swagger.Model.ObjectModel.TypeEnum.Cylinder:
                    Model = Instantiate(CylinderPrefab, Visual.transform);
                    if (customCollisionModels == null) {
                        Model.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Cylinder.Radius, (float) ActionObjectMetadata.ObjectModel.Cylinder.Height / 2, (float) ActionObjectMetadata.ObjectModel.Cylinder.Radius);
                    } else {
                        foreach (IO.Swagger.Model.Cylinder cylinder in customCollisionModels.Cylinders) {
                            if (cylinder.Id == ActionObjectMetadata.Type) {
                                Model.transform.localScale = new Vector3((float) cylinder.Radius, (float) cylinder.Height, (float) cylinder.Radius);
                                break;
                            }
                        }
                    }
                    break;
                case IO.Swagger.Model.ObjectModel.TypeEnum.Sphere:
                    Model = Instantiate(SpherePrefab, Visual.transform);
                    if (customCollisionModels == null) {
                        Model.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Sphere.Radius, (float) ActionObjectMetadata.ObjectModel.Sphere.Radius, (float) ActionObjectMetadata.ObjectModel.Sphere.Radius);
                    } else {
                        foreach (IO.Swagger.Model.Sphere sphere in customCollisionModels.Spheres) {
                            if (sphere.Id == ActionObjectMetadata.Type) {
                                Model.transform.localScale = new Vector3((float) sphere.Radius, (float) sphere.Radius, (float) sphere.Radius);
                                break;
                            }
                        }
                    }
                    break;
                case ObjectModel.TypeEnum.Mesh:
                    MeshImporter.Instance.OnMeshImported += OnModelLoaded;
                    MeshImporter.Instance.LoadModel(ActionObjectMetadata.ObjectModel.Mesh, GetId());

                    Model = Instantiate(CubePrefab, Visual.transform);
                    Model.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
                    break;
                default:
                    Model = Instantiate(CubePrefab, Visual.transform);
                    Model.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
                    break;
            }
        }
        //if (IsRobot()) {
        //    Model.tag = "Robot";
        //}

        gameObject.GetComponent<BindParentToChild>().ChildToBind = Model;
        Collider = Model.GetComponent<Collider>();
        Colliders.Add(Collider);
        Model.GetComponent<OnClickCollider>().Target = gameObject;
        modelRenderer = Model.GetComponent<Renderer>();
        modelMaterial = modelRenderer.material;
        outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
        outlineOnClick.InitRenderers(new List<Renderer>() { modelRenderer });
        Model.AddComponent<GizmoOutlineHandler>().OutlineOnClick = outlineOnClick;

        aoRenderers.Clear();
        aoColliders.Clear();
        aoRenderers.AddRange(Model.GetComponentsInChildren<Renderer>(true));
        aoColliders.AddRange(Model.GetComponentsInChildren<Collider>(true));
    }

    public override GameObject GetModelCopy() {
        GameObject model = Instantiate(Model);
        model.transform.localScale = Model.transform.localScale;
        return model;
    }

    /// <summary>
    /// For meshes...
    /// </summary>
    /// <param name="assetLoaderContext"></param>
    public void OnModelLoaded(object sender, ImportedMeshEventArgs args) {
        if (args.Name != this.GetId())
            return;
        Model.SetActive(false);
        Destroy(Model);
        Model = args.RootGameObject;

        Model.gameObject.transform.parent = Visual.transform;
        Model.gameObject.transform.localPosition = Vector3.zero;

        gameObject.GetComponent<BindParentToChild>().ChildToBind = Model;
        Model.AddComponent<GizmoOutlineHandler>().OutlineOnClick = outlineOnClick;

        foreach (Renderer child in Model.GetComponentsInChildren<Renderer>(true)) {
            child.gameObject.AddComponent<OnClickCollider>().Target = gameObject;
            child.gameObject.AddComponent<MeshCollider>();
        }

        aoRenderers.Clear();
        aoColliders.Clear();
        aoRenderers.AddRange(Model.GetComponentsInChildren<Renderer>(true));
        aoColliders.AddRange(Model.GetComponentsInChildren<Collider>(true));

        outlineOnClick.ClearRenderers();
        outlineOnClick.InitRenderers(aoRenderers);

        transparent = false; //needs to be set before 1st call of SetVisibility after model loading
        SetVisibility(visibility);

        MeshImporter.Instance.OnMeshImported -= OnModelLoaded;
    }

    /// <summary>
    /// For meshes...
    /// </summary>
    /// <param name="obj"></param>
    private void OnModelLoadError(IContextualizedError obj) {
        Notifications.Instance.ShowNotification("Unable to show mesh " + this.GetName(), obj.GetInnerException().Message);
    }


    public override void OnHoverStart() {
        if (!enabled)
            return;
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionObject &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
                if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning)
                    return;
            } else {
                return;
            }
        }
        if (GameManager.Instance.GetGameState() != GameManager.GameStateEnum.SceneEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.ProjectEditor &&
            GameManager.Instance.GetGameState() != GameManager.GameStateEnum.PackageRunning) {
            return;
        }
        ActionObjectName.gameObject.SetActive(true);
        outlineOnClick.Highlight();
    }

    public override void OnHoverEnd() {
        ActionObjectName.gameObject.SetActive(false);
        outlineOnClick.UnHighlight();
    }

    public override void Disable() {
        base.Disable();
        modelMaterial.color = Color.gray;
    }

    public override void Enable() {
        base.Enable();
        modelMaterial.color = new Color(0.89f, 0.83f, 0.44f);
    }

    public override void OpenMenu() {
        tfGizmo.ClearTargets();
        outlineOnClick.GizmoUnHighlight();
        if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
            actionObjectMenu.CurrentObject = this;
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
        } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
            actionObjectMenuProjectEditor.CurrentObject = this;
            actionObjectMenuProjectEditor.UpdateMenu();
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
        }
    }

    public override bool HasMenu() {
        return true;
    }

    public async override void StartManipulation() {
        try {
            if (ActionObjectMetadata.HasPose) {
                await WebsocketManager.Instance.UpdateActionObjectPose(Data.Id, new IO.Swagger.Model.Pose(new Orientation(), new Position()), true);
                manipulationStarted = true;
                tfGizmo.AddTarget(Model.transform);
                outlineOnClick.GizmoHighlight();
            }            
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Object pose could not be changed", ex.Message);
        }
    }
}
