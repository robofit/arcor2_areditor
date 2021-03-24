using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformWheelItem : MonoBehaviour
{
    public float Value;
    public TMPro.TMP_Text Label;

    public void SetValue(float value) {
        Value = value;
        Label.SetText(value.ToString());
    }
}
