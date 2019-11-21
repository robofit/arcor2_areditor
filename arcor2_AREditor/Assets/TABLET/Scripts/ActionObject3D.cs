using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionObject3D : Base.ActionObject
{
    public GameObject ActionObjectName;
    private GameObject ActionObjectMenu, ActionObjectMenuProjectEditor;
    protected override void Start() {
        base.Start();
        transform.localScale = new Vector3(1f, 1f, 1f);
        UpdateId(Data.Id);
        ActionObjectMenu = MenuManager.Instance.InteractiveObjectMenu;
        ActionObjectMenuProjectEditor = MenuManager.Instance.ActionObjectMenuProjectEditor;
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

    public override void OnClick() {
        if (Base.GameManager.Instance.GameState == Base.GameManager.GameStateEnum.SceneEditor) {
            ActionObjectMenu.GetComponent<ActionObjectMenu>().CurrentObject = gameObject;
            ActionObjectMenu.GetComponent<ActionObjectMenu>().UpdateMenu();
            MenuManager.Instance.ShowMenu(ActionObjectMenu, Data.Id);
        } else if (Base.GameManager.Instance.GameState == Base.GameManager.GameStateEnum.ProjectEditor) {
            ActionObjectMenuProjectEditor.GetComponent<ActionObjectMenuProjectEditor>().CurrentObject = gameObject;
        }
    }

    public override void UpdateId(string newId) {
        base.UpdateId(newId);
        ActionObjectName.GetComponent<TextMeshPro>().text = newId;
    }

    public override void OnMouseDown() {
        Debug.Log("onMouseDown");
        OnClick();
    }
}
