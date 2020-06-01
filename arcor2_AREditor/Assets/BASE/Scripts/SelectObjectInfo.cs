using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectObjectInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private Button btn;

    [SerializeField]
    private ContentSizeFitter fitter;

    public void Show(string text, UnityAction closeCallback) {
        this.text.text = text;
        btn.onClick.RemoveAllListeners();
        gameObject.SetActive(true);
        fitter.enabled = false;
        fitter.enabled = true;
        if (closeCallback != null)
            btn.onClick.AddListener(() => closeCallback());
        btn.onClick.AddListener(() => gameObject.SetActive(false));
    }
}
