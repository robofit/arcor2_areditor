using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AddNewServiceDialog : MonoBehaviour
{
    public string ServiceToBeAdded;

    public GameObject ConfigIdInput;
    public async void AddServiceToScene() {
        IO.Swagger.Model.AddServiceToSceneResponse result = await Base.GameManager.Instance.AddServiceToScene(type: ServiceToBeAdded, configId: ConfigIdInput.GetComponent<TMPro.TMP_InputField>().text);
        if (result.Result) {
            gameObject.GetComponent<ModalWindowManager>().CloseWindow();
            ConfigIdInput.GetComponent<TMPro.TMP_InputField>().text = "";
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add service", result.Messages[0]);
        }
    }
}
