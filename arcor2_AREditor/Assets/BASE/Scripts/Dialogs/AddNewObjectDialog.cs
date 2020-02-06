using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class AddNewObjectDialog : Dialog
{
    public string ObjectToBeCreated;

    public TMPro.TMP_InputField NameInput;

   
    public void Init(string objectToBeCreated) {
        ObjectToBeCreated = objectToBeCreated;
        NameInput.text = objectToBeCreated;
        // TODO: find available name
    }

    public async void AddObjectToScene() {
        IO.Swagger.Model.AddObjectToSceneResponse result = await Base.GameManager.Instance.AddObjectToScene(type: ObjectToBeCreated, id: NameInput.text);
        if (result.Result) {
            WindowManager.CloseWindow();
            NameInput.text = "";
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", result.Messages[0]);
        }
    }
}
