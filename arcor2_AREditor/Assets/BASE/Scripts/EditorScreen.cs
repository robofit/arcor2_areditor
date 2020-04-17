using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class EditorScreen : MonoBehaviour {
       
    private CanvasGroup CanvasGroup;

    private void Start() {
        CanvasGroup = GetComponent<CanvasGroup>();
        Base.GameManager.Instance.OnOpenProjectEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenSceneEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenMainScreen += HideEditorWindow;
        Base.GameManager.Instance.OnDisconnectedFromServer += HideEditorWindow;
    }


    private void ShowEditorWindow(object sender, EventArgs args) {
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }

    private void HideEditorWindow(object sender, EventArgs args) {
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }
}
