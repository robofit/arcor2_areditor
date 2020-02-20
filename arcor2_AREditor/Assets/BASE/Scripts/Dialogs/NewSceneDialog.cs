using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class NewSceneDialog : Dialog
{
    public TMPro.TMP_InputField NewSceneName;

    public void NewScene() {
        string name = NewSceneName.text;

        if (Base.GameManager.Instance.NewScene(name)) {
            WindowManager.CloseWindow();
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to create new scene", "Scene with name " + name + " already exists");
        }
    }
}
