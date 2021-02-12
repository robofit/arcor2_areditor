using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SelectorItem : MonoBehaviour
{
    public TMPro.TMP_Text Label;
    public Image Icon;
    public InteractiveObject InteractiveObject;
    public Button Button;
    public Image SelectionBorder, ManualSelector;
    public float Score;
    private long lastUpdate;
    private bool selected;

    private void Awake() {
        Button = GetComponent<Button>();
    }


    public void SetText(string text) {
        Label.text = text;
    }
    public void SetObject(InteractiveObject interactiveObject, float score, long currentIteration) {
        InteractiveObject = interactiveObject;
        Score = score;
        Button.onClick.AddListener(() => SelectorMenu.Instance.SetSelectedObject(this, true));
        lastUpdate = currentIteration;
    }

    public void UpdateScore(float score, long currentIteration) {
        lastUpdate = currentIteration;
        Score = score;
    }

    public long GetLastUpdate() {
        return lastUpdate;
    }

    public void SetSelected(bool selected, bool manually) {
        try {        
            if (selected) {
                InteractiveObject.SendMessage("OnHoverStart");
            } else {
                if (this.selected)
                    InteractiveObject.SendMessage("OnHoverEnd");
            }
        } catch (MissingReferenceException ex) {
            return;
        }
        this.selected = selected;
        if (manually) {
            SelectionBorder.gameObject.SetActive(false);
            ManualSelector.gameObject.SetActive(selected);
        } else {
            ManualSelector.gameObject.SetActive(false);
            SelectionBorder.gameObject.SetActive(selected);
        }
    }

    public bool IsSelected() {
        return selected;
    }



}
