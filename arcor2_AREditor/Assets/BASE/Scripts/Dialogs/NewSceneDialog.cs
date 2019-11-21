using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class NewSceneDialog : MonoBehaviour
{
    public GameObject NewSceneName;

    public void NewScene() {
        string name = NewSceneName.GetComponent<TMPro.TMP_InputField>().text;

        if (Base.GameManager.Instance.NewScene(name)) {
            GetComponent<ModalWindowManager>().CloseWindow();
        } else {
            Base.NotificationsModernUI.Instance.ShowNotification("Failed to create new scene", "Scene with name " + name + " already exists");
        }
    }
}
