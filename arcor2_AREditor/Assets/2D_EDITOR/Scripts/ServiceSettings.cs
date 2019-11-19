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
            IO.Swagger.Model.SceneService sceneService;
            sceneService = Base.ServiceManager.Instance.GetService(type);
            ServiceName.GetComponent<TMPro.TMP_Text>().text = "Service name: " + sceneService.Type;
            ConfigID.GetComponent<TMPro.TMP_Text>().text = "Configuration ID: " + sceneService.ConfigurationId; // only first one, for now
            
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
