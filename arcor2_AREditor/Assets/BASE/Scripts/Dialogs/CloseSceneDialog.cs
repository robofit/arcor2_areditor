using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class CloseSceneDialog : MonoBehaviour {
    public void CloseScene() {
        Base.GameManager.Instance.CloseScene();
        GetComponent<ModalWindowManager>().CloseWindow();
    }
}
