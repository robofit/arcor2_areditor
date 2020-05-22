using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseProjectDialog : Dialog
{

    public async void CloseProject() {
        (bool success, _) = await Base.GameManager.Instance.CloseProject(true);
        if (success) {
            Base.GameManager.Instance.LoadingScreen.SetActive(true);
        }
        WindowManager.CloseWindow();
    }
}
