using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Michsky.UI.ModernUIPack;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonWithTooltip : MonoBehaviour {
    [SerializeField]
    private ManualTooltip tooltip;
    public Button Button, Button2;
    [SerializeField]
    private TooltipContent TooltipContent;
    [SerializeField]

    private void Awake() {
        Button = gameObject.GetComponent<Button>();
        TooltipContent = gameObject.GetComponent<TooltipContent>();
    }


    public virtual void SetInteractivity(bool interactable) {
        if (Button == null)
            return;
        Button.interactable = interactable;
        if (Button2 != null)
            Button2.interactable = interactable;

        tooltip.DisplayAlternativeDescription = !interactable;
    }

    public virtual void SetInteractivity(bool interactable, string alternativeDescription) {
        if (tooltip == null)
            return;
        tooltip.DescriptionAlternative = alternativeDescription;
        SetInteractivity(interactable);
    }

    public void SetDescription(string description) {
        if (tooltip == null || string.IsNullOrEmpty(description))
            return;

        tooltip.Description = description;
        tooltip.DisplayAlternativeDescription = false;
    }

    public void HideTooltip() {
        if (TooltipContent == null)
            return;
        TooltipContent.tooltipAnimator.Play("Out");
    }

    public bool IsInteractive() {
        if (Button == null)
            return false;
        return Button.interactable;
    }

    public string GetDescription() {
        return tooltip.Description;
    }

    public string GetAlternativeDescription() {
        return tooltip.DescriptionAlternative;
    }


}
