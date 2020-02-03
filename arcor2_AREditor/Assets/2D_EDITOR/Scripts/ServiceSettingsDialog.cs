using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class ServiceSettingsDialog : Dialog
{
    private string type;
    public TMPro.TMP_Text ServiceName, ConfigID; // set in inspector

    public string Type {
        get => type;
        set {
            type = value;
            Base.Service sceneService;
            sceneService = Base.ActionsManager.Instance.GetService(type);
            ServiceName.text = "Service name: " + sceneService.Data.Type;
            ConfigID.text = "Configuration ID: " + sceneService.Data.ConfigurationId; // only first one, for now
            
         }
    }

    public async void RemoveService() {
        IO.Swagger.Model.RemoveFromSceneResponse response = await Base.GameManager.Instance.RemoveFromScene(type);
        if (!response.Result) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to remove service " + type, response.Messages[0]);
        } else {
            WindowManager.CloseWindow();
        }
    }
}
