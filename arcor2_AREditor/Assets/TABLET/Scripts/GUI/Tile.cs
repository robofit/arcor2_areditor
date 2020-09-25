using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text Label;
    [SerializeField]
    private Button MainButton, OptionButton;
    [SerializeField]
    private Image Background;
    [SerializeField]
    private GameObject star;
    public Image TopImage;
    public DateTime Created, Modified;


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

    public virtual void InitTile(string tileLabel, UnityAction mainCallback, UnityAction optionCallback, bool starVisible, DateTime created, DateTime modified) {
        SetLabel(tileLabel);
        if (mainCallback != null) {
            AddListener(mainCallback);
        }
        Created = created;
        Modified = modified;
        OptionAddListener(optionCallback);
        SetStar(starVisible);
    }

    /// <summary>
    /// Start blinking with tile 
    /// </summary>
    public void Highlight() {
        StartCoroutine(Blink());
    }

    private IEnumerator Blink() {
        float timePassed = 0;
        while (timePassed < 1) {
            // Code to go left here
            timePassed += Time.deltaTime;
            Background.color = Color.Lerp(Color.white, new Color(0.3f, 0.3f, 0.3f), Mathf.PingPong(timePassed, 0.5f)*2);
            yield return null;
        }
        Background.color = Color.white;
    }
    public Button GetOptionButton() {
        return OptionButton;
    }

}
