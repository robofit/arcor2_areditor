using System;
using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Dialog : MonoBehaviour
{
    protected ModalWindowManager windowManager;

    public bool Visible {
        get;
        private set;
    }

    public virtual void Awake() {
        windowManager = GetComponent<ModalWindowManager>();
    }

    protected virtual void UpdateToggleGroup(GameObject togglePrefab, GameObject toggleGroup, List<ListScenesResponseData> scenes) {
        List<string> items = new();
        foreach (ListScenesResponseData scene in scenes) {
            items.Add(scene.Name);
        }
        UpdateToggleGroup(togglePrefab, toggleGroup, items);
    }


    protected virtual void UpdateToggleGroup(GameObject togglePrefab, GameObject toggleGroup, List<string> items) {
        foreach (Transform toggle in toggleGroup.transform) {
            if (toggle.gameObject.tag != "Persistent")
                Destroy(toggle.gameObject);
        }
        foreach (string item in items) {

            GameObject toggle = Instantiate(togglePrefab, toggleGroup.transform);
            foreach (TextMeshProUGUI text in toggle.GetComponentsInChildren<TextMeshProUGUI>()) {
                text.text = item;
            }
            toggle.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();
            toggle.transform.SetAsFirstSibling();
        }
    }

    protected virtual string GetSelectedValue(GameObject toggleGroup) {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>()) {
            if (toggle.isOn) {
                return toggle.GetComponentInChildren<TextMeshProUGUI>().text;
            }
        }
        throw new ItemNotFoundException("Nothing selected");
    }

    protected virtual void SetSelectedValue(GameObject toggleGroup, string name) {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>()) {
            TextMeshProUGUI text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (text.text == name) {
                toggle.isOn = true;
                return;
            }
        }
        throw new ItemNotFoundException("Nothing selected");
    }

    public virtual void Close() {
        Visible = false;
        InputHandler.Instance.OnEscPressed -= OnEscPressed;
        InputHandler.Instance.OnEnterPressed -= OnEnterPressed;
        windowManager.CloseWindow();
        
    }

    public virtual void Open() {
        Visible = true;
        InputHandler.Instance.OnEscPressed += OnEscPressed;
        InputHandler.Instance.OnEnterPressed += OnEnterPressed;
        windowManager.OpenWindow();
    }

    private void OnEnterPressed(object sender, EventArgs e) {
        Confirm();
    }

    private void OnEscPressed(object sender, EventArgs e) {
        Close();
    }

    public abstract void Confirm();
}
