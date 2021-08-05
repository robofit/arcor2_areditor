using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonWithIcon : ActionButton
{
    public Image Icon;

    public void SetIcon(Sprite sprite) {
        Icon.sprite = sprite;
    }

    public override void SetInteractable(bool interactable) {
        base.SetInteractable(interactable);
        text.color = interactable ? Color.white : Color.grey;
    }
}
