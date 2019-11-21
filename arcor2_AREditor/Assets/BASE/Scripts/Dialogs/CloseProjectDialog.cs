using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseProjectDialog : MonoBehaviour
{
    public void CloseProject() {
        Base.GameManager.Instance.CloseProject();
        GetComponent<ModalWindowManager>().CloseWindow();
    }
}
