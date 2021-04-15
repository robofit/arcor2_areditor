using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionSelectorDialog : Dialog {

    public GameObject DialogButtonPrefab;

    [SerializeField]
    private GameObject content;


    public override void Confirm() {
        throw new System.NotImplementedException();
    }

    public void Open(Dictionary<string, LogicItem> connections, bool newConnection, InputOutput sender) {
        foreach (Transform t in content.transform) {
            if (t.gameObject.tag != "Persistent")
                Destroy(t.gameObject);
        }
        foreach (KeyValuePair<string, LogicItem> c in connections) {
            DialogButton dialogButton = Instantiate(DialogButtonPrefab, content.transform).GetComponent<DialogButton>();
            dialogButton.Init(c.Key, async () => await sender.SelectedConnection(c.Value));
        }
        if (newConnection) {
            DialogButton dialogButton = Instantiate(DialogButtonPrefab, content.transform).GetComponent<DialogButton>();
            dialogButton.Init("New connection", async () => await sender.SelectedConnection(null));
        }
        Open();
    }

}
