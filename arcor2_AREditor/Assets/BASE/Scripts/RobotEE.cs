using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using RosSharp.Urdf;
using UnityEngine;

public class RobotEE : InteractiveObject {
    
    [SerializeField]
    private TMPro.TMP_Text eeName;

    public string RobotId, EEId;
    

    public void InitEE(IRobot robot, string eeId) {
        RobotId = robot.GetId();
        EEId = eeId;
        SetLabel(robot.GetName(), eeId);
    }

    public void SetLabel(string robotName, string eeName) {
        this.eeName.text = robotName + "/" + eeName;
    }

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
    }

    public override void OnHoverStart() {
        eeName.gameObject.SetActive(true);
    }

    public override void OnHoverEnd() {
        eeName.gameObject.SetActive(false);
    }

    /// <summary>
    /// Takes world space pose of end effector, converts them to SceneOrigin frame and apply to RobotEE
    /// </summary>
    /// <param name="position">Position in world frame</param>
    /// <param name="orientation">Orientation in world frame</param>
    public void UpdatePosition(Vector3 position, Quaternion orientation) {
        transform.position = SceneManager.Instance.SceneOrigin.transform.TransformPoint(position);
        // rotation set according to this
        // https://answers.unity.com/questions/275565/what-is-the-rotation-equivalent-of-inversetransfor.html
        transform.rotation = GameManager.Instance.Scene.transform.rotation * orientation;
    }

    public override string GetName() {
        return EEId;
    }

    public override string GetId() {
        return EEId;
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public async override Task<RequestResult> Movable() {
        return new RequestResult(false, "Robot EE could not be moved at the moment (will be added in next release)");
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }

    public async override Task<RequestResult> Removable() {
        return new RequestResult(false, "Robot EE could not be removed");
    }

    public override void Remove() {
        throw new System.NotImplementedException();
    }

    public override Task Rename(string name) {
        throw new System.NotImplementedException();
    }

    public override string GetObjectTypeName() {
        return "End effector";
    }

    public override void UpdateColor() {
        //nothing to do here
    }
}
