using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreen : MonoBehaviour
{

    public TMPro.TMP_Text ScenesBtn, ProjectsBtn;
    public void SwitchToProjects() {
        ScenesBtn.color = new Color(0.687f, 0.687f, 0.687f);
        ProjectsBtn.color = new Color(0, 0, 0);
    }

    public void SwitchToScenes() {
        ScenesBtn.color = new Color(0, 0, 0);
        ProjectsBtn.color = new Color(0.687f, 0.687f, 0.687f);
        
    }
}
