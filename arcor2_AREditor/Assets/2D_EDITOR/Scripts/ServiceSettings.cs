using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class ServiceSettings : MonoBehaviour
{
    private string type;
    public GameObject ServiceName, ConfigID; // set in inspector

    public string Type {
        get => type;
        set {
            type = value;
            Base.Service sceneService;
            sceneService = Base.ActionsManager.Instance.GetService(type);
            ServiceName.GetComponent<TMPro.TMP_Text>().text = "Service name: " + sceneService.Data.Type;
            ConfigID.GetComponent<TMPro.TMP_Text>().text = "Configuration ID: " + sceneService.Data.ConfigurationId; // only first one, for now
            
         }
    }

    public async void RemoveService() {
        IO.Swagger.Model.RemoveFromSceneResponse response = await Base.GameManager.Instance.RemoveFromScene(type);
        if (!response.Result) {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to remove service " + type, response.Messages[0]);
        } else {
            gameObject.GetComponent<ModalWindowManager>().CloseWindow();
        }
    }
}
