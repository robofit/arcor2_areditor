using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Base;
using RuntimeGizmos;

public class ActionObject3D : ActionObject
{
    public GameObject ActionObjectName;
    private ActionObjectMenu actionObjectMenu;
    private ActionObjectMenuProjectEditor actionObjectMenuProjectEditor;

    public GameObject Visual;

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
    }


    private void Update() {
        if (manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null) {
                if (Time.time >= nextUpdate) {
                    nextUpdate += interval;

                    // check if gameobject with whom is Gizmo manipulating is our Visual gameobject
                    if (GameObject.ReferenceEquals(Visual, tfGizmo.mainTargetRoot.gameObject)) {
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
        Data.Pose.Position = DataHelper.Vector3ToPosition(new Vector3(transform.position.x, transform.position.z, transform.position.y));
    }

    public override void OnClick(Click type) {
        if (type == Click.MOUSE_LEFT_BUTTON) {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
        }
        if (type == Click.MOUSE_RIGHT_BUTTON) {
            if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.SceneEditor) {
                actionObjectMenu.CurrentObject = this;
                actionObjectMenu.UpdateMenu();
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuSceneEditor);
            } else if (Base.GameManager.Instance.GetGameState() == Base.GameManager.GameStateEnum.ProjectEditor) {
                actionObjectMenuProjectEditor.CurrentObject = this;
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionObjectMenuProjectEditor);
            }
        }
    }

    public override void UpdateId(string newId) {
        base.UpdateId(newId);
        ActionObjectName.GetComponent<TextMeshPro>().text = newId;
    }


    public override bool SceneInteractable() {
        return base.SceneInteractable() && !MenuManager.Instance.IsAnyMenuOpened();
    }

}
