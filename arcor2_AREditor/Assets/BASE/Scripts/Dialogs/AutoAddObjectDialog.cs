using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AutoAddObjectDialog : Dialog
{
    public string ObjectToBeAdded;

    public async void AutoAddObjectToScene() {
        IO.Swagger.Model.AutoAddObjectToSceneResponse result = await Base.GameManager.Instance.AutoAddObjectToScene(type: ObjectToBeAdded);
        if (result.Result) {
            WindowManager.CloseWindow();
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", result.Messages[0]);
        }
    }
}
