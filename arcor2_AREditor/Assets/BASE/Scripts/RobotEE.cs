using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class RobotEE : Base.Clickable {
    private string endEffectorId;
    private string robotId;

    [SerializeField]
    private TMPro.TMP_Text eeName;

    

    public void SetEEName(string robotName, string eeName) {
        this.eeName.text = robotName + "/" + eeName;
        endEffectorId = eeName;
        robotId = robotName;
    }

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.Normal) {
            return;
        }
    }


}
