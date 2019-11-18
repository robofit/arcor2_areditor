using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AddNewObjectDialog : MonoBehaviour
{
    public string ObjectToBeCreated;

    public GameObject NameInput;
   
    public async void AddObjectToScene() {
        IO.Swagger.Model.AddObjectToSceneResponse result = await Base.GameManager.Instance.AddObjectToScene(type: ObjectToBeCreated, id: NameInput.GetComponent<TMPro.TMP_InputField>().text);
        if (result.Result) {
            gameObject.GetComponent<ModalWindowManager>().CloseWindow();
            NameInput.GetComponent<TMPro.TMP_InputField>().text = "";
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", result.Messages[0]);
        }
    }
}
