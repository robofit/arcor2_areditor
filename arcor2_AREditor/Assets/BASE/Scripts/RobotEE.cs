using System.Collections;
using System.Collections.Generic;
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
        // nothing to do yet
    }


}
