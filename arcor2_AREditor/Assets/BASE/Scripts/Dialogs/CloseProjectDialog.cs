using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseProjectDialog : Dialog
{

    public async void CloseProject() {
        bool result = await Base.GameManager.Instance.CloseProject(true);
        if (result) {
            Base.GameManager.Instance.LoadingScreen.SetActive(true);
        }
        WindowManager.CloseWindow();
    }
}
