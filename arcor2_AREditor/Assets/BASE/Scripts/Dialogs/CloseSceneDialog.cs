using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseSceneDialog : Dialog {

    

    public async void CloseScene() {
        _ = await Base.GameManager.Instance.CloseScene(true);
        WindowManager.CloseWindow();
    }
}
