using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;
using IO.Swagger.Model;

public class ActionObject3D : ActionObject
{
    public TextMeshPro ActionObjectName;
    public GameObject Visual, Model;

    public GameObject CubePrefab, CylinderPrefab, SpherePrefab;

    public Material ActionObjectMaterialTransparent;
    public Material ActionObjectMaterialOpaque;
    private bool transparent = false;

    private bool manipulationStarted = false;
    private TransformGizmo tfGizmo;

    private float interval = 0.1f;
    private float nextUpdate = 0;

    private bool updatePose = false;
    private Renderer modelRenderer;
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
                    if (!await GameManager.Instance.UpdateActionObjectPose(Data.Id, GetPose())) {
                        ResetPosition();
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
        return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Pose.Position));
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Pose.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
    }

    public override Quaternion GetSceneOrientation() {
        return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Pose.Orientation));
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        Data.Pose.Orientation = DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation));
    }

    public IO.Swagger.Model.Pose GetPose() {
        return new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(transform.localPosition)),
            orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(transform.localRotation)));
    }

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.SelectingActionObject) {
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
        if (type == Click.MOUSE_LEFT_BUTTON) {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
        }
        else if (type == Click.MOUSE_RIGHT_BUTTON) {
            ShowMenu();
            tfGizmo.ClearTargets();
        }

        // HANDLE TOUCH
        else if (type == Click.TOUCH) {
            if ((ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                // We have clicked with left mouse and started manipulation with object
                manipulationStarted = true;
            }
            else {
                ShowMenu();
            }
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
            modelRenderer.material.shader = standardShader;
        }
        // Set transparent shader
        else {
            if (!transparent) {
                modelRenderer.material.shader = transparentShader;
                transparent = true;
            }
            // set alpha of the material
            Color color = modelRenderer.material.color;
            color.a = value;
            modelRenderer.material.color = color;
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
        outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
        outlineOnClick.InitRenderers(new List<Renderer>() { modelRenderer });
    }

    public override GameObject GetModelCopy() {
        GameObject model = Instantiate(Model);
        model.transform.localScale = Model.transform.localScale;
        return model;
    }

    public override Vector3 GetTopPoint() {
        Vector3 position = transform.position;
        position.y += Collider.bounds.extents.y + 0.1f;
        return position;
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }
}
