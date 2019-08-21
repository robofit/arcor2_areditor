using UnityEngine;

public class ActionPoint2D : Base.ActionPoint {

    private void Awake() {


    }

    void Update() {

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
        Data.Pose = DataHelper.CreatePose(GetScenePosition(), transform.rotation);
        GameManager.Instance.UpdateProject();
    }

    void Touch() {
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().CurrentActionPoint = this;
        MenuManager.Instance.ActionPointMenu.GetComponent<ActionPointMenu>().UpdateMenu();
        MenuManager.Instance.ShowMenu(MenuManager.Instance.ActionPointMenu, Data.Id);

    }

    public Vector3 GetScenePosition() {
        Vector3 position = Vector3.Scale(GameManager.Instance.Scene.transform.InverseTransformPoint(transform.position) +
            new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0), new Vector3(0.001f, 0.001f, 1));
        position.z = 0.7f;
        return position;
    }

    public void SetScenePosition(Vector3 position) => transform.position = GameManager.Instance.Scene.transform.TransformPoint(Vector3.Scale(position, new Vector3(1000f, 1000f, 1)) -
        new Vector3(GameManager.Instance.Scene.GetComponent<RectTransform>().rect.width / 2, GameManager.Instance.Scene.GetComponent<RectTransform>().rect.height / 2, 0));



}
