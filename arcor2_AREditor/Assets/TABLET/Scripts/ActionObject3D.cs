using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;

public class ActionObject3D : ActionObject
{
    public TextMeshPro ActionObjectName;
    private ActionObjectMenu actionObjectMenu;
    private ActionObjectMenuProjectEditor actionObjectMenuProjectEditor;

    public GameObject Visual, Model;

    public GameObject CubePrefab, CylinderPrefab, SpherePrefab;

    private bool manipulationStarted = false;
    private TransformGizmo tfGizmo;

    private float interval = 0.1f;
    private float nextUpdate = 0;

    private bool updateScene = false;


    protected override void Start() {
        base.Start();
        transform.localScale = new Vector3(1f, 1f, 1f);
        UpdateId(Data.Id);
        actionObjectMenu = MenuManager.Instance.ActionObjectMenuSceneEditor.gameObject.GetComponent<ActionObjectMenu>();
        actionObjectMenuProjectEditor = MenuManager.Instance.ActionObjectMenuProjectEditor.gameObject.GetComponent<ActionObjectMenuProjectEditor>();
       
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
        // disable update method until model is ready
        //enabled = false;
    }


    private void Update() {
        if (manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null) {
                if (Time.time >= nextUpdate) {
                    nextUpdate += interval;

                    // check if gameobject with whom is Gizmo manipulating is our Model gameobject
                    if (GameObject.ReferenceEquals(Model, tfGizmo.mainTargetRoot.gameObject)) {
                        // if Gizmo is moving, we can send UpdateProject to server
                        if (tfGizmo.isTransforming) {
                            updateScene = true;
                        } else if (updateScene) {
                            updateScene = false;
                            GameManager.Instance.UpdateScene();
                        }
                    }
                }
            } else {
                if (updateScene) {
                    updateScene = false;
                    GameManager.Instance.UpdateScene();
                }
                manipulationStarted = false;
                GameManager.Instance.ActivateGizmoOverlay(false);
            }
        }

        base.Update();
    }


    public override Quaternion GetSceneOrientation() {
        return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
    }

    public override Vector3 GetScenePosition() {
        Vector3 v = DataHelper.PositionToVector3(Data.Pose.Position);
        return new Vector3(v.x, v.z, v.y); //swapped y and z!!
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Pose.Position = DataHelper.Vector3ToPosition(new Vector3(position.x, position.z, position.y));
    }

    public override void OnClick(Click type) {
        // HANDLE MOUSE
        if (type == Click.MOUSE_LEFT_BUTTON) {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
            GameManager.Instance.ActivateGizmoOverlay(true);
        }
        else if (type == Click.MOUSE_RIGHT_BUTTON) {
            if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
                actionObjectMenu.CurrentObject = this;
                actionObjectMenu.UpdateMenu();
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
            } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
                actionObjectMenuProjectEditor.CurrentObject = this;
                actionObjectMenuProjectEditor.UpdateMenu();
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
            }
        }

        // HANDLE TOUCH
        else if (type == Click.TOUCH) {
            if ((ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                // We have clicked with left mouse and started manipulation with object
                manipulationStarted = true;
                GameManager.Instance.ActivateGizmoOverlay(true);
            }
            else {
                if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
                    actionObjectMenu.CurrentObject = this;
                    actionObjectMenu.UpdateMenu();
                    MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
                } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
                    actionObjectMenuProjectEditor.CurrentObject = this;
                    actionObjectMenuProjectEditor.UpdateMenu();
                    MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
                }
            }
        }
    }

    public override void UpdateId(string newId, bool updateScene = true) {
        base.UpdateId(newId);
        ActionObjectName.text = newId;
    }

    public override void ActionObjectUpdate(IO.Swagger.Model.SceneObject actionObjectSwagger, bool visibility, bool interactivity) {
        Debug.Assert(Model != null);
        base.ActionObjectUpdate(actionObjectSwagger, visibility, interactivity);
        ActionObjectName.text = actionObjectSwagger.Id;

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
        Data.Uuid = uuid;
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
                    Visual.transform.localScale = new Vector3((float) ActionObjectMetadata.ObjectModel.Box.SizeX, (float) ActionObjectMetadata.ObjectModel.Box.SizeY, (float) ActionObjectMetadata.ObjectModel.Box.SizeZ);
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
        Model.GetComponent<OnClickCollider>().Target = gameObject;
        Model.transform.localScale = new Vector3(1, 1, 1);
        gameObject.GetComponent<OutlineOnClick>().InitRenderers(new List<Renderer>() { Model.GetComponent<Renderer>() });
    }


    public override void SetVisibility(float value) {
        base.SetVisibility(value);
        Model.GetComponent<Renderer>().material.color = new Color(Model.GetComponent<Renderer>().material.color.r,
                                                                  Model.GetComponent<Renderer>().material.color.g,
                                                                  Model.GetComponent<Renderer>().material.color.b,
                                                                  value);
        
        
    }

    public override void Show() {
        Debug.Assert(Model != null);
        Model.GetComponent<Renderer>().enabled = true;
    }

    public override void Hide() {
        Debug.Assert(Model != null);
        Model.GetComponent<Renderer>().enabled = false;
    }

    public override void SetInteractivity(bool interactivity) {
        Debug.Assert(Model != null);
        Model.GetComponent<Collider>().enabled = interactivity;
    }
}
