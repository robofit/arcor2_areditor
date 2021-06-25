using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonWithIcon : ActionButton
{
    public Image Icon;

    public void SetIcon(Sprite sprite) {
        Icon.sprite = sprite;
    }
}
