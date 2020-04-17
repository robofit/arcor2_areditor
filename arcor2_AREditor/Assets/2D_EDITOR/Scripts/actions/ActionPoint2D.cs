using Base;
using UnityEngine;

public class ActionPoint2D : Base.ActionPoint {

   
    public void OnMouseDown() {
        if (Base.GameManager.Instance.SceneInteractable) {
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

    private async void OnMouseUp() {
        await Base.GameManager.Instance.UpdateActionPointPosition(this, Data.Position);
    }


    public override void OnClick(Click type) {
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu);
    }

    public override Vector3 GetScenePosition() {
        return Vector3.Scale(DataHelper.PositionToVector3(Data.Position), new Vector3(1000, 1000, 1));
    }

    public override void SetScenePosition(Vector3 position) {
        Data.Position = DataHelper.Vector3ToPosition(Vector3.Scale(position, new Vector3(0.001f, 0.001f, 1)));
    }

    public override Quaternion GetSceneOrientation() {
        //return DataHelper.OrientationToQuaternion(Data.Pose.Orientation);
        return new Quaternion();
    }

    public override void SetSceneOrientation(Quaternion orientation) {
        //Data.Pose.Orientation = DataHelper.QuaternionToOrientation(orientation);
    }

    public override bool ProjectInteractable() {
        return base.ProjectInteractable() && !MenuManager.Instance.IsAnyMenuOpened();
    }

    public override void UpdatePositionsOfPucks() {
        int i = 0;
        foreach (Puck2D action in Actions.Values) {
            action.transform.localPosition = new Vector3(0, i * 60, 0);
            ++i;
        }
    }

    public override void SetSize(float size) {
        throw new System.NotImplementedException();
    }
}
