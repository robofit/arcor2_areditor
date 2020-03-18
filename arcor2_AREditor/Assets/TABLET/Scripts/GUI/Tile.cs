using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text Label;
    [SerializeField]
    private Button MainButton, OptionButton;

    public void SetLabel(string label) {
        Label.text = label;
    }

    public void AddListener(UnityAction callback) {
        MainButton.onClick.AddListener(callback);
    }
}
