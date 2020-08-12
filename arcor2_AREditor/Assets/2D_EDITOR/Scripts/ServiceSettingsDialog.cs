using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
/*
public class ServiceSettingsDialog : Dialog
{
    private string type;
    public TMPro.TMP_Text ServiceName, ConfigID; // set in inspector
    public Button RemoveButton;

    public string Type {
        get => type;
        set {
            type = value;
            Base.Service sceneService;
            sceneService = Base.SceneManager.Instance.GetService(type);
            ServiceName.text = "Service name: " + sceneService.Data.Type;
            ConfigID.text = "Configuration ID: " + sceneService.Data.ConfigurationId; // only first one, for now
            
         }
    }

    public void Show(string type, bool showRemove) {
        Type = type;
        if (showRemove) {
            RemoveButton.gameObject.SetActive(true);
        } else {
            RemoveButton.gameObject.SetActive(false);
        }
        Open();
    }

    public async void RemoveService() {
        IO.Swagger.Model.RemoveFromSceneResponse response = await Base.GameManager.Instance.RemoveFromScene(type);
        if (!response.Result) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to remove service " + type, response.Messages[0]);
        } else {
            Close();
        }
    }

    public override void Confirm() {
        if (RemoveButton.IsActive()) {
            RemoveService();
        } else {
            Close();
        }
    }
}
*/
