using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformWheelItem : MonoBehaviour
{
    public float Value;
    public TMPro.TMP_Text Label;

    public void SetValue(float value, float valueUnit) {
        Value = value;
        SetLabel(valueUnit);
    }

    public void SetLabel(float value) {
        Label.SetText(value.ToString());
    }

    public void SetLabel(string value) {
        Label.SetText(value);
    }
}
