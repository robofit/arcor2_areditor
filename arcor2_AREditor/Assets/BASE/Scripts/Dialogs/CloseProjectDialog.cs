using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseProjectDialog : Dialog
{

    public void CloseProject() {
        Base.GameManager.Instance.CloseProject(true);
        WindowManager.CloseWindow();
    }
}
