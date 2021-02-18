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


    public void SetInteractivity(bool interactable) {
        Button.interactable = interactable;
        if (interactable) {
            tooltip.ShowDefaultDescription();
        } else {
            tooltip.ShowAlternativeDescription();
        }
    }

    public void SetInteractivity(bool interactable, string alternativeDescription) {
        tooltip.DescriptionAlternative = alternativeDescription;
        SetInteractivity(interactable);
        
    }

    public void SetDescription(string description) {
        tooltip.Description = description;
        tooltip.ShowDefaultDescription();
    }

    public void HideTooltip() {
        TooltipContent.tooltipAnimator.Play("Out");
    }

    public bool IsInteractive() {
        return Button.interactable;
    }

}
