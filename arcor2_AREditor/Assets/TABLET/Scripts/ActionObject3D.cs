using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;

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

    protected override void Start() {
        base.Start();
        transform.localScale = new Vector3(1f, 1f, 1f);
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
        // disable update method until model is ready
        //enabled = false;
    }


    protected override async void Update() {
        if (manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null) {
                if (Time.time >= nextUpdate) {
                    nextUpdate += interval;

                    // check if gameobject with whom is Gizmo manipulating is our Model gameobject
                    if (GameObject.ReferenceEquals(Model, tfGizmo.mainTargetRoot.gameObject)) {
                        // if Gizmo is moving, we can send UpdateProject to server
                        if (tfGizmo.isTransforming) {
                            updatePose = true;
                        } else if (updatePose) {
                            updatePose = false;
                            if (!await GameManager.Instance.UpdateActionObjectPose(Data.Id, GetPose())) {
                                ResetPosition();
                            }
                        }
                    }
                }
            } else {
                if (updatePose) {
                    updatePose = false;
                    if (!await GameManager.Instance.UpdateActionObjectPose(Data.Id, GetPose())) {
                        ResetPosition();
                    }
                }
                manipulationStarted = false;
                GameManager.Instance.ActivateGizmoOverlay(false);
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
        return new IO.Swagger.Model.Pose(position: DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(transform.position)),
            orientation: DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(transform.rotation)));
    }

    public override void OnClick(Click type) {
        // HANDLE MOUSE
        if (type == Click.MOUSE_LEFT_BUTTON) {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
            GameManager.Instance.ActivateGizmoOverlay(true);
        }
        else if (type == Click.MOUSE_RIGHT_BUTTON) {
            ShowMenu();
        }

        // HANDLE TOUCH
        else if (type == Click.TOUCH) {
            if ((ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                // We have clicked with left mouse and started manipulation with object
                manipulationStarted = true;
                GameManager.Instance.ActivateGizmoOverlay(true);
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
        return base.SceneInteractable() && !MenuManager.Instance.IsAnyMenuOpened();
    }

    public override void InitActionObject(string id, string type, Vector3 position, Quaternion orientation, string uuid, ActionObjectMetadata actionObjectMetadata) {
        base.InitActionObject(id, type, position, orientation, uuid, actionObjectMetadata);
        Data.Id = id;
        Data.Type = type;
        SetScenePosition(position);
        SetSceneOrientation(orientation);
        Data.Id = uuid;
        ActionObjectMetadata = actionObjectMetadata;
        CreateModel();
        enabled = true;
        SetVisibility(visibility);
    }

    public void CreateModel() {
        if (ActionObjectMetadata.ObjectModel == null || ActionObjectMetadata.ObjectModel.Type == IO.Swagger.Model.ObjectModel.TypeEnum.None) {
            Model = Instantiate(CubePrefab, Visual.transform);
            Visual.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
        } else {
            switch (ActionObjectMetadata.ObjectModel.Type) {
                case IO.Swagger.Model.ObjectModel.TypeEnum.Box:
                    Model = Instantiate(CubePrefab, Visual.transform);
                    Visual.transform.localScale = TransformConvertor.ROSToUnityScale(new Vector3((float) ActionObjectMetadata.ObjectModel.Box.SizeX, (float) ActionObjectMetadata.ObjectModel.Box.SizeY, (float) ActionObjectMetadata.ObjectModel.Box.SizeZ));
                    break;
                case IO.Swagger.Model.ObjectModel.TypeEnum.Cylinder:
                    Model = Instantiate(CylinderPrefab, Visual.transform);
                    Visual.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Cylinder.Radius, (float) ActionObjectMetadata.ObjectModel.Cylinder.Height, (float) ActionObjectMetadata.ObjectModel.Cylinder.Radius);
                    break;
                case IO.Swagger.Model.ObjectModel.TypeEnum.Sphere:
                    Model = Instantiate(SpherePrefab, Visual.transform);
                    Visual.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Sphere.Radius, (float) ActionObjectMetadata.ObjectModel.Sphere.Radius, (float) ActionObjectMetadata.ObjectModel.Sphere.Radius);
                    break;
                default:
                    Model = Instantiate(CubePrefab, Visual.transform);
                    Visual.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
                    break;
            }
        }
        if (IsRobot()) {
            Model.tag = "Robot";
        }
        gameObject.GetComponent<BindParentToChild>().ChildToBind = Model;
        Collider = Model.GetComponent<Collider>();
        Model.GetComponent<OnClickCollider>().Target = gameObject;
        Model.transform.localScale = new Vector3(1, 1, 1);
        modelRenderer = Model.GetComponent<Renderer>();
        outlineOnClick = gameObject.GetComponent<OutlineOnClick>();
        outlineOnClick.InitRenderers(new List<Renderer>() { modelRenderer });
    }


    public override void SetVisibility(float value) {
        base.SetVisibility(value);
        // Set opaque material
        if (value >= 1) {
            transparent = false;
            Material oldMaterial = modelRenderer.material;
            modelRenderer.material = ActionObjectMaterialOpaque;
            // actualize switched materials in OutlineOnClick, otherwise the script would mess up the materials 
            outlineOnClick.SwapMaterials(oldMaterial, modelRenderer.material);
        }
        // Set transparent material
        else {
            if (!transparent) {
                Material oldMaterial = modelRenderer.material;
                modelRenderer.material = ActionObjectMaterialTransparent;
                // actualize switched materials in OutlineOnClick, otherwise the script would mess up the materials 
                outlineOnClick.SwapMaterials(oldMaterial, modelRenderer.material);
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
}
