using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class NewSceneDialog : Dialog
{
    public TMPro.TMP_InputField NewSceneName;

    public async void NewScene() {
        string name = NewSceneName.text;

        if (await Base.GameManager.Instance.NewScene(name)) {
            WindowManager.CloseWindow();
        } 
    }
}
