using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonWithTooltip : MonoBehaviour {
    [SerializeField]
    private ManualTooltip tooltip;
    public Button Button;
    [SerializeField]
    private TooltipContent TooltipContent;

    private void Awake() {
        Button = gameObject.GetComponent<Button>();
        TooltipContent = gameObject.GetComponent<TooltipContent>();
    }


    public virtual void SetInteractivity(bool interactable) {
        if (Button == null)
            return;
        Button.interactable = interactable;
        if (interactable) {
            tooltip.DisplayAlternativeDescription = false;
        } else {
            tooltip.DisplayAlternativeDescription = true;
        }
    }

    public virtual void SetInteractivity(bool interactable, string alternativeDescription) {
        if (tooltip == null)
            return;
        tooltip.DescriptionAlternative = alternativeDescription;
        SetInteractivity(interactable);
        
    }

    public void SetDescription(string description) {
        if (tooltip == null)
            return;
        tooltip.Description = description;
        tooltip.DisplayAlternativeDescription = false;
        if (TooltipContent.descriptionText != null)
            TooltipContent.descriptionText.text = description;
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

}
