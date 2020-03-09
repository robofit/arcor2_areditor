using Base;
using RuntimeGizmos;
using UnityEngine;

public class ActionPoint3D : Base.ActionPoint {

    public GameObject Visual;
    
    private bool manipulationStarted = false;
    private TransformGizmo tfGizmo;

    private float interval = 0.1f;
    private float nextUpdate = 0;

    private bool updateProject = false;

    private void Start() {
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
                            updateProject = true;
                        } else if (updateProject) {
                            updateProject = false;
                            GameManager.Instance.UpdateProject();
                        }
                    }
                }
            } else {
                if (updateProject) {
                    updateProject = false;
                    GameManager.Instance.UpdateProject();
                }
                manipulationStarted = false;
                GameManager.Instance.ActivateGizmoOverlay(false);
            }
        }

        base.Update();
    }

    public override void OnClick(Click type) {
        // HANDLE MOUSE
        if (type == Click.MOUSE_LEFT_BUTTON) {
            // We have clicked with left mouse and started manipulation with object
            manipulationStarted = true;
            GameManager.Instance.ActivateGizmoOverlay(true);
        } else if (type == Click.MOUSE_RIGHT_BUTTON) {
            MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
            MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
            MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu);
        }

        // HANDLE TOUCH
        else if (type == Click.TOUCH) {
            if ((ControlBoxManager.Instance.UseGizmoMove || ControlBoxManager.Instance.UseGizmoRotate)) {
                // We have touched and started manipulation with object
                manipulationStarted = true;
                GameManager.Instance.ActivateGizmoOverlay(true);
            } else {
                MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
                MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
                MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu);
            }
        }
    }

    public override Vector3 GetScenePosition() {
        /*return Base.GameManager.Instance.Scene.transform.TransformPoint(Vector3.Scale(DataHelper.PositionToVector3(Data.Pose.Position), new Vector3(1000, 1000, 1)) -
             new Vector3(Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0));*/
        Vector3 v = DataHelper.PositionToVector3(Data.Position);
        return new Vector3(v.x, v.z, v.y);
    }

    public override void SetScenePosition(Vector3 position) {
        /* Data.Pose.Position = DataHelper.Vector3ToPosition(Vector3.Scale(Base.GameManager.Instance.Scene.transform.InverseTransformPoint(transform.position) +
             new Vector3(Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1)));*/
        Data.Position = DataHelper.Vector3ToPosition(new Vector3(position.x, position.z, position.y));
    }

    public override Quaternion GetSceneOrientation() {
        //return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
        return new Quaternion();
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        //Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
    }

    public override void SetScenePosition(IO.Swagger.Model.Position position) {
        Data.Position = position;
        //Data.Pose.Position = DataHelper.Vector3ToPosition(new Vector3(transform.position.x, transform.position.z, transform.position.y));
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

}
