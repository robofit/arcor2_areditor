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
    [SerializeField]
    private GameObject star;
    public Image TopImage;



    public void SetLabel(string label) {
        Label.text = label;
    }

    public string GetLabel() {
        return Label.text;
    }

    public void AddListener(UnityAction callback) {
        if (callback != null)
            MainButton.onClick.AddListener(callback);
    }

    public void OptionAddListener(UnityAction callback) {
        if (callback == null) {
            OptionButton.gameObject.SetActive(false);
        } else {
            OptionButton.gameObject.SetActive(true);
            OptionButton.onClick.AddListener(callback);
        }        
    }

    public void SetStar(bool visible) {
        star.SetActive(visible);
    }

    public bool GetStarred() {
        return star.activeSelf;
    }

    public virtual void InitTile(string tileLabel, UnityAction mainCallback, UnityAction optionCallback, bool starVisible) {
        SetLabel(tileLabel);
        if (mainCallback != null) {
            AddListener(mainCallback);
        }
        OptionAddListener(optionCallback);
        SetStar(starVisible);
    }
}
