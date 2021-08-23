using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoordBtn : MonoBehaviour
{
    private void Awake() {
        Background.color = Color.clear;
    }

    public Image Background, Outline;
    public TMP_Text Value, Delta;

    public void SetDeltaMeters(float value) {
        SetMeters(value, Delta, true);
    }

    public void SetValueMeters(float value) {
        SetMeters(value, Value, false);
    }

    private void SetMeters(float value, TMP_Text field, bool delta) {
        if (delta) {
            field.text = "Δ ";
        } else {
            field.text = "";
        }
        if (value < 0.99 && value >= 0.009 || value > -0.99 && value < -0.009 )
            field.text += string.Format("{0:0.##cm}", value * 100);
        else if (value < 0.009 && value > 0.009)
            field.text += string.Format("{0:0.##mm}", value * 1000);
        else
            field.text += string.Format("{0:0.##m}", value);
    }

    public void SetDeltaDegrees(float value) {
        SetDegrees(value, Delta, true);
    }

    public void SetValueDegrees(float value) {
        SetDegrees(value, Value, false);
    }

    private void SetDegrees(float value, TMP_Text field, bool delta) {
        if (delta) {
            field.text = "Δ ";
        } else {
            field.text = "";
        }
        if (value < 0.99 && value >= 0.009 || value > -0.99 && value < -0.009)
            field.text += string.Format("{0:0.##'}", value * 60);
        else if (value < 0.009 && value > 0.009)
            field.text += string.Format("{0:0.##''}", value * 3600);
        else
            field.text += string.Format("{0:0.##°}", value);
    }

    public TransformMenu.Axis Axis;
    public void Deselect() {
        Background.color = new Color(Outline.color.r, Outline.color.g, Outline.color.b, 0f);
    }

    public void Select() {
        Background.color = new Color(Outline.color.r, Outline.color.g, Outline.color.b, 0.5f);
        TransformMenu.Instance.SetRotationAxis(Axis);
    }

    public void OnClick() {

    }
}
