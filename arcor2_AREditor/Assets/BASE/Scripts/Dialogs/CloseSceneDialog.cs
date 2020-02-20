using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseSceneDialog : Dialog {

    

    public void CloseScene() {
        Base.GameManager.Instance.CloseScene();
        WindowManager.CloseWindow();
    }
}
