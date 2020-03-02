using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class LayoutDropdown : MonoBehaviour
{
    public GameObject Trigger;
    public VerticalLayoutGroup Layout;


    private void Start() {
        GetComponent<Button>().onClick.AddListener(OnClick);
        enabled = false;
    }

    private void OnClick() {
        Layout.enabled = false;
        enabled = true;
    }

    private void Update() {
        if (!Trigger.activeSelf) {
            Layout.enabled = true;
            enabled = false;
        }
    }
}
