using System;
using UnityEngine;
using UnityEngine.Events;

public class UniversalDialog : Dialog
{
    [SerializeField]
    private TMPro.TMP_Text OKButtonLabelNormal, OKButtonLabelHighlighted, CancelButtonLabelNormal, CancelButtonLabelHighlighted;

    public void SetConfirmLabel(string name) {
        OKButtonLabelNormal.text = name;
        OKButtonLabelHighlighted.text = name;
    }

    public void SetCancelLabel(string name) {
        CancelButtonLabelNormal.text = name;
        CancelButtonLabelHighlighted.text = name;
    }

    public void AddConfirmCallback(UnityAction callback) {
        WindowManager.onConfirm.AddListener(callback);
    }

    public void AddCancelCallback(UnityAction callback) {
        WindowManager.onCancel.AddListener(callback);
    }

    public void SetDescription(string description) {
        if (string.IsNullOrEmpty(description)) {
            WindowManager.windowDescription.gameObject.SetActive(false);
        } else {
            WindowManager.windowDescription.gameObject.SetActive(true);
            WindowManager.windowDescription.text = description;
        }        
    }

    public void SetTitle(string title) {
        WindowManager.windowTitle.text = title;
    }

    public virtual void Open(string title, string description, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel") {
        WindowManager.onConfirm.RemoveAllListeners();
        WindowManager.onCancel.RemoveAllListeners();
        SetTitle(title);
        SetDescription(description);
        AddConfirmCallback(confirmationCallback);
        AddCancelCallback(cancelCallback);
        SetConfirmLabel(confirmLabel);
        SetCancelLabel(cancelLabel);
        WindowManager.OpenWindow();
    }

    public void Close() {
        WindowManager.CloseWindow();
    }
}
