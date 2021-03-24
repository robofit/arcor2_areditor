using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using static Base.GameManager;

[RequireComponent(typeof(Button))]
public class DisableButtonWhenRequestingObject : MonoBehaviour
{
    private Button button;
    private void Awake() {
        button = GetComponent<Button>();
    }
    private void Start() {
        GameManager.Instance.OnEditorStateChanged += OnEditorStateChanged;
    }

    private void OnEditorStateChanged(object sender, EditorStateEventArgs args) {
        switch (args.Data) {
            case GameManager.EditorStateEnum.Normal:
            case GameManager.EditorStateEnum.InteractionDisabled:
            case GameManager.EditorStateEnum.Closed:
                button.interactable = true;
                break;
            case EditorStateEnum.SelectingAction:
            case EditorStateEnum.SelectingActionInput:
            case EditorStateEnum.SelectingActionObject:
            case EditorStateEnum.SelectingActionOutput:
            case EditorStateEnum.SelectingActionPoint:
            case EditorStateEnum.SelectingActionPointParent:
                button.interactable = false;
                break;
        }
    }
}
