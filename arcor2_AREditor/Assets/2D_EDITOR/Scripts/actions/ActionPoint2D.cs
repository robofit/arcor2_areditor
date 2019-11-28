using UnityEngine;

public class ActionPoint2D : Base.ActionPoint {

    protected override void Awake() {
        base.Awake();
    }

    public void OnMouseDown() {
        if (Base.GameManager.Instance.SceneInteractable && !MenuManager.Instance.IsAnyMenuOpened()) {
            offset = gameObject.transform.position -
                        Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        } else {
            offset = new Vector3(0, 0, 0);
        }
            
    }

    private void OnMouseDrag() {
        if (!ProjectInteractable())
            return;

        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 transformedPosition = Camera.main.ScreenToWorldPoint(newPosition) + offset;
        transformedPosition.z = transform.position.z; // we only want to update x and y in 2D
        transform.position = transformedPosition;
    }

    private void OnMouseUp() {
        Base.GameManager.Instance.UpdateProject();
    }

    public void Touch() {
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu, Data.Id);
    }

    public override Vector3 GetScenePosition() {
        return Vector3.Scale(DataHelper.PositionToVector3(Data.Pose.Position), new Vector3(1000, 1000, 1));
    }

    public override void SetScenePosition(Vector3 position) {
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

    public override bool ProjectInteractable() {
        return base.ProjectInteractable() && !MenuManager.Instance.IsAnyMenuOpened();
    }

    public override void OnClick() {
        throw new System.NotImplementedException();
    }

}
