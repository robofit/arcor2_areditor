using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConnectionSelectorDialog : Dialog {

    public GameObject DialogButtonPrefab;

    [SerializeField]
    private GameObject content;

    private UnityAction OnCancelCallback;

    public override void Confirm() {
        throw new System.NotImplementedException();
    }

    public void OnCancel() {
        Close();
        OnCancelCallback?.Invoke();
    }

    public void Open(Dictionary<string, LogicItem> connections, bool newConnection, InputOutput sender, UnityAction onCancel = null) {
        DestroyButtons();
        foreach (KeyValuePair<string, LogicItem> c in connections) {
            DialogButton dialogButton = Instantiate(DialogButtonPrefab, content.transform).GetComponent<DialogButton>();
            dialogButton.Init(c.Key, async () => await sender.SelectedConnection(c.Value),
                new List<InteractiveObject> { ProjectManager.Instance.GetAction(c.Value.Data.Start), ProjectManager.Instance.GetAction(c.Value.Data.End) });
        }
        if (newConnection) {
            DialogButton dialogButton = Instantiate(DialogButtonPrefab, content.transform).GetComponent<DialogButton>();
            dialogButton.Init("New connection", async () => await sender.SelectedConnection(null));
        }
        OnCancelCallback = onCancel;
        Open();
    }

    private void DestroyButtons() {
        foreach (Transform t in content.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
    }

    public override void Close() {
        DestroyButtons();
        base.Close();
    }
}
