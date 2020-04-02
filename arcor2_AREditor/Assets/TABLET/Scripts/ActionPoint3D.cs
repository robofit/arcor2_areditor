using Base;
using RuntimeGizmos;
using UnityEngine;

public class ActionPoint3D : Base.ActionPoint {

    public GameObject Visual;
    
    private bool manipulationStarted = false;
    private TransformGizmo tfGizmo;

    private float interval = 0.1f;
    private float nextUpdate = 0;

    private bool updatePosition = false;

    protected override void Start() {
        base.Start();
        tfGizmo = Camera.main.GetComponent<TransformGizmo>();
    }

    protected override async void Update() {
        if (manipulationStarted) {
            if (tfGizmo.mainTargetRoot != null) {
                if (Time.time >= nextUpdate) {
                    nextUpdate += interval;

                    // check if gameobject with whom is Gizmo manipulating is our Visual gameobject
                    if (GameObject.ReferenceEquals(Visual, tfGizmo.mainTargetRoot.gameObject)) {
                        // if Gizmo is moving, we can send UpdateProject to server
                        if (tfGizmo.isTransforming) {
                            updatePosition = true;
                        } else if (updatePosition) {
                            updatePosition = false;
                            //GameManager.Instance.UpdateProject();
                            await GameManager.Instance.UpdateActionPointPosition(this, Data.Position);

                        }
                    }
                }
            } else {
                if (updatePosition) {
                    updatePosition = false;
                    await GameManager.Instance.UpdateActionPointPosition(this, Data.Position);
                }
                manipulationStarted = false;
                GameManager.Instance.ActivateGizmoOverlay(false);
            }
        }
        //TODO shouldn't this be called first?
        base.Update();
    }

    private void LateUpdate() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        // set rotation to the WorldAnchor and ignore its parent rotation
        if (CalibrationManager.Instance.WorldAnchor != null) {
            transform.rotation = CalibrationManager.Instance.WorldAnchor.transform.rotation;
        }
#else
        transform.rotation = Quaternion.identity;
#endif
    }

    public override void OnClick(Click type) {
        // HANDLE MOUSE
        if (type == Click.MOUSE_LEFT_BUTTON) {
            StartManipulation();
        } else if (type == Click.MOUSE_RIGHT_BUTTON) {
            ShowMenu();
        }

        // HANDLE TOUCH
        else if (type == Click.TOUCH) {
            if (ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate) {
                StartManipulation();
            } else {
                ShowMenu();
            }
        }
    }

    public void StartManipulation() {
        if (Locked) {
            Notifications.Instance.ShowNotification("Locked", "This action point is locked and can't be manipulated");
        } else {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
            GameManager.Instance.ActivateGizmoOverlay(true);
        }
    }

    public override Vector3 GetScenePosition() {
        return TransformConvertor.ROSToUnity(DataHelper.PositionToVector3(Data.Position));
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Position = DataHelper.Vector3ToPosition(TransformConvertor.UnityToROS(position));
    }

    public override Quaternion GetSceneOrientation() {
        //return TransformConvertor.ROSToUnity(DataHelper.OrientationToQuaternion(Data.Orientations[0].Orientation));
        return new Quaternion();
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        //Data.Orientations.Add(new IO.Swagger.Model.NamedOrientation(id: "default", orientation:DataHelper.QuaternionToOrientation(TransformConvertor.UnityToROS(orientation))));
    }

    public override void UpdatePositionsOfPucks() {
        int i = 1;
        foreach (Action3D action in Actions.Values) {
            action.transform.localPosition = new Vector3(0, i * 0.1f, 0);
            ++i;
        }
    }
    
    public override bool ProjectInteractable() {
        return base.ProjectInteractable() && !MenuManager.Instance.IsAnyMenuOpened();
    }

    public override void ActivateForGizmo(string layer) {
        if (!Locked) {
            base.ActivateForGizmo(layer);
            Visual.layer = LayerMask.NameToLayer(layer);
        }
    }

}
