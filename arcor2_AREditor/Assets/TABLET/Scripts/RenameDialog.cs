using Arcor2.ClientSdk.Communication;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RenameDialog : Dialog {
    //public GameObject CanvasRoot;
    public TMP_Text Title;

    [SerializeField]
    private LabeledInput nameInput;
    private GameObject overlay;

    private InteractiveObject selectedObject;

    private UnityAction _updateVisibilityCallback;
    private bool isNewObject;
    public ButtonWithTooltip CloseBtn, ConfirmButton;
    private UnityAction confirmCallback;
    private bool keepObjectLocked;

    public async void Init(InteractiveObject objectToRename, UnityAction updateVisibilityCallback, bool isNewObject = false, UnityAction cancelCallback = null, UnityAction confirmCallback = null, bool keepObjectLocked = false) {
        if (objectToRename == null)
            return;
        if (!await objectToRename.WriteLock(false))
            return;

        this.isNewObject = isNewObject;
        _updateVisibilityCallback = updateVisibilityCallback;
        selectedObject = objectToRename;


        Title.text = "Rename " + selectedObject.GetObjectTypeName();
        this.keepObjectLocked = keepObjectLocked;

        nameInput.SetValue(objectToRename.GetName());
        nameInput.SetLabel("Name", "New name");
        nameInput.SetType("string");
        CloseBtn.Button.onClick.RemoveAllListeners();
        CloseBtn.Button.onClick.AddListener(Cancel);
        if (cancelCallback != null)
            CloseBtn.Button.onClick.AddListener(cancelCallback);
        this.confirmCallback = confirmCallback;
    }

    public void ValidateInput() {
        if (isNewObject) {
            ConfirmButton.SetInteractivity(true);
            return;
        }

        bool valid = (string) nameInput.GetValue() != selectedObject.GetName();

        ConfirmButton.SetInteractivity(valid, "Name has not been changed");
    }

    public override async void Confirm() {
        string name = (string) nameInput.GetValue();
        if (name == selectedObject.GetName()) { //for new objects, without changing name
            Cancel();
            confirmCallback?.Invoke();
            return;
        }

        try {
            // Replace whitespace with underscore
            name = name.Replace(" ", "_");

            await selectedObject.Rename(name);
            Cancel();
            confirmCallback?.Invoke();
        } catch (Arcor2ConnectionException) {
            //notification already shown, nothing else to do
        }
    }

    public override async void Close() {
        //LeftMenu.Instance.UpdateVisibility();

        SelectorMenu.Instance.gameObject.SetActive(true);
        if (_updateVisibilityCallback != null)
            _updateVisibilityCallback.Invoke();
        base.Close();

        selectedObject = null;
    }

    public void Cancel() {
        if (selectedObject != null && !keepObjectLocked) {
            _ = selectedObject.WriteUnlock();
        }
        Close();

    }
}
