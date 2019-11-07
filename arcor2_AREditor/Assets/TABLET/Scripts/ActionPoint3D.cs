using UnityEngine;

public class ActionPoint3D : Base.ActionPoint {

    protected override void Awake() {
        base.Awake();
    }    

    void OnMouseDown() {
        if (Base.GameManager.Instance.SceneInteractable && !MenuManager.Instance.IsAnyMenuOpened()) {
            offset = gameObject.transform.position -
                        Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
        } else {
            offset = new Vector3(0, 0, 0);
        }
            
    }

    void OnMouseDrag() {
        
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
    }

    void OnMouseUp() {
        Base.GameManager.Instance.UpdateProject();
    }

    void Touch() {
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu, Data.Id);

    }

    public override Vector3 GetScenePosition() {
        /*return Base.GameManager.Instance.Scene.transform.TransformPoint(Vector3.Scale(DataHelper.PositionToVector3(Data.Pose.Position), new Vector3(1000, 1000, 1)) -
             new Vector3(Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0));*/
        return Vector3.Scale(DataHelper.PositionToVector3(Data.Pose.Position), new Vector3(1000, 1000, 1));
    }

    public override void SetScenePosition(Vector3 position) {
        /* Data.Pose.Position = DataHelper.Vector3ToPosition(Vector3.Scale(Base.GameManager.Instance.Scene.transform.InverseTransformPoint(transform.position) +
             new Vector3(Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, Base.GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1)));*/
        Data.Pose.Position = DataHelper.Vector3ToPosition(Vector3.Scale(position, new Vector3(0.001f, 0.001f, 1)));
    }

    public override Quaternion GetSceneOrientation() {
        return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
    }

    public override void SetScenePosition(IO.Swagger.Model.Position position) {
        Data.Pose.Position = position;
    }
}
