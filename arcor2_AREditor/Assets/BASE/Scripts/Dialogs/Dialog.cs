using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using System;

public abstract class Dialog : MonoBehaviour
{
    protected ModalWindowManager windowManager;

    private bool visible;
    public bool Visible => visible;

    public virtual void Awake() {
        windowManager = GetComponent<ModalWindowManager>();
    }

    protected virtual void UpdateToggleGroup(GameObject togglePrefab, GameObject toggleGroup, List<IO.Swagger.Model.ListScenesResponseData> scenes) {
        List<string> items = new List<string>();
        foreach (IO.Swagger.Model.ListScenesResponseData scene in scenes) {
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
            foreach (TMPro.TextMeshProUGUI text in toggle.GetComponentsInChildren<TMPro.TextMeshProUGUI>()) {
                text.text = item;
            }
            toggle.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();
            toggle.transform.SetAsFirstSibling();
        }
    }

    protected virtual string GetSelectedValue(GameObject toggleGroup) {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>()) {
            if (toggle.isOn) {
                return toggle.GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
            }
        }
        throw new Base.ItemNotFoundException("Nothing selected");
    }

    protected virtual void SetSelectedValue(GameObject toggleGroup, string name) {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>()) {
            TMPro.TextMeshProUGUI text = toggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text.text == name) {
                toggle.isOn = true;
                return;
            }
        }
        throw new Base.ItemNotFoundException("Nothing selected");
    }

    public virtual void Close() {
        visible = false;
        InputHandler.Instance.OnEscPressed -= OnEscPressed;
        InputHandler.Instance.OnEnterPressed -= OnEnterPressed;
        windowManager.CloseWindow();
        
    }

    public virtual void Open() {
        visible = true;
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
