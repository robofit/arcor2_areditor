using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DialogButton : MonoBehaviour {
    [SerializeField]
    private TMPro.TMP_Text normal, highlighted;
    private Button btn;
    private void Awake() {
        btn = GetComponent<Button>();
    }

    public void SetLabel(string text) {
        normal.text = text;
        highlighted.text = text;
    }

    public void AddListener(UnityAction callback) {
        btn.onClick.AddListener(callback);
    }

    public void Init(string label, UnityAction callback) {
        SetLabel(label);
        AddListener(callback);
    }
}
