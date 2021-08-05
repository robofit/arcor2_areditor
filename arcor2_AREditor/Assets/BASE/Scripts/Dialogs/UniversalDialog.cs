using System;
using UnityEngine;
using UnityEngine.Events;

public class UniversalDialog : Dialog
{
    [SerializeField]
    private TMPro.TMP_Text OKButtonLabelNormal, OKButtonLabelHighlighted, CancelButtonLabelNormal, CancelButtonLabelHighlighted;

    [SerializeField]
    protected ButtonWithTooltip okBtn, cancelBtn;

    protected UnityAction confirmCallback;

    public void SetConfirmLabel(string name) {
        OKButtonLabelNormal.text = name;
        OKButtonLabelHighlighted.text = name;
    }

    public void SetCancelLabel(string name) {
        CancelButtonLabelNormal.text = name;
        CancelButtonLabelHighlighted.text = name;
    }

    public void AddConfirmCallback(UnityAction callback) {
        windowManager.onConfirm.AddListener(callback);
        confirmCallback = callback;
    }

    public void AddCancelCallback(UnityAction callback) {
        if (callback != null)
            windowManager.onCancel.AddListener(callback);
    }

    public void SetDescription(string description) {
        if (string.IsNullOrEmpty(description)) {
            windowManager.windowDescription.gameObject.SetActive(false);
        } else {
            windowManager.windowDescription.gameObject.SetActive(true);
            windowManager.windowDescription.text = description;
        }        
    }

    public void SetTitle(string title) {
        windowManager.windowTitle.text = title;
    }

    public virtual void Open(string title, string description, UnityAction confirmationCallback, UnityAction cancelCallback, string confirmLabel = "Confirm", string cancelLabel = "Cancel", bool wideButtons = false) {
        windowManager.onConfirm.RemoveAllListeners();
        windowManager.onCancel.RemoveAllListeners();
        SetTitle(title);
        SetDescription(description);
        AddConfirmCallback(confirmationCallback);
        AddCancelCallback(cancelCallback);
        SetConfirmLabel(confirmLabel);
        SetCancelLabel(cancelLabel);
        Open();
        RectTransform okBtnTrans = okBtn.GetComponent<RectTransform>();
        RectTransform cancelBtnTrans = cancelBtn.GetComponent<RectTransform>();
        if (wideButtons) {
            okBtnTrans.sizeDelta = new Vector2(400, okBtnTrans.rect.height);
            cancelBtnTrans.sizeDelta = new Vector2(400, cancelBtnTrans.rect.height);
        } else {
            okBtnTrans.sizeDelta = new Vector2(250, okBtnTrans.rect.height);
            cancelBtnTrans.sizeDelta = new Vector2(250, cancelBtnTrans.rect.height);
        }
    }

    public override void Confirm() {
        confirmCallback();
    }
}
