using System.Collections;
using System.Collections.Generic;
using Base;
using RosSharp.Urdf;
using UnityEngine;

public class RobotEE : Base.Clickable {
    
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
        
    }

    public override void OnHoverEnd() {
        
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
        transform.rotation = SceneManager.Instance.SceneOrigin.transform.rotation * orientation;
    }
}
