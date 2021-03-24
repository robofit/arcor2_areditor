using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TranformWheelUnits : MonoBehaviour
{
    private int index;
    public string DefaultValue;
    public List<string> Units;
    public Button PrevBtn, NextBtn;
    public TMP_Text Label;
    public TransformWheel TransformWheel;
    public UnityEvent OnChange;

    private void Awake() {
        if (Units.Contains(DefaultValue)) {
            index = Units.IndexOf(DefaultValue);
        } else {
            index = 0;
        }
        Label.text = Units[index];
        CheckBounds();

    }

    public string GetValue() {
        return Units[index];
    }

    public void SetIndex(int index) {
        this.index = index;
        Label.text = Units[index];
        CheckBounds();
    }

    public void Next() {
        if (index < Units.Count - 1) {
            ++index;
        }
        Label.text = Units[index];
        CheckBounds();
        OnChange?.Invoke();
    }

    public void Previous() {
        if (index > 0) {
            --index;
        }
        Label.text = Units[index];
        CheckBounds();
        OnChange?.Invoke();
    }

    private void CheckBounds() {
        PrevBtn.interactable = index != 0;
        NextBtn.interactable = index != Units.Count - 1;
    }

}
