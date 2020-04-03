using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class AddNewObjectDialog : Dialog
{
    public string ObjectToBeCreated;

    public TMPro.TMP_InputField NameInput;

    
    public static string ToUnderscoreCase(string str) {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
   
    public void Init(string objectToBeCreated) {
        ObjectToBeCreated = objectToBeCreated;
        NameInput.text = ToUnderscoreCase(objectToBeCreated);
        // TODO: find available name
    }

    public async void AddObjectToScene() {
        IO.Swagger.Model.AddObjectToSceneResponse result = await Base.GameManager.Instance.AddObjectToScene(type: ObjectToBeCreated, name: NameInput.text);
        if (result.Result) {
            WindowManager.CloseWindow();
            NameInput.text = "";
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to add object", result.Messages[0]);
        }
    }
}
