using System.Collections;
using System.Collections.Generic;
using Base;
using TMPro;
using UnityEngine;

public class DisplayTextOnHover : Clickable {
    public TextMeshPro TextToDisplay;

    public override void OnHoverStart() {
        TextToDisplay.gameObject.SetActive(true);
    }

    public override void OnHoverEnd() {
        TextToDisplay.gameObject.SetActive(false);
    }

}
