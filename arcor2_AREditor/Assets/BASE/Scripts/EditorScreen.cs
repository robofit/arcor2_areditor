using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Base;
using TMPro.EditorUtilities;

[RequireComponent(typeof(CanvasGroup))]
public class EditorScreen : MonoBehaviour {
       
    private CanvasGroup CanvasGroup;

    [SerializeField]
    private TMPro.TMP_Text editorInfo;

    private void Start() {
        CanvasGroup = GetComponent<CanvasGroup>();
        Base.GameManager.Instance.OnOpenProjectEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnRunPackage += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenSceneEditor += ShowEditorWindow;
        Base.GameManager.Instance.OnOpenMainScreen += HideEditorWindow;
        Base.GameManager.Instance.OnDisconnectedFromServer += HideEditorWindow;
        Base.GameManager.Instance.OnRunPackage += OnRunPackage;
        Base.GameManager.Instance.OnResumePackage += OnRunPackage;
        Base.GameManager.Instance.OnPausePackage += OnPausePackage;
    }

    private void OnPausePackage(object sender, ProjectMetaEventArgs args) {
        editorInfo.text = "Paused: " + args.Name;
    }

    private void OnRunPackage(object sender, ProjectMetaEventArgs args) {
        editorInfo.text = "Running: " + args.Name;
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
