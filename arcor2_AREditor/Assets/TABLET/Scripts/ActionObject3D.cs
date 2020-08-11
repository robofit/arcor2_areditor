using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;
using IO.Swagger.Model;

[RequireComponent(typeof(OutlineOnClick))]
public class ActionObject3D : ActionObject
{
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

    protected override void Start() {
        base.Start();
        transform.localScale = new Vector3(1f, 1f, 1f);
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
        
    }


    protected override async void Update() {
        if (manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null && GameObject.ReferenceEquals(tfGizmo.mainTargetRoot.gameObject, Model)) {
                if (!tfGizmo.isTransforming && updatePose) {
                    updatePose = false;

                    if (ActionObjectMetadata.HasPose) {
                        try {
                            await WebsocketManager.Instance.UpdateActionObjectPose(Data.Id, GetPose());
                        } catch (RequestFailedException e) {
                            Notifications.Instance.ShowNotification("Failed to update action object pose", e.Message);
                            ResetPosition();
                        }
                    } else {
                        PlayerPrefsHelper.SavePose("scene/" + SceneManager.Instance.SceneMeta.Id + "/action_object/" + Data.Id + "/pose",
                            transform.localPosition, transform.localRotation);
                    }
                }

                if (tfGizmo.isTransforming)
                    updatePose = true;

            } else {
                manipulationStarted = false;
            }           

        }

        base.Update();
    }

    public override Vector3 GetScenePosition() {
        if (ActionObjectMetadata.HasPose)
            return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Pose.Position));
        else
            return PlayerPrefsHelper.LoadVector3("scene/" + SceneManager.Instance.SceneMeta.Id + "/action_object/" + Data.Id + "/pose/position",
                            Vector3.zero);
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Pose.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));        
    }

    public override Quaternion GetSceneOrientation() {
        if (ActionObjectMetadata.HasPose)
            return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Pose.Orientation));
        else
            return PlayerPrefsHelper.LoadQuaternion("scene/" + SceneManager.Instance.SceneMeta.Id + "/action_object/" + Data.Id + "/pose/rotation",
                            Quaternion.identity);
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        Data.Pose.Orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation));
    }

    public IO.Swagger.Model.Pose GetPose() {
        if (ActionObjectMetadata.HasPose)
            return new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(transform.localPosition)),
                orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(transform.localRotation)));
        else
            return new IO.Swagger.Model.Pose(new Orientation(), new Position());
    }

    public override void OnClick(Click type) {        
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
            if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.SceneEditor) {
                manipulationStarted = true;
                tfGizmo.AddTarget(Model.transform);
                outlineOnClick.GizmoHighlight();
            }
        }
        else if (type == Click.MOUSE_RIGHT_BUTTON || type == Click.TOUCH) {
            ShowMenu();
            tfGizmo.ClearTargets();
            outlineOnClick.GizmoUnHighlight();
        }
                
    }

    public override void UpdateUserId(string newUserId) {
        base.UpdateUserId(newUserId);
        ActionObjectName.text = newUserId;
    }

    public override void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger, bool visibility, bool interactivity) {
        Debug.Assert(Model != null);
        base.ActionObjectUpdate(actionObjectSwagger, visibility, interactivity);
        ActionObjectName.text = actionObjectSwagger.Name;

    }


    public override bool SceneInteractable() {
        return base.SceneInteractable() && !MenuManager.Instance.IsAnyMenuOpened;
    }

    public override void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata, IO.Swagger.Model.CollisionModels customCollisionModels = null) {
        base.InitActionObject(id, type, position, orientation, uuid, actionObjectMetadata);
        Data.Id = id;
        Data.Type = type;
        SetScenePosition(position);
        SetSceneOrientation(orientation);
        Data.Id = uuid;
        ActionObjectMetadata = actionObjectMetadata;
        CreateModel(customCollisionModels);
        enabled = true;
        SetVisibility(visibility);
    }

    public override void SetVisibility(float value) {
        base.SetVisibility(value);

        if (standardShader == null) {
            standardShader = Shader.Find("Standard");
        }

        if (transparentShader == null) {
            transparentShader = Shader.Find("Transparent/Diffuse");
        }

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

    public override void Show() {
        Debug.Assert(Model != null);
        foreach (Renderer renderer in Visual.GetComponentsInChildren<Renderer>()) {
            renderer.enabled = true;
        }
    }

    public override void Hide() {
        Debug.Assert(Model != null);
        foreach (Renderer renderer in Visual.GetComponentsInChildren<Renderer>()) {
            renderer.enabled = false;
        }
    }

    public override void SetInteractivity(bool interactivity) {
        Debug.Assert(Model != null);
        Model.GetComponent<Collider>().enabled = interactivity;
    }

    public override void ActivateForGizmo(string layer) {
        base.ActivateForGizmo(layer);
        Model.layer = LayerMask.NameToLayer(layer);
    }

    public override void CreateModel(CollisionModels customCollisionModels = null) {
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
                        Model.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Cylinder.Radius, (float) ActionObjectMetadata.ObjectModel.Cylinder.Height, (float) ActionObjectMetadata.ObjectModel.Cylinder.Radius);
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
                default:
                    Model = Instantiate(CubePrefab, Visual.transform);
                    Model.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
                    break;
            }
        }
        if (IsRobot()) {
            Model.tag = "Robot";
        }
        gameObject.GetComponent<BindParentToChild>().ChildToBind = Model;
        Collider = Model.GetComponent<Collider>();
        Model.GetComponent<OnClickCollider>().Target = gameObject;
        modelRenderer = Model.GetComponent<Renderer>();
        modelMaterial = modelRenderer.material;
        outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
        outlineOnClick.InitRenderers(new List<Renderer>() { modelRenderer });
        Model.AddComponent<GizmoOutlineHandler>().OutlineOnClick = outlineOnClick;
    }

    public override GameObject GetModelCopy() {
        GameObject model = Instantiate(Model);
        model.transform.localScale = Model.transform.localScale;
        return model;
    }


    public override void OnHoverStart() {
        if (!enabled)
            return;
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionObject &&
            GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionPointParent) {
            if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Closed) {
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

}
