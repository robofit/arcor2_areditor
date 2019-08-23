using UnityEngine;

public class ActionPoint2D : Base.ActionPoint {

    protected override void Awake() {
        base.Awake();
    }    

    void OnMouseDown() {
        offset = gameObject.transform.position -
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
    }

    void OnMouseDrag() {
        Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
    }

    void OnMouseUp() {
        GameManager.Instance.UpdateProject();
    }

    void Touch() {
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu, Data.Id);

    }

    public override Vector3 GetScenePosition() {
        return GameManager.Instance.Scene.transform.TransformPoint(Vector3.Scale(DataHelper.PositionToVector3(Data.Pose.Position), new Vector3(1000, 1000, 1)) -
             new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0));
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Pose.Position = DataHelper.Vector3ToPosition(Vector3.Scale(GameManager.Instance.Scene.transform.InverseTransformPoint(transform.position) +
            new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1)));
    }

    public override Quaternion GetSceneOrientation() {
        return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
    }


}
